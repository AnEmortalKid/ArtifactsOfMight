using System;
using UnityEngine;
using UnityEngine.UI;

namespace ArtifactsOfMight.UI.Tooltips
{
    public class TooltipSystem : MonoBehaviour
    {
        public static TooltipSystem Instance { get; private set; }


        private TooltipView tooltipView;
        /// <summary>
        /// The SafeArea rect transform
        /// </summary>
        private RectTransform tooltipParent;


        private float showDelay = 0.15f;
        private Vector2 cursorOffset = new(16, -16);

        /// <summary>
        /// space between anchor and tooltip
        /// </summary>
        private const float anchorGap = 8f;

        private const float bleedPad = 8f;

        /// <summary>
        /// How much to offset by in the tile's scale
        /// </summary>
        private const float tileOffsetFactor = 1.0f;

        float hoverStartTime;
        bool pendingShow;
        TooltipData pendingData;

        /// <summary>
        /// The preferred point to anchor too when we are in lock to anchor mode
        /// 
        /// This would usually be the element we need to be fixed to
        /// </summary>
        private RectTransform anchorPoint;

        /// <summary>
        /// Whether to update the position every time to follow the mouse or not
        /// 
        /// When locked, an anchor is picked and we don't allow further movement
        /// </summary>
        private bool lockToAnchor;

        private Camera uiCamera;

        public void Initialize(RectTransform tooltipParent, TooltipView tooltipView)
        {
            Instance = this;

            this.tooltipParent = tooltipParent;
            this.tooltipView = tooltipView;

            // need to store the camera since we are going to use ScreenPointToLocal
            var canvas = tooltipParent.GetComponentInParent<Canvas>();
            this.uiCamera = canvas && canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? canvas.worldCamera
                : null;

            // start with tooltip off
            tooltipView.gameObject.SetActive(false);
        }

        /// <summary>
        /// Queues a tooltip to be displayed after the configured hover delay.
        /// </summary>
        /// <param name="data">
        /// The <see cref="TooltipData"/> to display.  
        /// Contains the title, description, and optional icon for the tooltip.
        /// </param>
        /// <param name="preferredAnchor">
        /// (Optional) A <see cref="RectTransform"/> representing the UI element or screen-space region
        /// the tooltip should appear near.  
        /// If <paramref name="snapToAnchor"/> is <c>true</c>, the tooltip will be positioned
        /// relative to this anchor instead of following the cursor.
        /// </param>
        /// <param name="snapToAnchor">
        /// If <c>true</c>, the tooltip will lock to the provided <paramref name="preferredAnchor"/> and remain fixed.  
        /// If <c>false</c>, the tooltip will follow the cursor after it becomes visible.
        /// </param>
        /// <remarks>
        /// This method does not immediately show the tooltip.  
        /// Instead, it sets up internal state so that during <see cref="Update"/> the system
        /// can display the tooltip once the hover delay has elapsed.  
        /// This prevents instant pop-ins when moving the cursor quickly across multiple elements.
        /// </remarks>
        public void QueueShow(TooltipData data, RectTransform preferredAnchor = null, bool snapToAnchor = false)
        {
            Log.Info($"Queued: {data}");
            pendingData = data;
            anchorPoint = preferredAnchor;
            lockToAnchor = snapToAnchor;
            hoverStartTime = Time.unscaledTime;
            pendingShow = true;
        }

        public void CancelShow()
        {
            pendingShow = false;
            if (tooltipView)
            {
                tooltipView.gameObject.SetActive(false);
            }
        }

        public void Move(Vector2 screenPosition)
        {
            if (!tooltipView || !tooltipView.gameObject.activeSelf)
            {
                return;
            }

            var tipRT = (RectTransform)tooltipView.transform;
            // anchorPoint is the tile RectTransform you passed in QueueShow (can be null)
            PlaceOffTileFollowingMouse(tipRT, anchorPoint, screenPosition);
        }

        void DoShow(TooltipData data, RectTransform preferredAnchor)
        {
            tooltipView.SetData(data);
            tooltipView.gameObject.SetActive(true);

            // Place immediately
            var tipRT = (RectTransform)tooltipView.transform;
            PlaceOffTileFollowingMouse(tipRT, preferredAnchor, Input.mousePosition);
        }

