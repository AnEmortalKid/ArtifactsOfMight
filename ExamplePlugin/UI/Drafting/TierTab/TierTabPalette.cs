using System;
using System.Collections.Generic;
using System.Text;
using ExamplePlugin.Loadout.Draft;
using UnityEngine;

namespace ExamplePlugin.UI.Drafting.TierTab
{
    public static class TierTabPalette
    {

        public static readonly ShuffleTabColors DiceColorsWhite = new()
        {
            BaseColor = ColorPalette.FromRGB(235, 235, 235, 0.22f),
            HighlightColor = ColorPalette.FromRGB(255, 255, 255, 0.32f),
            PressedColor = ColorPalette.FromRGB(245, 245, 245, 0.26f),
        };

        public static readonly ShuffleTabColors DiceColorsGreen = new()
        {
            BaseColor = ColorPalette.FromRGB(72, 255, 112, 0.20f),
            HighlightColor = ColorPalette.FromRGB(92, 255, 140, 0.28f),
            PressedColor = ColorPalette.FromRGB(72, 255, 112, 0.22f),
        };

        public static readonly ShuffleTabColors DiceColorsRed = new()
        {
            BaseColor = ColorPalette.FromRGB(255, 92, 92, 0.22f),
            HighlightColor = ColorPalette.FromRGB(255, 120, 110, 0.30f),
            PressedColor = ColorPalette.FromRGB(255, 92, 92, 0.24f),
        };

        public static readonly ShuffleTabColors DiceColorsYellow = new()
        {
            BaseColor = ColorPalette.FromRGB(255, 222, 82, 0.20f),   // ← yellow (fix)
            HighlightColor = ColorPalette.FromRGB(255, 235, 120, 0.28f),
            PressedColor = ColorPalette.FromRGB(255, 222, 82, 0.22f),
        };

        public static readonly ShuffleTabColors DiceColorsPurple = new()
        {
            BaseColor = ColorPalette.FromRGB(216, 156, 255, 0.22f),
            HighlightColor = ColorPalette.FromRGB(232, 176, 255, 0.30f),
            PressedColor = ColorPalette.FromRGB(216, 156, 255, 0.24f),
        };

        public static readonly ShuffleTabColors DiceColorsDefault = new ShuffleTabColors()
        {
            BaseColor = ColorPalette.FromRGB(1, 0, 0),
            HighlightColor = ColorPalette.FromRGB(0, 1, 0),
            PressedColor = ColorPalette.FromRGB(0,0,1),
        };

        public static ShuffleTabColors GetDiceColors(DraftItemTier itemTier)
        {
            switch (itemTier)
            {
                case DraftItemTier.White: return DiceColorsWhite;
                case DraftItemTier.Green: return DiceColorsGreen;
                case DraftItemTier.Red: return DiceColorsRed;
                case DraftItemTier.Yellow: return DiceColorsYellow;
                case DraftItemTier.Purple: return DiceColorsPurple;
                default:
                    return DiceColorsDefault;

            }
        }
    }
}
