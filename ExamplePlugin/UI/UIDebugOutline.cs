
using UnityEngine;
using UnityEngine.UI;

namespace ArtifactsOfMight.UI
{
    /// <summary>
    /// Utility for visualizing UI layout bounds with a colored border.
    /// Creates 4 thin Image children (top, bottom, left, right) under the given RectTransform.
    /// </summary>
    public static class UIDebugOutline
    {
        /// <summary>
        /// Adds a visual outline (non-filled) around a RectTransform.
        /// </summary>
        /// <param name="target">The RectTransform to outline.</param>
        /// <param name="color">Outline color.</param>
        /// <param name="thickness">Border thickness in pixels (default 1).</param>
        public static void AddOutline(RectTransform target, Color? color = null, float thickness = 1f)
        {
            if (!target)
            {
                Log.Warning("AddOutline on null target");
                return;
            }

            var outlineColor = color ?? Color.magenta;
            var parentGO = target.gameObject;

            void MakeEdge(string name, Vector2 anchorMin, Vector2 anchorMax)
            {
                var edge = new GameObject(name, typeof(RectTransform), typeof(Image));
                edge.layer = parentGO.layer;
                edge.transform.SetParent(target, false);

                var rt = edge.GetComponent<RectTransform>();
                rt.anchorMin = anchorMin;
                rt.anchorMax = anchorMax;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;

                var img = edge.GetComponent<Image>();
                img.color = outlineColor;
                img.raycastTarget = false;
            }

            // Top
            MakeEdge("_DebugTop", new Vector2(0f, 1f), new Vector2(1f, 1f));
            target.Find("_DebugTop").GetComponent<RectTransform>().sizeDelta = new Vector2(0, thickness);

            // Bottom
            MakeEdge("_DebugBottom", new Vector2(0f, 0f), new Vector2(1f, 0f));
            target.Find("_DebugBottom").GetComponent<RectTransform>().sizeDelta = new Vector2(0, thickness);

            // Left
            MakeEdge("_DebugLeft", new Vector2(0f, 0f), new Vector2(0f, 1f));
            target.Find("_DebugLeft").GetComponent<RectTransform>().sizeDelta = new Vector2(thickness, 0);

            // Right
            MakeEdge("_DebugRight", new Vector2(1f, 0f), new Vector2(1f, 1f));
            target.Find("_DebugRight").GetComponent<RectTransform>().sizeDelta = new Vector2(thickness, 0);
        }
    }
}
