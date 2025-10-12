
using System.Collections.Generic;
using ExamplePlugin.Loadout.Corruption;
using RoR2;

namespace ExamplePlugin.DraftArtifact.Game
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

            // Voids
            if (CorruptedItemDefs.TryGetItemDef(CorruptedItem.NewlyHatchedZoea, out var zoeaDef))
            {
                allowedSet.Add(zoeaDef.itemIndex);
            }

            return allowedSet;
        }
    }
}
