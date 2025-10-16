using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using UnityEngine;
using ExamplePlugin.UI.Drafting;

namespace ExamplePlugin.UI.Tooltips
{
    public static class TooltipFactory
    {
        /// <summary>
        /// The name for the game object we are binding our Tooltip System to
        /// </summary>
        private const string DraftTooltipsRoot = "DraftTooltipsRoot";

        // Uses RoR2’s UI sprites when available
        const string SPRITE_BORDER = "RoR2/Base/UI/texUIAnimateSliceNakedButton.png"; // nice nine-slice outline
        const string SPRITE_BACKDROP = "RoR2/Base/UI/texUIBackdropFadedEnds.png";     // soft backdrop (optional)

        public static TooltipSystem CreateSystemRoot(RectTransform safeArea)
        {
            var draftTooltipRootGO = new GameObject(DraftTooltipsRoot, typeof(RectTransform), typeof(TooltipSystem));
            FactoryUtils.ParentToRectTransform(draftTooltipRootGO, safeArea);

            var draftTooltipRootRT = draftTooltipRootGO.GetComponent<RectTransform>();
            FactoryUtils.StretchToFillParent(draftTooltipRootRT);

            // ==== Debug Teal Background ====
            var debugBG = new GameObject(DraftTooltipsRoot + "_DebugBG", typeof(RectTransform), typeof(Image));
            var bgRt = (RectTransform)debugBG.transform;
            bgRt.SetParent(draftTooltipRootRT, worldPositionStays: false);
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.pivot = new Vector2(0.5f, 0.5f);
            bgRt.offsetMin = Vector2.zero;   // no margins
            bgRt.offsetMax = Vector2.zero;

            var img = debugBG.GetComponent<Image>();
            img.color = new Color(0.5f, 0f, 0.5f, .25f);           // some sorta purplish
            img.raycastTarget = true;        // optional click blocker


            var tooltipGORoot = (RectTransform)draftTooltipRootGO.transform;
            tooltipGORoot.anchorMin = new Vector2(0, 1);
            tooltipGORoot.anchorMax = new Vector2(0, 1);
            tooltipGORoot.pivot = new Vector2(0, 1);
            tooltipGORoot.sizeDelta = Vector2.zero;
            tooltipGORoot.anchoredPosition = Vector2.zero;
            draftTooltipRootGO.layer = safeArea.gameObject.layer;

            var system = draftTooltipRootGO.GetComponent<TooltipSystem>();

            var tooltipView = CreateTooltipView(tooltipGORoot);
            system.Initialize(safeArea, tooltipView);

            tooltipView.transform.SetAsLastSibling();

            return system;
        }

