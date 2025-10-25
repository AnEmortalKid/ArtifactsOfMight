using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using UnityEngine;
using ArtifactsOfMight.UI.Drafting;
using ArtifactsOfMight.UI.Branding.Panel;
using ArtifactsOfMight.UI.Branding.Inspection;
using System.Collections;

namespace ArtifactsOfMight.UI.Tooltips
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

        public static TooltipView CreateGlassTooltipView(RectTransform parent)
        {
            var desiredToolTip = new Vector2(420, 200);
            var noPadding = Vector4.zeroVector;
            var glass = GlassPanelFactory.BuildGlassBody(
                "TooltipGlass", parent, size: desiredToolTip, true,
                borderOptions: new BorderOptions
                {
                    Style = BorderStyle.None,
                    Color = new Color(.380f, 0.475f, 0.541f, 1f),
                    RespectSpritePadding = true
                });

            var glassPanelBackgroundImage = glass.primaryImage;
            glassPanelBackgroundImage.color = ColorPalette.TooltipGlassPaneBackgroundColor;

            // then add the outline as a sibling to the parent holder and stretch out 2x2
            var outlineGO = new GameObject("Outline", typeof(RectTransform), typeof(Image));
            var outlineRT = (RectTransform)outlineGO.transform;
            outlineRT.SetParent(glass.primaryImage.transform.parent, false);

            // Stretch to match the panel, but extend out by 2 px (like RoR2)
            outlineRT.anchorMin = Vector2.zero;
            outlineRT.anchorMax = Vector2.one;
            outlineRT.offsetMin = new Vector2(-2, -2);
            outlineRT.offsetMax = new Vector2(2, 2);
            outlineRT.pivot = new Vector2(0.5f, 0.5f);

            // Configure the outline
            var outlineImg = outlineGO.GetComponent<Image>();
            outlineImg.sprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/UI/texUIOutlineOnly.png").WaitForCompletion();
            outlineImg.type = UnityEngine.UI.Image.Type.Sliced;
            outlineImg.fillCenter = true;
            outlineImg.raycastTarget = false;
            outlineImg.color = new Color(0.445f, 0.567f, 0.642f, 1f); // #7291A4FF (tooltip rim)

            var tipLE = glass.glassPanelHolder.AddComponent<LayoutElement>();
            tipLE.minWidth = 220;
            tipLE.minHeight = 80;
            tipLE.preferredWidth = desiredToolTip.x;
            // allow grow
            tipLE.flexibleHeight = 1f;
            //tipLE.preferredHeight = desiredToolTip.y;

            var tipFit = glass.glassPanelHolder.AddComponent<ContentSizeFitter>();
            tipFit.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            tipFit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var contentRoot = glass.contentRoot;

            var stackGO = new GameObject("Stack", typeof(RectTransform), typeof(VerticalLayoutGroup));
            var stackRT = (RectTransform)stackGO.transform;
            stackRT.SetParent(contentRoot, false);
            stackRT.anchorMin = Vector2.zero; stackRT.anchorMax = Vector2.one;
            // child components will set their own margins
            stackRT.offsetMin = Vector2.zero;
            stackRT.offsetMax = Vector2.zero;

            var stackVLG = stackGO.GetComponent<VerticalLayoutGroup>();
            // elements will have their own padding
            stackVLG.padding = new RectOffset(0, 0, 0, 0);
            stackVLG.spacing = 0;
            stackVLG.childControlWidth = true;
            stackVLG.childControlHeight = true;
            stackVLG.childForceExpandWidth = true;
            stackVLG.childForceExpandHeight = false;

            // Header stretches the full width with a vertial centered text
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
            // this gets swapped out almost instantly
            // but we should start as not visible
            headerImg.color = new Color(0.53f, 0.65f, 0.31f, 1f);

            var titleTMP = CreateTMP("Title", headerRT, 24, TMPro.FontStyles.Bold, Color.white);
            titleTMP.alignment = TextAlignmentOptions.MidlineLeft; // vertical center, left-aligned

            var titleRT = (RectTransform)titleTMP.transform;
            titleRT.anchorMin = Vector2.zero;
            titleRT.anchorMax = Vector2.one;
            titleRT.offsetMin = new Vector2(8, 4);
            titleRT.offsetMax = new Vector2(-8, -4);

            var bodyGO = new GameObject("Body", typeof(RectTransform), typeof(VerticalLayoutGroup));
            FactoryUtils.ParentToRectTransform(bodyGO, stackRT);

            var bodyLE = bodyGO.GetComponent<LayoutElement>() ?? bodyGO.AddComponent<LayoutElement>();
            // make TMP wrap
            bodyLE.preferredWidth = 400f;
            // allow to expand vertically
            bodyLE.flexibleHeight = 1f;
            // let it collapse if short text
            bodyLE.minHeight = 0f;

            var bodyRT = (RectTransform)bodyGO.transform;
            var bodyVLG = bodyGO.GetComponent<VerticalLayoutGroup>();
            bodyVLG.padding = new RectOffset(12, 12, 12, 12);
            bodyVLG.spacing = 4;
            bodyVLG.childControlWidth = true;
            bodyVLG.childControlHeight = true;
            bodyVLG.childForceExpandWidth = true;
            // dont vertical stretch
            bodyVLG.childForceExpandHeight = false;

            // word wrappin not workin?
            var descTMP = CreateTMP("Description", bodyRT, 18, FontStyles.Normal, Color.white);
            descTMP.enableWordWrapping = true;
            descTMP.overflowMode = TextOverflowModes.Overflow;

            // Hook up the view component
            var tipGO = glass.glassPanelHolder;
            var view = tipGO.AddComponent<TooltipView>();
            view.Bind(headerImg, titleTMP, descTMP);

            return view;
        }

        static readonly Color OutlineTint = new Color(97 / 255f, 121 / 255f, 138 / 255f, 1f); // #61798AFF

        public static TooltipView CreateTooltipMirror(RectTransform parent)
        {
            // ─────────────────────────────────────────────────────────────────────────────
            // Tooltip (root container)  -> TooltipOffset (cursor-based anchor)
            //   └─ Panel (pivot 1,0) [VerticalLayoutGroup + ContentSizeFitter]
            //        ├─ Background (Image, ignoreLayout, stretched)
            //        ├─ TitleSection (VLG + LE preferredWidth=400)
            //        │    └─ TitleLabel (TMP)
            //        ├─ BodySection (VLG + LE preferredWidth=400)
            //        │    └─ BodyLabel (TMP)
            //        └─ Outline (Image texUIOutlineOnly, ±2 px)
            // ─────────────────────────────────────────────────────────────────────────────

            // Root
            var rootGO = new GameObject("Tooltip(Clone)", typeof(RectTransform));
            var rootRT = (RectTransform)rootGO.transform;
            rootRT.SetParent(parent, false);
            rootRT.anchorMin = rootRT.anchorMax = Vector2.zero; // screen-space canvas usually handles world pos; we’ll position via TooltipOffset
            rootRT.sizeDelta = Vector2.zero;
            rootRT.pivot = new Vector2(0.5f, 0.5f);

            // TooltipOffset (where you position near the cursor)
            var offsetGO = new GameObject("TooltipOffset", typeof(RectTransform));
            var offsetRT = (RectTransform)offsetGO.transform;
            offsetRT.SetParent(rootRT, false);
            offsetRT.anchorMin = offsetRT.anchorMax = new Vector2(0.5f, 0.5f);
            offsetRT.pivot = new Vector2(0.5f, 0.5f);
            offsetRT.anchoredPosition = Vector2.zero;

            // Panel (this object OWNS the size; it stacks Title/Body and grows)
            var panelGO = new GameObject("Panel",
                typeof(RectTransform),
                typeof(VerticalLayoutGroup),
                typeof(ContentSizeFitter));
            var panelRT = (RectTransform)panelGO.transform;
            panelRT.SetParent(offsetRT, false);
            panelRT.anchorMin = panelRT.anchorMax = new Vector2(1f, 0f); // grow up/right from bottom-right
            panelRT.pivot = new Vector2(1f, 0f);
            panelRT.anchoredPosition = Vector2.zero;

            var panelVLG = panelGO.GetComponent<VerticalLayoutGroup>();
            panelVLG.padding = new RectOffset(0, 0, 0, 0);
            panelVLG.spacing = 0;
            panelVLG.childControlWidth = true;
            panelVLG.childControlHeight = true;
            panelVLG.childForceExpandWidth = true;
            // let children decide height dont auto force
            panelVLG.childForceExpandHeight = false;

            var panelFit = panelGO.GetComponent<ContentSizeFitter>();
            panelFit.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            panelFit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Background (glass) — stretch inside Panel, but do NOT affect layout
            var bgGO = new GameObject("Background", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            var bgRT = (RectTransform)bgGO.transform;
            bgRT.SetParent(panelRT, false);
            bgRT.anchorMin = Vector2.zero; bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = Vector2.zero; bgRT.offsetMax = Vector2.zero;
            var bgLE = bgGO.GetComponent<LayoutElement>();
            // critical
            bgLE.ignoreLayout = true; 
            var bgImg = bgGO.GetComponent<Image>();
            // your glass effect (pure color + alpha), blocks clicks:
            bgImg.sprite = null;
            bgImg.raycastTarget = false;
            bgImg.color = ColorPalette.TooltipGlassPaneBackgroundColor;

            // Header section = background + padding + width constraints
            var headerGO = new GameObject("TitleRect",
                typeof(RectTransform),
                typeof(Image),
                typeof(VerticalLayoutGroup),
                typeof(LayoutElement));

            var headerRT = (RectTransform)headerGO.transform;
            headerRT.SetParent(panelRT, false);                 // direct child of Panel (the VLG+Fitter owner)

            var headerImg = headerGO.GetComponent<Image>();
            headerImg.sprite = null;
            headerImg.type = Image.Type.Simple;
            headerImg.raycastTarget = false;
            // each tier will pass a tint
            headerImg.color = new Color(0.53f, 0.65f, 0.31f, 1f); 

            var headerLE = headerGO.GetComponent<LayoutElement>();
            headerLE.minHeight = 28f;
            headerLE.preferredHeight = 48f;                      // fixed strip height (match RoR2)
            headerLE.preferredWidth = 400f;                     // key: fixes wrap width for the section
            headerLE.flexibleHeight = 0f;                       // header is a strip

            var headerVLG = headerGO.GetComponent<VerticalLayoutGroup>();
            headerVLG.padding = new RectOffset(12, 12, 12, 12); // 12 px inset all around
            headerVLG.spacing = 0;
            headerVLG.childControlWidth = true;
            headerVLG.childControlHeight = true;
            headerVLG.childForceExpandWidth = true;
            headerVLG.childForceExpandHeight = false;

            // TMP inside the section, stretched to padding box
            var titleTMP = CreateTMP("TitleLabel", headerRT, 24, TMPro.FontStyles.Bold, Color.white);
            // vertical center, left aligned
            titleTMP.alignment = TextAlignmentOptions.MidlineLeft;
            // (not super needed for header)
            titleTMP.enableWordWrapping = true;                             

            // Body section
            var bodyGO = new GameObject("BodyRect", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            var bodyRT = (RectTransform)bodyGO.transform;
            bodyRT.SetParent(panelRT, false);
            var bodyLE = bodyGO.GetComponent<LayoutElement>();
            bodyLE.minHeight = 32f;
            bodyLE.preferredWidth = 400f;  // key: fixes wrap width
            bodyLE.flexibleHeight = 1f;    // allow vertical growth
            var bodyVLG = bodyGO.GetComponent<VerticalLayoutGroup>();
            bodyVLG.padding = new RectOffset(12, 12, 12, 12);
            bodyVLG.spacing = 4;
            bodyVLG.childControlWidth = bodyVLG.childControlHeight = true;
            bodyVLG.childForceExpandWidth = true;
            bodyVLG.childForceExpandHeight = false;

            var bodyTMP = CreateTMP("BodyLabel", bodyRT, 18, FontStyles.Normal, Color.white);
            bodyTMP.enableWordWrapping = true;
            bodyTMP.overflowMode = TextOverflowModes.Overflow;

            // Outline (sibling under Panel; ±2px outside)
            var outlineGO = new GameObject("Outline", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            var outlineRT = (RectTransform)outlineGO.transform;
            outlineRT.SetParent(panelRT, false);
            outlineRT.anchorMin = Vector2.zero; outlineRT.anchorMax = Vector2.one;
            outlineRT.offsetMin = new Vector2(-2, -2);
            outlineRT.offsetMax = new Vector2(+2, +2);
            outlineRT.pivot = new Vector2(0.5f, 0.5f);
            // dont be part of calculations
            outlineGO.GetComponent<LayoutElement>().ignoreLayout = true;

            var outlineImg = outlineGO.GetComponent<Image>();
            outlineImg.sprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/UI/texUIOutlineOnly.png").WaitForCompletion();
            outlineImg.type = Image.Type.Sliced;
            outlineImg.fillCenter = true;              
            outlineImg.raycastTarget = false;
            outlineImg.color = OutlineTint;

            // Bind and return a view
            var view = rootGO.AddComponent<TooltipView>();
            view.Bind(headerImg, titleTMP, bodyTMP);
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
