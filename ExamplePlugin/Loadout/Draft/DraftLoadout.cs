using System;
using System.Collections.Generic;
using System.Text;
using ExamplePlugin.Attributes;
using RoR2;

namespace ExamplePlugin.Loadout.Draft
{
    /// <summary>
    /// Model for the UI to manage our draft picks, restrictions, etc
    /// </summary>
    public class DraftLoadout
    {
        private static DraftLoadout _instance;

        /// <summary>
        /// Singleton instance of the loadout
        /// </summary>
        public static DraftLoadout Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DraftLoadout();
                }
                return _instance;
            }
        }

        private readonly Dictionary<DraftItemTier, DraftTierRestrictionSettings> settingsByDraftTier = new();

        /// <summary>
        /// Tracks the order in which we have picked things in case we need to trim down,
        /// we will remove the latest pick
        /// </summary>
        private readonly Dictionary<DraftItemTier, List<DraftPick>> pickOrderByDraftTier = new();

        /// <summary>
        /// Save what we have picked by the RoR2 model's tiers
        /// </summary>
        private readonly Dictionary<ItemTier, HashSet<ItemIndex>> itemsPickedByNativeTier = new();

        /// <summary>
        /// Tracks what normal items are locked due to a corruption pick
        /// </summary>
        private readonly Dictionary<DraftItemTier, HashSet<ItemIndex>> itemsLockedByDraftTier = new();

        private DraftLoadout()
        {
            InitializeSettings();
            InitializeNativeTiers();
        }

        private void InitializeSettings()
        {
            foreach (DraftItemTier draftTier in Enum.GetValues(typeof(DraftItemTier)))
            {
                var draftSettings = new DraftTierRestrictionSettings();
                settingsByDraftTier[draftTier] = draftSettings;
                pickOrderByDraftTier[draftTier] = new List<DraftPick>();
                // mode always restricted , allows for a none mode later if we want
                draftSettings.Mode = DraftLimitMode.Restricted;
                draftSettings.Max = GetSeedMaxForTier(draftTier);
                draftSettings.HasSeededMax = true;
            }
        }

        private void InitializeNativeTiers()
        {
            var itemTiers = new[]
            {
                ItemTier.Tier1, ItemTier.Tier2, ItemTier.Tier3,
                ItemTier.VoidTier1, ItemTier.VoidTier2, ItemTier.VoidTier3,
                ItemTier.Boss, ItemTier.Lunar
            };

            foreach (var nativeTier in itemTiers)
            {
                itemsPickedByNativeTier[nativeTier] = new HashSet<ItemIndex>();
            }
        }

        #region Settings Management
        public void SetMode(DraftItemTier tier, DraftLimitMode mode)
        {
            var settings = GetOrCreate(tier);
            if (settings.Mode == mode)
            {
                // already se somehow;
                return;
            }

            settings.Mode = mode;

            // do we have to seed on first switch
            if (mode == DraftLimitMode.Restricted && !settings.HasSeededMax)
            {
                settings.Max = GetSeedMaxForTier(tier);
                settings.HasSeededMax = true;
            }
        }

        public DraftLimitMode GetMode(DraftItemTier tier)
        {
            var settings = GetOrCreate(tier);

            return settings.Mode;
        }

        public void SetLimit(DraftItemTier tier, int newMax)
        {
            var settings = GetOrCreate(tier);
            settings.Max = Math.Max(0, newMax);
        }

        public int GetLimit(DraftItemTier tier)
        {
            var settings = GetOrCreate(tier);
            return settings.Max;
        }

        public int GetCount(DraftItemTier tier)
        {
            var settings = GetOrCreate(tier);
            //if (settings.Mode == DraftLimitMode.None)
            //{
            //    return 0;
            //}

            return GetPickedCountForDraftTier(tier);
        }

        #endregion

        #region Item Management API

        /// <summary>
        /// Determines whether the given item can be picked for the loadout
        /// </summary>
        /// <param name="pickupDef"></param>
        /// <param name="reason"></param>
        /// <returns></returns>
        public bool CanPick(PickupDef pickupDef, out string reason)
        {
            reason = null;

            if (pickupDef == null)
            {
                reason = "Invalid PickupDef";
                return false;
            }

            var draftTier = DraftTierMaps.ToDraft(pickupDef.itemTier);

            var settings = GetOrCreate(draftTier);

            // unrestricted
            //if (settings.Mode == DraftLimitMode.None)
            //{
            //    return true;
            //}

            if (settings.Max <= 0)
            {
                reason = $"{draftTier} picks are disabled.";
                return false;
            }

            int count = GetPickedCountForDraftTier(draftTier);
            if (count >= settings.Max)
            {
                reason = $"Limit reached for {draftTier} ({count}/{settings.Max}).";
                return false;
            }

            return true;
        }

        public bool CanPick(PickupDef pickupDef)
        {
            return CanPick(pickupDef, out _);
        }

        /// <summary>
        /// Determines whether this item will be available in our run or not
        /// </summary>
        /// <param name="pickupDef">the pickup definition</param>
        /// TODO rename
        /// <returns></returns>
        public bool IsAvailable(PickupDef pickupDef)
        {
            if (pickupDef == null)
            {
                return false;
            }


            var draftTier = DraftTierMaps.ToDraft(pickupDef.itemTier);
            var settings = GetOrCreate(draftTier);

            //if (settings.Mode == DraftLimitMode.None)
            //{
            //    // always available
            //    return true;
            //}

            return IsPicked(pickupDef);
        }

        public bool IsPicked(PickupDef pickupDef)
        {
            if (pickupDef == null)
            {
                return false;
            }

            var draftTier = DraftTierMaps.ToDraft(pickupDef.itemTier);
            var settings = GetOrCreate(draftTier);

            //if (settings.Mode == DraftLimitMode.None)
            //{
            //    // always picked
            //    return true;
            //}

            var itemTier = pickupDef.itemTier;
            var selectedSet = GetOrCreateNativeSet(itemTier);
            return selectedSet.Contains(pickupDef.itemIndex);
        }

        public bool TryPick(PickupDef pickupDef, out string reason)
        {
            if (!CanPick(pickupDef, out reason))
            {
                return false;
            }

            var itemTier = pickupDef.itemTier;
            var selectedSet = GetOrCreateNativeSet(itemTier);

            var itemIndex = pickupDef.itemIndex;
            selectedSet.Add(pickupDef.itemIndex);

            // track order
            var draftTier = DraftTierMaps.ToDraft(pickupDef.itemTier);
            var pickOrderList = GetOrCreatePickupOrder(draftTier);
            var draftPick = ToDraftPick(pickupDef);
            if (!pickOrderList.Contains(draftPick))
            {
                pickOrderList.Add(draftPick);
            }

            return true;
        }

        public void UnPick(PickupDef pickupDef)
        {
            var itemTier = pickupDef.itemTier;
            var selectedSet = GetOrCreateNativeSet(itemTier);
            selectedSet.Remove(pickupDef.itemIndex);

            // remove tracking
            var draftTier = DraftTierMaps.ToDraft(pickupDef.itemTier);
            var pickOrderList = GetOrCreatePickupOrder(draftTier);
            var draftPick = ToDraftPick(pickupDef);
            pickOrderList.Remove(draftPick);
        }

        /// <summary>
        /// Uses a First In Last Out approach when trimming the picked items if the limit decreases the current capacity
        /// </summary>
        /// <param name="draftTier"></param>
        public HashSet<ItemIndex> TrimToLimit(DraftItemTier draftTier)
        {
            var settings = GetOrCreate(draftTier);
            //// nothing to do
            //if (settings.Mode == DraftLimitMode.None)
            //{
            //    return;
            //}

            var desiredAmount = Math.Max(0, settings.Max);
            var currAmount = GetPickedCountForDraftTier(draftTier);
            var amountToRemove = currAmount - desiredAmount;
            if (amountToRemove <= 0)
            {
                // we are within bounds
                return [];
            }

            // cap to list size
            var pickOrderList = GetOrCreatePickupOrder(draftTier);
            amountToRemove = Math.Min(amountToRemove, pickOrderList.Count);


            var removedIndices = new HashSet<ItemIndex>();
            // work backwards
            for (int i = 0; i < amountToRemove; i++)
            {
                var itemToRemove = pickOrderList[^1];
                var itemToRemoveIndex = itemToRemove.ItemIndex;

                // remove our pick
                pickOrderList.Remove(itemToRemove);
                removedIndices.Add(itemToRemoveIndex);

                // remove it from the tier set
                var nativeSet = GetOrCreateNativeSet(itemToRemove.ItemTier);
                nativeSet.Remove(itemToRemoveIndex);
            }

            return removedIndices;
        }

        public bool IsLocked(PickupDef pickupDef)
        {
            var draftTier = DraftTierMaps.ToDraft(pickupDef.itemTier);

            var lockedSet = GetOrCreateLockedSet(draftTier);
            return lockedSet.Contains(pickupDef.itemIndex);
        }

        /// <summary>
        /// Attemps to lock this item, this usually would happen if a corrupt item is picked
        /// and we need to lock the normal version to avoid picking that too
        /// </summary>
        /// <param name="pickupDef">the pickup definition</param>
        /// <returns>true if we were able to lock the item false otherwise</returns>
        public bool TryLock(PickupDef pickupDef)
        {
            // if its already locked do nothing
            var currentlyLocked = IsLocked(pickupDef);
            if (currentlyLocked)
            {
                // TODO remove warning this is fine or well we check if already locked
                Log.Warning($"Attempting to relock {pickupDef.itemIndex}");
                return false;
            }

            // lock it
            var draftTier = DraftTierMaps.ToDraft(pickupDef.itemTier);
            var itemsLocked = GetOrCreateLockedSet(draftTier);
            itemsLocked.Add(pickupDef.itemIndex);
            return true;
        }

        public void Unlock(PickupDef pickupDef)
        {
            if (!IsLocked(pickupDef))
            {
                return;
            }

            var draftTier = DraftTierMaps.ToDraft(pickupDef.itemTier);
            var itemsLocked = GetOrCreateLockedSet(draftTier);
            itemsLocked.Remove(pickupDef.itemIndex);
        }

        /// <summary>
        /// Removes all picks and locks
        /// </summary>
        public void ClearAllPicksAndLocks()
        {
            foreach (DraftItemTier draftTier in Enum.GetValues(typeof(DraftItemTier)))
            {
                var itemTiers = DraftTierMaps.ToNativeSet(draftTier);
                foreach (var itemTier in itemTiers)
                {
                    var selectedSet = GetOrCreateNativeSet(itemTier);
                    selectedSet.Clear();
                }

                var pickOrderList = GetOrCreatePickupOrder(draftTier);
                pickOrderList.Clear();

                var itemsLocked = GetOrCreateLockedSet(draftTier);
                itemsLocked.Clear();
            }
        }

        public HashSet<ItemIndex> ClearPicks(DraftItemTier draftTier)
        {
            var originallyPicked = new HashSet<ItemIndex>();

            var itemTiers = DraftTierMaps.ToNativeSet(draftTier);
            foreach (var itemTier in itemTiers)
            {
                var selectedSet = GetOrCreateNativeSet(itemTier);
                originallyPicked.UnionWith(selectedSet);
                selectedSet.Clear();
            }

            return originallyPicked;
        }

        #endregion

        #region Model Translation

        public PlayerLoadout ToPlayerLoadout()
        {
            var playerLoadout = new PlayerLoadout();

            // map all our sertings to the loadout model
            foreach (var draftKV in settingsByDraftTier)
            {
                var draftTier = draftKV.Key;
                var draftSetting = draftKV.Value;

                // get the native item tiers, accounts for purples
                foreach (var nativeTier in DraftTierMaps.ToNativeSet(draftTier))
                {
                    var playerLoadoutMode = ToTierLimitMode(draftSetting.Mode);
                    var playerLoadoutTierLimit = new TierLimit
                    {
                        tier = nativeTier,
                        mode = playerLoadoutMode,
                        allowed = new HashSet<ItemIndex>(),
                        restrictedByVoid = new HashSet<ItemIndex>()
                    };

                    // only populate when we are in restrictions mode
                    if (playerLoadoutMode == TierLimitMode.Restricted)
                    {
                        var pickedIndices = GetOrCreateNativeSet(nativeTier);
                        foreach (var index in pickedIndices)
                        {
                            playerLoadoutTierLimit.allowed.Add(index);
                        }

                        // If we have any locked ones in the tier, add them here
                        var lockedIndices = GetOrCreateLockedSet(draftTier);
                        foreach (var index in lockedIndices)
                        {
                            playerLoadoutTierLimit.restrictedByVoid.Add(index);
                        }
                    }

                    playerLoadout.byTier[nativeTier] = playerLoadoutTierLimit;
                }
            }

            return playerLoadout;
        }

        private TierLimitMode ToTierLimitMode(DraftLimitMode limitMode)
        {
            switch (limitMode)
            {
                //case DraftLimitMode.None:
                //    return TierLimitMode.None;
                case DraftLimitMode.Restricted:
                    return TierLimitMode.Restricted;
            }

            throw new NotImplementedException($"DraftLimitMode: {limitMode} has no mapping");
        }
        #endregion

        private int GetPickedCountForDraftTier(DraftItemTier draftTier)
        {
            var sum = 0;

            var nativeTiers = DraftTierMaps.ToNativeSet(draftTier);
            foreach (var ti in nativeTiers)
            {
                sum += GetOrCreateNativeSet(ti).Count;
            }

            return sum;
        }

        private HashSet<ItemIndex> GetOrCreateNativeSet(ItemTier tier)
        {
            if (!itemsPickedByNativeTier.TryGetValue(tier, out var set))
            {
                set = new HashSet<ItemIndex>();
                itemsPickedByNativeTier[tier] = set;
            }
            return set;
        }

        private List<DraftPick> GetOrCreatePickupOrder(DraftItemTier draftTier)
        {
            if (!pickOrderByDraftTier.TryGetValue(draftTier, out var order))
            {
                order = new List<DraftPick>();
                pickOrderByDraftTier[draftTier] = order;
            }

            return order;
        }

        private HashSet<ItemIndex> GetOrCreateLockedSet(DraftItemTier draftTier)
        {
            if (!itemsLockedByDraftTier.TryGetValue(draftTier, out var set))
            {
                set = new HashSet<ItemIndex>();
                itemsLockedByDraftTier[draftTier] = set;
            }

            return set;
        }

        private DraftTierRestrictionSettings GetOrCreate(DraftItemTier tier)
        {
            // should never happen but JIC
            if (!settingsByDraftTier.TryGetValue(tier, out var settings))
            {
                settings = new DraftTierRestrictionSettings();
                settingsByDraftTier[tier] = settings;
            }

            return settings;
        }

        private static int GetSeedMaxForTier(DraftItemTier tier)
        {
            switch (tier)
            {
                case DraftItemTier.White: return 7;
                case DraftItemTier.Green: return 5;
                case DraftItemTier.Red: return 3;
                case DraftItemTier.Yellow: return 1;
                case DraftItemTier.Purple: return 1;
                // catchall
                default:
                    return 10;
            }
        }

        private static DraftPick ToDraftPick(PickupDef pickupDef)
        {
            return ToDraftPick(pickupDef.itemIndex, pickupDef.itemTier);
        }

        private static DraftPick ToDraftPick(ItemIndex itemIndex, ItemTier itemTier)
        {
            return new DraftPick(itemIndex, itemTier);
        }
    }
}