        public static TooltipView CreateTooltipView(RectTransform parent)
        {
            // vertical 48 + 64 / possibly more
            // horizontal 400

            // Root
            var tipGO = new GameObject("TooltipView", typeof(RectTransform), typeof(LayoutElement), typeof(ContentSizeFitter));
            FactoryUtils.ParentToRectTransform(tipGO, parent);

            var tipFitter = tipGO.GetComponent<ContentSizeFitter>();
            tipFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            tipFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var tipRT = tipGO.GetComponent<RectTransform>();
            tipRT.anchorMin = tipRT.anchorMax = new Vector2(0, 1);
            tipRT.pivot = new Vector2(0, 1);
            tipRT.sizeDelta = new Vector2(400, 120);

            var tipLE = tipGO.GetComponent<LayoutElement>();
            tipLE.minWidth = 220;
            tipLE.minHeight = 80;
            tipLE.preferredWidth = 400;
            tipLE.preferredHeight = 200;

            var bgGO = new GameObject("Background", typeof(RectTransform), typeof(Image));
            FactoryUtils.ParentToRectTransform(bgGO, tipRT);

            var bgRT = (RectTransform)bgGO.transform;
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.pivot = new Vector2(0.5f, 0.5f);
            bgRT.offsetMin = new Vector2(0, 0);
            bgRT.offsetMax = new Vector2(0, 0);

            // TODO good background color

            // White square worked, so now we can keep iterating
            var bgImg = bgGO.GetComponent<Image>();
            bgImg.color = ColorPalette.DarkGray; // dark panel
            bgImg.raycastTarget = false;

            var stackGO = new GameObject("Stack", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            var stackRT = (RectTransform)stackGO.transform;
            stackRT.SetParent(tipRT, false);
            stackRT.anchorMin = Vector2.zero; stackRT.anchorMax = Vector2.one;
            // inset for border later
            stackRT.offsetMin = new Vector2(1, 1);  
            stackRT.offsetMax = new Vector2(-1, -1);

            var stackVLG = stackGO.GetComponent<VerticalLayoutGroup>();
            // inner padding for the body (below header we’ll override)
            stackVLG.padding = new RectOffset(10, 10, 10, 10); 
            stackVLG.spacing = 6;
            stackVLG.childAlignment = TextAnchor.UpperLeft;
            stackVLG.childControlWidth = true; stackVLG.childControlHeight = true;
            stackVLG.childForceExpandWidth = true; stackVLG.childForceExpandHeight = false;

            var fitter = stackGO.GetComponent<ContentSizeFitter>();
            tipFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            tipFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Our elements
            var headerGO = new GameObject("Header", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            FactoryUtils.ParentToRectTransform(headerGO, stackRT);

            var headerRT = (RectTransform)headerGO.transform;
            var headerLE = headerGO.GetComponent<LayoutElement>();
            headerLE.minHeight = 28f;       
            headerLE.flexibleWidth = 1f;
            headerLE.preferredHeight = 48f;

            var headerImg = headerGO.GetComponent<Image>();
            headerImg.sprite = null;
            headerImg.type = Image.Type.Simple;
            headerImg.color = new Color(0.53f, 0.65f, 0.31f, 1f);


            var titleTMP = CreateTMP("Title", headerRT, 24, TMPro.FontStyles.Bold, Color.white);
            var titleRT = (RectTransform)titleTMP.transform;
            titleRT.anchorMin = Vector2.zero; titleRT.anchorMax = Vector2.one;
            titleRT.offsetMin = new Vector2(8, 4); titleRT.offsetMax = new Vector2(-8, -4);

            var bodyGO = new GameObject("Body", typeof(RectTransform), typeof(VerticalLayoutGroup));
            FactoryUtils.ParentToRectTransform(bodyGO, stackRT);

            var bodyLE = bodyGO.GetComponent<LayoutElement>() ?? bodyGO.AddComponent<LayoutElement>();
            bodyLE.flexibleWidth = 1f;
            // avoids collapsing
            bodyLE.minHeight = 64f;

            var bodyRT = (RectTransform)bodyGO.transform;
            var bodyVLG = bodyGO.GetComponent<VerticalLayoutGroup>();
            bodyVLG.padding = new RectOffset(0, 0, 4, 0);   // small gap below header
            bodyVLG.spacing = 4;
            bodyVLG.childControlWidth = true; 
            bodyVLG.childControlHeight = true;
            bodyVLG.childForceExpandWidth = false; 
            bodyVLG.childForceExpandHeight = false;

            // word wrappin not workin?
            var descTMP = CreateTMP("Description", bodyRT, 18, FontStyles.Normal, Color.white);
            descTMP.enableWordWrapping = true;
            descTMP.overflowMode = TextOverflowModes.Overflow;

            // Hook up the view component
            var view = tipGO.AddComponent<TooltipView>();
            view.Bind(headerImg, titleTMP, descTMP);

            return view;
        }

        static TMP_Text CreateTMP(string name, RectTransform parent, int size, FontStyles style, Color fontColor)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            FactoryUtils.ParentToRectTransform(go, parent);
            var rt = (RectTransform)go.transform;

            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = "";
            tmp.fontSize = size;
            tmp.fontStyle = style;
            tmp.raycastTarget = false;
            tmp.enableWordWrapping = false;
            tmp.margin = Vector4.zero;
            tmp.alignment = TextAlignmentOptions.TopLeft;
            return tmp;
        }
    }
}
