using System;
using System.Collections.Generic;
using System.Linq;
using RoR2;
using ArtifactsOfMight.Loadout.Corruption;
using ArtifactsOfMight.Logger;

namespace ArtifactsOfMight.Loadout.Draft
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

        private static readonly ScoppedLogger.Scoped LOGGER = ScoppedLogger.For<DraftPools>();

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
            bannedItems.Add(DLC2Content.Items.ExtraStatsOnLevelUp.itemIndex);

            // red
            bannedItems.Add(RoR2Content.Items.ScrapRed.itemIndex);
            bannedItems.Add(RoR2Content.Items.CaptainDefenseMatrix.itemIndex);

            // yellow
            bannedItems.Add(RoR2Content.Items.ArtifactKey.itemIndex);
            bannedItems.Add(RoR2Content.Items.ScrapYellow.itemIndex);
            bannedItems.Add(RoR2Content.Items.ShinyPearl.itemIndex);
            bannedItems.Add(RoR2Content.Items.TitanGoldDuringTP.itemIndex);
            bannedItems.Add(RoR2Content.Items.Pearl.itemIndex);

            // voids TODO use DLC1 refs
            if (CorruptedItemDefs.TryGetItemDef(CorruptedItem.EncrustedKey, out var voidTreasureDef))
            {
                bannedItems.Add(voidTreasureDef.itemIndex);
            }
            if (CorruptedItemDefs.TryGetItemDef(CorruptedItem.NewlyHatchedZoea, out var zoeaDef))
            {
                bannedItems.Add(zoeaDef.itemIndex);
            }

            // Alloyed Draft Bans (like the ones from cerebellum shit)
            bannedItems.Add(DLC3Content.Items.MasterCore.itemIndex);
            bannedItems.Add(DLC3Content.Items.MasterBattery.itemIndex);
            bannedItems.Add(DLC3Content.Items.PowerCube.itemIndex);
            bannedItems.Add(DLC3Content.Items.PowerPyramid.itemIndex);

            // ExtraEquipment = functional coupler, allowed
            // ShockDamageAura = faulty conductor , allowed

        }

        private void BuildDraftableTiers()
        {
            // init our map
            foreach (DraftItemTier draftTier in Enum.GetValues(typeof(DraftItemTier)))
            {
                allowedByDraftTier[draftTier] = new();
            }

            var allDefs = PickupCatalog.allPickups;
            LOGGER.Info($"Going through {allDefs.Count()} defs");
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
                    LOGGER.Debug($"Unmapped {def.nameToken}");
                    
                    continue;
                }

                if (bannedItems.Contains(def.itemIndex))
                {
                    LOGGER.Debug($" Skipping banned {def.nameToken}");

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
