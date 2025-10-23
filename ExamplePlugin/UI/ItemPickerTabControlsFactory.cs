
using ExamplePlugin.UI.Drafting;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace ExamplePlugin.UI
{
    public static class ItemPickerTabControlsFactory
    {

        /// <summary>
        /// Creates the hierarchy for our tab controls:
        /// 
        /// ItemPickerTabControls
        ///   ModeGroup
        ///   RestrictionGroup
        ///   ButtonGroup
        ///   
        /// </summary>
        /// <returns></returns>
        public static DraftTabControls CreateTabControls(string componentName, RectTransform parentRectTransform)
        {
            var controlsGO = new GameObject(componentName, typeof(RectTransform), typeof(DraftTabControls), typeof(Image));
            var controlsBarRT = controlsGO.GetComponent<RectTransform>();
            controlsBarRT.SetParent(parentRectTransform);

            // Green ish
            var controlsOutlineColor = ColorPalette.FromRGB(51, 99, 66, 1f);
            var img = controlsGO.GetComponent<Image>();
            img.color = controlsOutlineColor;
            img.raycastTarget = true;        // optional click blocker

            var controlsHG = controlsGO.AddComponent<HorizontalLayoutGroup>();
            controlsHG.spacing = 4;
            controlsHG.childControlHeight = true;
            controlsHG.childControlWidth = true;
            controlsHG.childForceExpandHeight = false;
            controlsHG.childForceExpandWidth = false;
            controlsHG.childAlignment = TextAnchor.MiddleCenter;
            controlsHG.padding = new RectOffset(8, 8, 8, 8);

            var tabControls = controlsGO.GetComponent<DraftTabControls>();


            var modeGroup = CreateModeGroup(componentName + "_Mode", controlsBarRT, tabControls);
            var spacer = CreateSpacer(componentName + "_Spacer", controlsBarRT);
            var restrictionsGroup = CreateRestrictionGroup(componentName + "_RestrictionGroup", controlsBarRT, tabControls);




            return tabControls;
        }


        /// <summary>
        /// Creates the UI components for toggling between restricted mode or none
        /// [ Label Toggle ]
        ///
        private static GameObject CreateModeGroup(string groupName, RectTransform modeGroupParent, DraftTabControls pickerTabControls)
        {
            var modeGroup = new GameObject(groupName, typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            ParentToRectTransform(modeGroup, modeGroupParent);

            // parent for our components
            var modeGroupRT = modeGroup.GetComponent<RectTransform>();

            // blue ish
            var groupColor = new Color(50 / 255f, 141 / 255f, 168 / 255f, 1f);
            var debugBackground = modeGroup.AddComponent<Image>();
            debugBackground.color = groupColor;
            debugBackground.raycastTarget = true;        // optional click blocker

            // set its layout element
            var modeLE = modeGroup.GetComponent<LayoutElement>();
            modeLE.minHeight = 24;
            modeLE.preferredHeight = 48;
            modeLE.flexibleWidth = 0;

            var hlg = modeGroup.GetComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.spacing = 6;
            hlg.padding = new RectOffset(4, 4, 4, 4);
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            var restrictLabel = new GameObject(groupName + "_ModeLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
            ParentToRectTransform(restrictLabel, modeGroupRT);

            var restrictLabelRT = restrictLabel.GetComponent<RectTransform>();
            restrictLabelRT.anchorMin = new Vector2(0, 0);
            restrictLabelRT.anchorMax = new Vector2(1, 0);
            restrictLabelRT.sizeDelta = new Vector2(0, 22);

            var restrictText = restrictLabel.GetComponent<TextMeshProUGUI>();
            restrictText.text = "Restricted Mode?: ";
            restrictText.alignment = TextAlignmentOptions.Left;

            var toggleGOName = groupName + "_ModeToggle";

            var restrictToggleGO = new GameObject(toggleGOName, typeof(RectTransform), typeof(Toggle), typeof(LayoutElement));
            ParentToRectTransform(restrictToggleGO, modeGroupRT);

            var toggleDimensions = new Vector2(24, 24);
            var restrictToggle = restrictToggleGO.GetComponent<Toggle>();
            var toggleLE = restrictToggleGO.GetComponent<LayoutElement>();
            toggleLE.minWidth = toggleLE.preferredWidth = toggleDimensions.x;   // tweak size here
            toggleLE.minHeight = toggleLE.preferredHeight = toggleDimensions.y;
            toggleLE.flexibleWidth = toggleLE.flexibleHeight = 0;

            var restrictToggleRT = restrictToggleGO.GetComponent<RectTransform>();
            restrictToggleRT.sizeDelta = toggleDimensions;

            // Setup toggle graphic with background
            var toggleBG = new GameObject(toggleGOName + "_Background", typeof(RectTransform), typeof(Image));
            ParentToRectTransform(toggleBG, restrictToggleRT);

            // fill full
            var toggleBGRt = (RectTransform)toggleBG.transform;
            toggleBGRt.anchorMin = Vector2.zero;
            toggleBGRt.anchorMax = Vector2.one;
            toggleBGRt.offsetMin = Vector2.zero;
            toggleBGRt.offsetMax = Vector2.zero;

            var toggleBGImage = toggleBG.GetComponent<Image>();
            toggleBGImage.color = ColorPalette.DarkGray; // dark gray box
            toggleBGImage.raycastTarget = true;

            var checkGO = new GameObject(toggleGOName + "_Checkmark", typeof(RectTransform), typeof(Image));
            ParentToRectTransform(checkGO, restrictToggleRT);
            //checkGO.transform.SetAsLastSibling();

            var checkRT = (RectTransform)checkGO.transform;
            checkRT.anchorMin = new Vector2(0.5f, 0.5f);
            checkRT.anchorMax = new Vector2(0.5f, 0.5f);
            checkRT.sizeDelta = new Vector2(toggleDimensions.x - 4, toggleDimensions.y - 4);          // smaller than bg
            checkRT.anchoredPosition = Vector2.zero;

            // this worked
            var checkImg = checkGO.GetComponent<Image>();
            checkImg.color = new Color(0.92f, 0.92f, 0.92f, 1f); // light mark
            checkImg.raycastTarget = false;

            // Wire the toggle graphics
            restrictToggle.targetGraphic = toggleBGImage;              // background will highlight/press, etc.
            restrictToggle.graphic = checkImg;                 // this is shown/hidden by Toggle.isOn
            restrictToggle.isOn = false;

            // set the required refs
            pickerTabControls.BindModeToggle(restrictToggle);

            return modeGroup;
        }


        private static GameObject CreateRestrictionGroup(string groupName, RectTransform controlsBarRT, DraftTabControls pickerTabControls)
        {
            var restrictionsGroup = new GameObject(groupName, typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            ParentToRectTransform(restrictionsGroup, controlsBarRT);

            var horizontalSettings = restrictionsGroup.GetComponent<HorizontalLayoutGroup>();
            horizontalSettings.spacing = 6;
            horizontalSettings.childControlWidth = true;
            horizontalSettings.childControlHeight = true;
            horizontalSettings.childForceExpandHeight = false;
            horizontalSettings.childForceExpandWidth = false;

            // add our components
            var controlsGroupRT = restrictionsGroup.GetComponent<RectTransform>();
            var itemCountControls = CreateItemCountControls(groupName + "_Counts", controlsGroupRT, pickerTabControls);
            var miscOptsionGroup = CreateGridOptionsGroup(groupName + "_Options", controlsGroupRT, pickerTabControls);

            return restrictionsGroup;
        }

        /// <summary>
        /// Creates the UI controls for managing the item limit when restricted
        /// [ Button Text Button]
        /// </summary>
        private static GameObject CreateItemCountControls(string groupName, RectTransform controlGroupRT, DraftTabControls pickerTabControls)
        {
            // parent object for everything
            var countsGroupGO = new GameObject(groupName, typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            ParentToRectTransform(countsGroupGO, controlGroupRT);


            var restrictionsRT = countsGroupGO.GetComponent<RectTransform>();

            // orange ish ish
            var groupColor = ColorPalette.FromRGB(209, 63, 63);
            var debugBackground = countsGroupGO.AddComponent<Image>();
            debugBackground.color = groupColor;
            debugBackground.raycastTarget = true;        // optional click blocker


            var hlg = countsGroupGO.GetComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.spacing = 6;
            hlg.padding = new RectOffset(4, 4, 4, 4);
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            var groupLE = countsGroupGO.GetComponent<LayoutElement>();
            groupLE.preferredWidth = 240;

            var buttonDimensions = new Vector2(32, 32);

            // - button
            var leftArrowSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texOptionsArrowLeft.png").WaitForCompletion();
            var decreaseGO = CreateImageButton(groupName + "_Minus", buttonDimensions, leftArrowSprite);
            ParentToRectTransform(decreaseGO, restrictionsRT);
            var decreaseRT = decreaseGO.GetComponent<RectTransform>();

            // text display
            var restrictCountLabel = new GameObject(groupName + "_Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            ParentToRectTransform(restrictCountLabel, restrictionsRT);

            var restrictCountLabelRT = restrictCountLabel.GetComponent<RectTransform>();
            restrictCountLabelRT.anchorMin = new Vector2(0, 0);
            restrictCountLabelRT.anchorMax = new Vector2(1, 0);
            restrictCountLabelRT.sizeDelta = new Vector2(0, 22);

            var restrictText = restrictCountLabel.GetComponent<TextMeshProUGUI>();
            restrictText.text = "N/A";
            restrictText.alignment = TextAlignmentOptions.Center;

            // + button
            var rightArrowSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texOptionsArrowRight.png").WaitForCompletion();
            var increaseGO = CreateImageButton(groupName + "_Plus", buttonDimensions, rightArrowSprite);
            ParentToRectTransform(increaseGO, restrictionsRT);

            pickerTabControls.BindItemLimitElements(decreaseGO.GetComponent<Button>(),
                restrictText, increaseGO.GetComponent<Button>());

            return countsGroupGO;
        }


        // 
        private static GameObject CreateGridOptionsGroup(string groupName, RectTransform parentRT, DraftTabControls pickerTabControls)
        {
            var gridOptionsGroup = new GameObject(groupName, typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            ParentToRectTransform(gridOptionsGroup, parentRT);

            // parent object for every child
            var gridControlsRT = gridOptionsGroup.GetComponent<RectTransform>();

            // pinkish
            var groupColor = ColorPalette.FromRGB(189, 58, 134);
            var debugBackground = gridOptionsGroup.AddComponent<Image>();
            debugBackground.color = groupColor;
            debugBackground.raycastTarget = true;        // optional click blocker

            var hlg = gridOptionsGroup.GetComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.spacing = 6;
            hlg.padding = new RectOffset(2, 2, 2, 2);
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;


            var diceIcon = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texRandomizeIcon.png").WaitForCompletion();
            var diceGO = CreateImageButton(groupName + "_Dice", new Vector2(40, 40), diceIcon);
            ParentToRectTransform(diceGO, gridControlsRT);

            pickerTabControls.BindDiceButton(diceGO.GetComponent<Button>());
            return gridOptionsGroup;
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

            var button = buttonParentGO.GetComponent<Button>();
            button.targetGraphic = buttonImage;
            button.transition = Selectable.Transition.ColorTint;

            return buttonParentGO;
        }

        private static void ParentToRectTransform(GameObject child, RectTransform parentRect)
        {
            var parentGO = parentRect.gameObject;
            child.layer = parentGO.layer;

            var childRT = child.GetComponent<RectTransform>();
            childRT.SetParent(parentRect);
        }

        private static GameObject CreateSpacer(string spacerName, RectTransform parentRect)
        {
            var spacerGO = new GameObject(spacerName, typeof(RectTransform), typeof(LayoutElement));
            ParentToRectTransform(spacerGO, parentRect);

            var spacerLE = spacerGO.GetComponent<LayoutElement>();
            spacerLE.flexibleWidth = 1;
            spacerLE.preferredWidth = 0;
            spacerLE.minWidth = 0;

            return spacerGO;
        }
    }
}
