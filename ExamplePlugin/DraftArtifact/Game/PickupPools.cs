
using System.Collections.Generic;
using ArtifactsOfMight.Loadout.Corruption;
using RoR2;

namespace ArtifactsOfMight.DraftArtifact.Game
{
    /// <summary>
    /// Stores mappings of items that may not be draftable,
    /// but should be allowed to be picked up.
    /// Since they are not draftable, these do not get added to the allowed state
    /// </summary>
    public static class PickupPools
    {

        private static HashSet<ItemIndex> allowedUndraftable = BuildAllowedUndraftable();


        /// <summary>
        /// Determines whether the given item is not draftable, but should be allowed
        /// to be grabbed during a run.
        /// </summary>
        /// <param name="itemIndex">the index of the item to check</param>
        /// <returns></returns>
        public static bool IsUndraftablePickup(ItemIndex itemIndex)
        {
            return allowedUndraftable.Contains(itemIndex);
        }

        private static HashSet<ItemIndex> BuildAllowedUndraftable()
        {
            var allowedSet = new HashSet<ItemIndex>();

            // yellows
            allowedSet.Add(RoR2Content.Items.ArtifactKey.itemIndex);
            allowedSet.Add(RoR2Content.Items.ShinyPearl.itemIndex);
            allowedSet.Add(RoR2Content.Items.TitanGoldDuringTP.itemIndex);
            allowedSet.Add(RoR2Content.Items.Pearl.itemIndex);


            // cerebellums
            allowedSet.Add(DLC3Content.Items.MasterCore.itemIndex); 
            allowedSet.Add(DLC3Content.Items.MasterBattery.itemIndex);
            // prison matrix
            allowedSet.Add(DLC3Content.Items.PowerCube.itemIndex);
            // sentry key
            allowedSet.Add(DLC3Content.Items.PowerPyramid.itemIndex);

            // Voids
            if (CorruptedItemDefs.TryGetItemDef(CorruptedItem.NewlyHatchedZoea, out var zoeaDef))
            {
                allowedSet.Add(zoeaDef.itemIndex);
            }

            // alloyed special oranges
            allowedSet.Add(DLC3Content.Items.BonusHealthBoost.itemIndex);
            allowedSet.Add(DLC3Content.Items.CookedSteak.itemIndex);
            allowedSet.Add(DLC3Content.Items.MoneyLoan.itemIndex);
            allowedSet.Add(DLC3Content.Items.Stew.itemIndex);
            allowedSet.Add(DLC3Content.Items.UltimateMeal.itemIndex);
            allowedSet.Add(DLC3Content.Items.WyrmOnHit.itemIndex);            

            return allowedSet;
        }
    }
}
