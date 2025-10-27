using System;
using System.Collections.Generic;
using System.Text;
using ArtifactsOfMight.RunConfig;
using RoR2;

namespace ArtifactsOfMight.Loadout.Corruption
{
    /// <summary>
    /// Handles mappings of the corrupted items and what items corrupt into them
    /// </summary>
    public static class CorruptionMaps
    {
        private static Dictionary<ItemIndex, ItemIndex> NormalToVoid = new();
        private static Dictionary<ItemIndex, HashSet<ItemIndex>> VoidToNormals = new();

        private static HashSet<ItemTier> VoidItemTiers = new HashSet<ItemTier>
        {
            ItemTier.VoidTier1, ItemTier.VoidTier2, ItemTier.VoidTier3, ItemTier.VoidBoss
        };

        // TODO build map RoR2 to CorruptItem index
        // Then do the build maps thing with item defs n shit

        static CorruptionMaps()
        {
            BuildMaps();
        }


        /// <summary>
        /// Gets the set of PickupDefs that are corrupted by a given void item
        /// </summary>
        /// <param name="voidIndex"></param>
        /// <returns></returns>
        public static List<PickupDef> GetNormalsPickupDefsForVoid(ItemIndex voidIndex)
        {
            var normalDefs = new List<PickupDef>();
            foreach (var normalIndex in VoidToNormals[voidIndex])
            {
                var normalDef = PickupCatalog.GetPickupDef(PickupCatalog.FindPickupIndex(normalIndex));
                if (normalDef == null)
                {
                    Log.Warning($"Got a null PickupDef with index {normalIndex}");
                }
                normalDefs.Add(normalDef);
            }

            return normalDefs;
        }

        /// <summary>
        /// Determines whether the given item is corruptible or not, returning the index of what item corrupts the given normal
        /// </summary>
        /// <param name="normalIndex"></param>
        /// <returns></returns>
        public static bool HasVoidMapping(ItemIndex normalIndex, out ItemIndex voidIndex)
        {
            if (NormalToVoid.TryGetValue(normalIndex, out voidIndex))
            {
                return true;
            }

            return false;
        }

        private static void BuildMaps()
        {
            // In wiki order except for NewlyHatched since it does all yellows
            TryToMap(RoR2Content.Items.Clover.itemIndex, CorruptedItem.BenthicBloom);
            TryToMap(RoR2Content.Items.TreasureCache.itemIndex, CorruptedItem.EncrustedKey);
            TryToMap(RoR2Content.Items.CritGlasses.itemIndex, CorruptedItem.LostSeersLenses);
            TryToMap(RoR2Content.Items.EquipmentMagazine.itemIndex, CorruptedItem.LysateCell);
            TryToMap(RoR2Content.Items.BleedOnHit.itemIndex, CorruptedItem.Needletick);
            TryToMap(RoR2Content.Items.Missile.itemIndex, CorruptedItem.PlasmaShrimp);
            TryToMap(RoR2Content.Items.ExtraLife.itemIndex, CorruptedItem.PluripotentLarva);
            TryToMap(RoR2Content.Items.ChainLightning.itemIndex, CorruptedItem.Polylute);
            TryToMap(RoR2Content.Items.Bear.itemIndex, CorruptedItem.SaferSpaces);
            TryToMap(RoR2Content.Items.IceRing.itemIndex, CorruptedItem.SingularityBand);
            TryToMap(RoR2Content.Items.FireRing.itemIndex, CorruptedItem.SingularityBand);
            TryToMap(RoR2Content.Items.SlowOnHit.itemIndex, CorruptedItem.Tentabauble);
            TryToMap(RoR2Content.Items.ExplodeOnDeath.itemIndex, CorruptedItem.VoidsentFlame);
            TryToMap(RoR2Content.Items.Mushroom.itemIndex, CorruptedItem.WeepingFungus);
            
            // Not adding hatched zoea as pickable
            //TryToMap(RoR2Content.Items.BleedOnHit.itemIndex, CorruptedItem.NewlyHatchedZoea);

            if (DebugSettings.LOG_DRAFT_POOLS_INFO)
            {
                Log.Info($"CorruptionMaps normalToVoid count: {NormalToVoid.Count}");
            }

            // Do the reverse buildup
            foreach (var normalToVoidPair in NormalToVoid)
            {
                var normalIndex = normalToVoidPair.Key;
                var voidIndex = normalToVoidPair.Value;

                if (!VoidToNormals.TryGetValue(voidIndex, out var set))
                {
                    set = new HashSet<ItemIndex>();
                    VoidToNormals[voidIndex] = set;
                }

                set.Add(normalIndex);
            }
        }

        /// <summary>
        /// Determines whether the item tier represents a void tier
        /// </summary>
        /// <param name="itemTier">the RoR2 item tier</param>
        /// <returns>true if this tier is in the void set or not</returns>
        public static bool IsVoidTier(ItemTier itemTier)
        {
            return VoidItemTiers.Contains(itemTier);
        }

        private static void TryToMap(ItemIndex normalIndex, CorruptedItem corrupted)
        {

            if (CorruptedItemDefs.TryGetItemDef(corrupted, out ItemDef corruptDef))
            {
                NormalToVoid[normalIndex] = corruptDef.itemIndex;
            }
            else
            {
                Log.Warning($"Could not load def for {corrupted}");
            }

        }
    }
}