        void Update()
        {
            if (pendingShow && Time.unscaledTime >= hoverStartTime + showDelay)
            {
                DoShow(pendingData, anchorPoint);
                pendingShow = false;
            }

            if (tooltipView && tooltipView.gameObject.activeSelf)
            {
                var tipRT = (RectTransform)tooltipView.transform;
                PlaceOffTileFollowingMouse(tipRT, anchorPoint, Input.mousePosition);
            }
        }

        /// <summary>
        /// Position the tooltip near the desired localposition, but clamp it within the tool tips parent
        /// </summary>
        /// <param name="localPos">the desired position in local space,use ReturnCursorAnchor</param>
        void PositionTipClamped(Vector2 localPos)
        {
            var tipRT = this.tooltipView.GetComponent<RectTransform>();

            // Tooltip pivot: top-left (0,1)
            tipRT.anchorMin = tipRT.anchorMax = new Vector2(0f, 1f);
            tipRT.pivot = new Vector2(0f, 1f);

            // Clamp inside parent space (in local/anchored space)
            var pRect = tooltipParent.rect;
            var size = tipRT.rect.size;

            Log.Info($"pRect {pRect}");
            const float pad = 4f;

            // For TL anchors: (0,0) is top-left; +x right, +y UP? No: Unity TL anchored y is negative going down.
            Vector2 anchored;
            anchored.x = localPos.x - pRect.xMin;  // shift from center to left edge
            anchored.y = localPos.y - pRect.yMax;  // shift from center to top edge (becomes negative below)

            // Clamp IN ANCHORED (TL) SPACE:
            float minAX = pad;
            float maxAX = pRect.width - size.x - pad;

            // top edge
            float maxAY = -pad;
            // bottom edge
            float minAY = -(pRect.height - size.y - pad);

            anchored.x = Mathf.Clamp(anchored.x, minAX, maxAX);
            anchored.y = Mathf.Clamp(anchored.y, minAY, maxAY);

            tipRT.anchoredPosition = anchored;
        }

        void PlaceOffTileFollowingMouse(RectTransform tipRT, RectTransform tileRT, Vector2 mouseScreen)
        {
            ForceLayout(tipRT);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(tooltipParent, mouseScreen, uiCamera, out var mouseLocal);

            // edge-aware vertical pivot so it grows inward
            var p = tooltipParent.rect;
            float tY = Mathf.InverseLerp(p.yMin, p.yMax, mouseLocal.y);
            float pivotY = (tY > 0.66f) ? 1f : (tY < 0.33f ? 0f : 0.5f);

            Vector2 pivot, posLocal;

            if (tileRT)
            {
                var tile = LocalRectInParent(tileRT, tooltipParent);
                var tipSize = tipRT.rect.size;
                var side = ChooseSideLeftBias(tile, p, tipSize);

                if (side == Side.Left)
                {
                    pivot = new Vector2(1f, pivotY);
                    posLocal = new Vector2(tile.xMin - anchorGap - tile.width * tileOffsetFactor, mouseLocal.y);
                }
                else // Right fallback
                {
                    pivot = new Vector2(0f, pivotY);
                    posLocal = new Vector2(tile.xMax + anchorGap + tile.width * tileOffsetFactor, mouseLocal.y);
                }
            }
            else
            {
                pivot = new Vector2(0f, 1f);
                posLocal = mouseLocal + cursorOffset;
            }

            tipRT.anchorMin = tipRT.anchorMax = new Vector2(0f, 1f);
            tipRT.pivot = pivot;

            var anchored = LocalToAnchored(posLocal, tipRT);
            anchored = ClampAnchoredSoft(anchored, tipRT, pivot);
            tipRT.anchoredPosition = anchored;
        }

        /// <summary>
        /// Determines which side of the tile (Left, Right, Top, Bottom)
        /// the mouse cursor is on, relative to the tile’s local rect.
        /// </summary>
        private enum Side { Left, Right, Top, Bottom }

