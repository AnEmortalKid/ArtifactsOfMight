using UnityEngine;

namespace ExamplePlugin.UI
{
    public static class ColorPalette
    {
        /// <summary>
        /// Useful for backgrounds n stuff
        /// </summary>
        public static Color DarkGray = new Color(0.25f, 0.25f, 0.25f, 1f);

        // TODO make this white a little less bright or everything is alpha .95 ish maybe

        public static Color OutlineWhite = FromRGB(221, 224, 232);
        public static Color OutlineGreen = FromRGB(156, 255, 156);
        public static Color OutlineRed = FromRGB(255,155,155);
        public static Color OutlineYellow = FromRGB(255, 215, 105);

        /// <summary>
        /// Used for the selection outline for the Grid Squares when drafting
        /// </summary>
        public static Color OutlinePurple = FromRGB(202,161,255);

        public static Color FromRGB(int r, int g, int b, float alpha = 1f)
        {
            return new Color(r / 255f, g / 255f, b / 255f, alpha);
        }

        public static Color WithAlpha(Color color, float newAlpha)
        {
            return new Color(color.r, color.g, color.b, newAlpha);
        }
    }
}
