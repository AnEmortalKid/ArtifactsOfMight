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

        /// <summary>
        /// Used for the selection outline for the Grid Squares when drafting
        /// </summary>
        //public static Color OutlineWhite = FromRGB(221, 224, 232);
        //public static Color OutlineGreen = FromRGB(156, 255, 156);
        //public static Color OutlineRed = FromRGB(255, 155, 155);
        //public static Color OutlineYellow = FromRGB(255, 215, 105);
        //public static Color OutlinePurple = FromRGB(202, 161, 255);

        // New hue based ones
        public static readonly Color OutlineWhite = FromRGB(225, 225, 225);
        public static readonly Color OutlineGreen = FromRGB(142, 222, 145);
        public static readonly Color OutlineRed = FromRGB(230, 125, 120);
        public static readonly Color OutlineYellow = FromRGB(245, 230, 110);
        public static readonly Color OutlinePurple = FromRGB(220, 140, 195);

        /// <summary>
        /// Took these colors from the tooltips
        /// Possibly this header white needs to be darker i think savy
        /// </summary>
        /// 

        ///
        /// <summary>
        /// This one is the darker grey header for the tooltip for artifacts since
        /// the one for items was too bright and hard to read, that one was 193,193,193
        /// maybe there's a mid ground in between
        /// </summary>
        ///
        public static Color HeaderWhite = FromRGB(99, 99, 99);
        public static Color HeaderGreen = FromRGB(88, 149, 89);
        public static Color HeaderRed = FromRGB(142, 51, 50);
        public static Color HeaderYellow = FromRGB(189, 180, 59);
        public static Color HeaderPurple = FromRGB(164, 77, 132);


        public static readonly Color TabHeaderWhite = new(0.75f, 0.75f, 0.75f);
        public static readonly Color TabHeaderGreen = new(0.36f, 0.72f, 0.38f);
        public static readonly Color TabHeaderRed = new(0.77f, 0.23f, 0.24f);
        public static readonly Color TabHeaderYellow = new(0.89f, 0.80f, 0.31f);
        public static readonly Color TabHeaderPurple = new(0.73f, 0.40f, 0.70f);

        /// <summary>
        /// Color that can be used as a sort of frosted pane effect, matches the
        /// lightness of the character select panels (the artifact modal) selection.
        /// 
        /// You can use the ColorTunerUI to tweak values of custom panels/images to try to mirror 
        /// the colors of other risk of rain components
        /// </summary>
        public static Color GlassPaneBackgroundColor = FromRGB_AlphaPercent(24, 20, 25, 60);

        /// <summary>
        /// Color that can be used as a sort of frosted pane effect on tooltips,
        /// selected to be darker just for readability
        /// 
        /// You can use the ColorTunerUI to tweak values of custom panels/images to try to mirror 
        /// the colors of other risk of rain components
        /// </summary>
        public static Color TooltipGlassPaneBackgroundColor = FromRGB_AlphaPercent(37, 39, 42, 76);

        // Tooltip blue green 78, 85, 113

        public static Color FromRGB(int r, int g, int b, float alpha = 1f)
        {
            return new Color(r / 255f, g / 255f, b / 255f, alpha);
        }

        /// <summary>
        /// Creates a color based on the given RGB (255) and Alpha (100)
        /// </summary>
        /// <param name="r">red tone in 0-255</param>
        /// <param name="g">green tone in 0-255</param>
        /// <param name="b">blue tone in 0-255</param>
        /// <param name="a">alpha percentage 0-100</param>
        /// <returns></returns>
        public static Color FromRGB_AlphaPercent(int r, int g, int b, int a = 100)
        {
            return new Color(r / 255f, g / 255f, b / 255f, a / 100f);
        }

        /// <summary>
        /// Creates a new color with the same RGB but different alpha
        /// </summary>
        /// <param name="color">reference color to keep RGB from</param>
        /// <param name="newAlpha">range 0 to 100</param>
        /// <returns></returns>
        public static Color WithAlpha(Color color, float newAlpha)
        {
            return new Color(color.r, color.g, color.b, newAlpha / 100f);
        }
    }
}
