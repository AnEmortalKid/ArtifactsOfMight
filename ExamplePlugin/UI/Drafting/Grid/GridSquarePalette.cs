using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.UI;
using UnityEngine;
using ArtifactsOfMight.Loadout.Draft;

namespace ArtifactsOfMight.UI.Drafting.Grid
{
    internal class GridSquarePalette
    {

        public static readonly Color HighlightOutlineWhiteBright = ColorPalette.FromRGB(255, 255, 255, .14f);   // pure white
        public static readonly Color HighlightOutlineGreenBright = ColorPalette.FromRGB(175, 255, 180, .14f);   // softer minty glow
        public static readonly Color HighlightOutlineRedBright = ColorPalette.FromRGB(255, 170, 165, .14f);   // warmer red-pink highlight
        public static readonly Color HighlightOutlineYellowBright = ColorPalette.FromRGB(255, 255, 160, .14f);   // bright golden-yellow glow
        public static readonly Color HighlightOutlinePurpleBright = ColorPalette.FromRGB(255, 185, 240, .14f);   // pink-lavender glow

        public static Color GetHoverOutlineColor(DraftItemTier draftItemTier)
        {
            switch (draftItemTier)
            {
                case DraftItemTier.White: return HighlightOutlineWhiteBright;
                case DraftItemTier.Green: return HighlightOutlineGreenBright;
                case DraftItemTier.Red: return HighlightOutlineRedBright;
                case DraftItemTier.Yellow: return HighlightOutlineYellowBright;
                case DraftItemTier.Purple: return HighlightOutlinePurpleBright;
                default:
                    return new Color(0f, 1f, 1f, 1f);
            }
        }
    }
}
