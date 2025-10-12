using System;
using System.Collections.Generic;
using ExamplePlugin.UI;
using System.Linq;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using ExamplePlugin.Loadout.Corruption;

namespace ExamplePlugin.Loadout.Draft
{
    /// <summary>
    /// Defines what can show up in a Drafting Grid for a player to pick
    /// </summary>
    public class DraftPools
    {

        private static DraftPools _instance;

        /// <summary>
        /// Singleton instance of the loadout
        /// </summary>
        public static DraftPools Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DraftPools();
                }
                return _instance;
            }
        }

        private Dictionary<DraftItemTier, List<PickupIndex>> allowedByDraftTier = new();

        private static HashSet<ItemIndex> bannedItems = new();

        private DraftPools()
        {
            // Build mappings
            BuildBannedItems();
            BuildDraftableTiers();
        }

        private void BuildBannedItems()
        {
            // white
            bannedItems.Add(RoR2Content.Items.ScrapWhite.itemIndex);
            bannedItems.Add(RoR2Content.Items.TreasureCache.itemIndex);

            // green
            bannedItems.Add(RoR2Content.Items.ScrapGreen.itemIndex);

            // red
            bannedItems.Add(RoR2Content.Items.ScrapRed.itemIndex);
            bannedItems.Add(RoR2Content.Items.CaptainDefenseMatrix.itemIndex);

            // yellow
            bannedItems.Add(RoR2Content.Items.ArtifactKey.itemIndex);
            bannedItems.Add(RoR2Content.Items.ScrapYellow.itemIndex);
            bannedItems.Add(RoR2Content.Items.ShinyPearl.itemIndex);
            bannedItems.Add(RoR2Content.Items.TitanGoldDuringTP.itemIndex);
            bannedItems.Add(RoR2Content.Items.Pearl.itemIndex);

            // voids
            if (CorruptedItemDefs.TryGetItemDef(CorruptedItem.EncrustedKey, out var voidTreasureDef))
            {
                bannedItems.Add(voidTreasureDef.itemIndex);
            }
            if (CorruptedItemDefs.TryGetItemDef(CorruptedItem.NewlyHatchedZoea, out var zoeaDef))
            {
                bannedItems.Add(zoeaDef.itemIndex);
            }
        }

        private void BuildDraftableTiers()
        {
            // init our map
            foreach (DraftItemTier draftTier in Enum.GetValues(typeof(DraftItemTier)))
            {
                allowedByDraftTier[draftTier] = new();
            }

            var allDefs = PickupCatalog.allPickups;
            Log.Info($"[DraftPools] Going through {allDefs.Count()} defs");
            foreach (var def in allDefs)
            {
                // not sure how these happen
                if (def == null)
                {
                    continue;
                }
                if (def.itemIndex == ItemIndex.None)
                {
                    continue;
                }

              
                if(!DraftTierMaps.HasDraftTier(def.itemTier))
                {
                    Log.Debug($"[DraftPools] Unmapped {def.nameToken}");
                    continue;
                }

                if (bannedItems.Contains(def.itemIndex))
                {
                    Log.Debug($"[DraftPools] Skipping banned {def.nameToken}");
                    continue;
                }

                var draftTier = DraftTierMaps.ToDraft(def.itemTier);
                allowedByDraftTier[draftTier].Add(def.pickupIndex);
            }
        }

        public List<PickupIndex> GetDraftablePickups(DraftItemTier draftTier)
        {
            return allowedByDraftTier[draftTier];
        }
    }
}
