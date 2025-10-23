using UnityEngine;

namespace ExamplePlugin.UI.Utils
{
    public static class PlacementUtils
    {

        public static Vector2 InkscapeToAnchoredTopLeft(Vector2 inkscapePos, Vector2 parentSize, Vector2 inkscapeCanvas = default)
        {
            // default to 1920x1080 if not provided
            if (inkscapeCanvas == default) inkscapeCanvas = new Vector2(1920, 1080);
            var sx = parentSize.x / inkscapeCanvas.x;
            var sy = parentSize.y / inkscapeCanvas.y;
            return new Vector2(inkscapePos.x * sx, -inkscapePos.y * sy);
        }

        /// <summary>
        /// Determines the center based anchor position based on a set of coordinates
        /// found when positioning things in Inkscape (a 1920x1080) document, scaled
        /// correctly to the size of the SafeArea(parentSize), so things are in the right spot
        /// </summary>
        /// <param name="inkscapeCenter"></param>
        /// <param name="parentSize"></param>
        /// <param name="inkscapeCanvas"></param>
        /// <returns>the computed location to set as an anchor to properly center a component in the given area</returns>
        public static Vector2 InkscapeCenterToAnchoredCenter(
            Vector2 inkscapeCenter, Vector2 parentSize, Vector2 inkscapeCanvas = default)
        {
            if (inkscapeCanvas == default) inkscapeCanvas = new Vector2(1920, 1080);
            var sx = parentSize.x / inkscapeCanvas.x;
            var sy = parentSize.y / inkscapeCanvas.y;
            var cx = inkscapeCenter.x * sx;
            var cy = inkscapeCenter.y * sy;
            return new Vector2(cx - parentSize.x * 0.5f, -cy + parentSize.y * 0.5f);
        }
    }
}