        Side SideFromMouse(Rect tileLocal, Vector2 mouseLocal)
        {
            // Find tile center
            float cx = (tileLocal.xMin + tileLocal.xMax) * 0.5f;
            float cy = (tileLocal.yMin + tileLocal.yMax) * 0.5f;

            // Offset of mouse from center
            float dx = mouseLocal.x - cx;
            float dy = mouseLocal.y - cy;

            // Whichever axis has greater magnitude determines the side
            if (Mathf.Abs(dx) > Mathf.Abs(dy))
                return dx >= 0 ? Side.Right : Side.Left;
            else
                return dy >= 0 ? Side.Top : Side.Bottom;
        }

        // --- Call this once right after SetData and before reading size ---
        static void ForceLayout(RectTransform rt)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
        }

        // Rect of a child in the local space of 'parent' (SafeArea)
        static Rect LocalRectInParent(RectTransform child, RectTransform parent)
        {
            Vector3[] w = new Vector3[4];
            child.GetWorldCorners(w);
            for (int i = 0; i < 4; i++) w[i] = parent.InverseTransformPoint(w[i]);
            float xMin = w[0].x, yMin = w[0].y, xMax = w[2].x, yMax = w[2].y;
            return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
        }

        // Does a tooltip of 'size' placed at 'pos' with 'pivot' fit in 'bounds' with padding?
        static bool FitsIn(Rect bounds, Vector2 pos, Vector2 size, Vector2 pivot, float pad)
        {
            var min = pos - Vector2.Scale(size, pivot);
            var max = min + size;
            min.x -= -bounds.xMin + pad; max.x -= bounds.xMax - pad;
            min.y -= -bounds.yMin + pad; max.y -= bounds.yMax - pad;
            return (min.x >= 0 && max.x <= 0 && min.y >= 0 && max.y <= 0);
        }

       
        /// <summary>
        /// Converts a point in the tooltip parent’s local space (center-origin)
        /// into anchored-space coordinates (0,1) top-left anchor convention).
        /// </summary>
        Vector2 LocalToAnchored(Vector2 local, RectTransform tipRT)
        {
            var p = tooltipParent.rect;
            // (0,0) anchoredPosition is top-left; +x = right, +y = down
            Vector2 anchored;
            anchored.x = local.x - p.xMin;
            anchored.y = local.y - p.yMax;   // flip because anchoredPosition.y is inverted
            return anchored;
        }

        // Always try LEFT; if it can't fit, use RIGHT. (No top/bottom.)
        Side ChooseSideLeftBias(Rect tile, Rect parent, Vector2 tipSize)
        {
            bool leftOK = (tile.xMin - parent.xMin) >= (anchorGap + tipSize.x + bleedPad + tile.width * tileOffsetFactor);
            bool rightOK = (parent.xMax - tile.xMax) >= (anchorGap + tipSize.x + bleedPad + tile.width * tileOffsetFactor);
            if (leftOK) return Side.Left;
            if (rightOK) return Side.Right;
            return Side.Left; // fallback; we'll clamp softly
        }

        /// <summary>
        /// Keeps the tooltip inside the SafeArea, but allows a small soft bleed margin
        /// so it feels less rigid than a hard clamp.
        /// </summary>
        Vector2 ClampAnchoredSoft(Vector2 anchored, RectTransform tipRT, Vector2 pivot)
        {
            var p = tooltipParent.rect;
            Vector2 size = tipRT.rect.size;

            // Calculate the tooltip's bounds in anchored space (top-left anchor system)
            Vector2 min = anchored - Vector2.Scale(size, pivot);
            Vector2 max = min + size;

            const float bleed = 12f; // how far outside screen edges we allow before clamping

            float minAX = -bleed;
            float maxAX = p.width + bleed;
            float maxAY = -bleed;                  // top edge (negative down)
            float minAY = -(p.height) - bleed;     // bottom edge

            if (min.x < minAX) anchored.x += minAX - min.x;
            if (max.x > maxAX) anchored.x -= max.x - maxAX;
            if (max.y > maxAY) anchored.y -= max.y - maxAY;
            if (min.y < minAY) anchored.y += minAY - min.y;

            return anchored;
        }
    }
}
