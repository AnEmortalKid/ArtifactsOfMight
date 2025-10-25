
using ArtifactsOfMight.Loadout.Draft;
using ArtifactsOfMight.UI.Drafting;
using ArtifactsOfMight.UI.Drafting.TierTab;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using static UnityEngine.UIElements.StylePropertyAnimationSystem;

namespace ArtifactsOfMight.UI
{
    public static class ItemPickerTabControlsFactory
    {

        #region DebugOptions
        private static bool SHOW_DEBUG_COLORS = false;
        #endregion

        /// <summary>
        /// Creates the hierarchy for our tab controls:
        /// 
        /// ItemPickerTabControls
        ///   RestrictionGroup
        ///   SelectionGroup
        ///   Spacer
        ///   ButtonGroup
        ///   
        /// </summary>
        /// <returns></returns>
        public static DraftTabControls CreateTabControls(string componentName, RectTransform parentRectTransform, DraftItemTier itemTier)
        {
            var controlsGO = new GameObject(componentName, typeof(RectTransform), typeof(DraftTabControls));
            var controlsBarRT = controlsGO.GetComponent<RectTransform>();
            controlsBarRT.SetParent(parentRectTransform, worldPositionStays: false);
            controlsBarRT.anchorMin = new Vector2(0f, 1f);
            controlsBarRT.anchorMax = new Vector2(1f, 1f);
            controlsBarRT.pivot = new Vector2(0.5f, 1f);
            controlsBarRT.offsetMin = new Vector2(0f, -32f);
            controlsBarRT.offsetMax = new Vector2(0f, 0f);

            if (SHOW_DEBUG_COLORS)
            {
                var debugImg = controlsGO.AddComponent<Image>();
                debugImg.raycastTarget = false;
                // Green ish
                var controlsOutlineColor = ColorPalette.FromRGB(51, 99, 66, 1f);
                debugImg.color = controlsOutlineColor;
            }

            var controlsHG = controlsGO.AddComponent<HorizontalLayoutGroup>();
            controlsHG.spacing = 12;
            controlsHG.childControlHeight = true;
            controlsHG.childForceExpandHeight = false;
            // let the spacer take the rest
            controlsHG.childControlWidth = true;
            // no stretching
            controlsHG.childForceExpandWidth = false;
            controlsHG.childAlignment = TextAnchor.MiddleLeft;
            // push more left right
            controlsHG.padding = new RectOffset(12, 12, 8, 8);

            var tabControls = controlsGO.GetComponent<DraftTabControls>();

            var restrictionsGroup = CreateRestrictionGroup(componentName + "_RestrictionGroup", controlsBarRT, tabControls);
            var selectionGroup = CreateSelectionGroup(componentName + "_SelectionGroup", controlsBarRT, tabControls);
            var spacer2 = CreateSpacer(componentName + "_Spacer", controlsBarRT);
            var miscOptsionGroup = CreateGridOptionsGroup(componentName + "_Options", controlsBarRT, tabControls, itemTier);

            return tabControls;
        }

