using System;
using ArtifactsOfMight.UI.Drafting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using TMPro;

namespace ArtifactsOfMight.UI.Branding.Inspection
{



    /// <summary>
    /// For sprite inspection purposes, load any assets from the asset wiki 
    /// just so we can see what they are on a grid
    /// </summary>
    public static class AssetDebugGridFactory
    {

        // Addressable keys to preview
        static readonly string[] Keys =
        {
            "RoR2/Base/Common/MiscIcons/texInventoryIcon.png",
            "RoR2/Base/Common/MiscIcons/texInventoryIconOutlined.png",
            "RoR2/Base/Common/MiscIcons/texLootIconOutlined.png",
            "RoR2/Base/Common/MiscIcons/texLunarPillarIcon.png",
            "RoR2/Base/Common/MiscIcons/texMysteryIcon.png",
            "RoR2/Base/Common/MiscIcons/texRuleMapIsRandom.png",

            "RoR2/Base/UI/texDetailPanel.png",
            "RoR2/Base/UI/texDetailPanelFrame.png",
            "RoR2/Base/UI/texUICleanPanel.png",
            "RoR2/Base/UI/texUIPopupRect.png",
            "RoR2/Base/UI/texUIHighlightHeader.png",
            "RoR2/Base/UI/texUIMainRect.png",
            "RoR2/Base/UI/texUIOutlineOnly.png",

            "RoR2/Base/UI/texUICornerTier1.png",
            "RoR2/Base/UI/texUICornerTier2.png",
            "RoR2/Base/UI/texUICornerTier3.png",
            "RoR2/Base/Common/texTier1BGIcon.png",
            "RoR2/Base/Common/texTier2BGIcon.png",
            "RoR2/Base/Common/texTier3BGIcon.png",
            "RoR2/Base/Common/texBossBGIcon.png",
            "RoR2/Base/Common/texEquipmentBGIcon.png",
            "RoR2/Base/Common/texLunarBGIcon.png",
            "RoR2/DLC1/Common/IconBackgroundTextures/texVoidBGIcon.png",
            "RoR2/Base/Common/texSurvivorBGIcon.png",

            "RoR2/Base/UI/texUIAnimateHeaderGradient.png",
            "RoR2/Base/UI/texUIAnimateHeaderGradientInverted.png",
            "RoR2/Base/UI/texUIAnimateSlice1Colored.png",
            "RoR2/Base/UI/texUIAnimateSlice3.png",
            "RoR2/Base/UI/texUIAnimateSlice4.png",
            "RoR2/Junk/UI/texUIAnimateSlice2.png",
            "RoR2/Base/UI/texUIAnimateRampShine.png",
            "RoR2/Base/UI/texUIAnimateRampPulse.png",
            "RoR2/Base/UI/texUIAnimateSliceNakedButton.png",
            "RoR2/Base/Common/texUIHighlightExecute.png",
            "RoR2/Base/UI/texUIBackdrop.png",
            "RoR2/Base/UI/texUIBackdropFadedEnds.png",
            "RoR2/Base/UI/texUIBottomUpFade.tga",
            "RoR2/Base/UI/texUICleanButton.png",
            "RoR2/Base/UI/texUICleanPanel.png",
            "RoR2/Base/UI/texUICombatHealthbar.png",
            "RoR2/Base/UI/texUICorner.png",
            "RoR2/Base/UI/texUICutOffCorner.png",
            "RoR2/Base/UI/texUIDifficultySegment.png",
            "RoR2/Base/UI/texUIDifficultySegmentFade.png",
            "RoR2/Base/UI/texUIHeaderDouble.png",
            "RoR2/Base/UI/texUIHeaderSingle.png",
            "RoR2/Base/UI/texUICornerSquared.png",
            "RoR2/Base/UI/texUICornerRounded.png",
            "RoR2/Base/UI/texUILaunchButton.png",
            "RoR2/Base/UI/texUILaunchButtonDepressed.png",
            "RoR2/Base/Achievements/texLogCollectorIcon.png",

            "RoR2/Base/UI/texUIHighlightBoxOutline.png",
            "RoR2/Base/UI/texUIHighlightBoxOutlineThick.png",
            "RoR2/Base/UI/texUIHighlightBoxOutlineThickIcon.png",
            "RoR2/Base/UI/texUIBoxSliced.png",
            "RoR2/Base/UI/texUIBoxThinOutline.png",
        };
        public static GameObject BuildTestGrid(RectTransform parentTransform)
        {
            // Root overlay (darken)
            var root = new GameObject("UITextureGrid", typeof(RectTransform), typeof(Image));
            FactoryUtils.ParentToRectTransform(root, parentTransform);
            var rootRT = (RectTransform)root.transform;
            FactoryUtils.StretchToFillParent(rootRT);

            var rootBG = root.GetComponent<Image>();
            rootBG.color = new Color(0f, 0f, 0f, 0.45f); // dimmer overlay
            rootBG.raycastTarget = true; // so clicks don't pass through when open

            // Container
            var container = new GameObject("GridContainer", typeof(RectTransform), typeof(Image));
            var contRT = (RectTransform)container.transform;
            contRT.SetParent(rootRT, false);
            contRT.sizeDelta = new Vector2(1024, 640);
            contRT.anchorMin = contRT.anchorMax = new Vector2(0.5f, 0.5f);
            contRT.pivot = new Vector2(0.5f, 0.5f);
            contRT.anchoredPosition = Vector2.zero;

            var contBG = container.GetComponent<Image>();
            contBG.color = new Color(0.10f, 0.12f, 0.14f, 0.95f);
            contBG.raycastTarget = true;

            // Viewport + ScrollRect
            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(RectMask2D));
            var viewportRT = (RectTransform)viewport.transform;
            viewportRT.SetParent(contRT, false);
            Stretch(viewportRT, new Vector2(10, 10), new Vector2(10, 10));
            viewport.GetComponent<Image>().color = new Color(0, 0, 0, 0.12f);
            viewport.GetComponent<Image>().raycastTarget = true;

