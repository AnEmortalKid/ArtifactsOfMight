using System;
using System.Collections.Generic;
using System.Text;
using RoR2;

namespace ExamplePlugin.Loadout.Draft
{
    /// <summary>
    /// Mapping utilities between the UI tier and the game models tiers
    /// </summary>
    public static class DraftTierMaps
    {
        /// <summary>
        /// DraftItemTier.White to the ItemTier's that match it
        /// </summary>
        public static readonly ItemTier[] WhiteSet = { ItemTier.Tier1 };
        /// <summary>
        /// DraftItemTier.Green to the ItemTier's that match it
        /// </summary>
        public static readonly ItemTier[] GreenSet = { ItemTier.Tier2 };
        /// <summary>
        /// DraftItemTier.Red to the ItemTier's that match it
        /// </summary>
        public static readonly ItemTier[] RedSet = { ItemTier.Tier3 };
        /// <summary>
        /// DraftItemTier.Yellow to the ItemTier's that match it
        /// </summary>
        public static readonly ItemTier[] YellowSet = { ItemTier.Boss };
        /// <summary>
        /// DraftItemTier.Purple to the ItemTier's that match it
        /// </summary>
        public static readonly ItemTier[] PurpleSet = { ItemTier.VoidTier1, ItemTier.VoidTier2, ItemTier.VoidTier3 };

        /// <summary>
        /// Gets the corresponding DraftItemTier based on ItemTier
        /// </summary>
        /// <param name="itemTier">the RoR2 model tier</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException">if a tier has no mapping</exception>
        public static DraftItemTier ToDraft(ItemTier itemTier)
        {
            switch (itemTier)
            {
                case ItemTier.Tier1:
                    return DraftItemTier.White;
                case ItemTier.Tier2:
                    return DraftItemTier.Green;
                case ItemTier.Tier3:
                    return DraftItemTier.Red;
                case ItemTier.Boss:
                    return DraftItemTier.Yellow;
                case ItemTier.VoidTier1:
                case ItemTier.VoidTier2:
                case ItemTier.VoidTier3:
                    return DraftItemTier.Purple;

            }

            throw new NotImplementedException("No mapping for tier: " + itemTier);
        }

        /// <summary>
        /// Checks whether the given item belongs to a draftable tier or not
        /// </summary>
        /// <param name="itemTier">the item tier to map to a draft</param>
        /// <returns>true if this tier has a draft counterpart, false otherwise</returns>

        public static bool HasDraftTier(ItemTier itemTier)
        {
            switch (itemTier)
            {
                case ItemTier.Tier1:
                case ItemTier.Tier2:
                case ItemTier.Tier3:
                case ItemTier.Boss:
                case ItemTier.VoidTier1:
                case ItemTier.VoidTier2:
                case ItemTier.VoidTier3:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Gets the corresponding ItemTiers based on our DraftItemTier
        /// </summary>
        /// <param name="draftTier">the draft tier to fetch RoR2 ItemTiers for</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static ItemTier[] ToNativeSet(DraftItemTier draftTier)
        {
            switch (draftTier)
            {
                case DraftItemTier.White:
                    return WhiteSet;
                case DraftItemTier.Green:
                    return GreenSet;
                case DraftItemTier.Red:
                    return RedSet;
                case DraftItemTier.Yellow:
                    return YellowSet;
                case DraftItemTier.Purple:
                    return PurpleSet;

            }

            throw new NotImplementedException("No mapping for draft tier: " + draftTier);
        }
    }
}
