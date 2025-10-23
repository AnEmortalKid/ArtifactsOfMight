using System;
using System.Collections.Generic;
using System.Text;
using ExamplePlugin.Loadout.Draft;

namespace ExamplePlugin.UI.Drafting.Tabs
{
    /// <summary>
    /// Colors for the Buttons that switch the active DraftTab
    /// </summary>
    public static class DraftTabButtonPalette
    {
        public static readonly DraftTabColors White = new()
        {
            NormalTint = new(0.75f, 0.75f, 0.75f),
            HoverTint = new(0.85f, 0.85f, 0.85f),
            SelectedTint = new(0.60f, 0.60f, 0.60f),
        };

        public static readonly DraftTabColors Green = new()
        {
            NormalTint = new(0.36f, 0.72f, 0.38f),
            HoverTint = new(0.42f, 0.84f, 0.44f),
            SelectedTint = new(0.22f, 0.46f, 0.24f)
        };

        public static readonly DraftTabColors Red = new()
        {
            NormalTint = new(0.77f, 0.23f, 0.24f),
            HoverTint = new(0.88f, 0.28f, 0.30f),
            SelectedTint = new(0.47f, 0.15f, 0.16f)
        };

        public static readonly DraftTabColors Yellow = new()
        {
            NormalTint = new(0.89f, 0.80f, 0.31f),
            HoverTint = new(1.00f, 0.90f, 0.38f),
            SelectedTint = new(0.55f, 0.50f, 0.19f)
        };

        public static readonly DraftTabColors Purple = new()
        {
            NormalTint = new(0.73f, 0.40f, 0.70f),
            HoverTint = new(0.83f, 0.46f, 0.80f),
            SelectedTint = new(0.44f, 0.25f, 0.43f)
        };

        private static readonly DraftTabColors ObviousDefault = new()
        {
            NormalTint = new(0f, 1f, 1f, 1f),
            HoverTint = new(0f, 1f, 1f, 1f),
            SelectedTint = new(0f, 1f, 1f, 1f),
        };

        public static DraftTabColors GetColorsForTab(DraftItemTier draftItemTier
            )
        {
            switch (draftItemTier)
            {
                case DraftItemTier.White: return White;
                case DraftItemTier.Green: return Green;
                case DraftItemTier.Red: return Red;
                case DraftItemTier.Yellow: return Yellow;
                case DraftItemTier.Purple: return Purple;
                default:
                    return ObviousDefault;
            }
        }
    }
}