            var scroll = container.AddComponent<ScrollRect>();
            scroll.viewport = viewportRT;
            scroll.horizontal = true;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Elastic;
            scroll.inertia = true;

            // Content (grid)
            var content = new GameObject("Content", typeof(RectTransform), typeof(GridLayoutGroup), typeof(ContentSizeFitter));
            var contentRT = (RectTransform)content.transform;
            contentRT.SetParent(viewportRT, false);
            contentRT.anchorMin = new Vector2(0, 1);
            contentRT.anchorMax = new Vector2(0, 1);
            contentRT.pivot = new Vector2(0, 1);
            contentRT.anchoredPosition = new Vector2(8, -8);

            var grid = content.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(160, 160);
            grid.spacing = new Vector2(10, 10);
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.UpperLeft;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 5;

            var fitter = content.GetComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.content = contentRT;

            // Populate cells
            foreach (var key in Keys)
            {
                var sprite = Addressables.LoadAssetAsync<Sprite>(key).WaitForCompletion();

                var cell = new GameObject(key, typeof(RectTransform), typeof(LayoutElement), typeof(Image));
                var cellRT = (RectTransform)cell.transform;
                cellRT.SetParent(contentRT, false);
                cellRT.sizeDelta = grid.cellSize;

                // checker bg so transparent sprites are visible
                var cellBG = cell.GetComponent<Image>();
                cellBG.color = new Color(0.18f, 0.20f, 0.23f, 1f);
                cellBG.raycastTarget = false;

                // sprite image
                var imgGO = new GameObject("Sprite", typeof(RectTransform), typeof(Image));
                var imgRT = (RectTransform)imgGO.transform;
                imgRT.SetParent(cellRT, false);
                imgRT.anchorMin = imgRT.anchorMax = new Vector2(0.5f, 0.5f);
                imgRT.pivot = new Vector2(0.5f, 0.5f);
                imgRT.sizeDelta = new Vector2(grid.cellSize.x - 16, grid.cellSize.y - 32); // leave room for label
                imgRT.anchoredPosition = new Vector2(0, 8);

                var img = imgGO.GetComponent<Image>();
                img.sprite = sprite;
                img.preserveAspect = true;
                img.raycastTarget = false;

                // label
                var labelGO = new GameObject("Label", typeof(RectTransform), typeof(Text));
                var labelRT = (RectTransform)labelGO.transform;
                labelRT.SetParent(cellRT, false);
                labelRT.anchorMin = new Vector2(0, 0);
                labelRT.anchorMax = new Vector2(1, 0);
                labelRT.pivot = new Vector2(0.5f, 0);
                labelRT.sizeDelta = new Vector2(0, 18);
                labelRT.anchoredPosition = new Vector2(0, 4);

                var txt = labelGO.GetComponent<Text>();
                txt.text = TruncateKey(key, 28);
                txt.alignment = TextAnchor.MiddleCenter;
                txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                txt.fontSize = 12;
                txt.color = new Color(0.88f, 0.90f, 0.95f, 0.95f);
            }