        static RectTransform MakeRowGroup(string name, RectTransform parent)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter), typeof(LayoutElement));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);

            var hg = go.GetComponent<HorizontalLayoutGroup>();
            hg.spacing = 8;
            hg.childAlignment = TextAnchor.MiddleLeft;
            hg.childControlWidth = true;
            hg.childControlHeight = true;
            hg.childForceExpandWidth = false;
            hg.childForceExpandHeight = false;

            // 🔑 make the group take the width of its children
            var fit = go.GetComponent<ContentSizeFitter>();
            fit.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fit.verticalFit = ContentSizeFitter.FitMode.MinSize;

            // keep rows aligned with buttons
            var rowLayout = go.GetComponent<LayoutElement>();
            // do not compete with spacer
            rowLayout.flexibleWidth = 0;
            rowLayout.minHeight = 28f;

            return rt;
        }


        private static GameObject CreateRestrictionGroup(string groupName, RectTransform controlsBarRT, DraftTabControls pickerTabControls)
        {
            var restrictionsGroupRT = MakeRowGroup(groupName, controlsBarRT);
            var restrictionsGroup = restrictionsGroupRT.gameObject;

            // add our components
            var controlsGroupRT = restrictionsGroupRT.GetComponent<RectTransform>();
            var limitLabel = CreateLabel(groupName + "_LimitLabel", restrictionsGroupRT, "Limit: ", 24);
            var itemCountControls = CreateItemCountControls(groupName + "_Counts", restrictionsGroupRT, pickerTabControls);

            return restrictionsGroup;
        }

        private static GameObject CreateSelectionGroup(string groupName, RectTransform controlsBarRT, DraftTabControls pickerTabControls)
        {
            var selectionGroupRT = MakeRowGroup(groupName, controlsBarRT);
            var selectionGroup = selectionGroupRT.gameObject;

            var limitLabel = CreateLabel(groupName + "_SelectLabel", selectionGroupRT, "Selected: 0/0", 24);
            var limitLabelText = limitLabel.GetComponent<TextMeshProUGUI>();

            pickerTabControls.BindItemSelectionElements(limitLabelText);

            return selectionGroup;
        }

        private static GameObject CreateLabel(string objectName, RectTransform parentRT, string labelText, int fontSize = 22)
        {
            var labeLGO = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
            FactoryUtils.ParentToRectTransform(labeLGO, parentRT);

            var labelRT = labeLGO.GetComponent<RectTransform>();
            labelRT.anchorMin = new Vector2(0, 0);
            labelRT.anchorMax = new Vector2(1, 0);
            labelRT.sizeDelta = new Vector2(0, 22);

            var tmpText = labeLGO.GetComponent<TextMeshProUGUI>();
            tmpText.text = labelText;
            tmpText.alignment = TextAlignmentOptions.MidlineLeft;
            tmpText.enableAutoSizing = false;
            tmpText.fontSize = fontSize;
            tmpText.enableWordWrapping = false;
            tmpText.overflowMode = TextOverflowModes.Overflow;

            var layoutElement = labeLGO.GetComponent<LayoutElement>();
            layoutElement.minHeight = 28;

            return labeLGO;
        }

        /// <summary>
        /// Creates the UI controls for managing the item limit when restricted
        /// [ Button Text Button]
        /// </summary>
        private static GameObject CreateItemCountControls(string groupName, RectTransform controlGroupRT, DraftTabControls pickerTabControls)
        {
            // parent object for everything
            var countsGroupGO = new GameObject(groupName, typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            FactoryUtils.ParentToRectTransform(countsGroupGO, controlGroupRT);

            var restrictionsRT = countsGroupGO.GetComponent<RectTransform>();

            if (SHOW_DEBUG_COLORS)
            {
                // orange ish ish
                var groupColor = ColorPalette.FromRGB(209, 63, 63);
                var debugBackground = countsGroupGO.AddComponent<Image>();
                debugBackground.color = groupColor;
                debugBackground.raycastTarget = true;
            }

            var hlg = countsGroupGO.GetComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.spacing = 6;
            hlg.padding = new RectOffset(4, 4, 4, 4);
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            // keep sizing stable
            var groupLE = countsGroupGO.GetComponent<LayoutElement>();
            groupLE.minWidth = 140;

            var buttonDimensions = new Vector2(28, 28);

            // - button
            var leftArrowSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texOptionsArrowLeft.png").WaitForCompletion();
            var decreaseGO = CreateImageButton(groupName + "_Minus", buttonDimensions, leftArrowSprite);
            FactoryUtils.ParentToRectTransform(decreaseGO, restrictionsRT);
            var decreaseRT = decreaseGO.GetComponent<RectTransform>();

            // text display
            var restrictCountLabel = new GameObject(groupName + "_Label", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
            FactoryUtils.ParentToRectTransform(restrictCountLabel, restrictionsRT);

            // HLG will control its size
            var labelLE = restrictCountLabel.GetComponent<LayoutElement>();
            labelLE.minWidth = 44f;   // width for "7", "10", "30" etc
            labelLE.minHeight = 28f;   // aligns vertically with buttons

            var restrictText = restrictCountLabel.GetComponent<TextMeshProUGUI>();
            restrictText.text = "N/A";
            restrictText.alignment = TextAlignmentOptions.Center;
            restrictText.fontSize = 22;

            // + button
            var rightArrowSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texOptionsArrowRight.png").WaitForCompletion();
            var increaseGO = CreateImageButton(groupName + "_Plus", buttonDimensions, rightArrowSprite);
            FactoryUtils.ParentToRectTransform(increaseGO, restrictionsRT);

            pickerTabControls.BindItemLimitElements(decreaseGO.GetComponent<Button>(),
                restrictText, increaseGO.GetComponent<Button>());

            return countsGroupGO;
        }


        // 
        private static GameObject CreateGridOptionsGroup(string groupName, RectTransform parentRT, DraftTabControls pickerTabControls, DraftItemTier itemTier)
        {
            var gridOptionsGroupRT = MakeRowGroup(groupName, parentRT);
            var gridOptionsGroup = gridOptionsGroupRT.gameObject;

            if (SHOW_DEBUG_COLORS)
            {
                // pinkish
                var groupColor = ColorPalette.FromRGB(189, 58, 134);
                var debugBackground = gridOptionsGroup.AddComponent<Image>();
                debugBackground.color = groupColor;
                debugBackground.raycastTarget = false;
            }

            //var diceIcon = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texRandomizeIcon.png").WaitForCompletion();
            var colorChoices = TierTabPalette.GetDiceColors(itemTier);
            var diceGO = CreateDiceButton(gridOptionsGroupRT, colorChoices, 36f);
            FactoryUtils.ParentToRectTransform(diceGO, gridOptionsGroupRT);

            pickerTabControls.BindDiceButton(diceGO.GetComponent<Button>());
            return gridOptionsGroup;
        }

        public static GameObject CreateDiceButton(RectTransform parent, ShuffleTabColors colorChoices, float size = 40f)
        {
            // Root (clickable background)
            var go = new GameObject("DiceButton",
                typeof(RectTransform), typeof(Button), typeof(Image), typeof(LayoutElement));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.sizeDelta = new Vector2(size, size);

            var le = go.GetComponent<LayoutElement>();
            le.preferredWidth = le.minWidth = size;
            le.preferredHeight = le.minHeight = size;

            // Background (clean sliced)
            var bg = go.GetComponent<Image>();
            bg.sprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/UI/texUICleanButton.png").WaitForCompletion();
            bg.type = Image.Type.Sliced;
            bg.color = new Color(0.18f, 0.18f, 0.20f, 1f); // subtle bright
            bg.raycastTarget = true;

            // CONTENT: a single centered dice image sized to fill with padding
            const float pad = 5f; // tweak to taste
            var diceGO = new GameObject("Dice", typeof(RectTransform), typeof(Image));
            var diceRT = (RectTransform)diceGO.transform;
            diceRT.SetParent(rt, false);
            diceRT.anchorMin = new Vector2(0, 0);
            diceRT.anchorMax = new Vector2(1, 1);
            diceRT.offsetMin = new Vector2(pad, pad);
            diceRT.offsetMax = new Vector2(-pad, -pad);
            diceRT.pivot = new Vector2(0.5f, 0.5f);

            var diceImg = diceGO.GetComponent<Image>();
            diceImg.sprite = Addressables.LoadAssetAsync<Sprite>(
                "RoR2/Base/Common/MiscIcons/texRandomizeIcon.png").WaitForCompletion();
            diceImg.type = Image.Type.Simple;
            diceImg.preserveAspect = true;
            diceImg.raycastTarget = false;

            // Outline overlay (separate image, stretched; ignores layout)
            var outlineGO = new GameObject("BaseOutline", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            var outlineRT = (RectTransform)outlineGO.transform;
            outlineRT.SetParent(rt, false);
            outlineRT.anchorMin = Vector2.zero; outlineRT.anchorMax = Vector2.one;
            outlineRT.offsetMin = Vector2.zero; outlineRT.offsetMax = Vector2.zero;
            outlineGO.GetComponent<LayoutElement>().ignoreLayout = true;

            var outlineImg = outlineGO.GetComponent<Image>();
            outlineImg.sprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/UI/texUIOutlineOnly.png").WaitForCompletion();
            outlineImg.type = Image.Type.Sliced;
            outlineImg.color = new Color(1f, 1f, 1f, 0.286f);
            outlineImg.raycastTarget = false;

            outlineGO.transform.SetAsLastSibling(); // keep outline above dice

            // Button transition (optional: mild hover)
            var btn = go.GetComponent<Button>();
            btn.targetGraphic = bg;
            btn.transition = Selectable.Transition.ColorTint;

            //var colors = btn.colors;
            //colors.normalColor = colorChoices.BaseColor;
            //colors.highlightedColor = colorChoices.HighlightColor;
            //colors.selectedColor = colorChoices.HighlightColor;
            //colors.pressedColor = colorChoices.HighlightColor * .9f;
            //btn.colors = colors;

            // add our highlighter
            var highlighter = go.AddComponent<ShuffleTabDiceHighlighter>();
            highlighter.Initialize(diceImg, colorChoices.BaseColor, colorChoices.HighlightColor, colorChoices.PressedColor);

            return go;
        }

        private static GameObject CreateImageButton(string componentName,
            Vector2 buttonDimensions,
            Sprite buttonSprite
            )
        {
            var buttonParentGO = new GameObject(componentName, typeof(RectTransform), typeof(Button), typeof(Image), typeof(LayoutElement));
            var buttonParentRT = buttonParentGO.GetComponent<RectTransform>();

            var buttonLE = buttonParentGO.GetComponent<LayoutElement>();
            buttonLE.preferredWidth = buttonDimensions.x;
            buttonLE.preferredHeight = buttonDimensions.y;
            buttonLE.minWidth = buttonDimensions.x;
            buttonLE.minHeight = buttonDimensions.y;
            buttonLE.flexibleWidth = 0;
            buttonLE.flexibleHeight = 0;

            var buttonImage = buttonParentGO.GetComponent<Image>();
            buttonImage.sprite = buttonSprite;
            buttonImage.type = Image.Type.Sliced;

            var button = buttonParentGO.GetComponent<Button>();
            button.targetGraphic = buttonImage;
            button.transition = Selectable.Transition.ColorTint;

            return buttonParentGO;
        }

        private static GameObject CreateBrandedImageButton(string componentName, Vector2 buttonDimensions, Sprite buttonSprite)
        {
            // Root (clickable + background raycast hitbox)
            var go = new GameObject(componentName,
                typeof(RectTransform), typeof(Button), typeof(Image), typeof(LayoutElement), typeof(VerticalLayoutGroup));
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = buttonDimensions;

            // LayoutElement sizing (so H/V LayoutGroups honor our size)
            var le = go.GetComponent<LayoutElement>();
            le.preferredWidth = le.minWidth = buttonDimensions.x;
            le.preferredHeight = le.minHeight = buttonDimensions.y;
            le.flexibleWidth = le.flexibleHeight = 0;

            // Background (clean sliced)
            var bg = go.GetComponent<Image>();
            bg.sprite = buttonSprite;                       // e.g., texUICleanButton
            bg.type = Image.Type.Sliced;
            bg.color = new Color(0.18f, 0.18f, 0.20f, 1f);  // subtle bright panel
            bg.raycastTarget = true;

            // Vertical layout for content (centered, no expansion)
            var group = go.GetComponent<VerticalLayoutGroup>();
            group.childAlignment = TextAnchor.MiddleCenter;
            group.childControlWidth = group.childControlHeight = false;
            group.childForceExpandWidth = group.childForceExpandHeight = false;
            group.spacing = 2f;
            group.padding = new RectOffset(4, 4, 4, 4);

            // Content: Dice (top)
            var diceGO = new GameObject("Dice", typeof(RectTransform), typeof(Image));
            var diceRT = (RectTransform)diceGO.transform;
            diceRT.SetParent(rt, false);
            // ~70% of the smaller side minus padding feels good
            var diceSize = Mathf.Floor(Mathf.Min(buttonDimensions.x, buttonDimensions.y) * 0.7f);
            diceRT.sizeDelta = new Vector2(diceSize, diceSize);
            var diceImg = diceGO.GetComponent<Image>();
            diceImg.sprite = Addressables.LoadAssetAsync<Sprite>(
                "RoR2/Base/Common/MiscIcons/texRandomizeIcon.png").WaitForCompletion();
            diceImg.preserveAspect = true;
            diceImg.raycastTarget = false;

            // Outline overlay (stretched, ignores layout)
            var outlineGO = new GameObject("BaseOutline", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            var outlineRT = (RectTransform)outlineGO.transform;
            outlineRT.SetParent(rt, false);
            outlineRT.anchorMin = Vector2.zero; outlineRT.anchorMax = Vector2.one;
            outlineRT.offsetMin = Vector2.zero; outlineRT.offsetMax = Vector2.zero;
            outlineGO.GetComponent<LayoutElement>().ignoreLayout = true;
            var outlineImg = outlineGO.GetComponent<Image>();
            outlineImg.sprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/UI/texUIOutlineOnly.png").WaitForCompletion();
            outlineImg.type = Image.Type.Sliced;
            outlineImg.color = new Color(1f, 1f, 1f, 0.286f);
            outlineImg.raycastTarget = false;
            outlineGO.transform.SetAsLastSibling(); // ensure overlay sits on top

            // Button setup
            var button = go.GetComponent<Button>();
            button.targetGraphic = bg;
            button.transition = Selectable.Transition.ColorTint;
            // (optional) tweak colors if you want stronger hover feedback:
            // var colors = button.colors; colors.highlightedColor = new Color(1f,1f,1f,0.12f); button.colors = colors;

            return go;
        }

        private static GameObject CreateSpacer(string spacerName, RectTransform parentRect)
        {
            var spacerGO = new GameObject(spacerName, typeof(RectTransform), typeof(LayoutElement));
            FactoryUtils.ParentToRectTransform(spacerGO, parentRect);

            var spacerLE = spacerGO.GetComponent<LayoutElement>();
            spacerLE.flexibleWidth = 1;
            spacerLE.preferredWidth = 0;
            spacerLE.minWidth = 0;

            return spacerGO;
        }
    }
}
