using ExamplePlugin.Loadout.Draft;
using UnityEngine;

namespace ExamplePlugin.UI.Drafting.Summary
{
    public static class SummaryBarPalette
    {

        /// <summary>
        /// Color of labels on the summary bar for each tier
        /// </summary>
        
        public static readonly Color TierLabelCommon = ColorPalette.FromRGB(208, 208, 208, 0.18f);   // soft neutral gray-white
        public static readonly Color TierLabelUncommon = ColorPalette.FromRGB(72, 255, 112, 0.18f);    // brighter lime-green
        public static readonly Color TierLabelLegendary = ColorPalette.FromRGB(255, 96, 96, 0.20f);     // brighter scarlet for readability
        public static readonly Color TierLabelBoss = ColorPalette.FromRGB(255, 214, 72, 0.18f);    // slightly stronger amber
        public static readonly Color TierLabelVoid = ColorPalette.FromRGB(208, 156, 255, 0.18f);   // richer violet

        public static Color GetTierTint(DraftItemTier draftItemTier)
        {
            switch (draftItemTier)
            {
                case DraftItemTier.White: return TierLabelCommon;
                case DraftItemTier.Green: return TierLabelUncommon;
                case DraftItemTier.Red: return TierLabelLegendary;
                case DraftItemTier.Yellow: return TierLabelBoss;
                case DraftItemTier.Purple: return TierLabelVoid;
                default:
                    return new Color(0f, 1f, 1f, 1f);
            }
        }
    }
}