            return root;
        }


        static readonly string[] CandidateBorders =
        {
           "RoR2/Base/UI/texUIHighlightBoxOutline.png",
           "RoR2/Base/UI/texUIHighlightBoxOutlineThick.png",
           "RoR2/Base/UI/texUIHighlightBoxOutlineThickIcon.png",
           "RoR2/Base/UI/texUIBoxSliced.png",
           "RoR2/Base/UI/texUIBoxThinOutline.png",
        };

        public static GameObject BuildTestBordersGrid(RectTransform parent)
        {
            var gridRoot = new GameObject("BorderDebugGrid", typeof(RectTransform), typeof(UnityEngine.UI.VerticalLayoutGroup), typeof(UnityEngine.UI.ContentSizeFitter));
            var rootRT = (RectTransform)gridRoot.transform;
            FactoryUtils.ParentToRectTransform(gridRoot, parent);
            var layout = gridRoot.GetComponent<UnityEngine.UI.VerticalLayoutGroup>();
            layout.childControlHeight = layout.childControlWidth = false;
            layout.spacing = 6;
            var fitter = gridRoot.GetComponent<UnityEngine.UI.ContentSizeFitter>();
            fitter.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;

            foreach (var addr in CandidateBorders)
            {
                Sprite sprite = null;
                try { sprite = Addressables.LoadAssetAsync<Sprite>(addr).WaitForCompletion(); }
                catch
                {
                    Log.Warning($"Could not load {addr}");
                }

                var square = CreateBorderTestSquare(rootRT, sprite, addr);
            }

            return gridRoot;
        }

        private static GameObject CreateBorderTestSquare(RectTransform parent, Sprite borderSprite, string label)
        {
            var root = new GameObject(label, typeof(RectTransform), typeof(UnityEngine.UI.Image));
            FactoryUtils.ParentToRectTransform(root, parent);

            var rt = root.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(128, 128);
            var bg = root.GetComponent<UnityEngine.UI.Image>();
            bg.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);

            // Border overlay
            var borderGO = new GameObject("Border", typeof(RectTransform), typeof(UnityEngine.UI.Image));
            FactoryUtils.ParentToRectTransform(borderGO, rt);
            var borderRT = (RectTransform)borderGO.transform;
            borderRT.offsetMin = Vector2.zero;
            borderRT.offsetMax = Vector2.zero;

            var borderImg = borderGO.GetComponent<UnityEngine.UI.Image>();
            borderImg.sprite = borderSprite;
            borderImg.type = borderSprite && borderSprite.border != Vector4.zero ? UnityEngine.UI.Image.Type.Sliced : UnityEngine.UI.Image.Type.Simple;
            borderImg.color = Color.white;

            // Label
            var textGO = new GameObject("Label", typeof(RectTransform), typeof(TMPro.TextMeshProUGUI));
            FactoryUtils.ParentToRectTransform(textGO, rt);
            var tmp = textGO.GetComponent<TMPro.TextMeshProUGUI>();
            tmp.text = borderSprite ? label.Split('/')[^1] : "(null)";
            tmp.fontSize = 12;
            tmp.alignment = TMPro.TextAlignmentOptions.Center;
            tmp.color = Color.white;
            var textRT = (RectTransform)textGO.transform;
            textRT.anchorMin = new Vector2(0, 0);
            textRT.anchorMax = new Vector2(1, 0);
            textRT.pivot = new Vector2(0.5f, 0);
            textRT.sizeDelta = new Vector2(0, 20);
            textRT.anchoredPosition = new Vector2(0, -10);

