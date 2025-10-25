using ExamplePlugin.UI.Branding.Panel;
using ExamplePlugin.UI.Tooltips;
using ExamplePlugin.UI.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace ExamplePlugin.UI.Drafting
{

    /// <summary>
    /// Responsible for constructing the DraftManager Hierarchy
    /// </summary>
    public static class DraftManagerFactory
    {
        /// <summary>
        /// The name for the game object we are binding our DraftManager to
        /// </summary>
        private const string DraftManagerRootName = "DraftManagerRoot";

        #region DebugLayouts
        private static bool SHOW_ROOT_DEBUG = false;
        private static bool DEBUG_RECT_SIZES = false;
        // some sorta blueish
        private static Color DEBUG_ROOT_COLOR = new Color(0f, 0.5f, 0.5f, .25f);
        #endregion

        /// <summary>
        /// Used inkscape to center the panel on a reference screenshot that is 1920x1080
        /// And this is the point of reference for the DraftManager panel
        /// </summary>
        private static Vector2 PANEL_CENTERED_INKSCAPE = new Vector2(1122, 480);

        /// <summary>
        /// Creates the DraftManager and its required hierarchy,
        /// the returned GameObject will have a DraftManager component attached to it
        /// </summary>
        /// <param name="safeArea">The parent of the DraftManager hierarchy, this should be the SafeArea element from the UI</param>
        public static GameObject Create(RectTransform safeArea)
        {
            var draftManagerRoot = new GameObject(DraftManagerRootName, typeof(RectTransform));
            FactoryUtils.ParentToRectTransform(draftManagerRoot, safeArea);

            var draftManagerRootRT = (RectTransform)draftManagerRoot.transform;
            FactoryUtils.StretchToFillParent(draftManagerRootRT);

            // TODO block raycasts here

            if (SHOW_ROOT_DEBUG)
            {
                var debugBG = new GameObject(DraftManagerRootName + "_DebugBG", typeof(RectTransform), typeof(Image));
                FactoryUtils.ParentToRectTransform(debugBG, draftManagerRootRT);

                // center and fill full 
                var bgRt = (RectTransform)debugBG.transform;
                FactoryUtils.StretchToFillParent(bgRt);

                var img = debugBG.GetComponent<Image>();
                img.color = DEBUG_ROOT_COLOR;
                // don't block clicks behind our rect
                img.raycastTarget = false;

                if (DEBUG_RECT_SIZES)
                {
                    HierarchyDumper.LogRect(DraftManagerRootName + "_BG", bgRt);
                }
            }

            var vectorSize = new Vector2(720, 720);
            var glassPanel = GlassPanelFactory.BuildGlassBody("DraftManagerDialog", draftManagerRootRT, vectorSize,
                borderOptions: new BorderOptions
                {
                    Style = BorderStyle.Panel,
                    Color = new Color(0.363f, 0.376f, 0.472f, 1f),
                    RespectSpritePadding = true,
                }
            );

            var centered = new Vector2(0.5f, 0.5f);
            var glassPanelRoot = glassPanel.glassPanelHolder.GetComponent<RectTransform>();
            glassPanelRoot.anchorMin = glassPanelRoot.anchorMax = centered;
            glassPanelRoot.pivot = centered;
            glassPanelRoot.anchoredPosition = PlacementUtils.InkscapeCenterToAnchoredCenter(PANEL_CENTERED_INKSCAPE,
                                                                safeArea.rect.size);
            glassPanel.Reflow(true, new Vector4(12, 12, 12, 12));

            var glassContentRoot = glassPanel.contentRoot;

            var draftManager = draftManagerRoot.AddComponent<DraftManager>();
            draftManager.Initialize(glassContentRoot);

            if (DEBUG_RECT_SIZES)
            {
                HierarchyDumper.LogRect("SafeArea", safeArea);
                HierarchyDumper.LogRect("Root", draftManagerRootRT);
                HierarchyDumper.LogRect("GlassContent", glassContentRoot);
            }

            // attach the tooltip system so it is owned by the draft manager hierarchy
            var toolTipComponent = BuildToolTipSystem(draftManagerRootRT);

            return draftManagerRoot;
        }

        private static GameObject BuildToolTipSystem(RectTransform parentRect)
        {
            var tooltipRoot = new GameObject("TooltipTest", typeof(RectTransform), typeof(TooltipSystem));
            FactoryUtils.ParentToRectTransform(tooltipRoot, parentRect);

            var tooltipRootRT = (RectTransform)tooltipRoot.transform;
            FactoryUtils.StretchToFillParent(tooltipRootRT);

            if (SHOW_ROOT_DEBUG)
            {
                var debugBG = new GameObject("ToolTipTest" + "_DebugBG", typeof(RectTransform), typeof(Image));
                var bgRt = (RectTransform)debugBG.transform;
                bgRt.SetParent(tooltipRootRT, worldPositionStays: false);
                bgRt.anchorMin = Vector2.zero;
                bgRt.anchorMax = Vector2.one;
                bgRt.pivot = new Vector2(0.5f, 0.5f);
                bgRt.offsetMin = Vector2.zero;   // no margins
                bgRt.offsetMax = Vector2.zero;

                var img = debugBG.GetComponent<Image>();
                img.color = new Color(0.5f, 0f, 0.5f, 0.25f);
                img.raycastTarget = false;

                if (DEBUG_RECT_SIZES)
                {
                    HierarchyDumper.LogRect("ToolTip.BG", bgRt);
                }
            }

            if (DEBUG_RECT_SIZES)
            {
                HierarchyDumper.LogRect("ToolTip.ParentRect", parentRect);
                HierarchyDumper.LogRect("ToolTip.Root", tooltipRootRT);
            }

            var system = tooltipRoot.GetComponent<TooltipSystem>();
            var tooltipView = TooltipFactory.CreateTooltipMirror(tooltipRootRT);
            system.Initialize(tooltipRootRT, tooltipView);
            tooltipView.transform.SetAsLastSibling();

            return tooltipRoot;
        }
    }
}
