using System.Collections.Generic;
using RoR2;
using UnityEngine.AddressableAssets;

namespace ExamplePlugin.Loadout.Corruption
{
    public static class CorruptedItemDefs
    {
        private static Dictionary<CorruptedItem, string> itemDefAssettPathsByItem = new Dictionary<CorruptedItem, string>()
        {
            { CorruptedItem.BenthicBloom, "RoR2/DLC1/CloverVoid/CloverVoid.asset" },
            { CorruptedItem.EncrustedKey, "RoR2/DLC1/TreasureCacheVoid/TreasureCacheVoid.asset" },
            { CorruptedItem.LostSeersLenses, "RoR2/DLC1/CritGlassesVoid/CritGlassesVoid.asset"  },
            { CorruptedItem.LysateCell, "RoR2/DLC1/EquipmentMagazineVoid/EquipmentMagazineVoid.asset" },
            { CorruptedItem.Needletick, "RoR2/DLC1/BleedOnHitVoid/BleedOnHitVoid.asset" },
            { CorruptedItem.NewlyHatchedZoea, "RoR2/DLC1/VoidMegaCrabItem.asset" },
            { CorruptedItem.PlasmaShrimp, "RoR2/DLC1/MissileVoid/MissileVoid.asset" },
            { CorruptedItem.PluripotentLarva, "RoR2/DLC1/ExtraLifeVoid/ExtraLifeVoid.asset" },
            { CorruptedItem.Polylute,  "RoR2/DLC1/ChainLightningVoid/ChainLightningVoid.asset"},
            { CorruptedItem.SaferSpaces, "RoR2/DLC1/BearVoid/BearVoid.asset" },
            { CorruptedItem.SingularityBand, "RoR2/DLC1/ElementalRingVoid/ElementalRingVoid.asset" },
            { CorruptedItem.Tentabauble, "RoR2/DLC1/SlowOnHitVoid/SlowOnHitVoid.asset"  },
            { CorruptedItem.VoidsentFlame, "RoR2/DLC1/ExplodeOnDeathVoid/ExplodeOnDeathVoid.asset" },
            { CorruptedItem.WeepingFungus, "RoR2/DLC1/MushroomVoid/MushroomVoid.asset" },
        };

        /// <summary>
        /// Intentionally null so we can lazy load it once
        /// </summary>
        private static Dictionary<CorruptedItem, ItemDef> defByItem;


        /// <summary>
        /// Attempts to get the ItemDef for a corrupted item.
        /// 
        /// IDK how this is gonna work when someone doesn't have the expansion
        /// </summary>
        /// <param name="corruptedItem"></param>
        /// <param name="hadDef"></param>
        /// <returns></returns>
        public static bool TryGetItemDef(CorruptedItem corruptedItem, out ItemDef itemDef)
        {
            if (defByItem == null)
            {
                LoadDefMaps();
            }

            return defByItem.TryGetValue(corruptedItem, out itemDef);
        }


        private static void LoadDefMaps()
        {
            defByItem = new();

            foreach(var pathDef in itemDefAssettPathsByItem)
            {
                var corruptKey = pathDef.Key;
                var assetPath = pathDef.Value;
                Log.Info($"CorruptedItemDefs loading {assetPath}");
                ItemDef itemDef = Addressables.LoadAssetAsync<ItemDef>(assetPath).WaitForCompletion();
                // Idk how this acts if someone doesn't have the expansion tbh
                if(itemDef != null )
                {
                    defByItem[corruptKey] = itemDef;
                }
            }
        }
    }
}