            return root;
        }



        public static GameObject CreateTestTabs(RectTransform parent)
        {
            // === ROOT BAR ===
            var tabsBar = new GameObject("TabsBar", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            var rt = (RectTransform)tabsBar.transform;
            FactoryUtils.ParentToRectTransform(tabsBar, parent);

            var hg = tabsBar.GetComponent<HorizontalLayoutGroup>();
            hg.spacing = 4;
            hg.childAlignment = TextAnchor.MiddleCenter;
            hg.childForceExpandHeight = false;
            hg.childForceExpandWidth = false;
            hg.padding = new RectOffset(8, 8, 8, 8);

            // full-bleed debug BG so we can see its extents
            var bg = new GameObject("TabsBarBG", typeof(RectTransform), typeof(Image));
            var bgRT = (RectTransform)bg.transform;
            bgRT.SetParent(rt, false);
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = bgRT.offsetMax = Vector2.zero;
            bg.GetComponent<Image>().color = new Color(0, 0.1f, 0.25f, 0.6f);

            // === TABS ===
            string[] names = { "Overview", "Skills", "Loadout" };
            var tabGroup = tabsBar.AddComponent<TabGroup>();

            var testTint = new Color(0.373f, 0.886f, 0.525f, 1.000f);
            foreach (string n in names)
            {
                var btnGO = CreateTabButton(n, new Vector2(128, 48),
                   testTint
                );
                var actualButton = btnGO.GetComponent<Button>();
                actualButton.onClick.AddListener(() => tabGroup.Select(btnGO));

                FactoryUtils.ParentToRectTransform(btnGO, rt);
                tabGroup.Register(btnGO);
            }

            // === SELECT MIDDLE ONE ===
            var middle = rt.GetChild(1).gameObject; // "Skills"
            tabGroup.Select(middle);

            return tabsBar;
        }


        // seems it is not bigger?
        const float SELECT_OUTSET = 2f;  
        const float HOVER_OUTSET = 6f;  // slightly bigger than body
        const float BODY_INSET = 10f;  // smaller than button rect

        private static GameObject CreateTabButton(string label, Vector2 size, Color tint)
        {
            var go = new GameObject($"Tab_{label}", typeof(RectTransform), typeof(Button), typeof(LayoutElement));
            var rt = go.GetComponent<RectTransform>();

            // Layout sizing
            var le = go.GetComponent<LayoutElement>();
            le.preferredWidth = le.minWidth = size.x;
            le.preferredHeight = le.minHeight = size.y;
            le.flexibleWidth = le.flexibleHeight = 0;

            // Background (texUICleanButton)
            var bg = go.AddComponent<Image>();
            bg.sprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/UI/texUICleanButton.png").WaitForCompletion();
            bg.type = Image.Type.Sliced;
            bg.color = tint;

            // These buttons have a base outline too that adds some depth
            var baseOutlineGO = new GameObject("BaseOutline", typeof(RectTransform), typeof(Image));
            FactoryUtils.ParentToRectTransform(baseOutlineGO, rt);
            var baseRT = (RectTransform)baseOutlineGO.transform;
            baseRT.anchorMin = Vector2.zero;
            baseRT.anchorMax = Vector2.one;
            baseRT.pivot = new Vector2(0.5f, 0.5f);
            baseRT.anchoredPosition = Vector2.zero;
            baseRT.offsetMin = Vector2.zero;
            baseRT.offsetMax = Vector2.zero;

            var baseImg = baseOutlineGO.GetComponent<Image>();
            baseImg.sprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/UI/texUIOutlineOnly.png").WaitForCompletion();
            baseImg.type = Image.Type.Sliced;
            baseImg.color = new Color(1f, 1f, 1f, 0.286f);
            baseImg.raycastTarget = false;


            // Border (texUIAnimateSliceNakedButton)
            var borderGO = new GameObject("SelectBorder", typeof(RectTransform), typeof(Image));
            FactoryUtils.ParentToRectTransform(borderGO, rt);
            var border = borderGO.GetComponent<Image>();
            // maybe its htis texUIHighlightBoxOutline and not the texUIAnimateSliceNakedButton
            border.sprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/UI/texUIHighlightBoxOutline.png").WaitForCompletion();
            border.type = Image.Type.Sliced;
            border.color = Color.white;
            border.enabled = false;
            // stretch border to the same span
            FactoryUtils.StretchToFillParent(borderGO.GetComponent<RectTransform>());
            var selectedRT = borderGO.GetComponent<RectTransform>();

            // Highlight on hover, on top of border
            //            "RoR2/Base/UI/texUIHighlightBoxOutlineThick.png",
            var highlightGO = new GameObject("HiglightBorder", typeof(RectTransform), typeof(Image));
            FactoryUtils.ParentToRectTransform(highlightGO, rt);

            var highlight = highlightGO.GetComponent<Image>();
            highlight.sprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/UI/texUIHighlightBoxOutlineThick.png").WaitForCompletion();
            highlight.type = Image.Type.Sliced;
            highlight.color = Color.white;
            highlight.enabled = false;
            // stretch 
            FactoryUtils.StretchToFillParent(highlightGO.GetComponent<RectTransform>());

            // the offset on the mirrored object is shifted a little bit and then expanded + 8
            var highlightRT = highlightGO.GetComponent<RectTransform>();
            highlightRT.anchorMin = Vector2.zero;
            highlightRT.anchorMax = Vector2.one;
            highlightRT.pivot = new Vector2(0.5f, 0.5f);

            highlightRT.anchoredPosition = new Vector2(4f, -4f);
            highlightRT.offsetMin = new Vector2(-4f, -12f);
            highlightRT.offsetMax = new Vector2(12f, 4f);

            // Selected header accent (fake the colored corners)
            var headerGO = new GameObject("SelectedHeaderAccent", typeof(RectTransform), typeof(Image));
            FactoryUtils.ParentToRectTransform(headerGO, rt);

            var headerRT = (RectTransform)headerGO.transform;
            headerRT.anchorMin = Vector2.zero;
            headerRT.anchorMax = Vector2.one;
            headerRT.pivot = new Vector2(0.5f, 0.5f);
            headerRT.anchoredPosition = Vector2.zero;

            // Match vanilla dump: a little larger than the button
            headerRT.offsetMin = new Vector2(-6f, -6f);
            headerRT.offsetMax = new Vector2(+6f, +6f);

            var headerImg = headerGO.GetComponent<Image>();
            headerImg.sprite = Addressables
                .LoadAssetAsync<Sprite>("RoR2/Base/UI/texUIHighlightHeader.png")
                .WaitForCompletion();
            headerImg.type = Image.Type.Sliced;
            headerImg.raycastTarget = false;

            // choose color: white (vanilla) or your tint for “colored corners”
            headerImg.color = Color.white; // or = tint;

            headerImg.enabled = false; // only on selected

            // Label
            var tmp = FactoryUtils.CreateTMP("Label", rt, 22, FontStyles.Bold, Color.white);
            tmp.text = label;
            tmp.alignment = TextAlignmentOptions.Center;

            // Button + selection behaviour
            var btn = go.GetComponent<Button>();
          
            var showOnSel = go.AddComponent<ShowBorderOnSelected>();
            showOnSel.border = border;
            showOnSel.background = bg;
            showOnSel.normalBg = tint;
            showOnSel.selectedBg = new Color(tint.r * 0.85f, tint.g * 0.85f, tint.b * 0.85f, tint.a);
            showOnSel.extrasOnSel = new[] { headerImg };

            var hover = go.AddComponent<HighlightOnHoverTest>();
            hover.target = highlight;

            return go;
        }

        // === helpers ===
        static void Stretch(RectTransform rt, Vector2 padMin, Vector2 padMax)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = new Vector2(padMin.x, padMin.y);
            rt.offsetMax = new Vector2(-padMax.x, -padMax.y);
        }

        static string TruncateKey(string s, int max)
            => s.Length <= max ? s : s.Substring(s.Length - max, max); // keep tail (file name)
    }
}
