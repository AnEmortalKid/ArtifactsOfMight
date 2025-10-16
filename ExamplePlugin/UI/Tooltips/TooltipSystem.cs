using System;
using System.Collections.Generic;
using System.Text;
using Rewired.UI;
using UnityEngine;

namespace ExamplePlugin.UI.Tooltips
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
        private bool followCursor = true;

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
        }

        void Update()
        {
            if (pendingShow && Time.unscaledTime >= hoverStartTime + showDelay)
            {
                DoShow(pendingData, anchorPoint, lockToAnchor);
                pendingShow = false;
            }

            //if (tooltipView && followCursor && !lockToAnchor)
            //{
            //    Vector2 anchored = ReturnCursorAnchor();
            //    PositionTipClamped(anchored + cursorOffset);
            //}
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
            if (!tooltipView)
            {
                return;
            }
            if (!followCursor)
            {
                return;
            }
            if (lockToAnchor)
            {
                return;
            }

            RectTransformUtility.ScreenPointToLocalPointInRectangle(tooltipParent, screenPosition, this.uiCamera, out var local);
            Debug.Log($"Mouse {Input.mousePosition} -> Local {local}, Parent Rect rect {tooltipParent.rect}");
            PositionTipClamped(local + cursorOffset);
        }

        void DoShow(TooltipData data, RectTransform preferredAnchor, bool snap)
        {
            tooltipView.SetData(data);
            tooltipView.gameObject.SetActive(true);


            Vector2 local;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                this.tooltipParent, Input.mousePosition, this.uiCamera, out local);
            PositionTipClamped(local + cursorOffset);
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

        void PositionTipClampedSmart(Vector2 localPos)
        {
            var tipRT = tooltipView.GetComponent<RectTransform>();
            var parent = tooltipParent;

            // Force layout to get correct rect sizes
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(tipRT);

            var pRect = parent.rect;
            var tRect = tipRT.rect;
            var pPivot = parent.pivot;  // (0.5, 0.5) by default
            var tPivot = tipRT.pivot;   // (0,1) for TL tooltip
            const float pad = 4f;

            // --------------------------------------------------------
            // 1. Convert local (pivot-centered) → anchored space
            // --------------------------------------------------------
            // Unity's RectTransform rect is always centered around its own pivot.
            // To translate the coordinate system so (0,0) means the parent's top-left corner:
            Vector2 anchored;
            anchored.x = localPos.x + (pRect.width * (pPivot.x - 0f)); // add offset if parent pivot ≠ 0
            anchored.y = localPos.y - (pRect.height * (1f - pPivot.y)); // account for top/bottom pivot difference

            // --------------------------------------------------------
            // 2. Calculate clamp limits based on parent pivot and tooltip pivot
            // --------------------------------------------------------
            float left = pRect.xMin - (pRect.width * (pPivot.x - 0f)) + pad;
            float right = pRect.xMax - (pRect.width * (pPivot.x - 0f)) - tRect.width - pad;
            float top = pRect.yMax - (pRect.height * (pPivot.y - 1f)) - pad;
            float bottom = pRect.yMin - (pRect.height * (pPivot.y - 1f)) + tRect.height + pad;

            // --------------------------------------------------------
            // 3. Clamp inside parent rect (works for any parent/tooltip pivot combo)
            // --------------------------------------------------------
            anchored.x = Mathf.Clamp(anchored.x, left, right);
            anchored.y = Mathf.Clamp(anchored.y, bottom, top);

            tipRT.anchoredPosition = anchored;

            Log.Info($"[TooltipSmart] Parent pivot={pPivot}, Tooltip pivot={tPivot}, pRect={pRect}, final anchored={anchored}");
        }
    }
}
