using System;
using System.Collections.Generic;
using System.Text;
using ArtifactsOfMight.UI.Branding.Inspection;
using TMPro;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using UnityEngine;
using ArtifactsOfMight.Loadout.Draft;

namespace ArtifactsOfMight.UI.Drafting.Tabs
{
    /// <summary>
    /// Responsible for constructing the visual elements required for a DraftTabButton
    /// </summary>
    public static class DraftTabButtonFactory
    {

        // TODO use grab highlight and select colors

        public static DraftTabButton CreateTabButton(DraftItemTier itemTier, string label, Vector2 size, DraftTabColors tabColors)
        {
            var go = new GameObject($"Tab_{label}", typeof(RectTransform), typeof(Button), typeof(LayoutElement));
            var rt = go.GetComponent<RectTransform>();

            // Layout sizing
            var le = go.GetComponent<LayoutElement>();
            le.preferredWidth = le.minWidth = size.x;
            le.preferredHeight = le.minHeight = size.y;
            le.flexibleWidth = le.flexibleHeight = 0;

            // Background (texUICleanButton)
            var buttonBackground = go.AddComponent<Image>();
            buttonBackground.sprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/UI/texUICleanButton.png").WaitForCompletion();
            buttonBackground.type = Image.Type.Sliced;
            buttonBackground.color = tabColors.NormalTint;

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
            var selectOverlay = borderGO.GetComponent<Image>();
            selectOverlay.sprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/UI/texUIHighlightBoxOutline.png").WaitForCompletion();
            selectOverlay.type = Image.Type.Sliced;
            selectOverlay.color = Color.white;
            selectOverlay.enabled = false;
            // stretch border to the same span
            FactoryUtils.StretchToFillParent(borderGO.GetComponent<RectTransform>());
            var selectedRT = borderGO.GetComponent<RectTransform>();

            // Highlight on hover, on top of border
            //            "RoR2/Base/UI/texUIHighlightBoxOutlineThick.png",
            var highlightGO = new GameObject("HiglightBorder", typeof(RectTransform), typeof(Image));
            FactoryUtils.ParentToRectTransform(highlightGO, rt);

            var hoverHighlight = highlightGO.GetComponent<Image>();
            hoverHighlight.sprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/UI/texUIHighlightBoxOutlineThick.png").WaitForCompletion();
            hoverHighlight.type = Image.Type.Sliced;
            hoverHighlight.color = Color.white;
            hoverHighlight.enabled = false;
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

            var selectHiglightBorder = headerGO.GetComponent<Image>();
            selectHiglightBorder.sprite = Addressables
                .LoadAssetAsync<Sprite>("RoR2/Base/UI/texUIHighlightHeader.png")
                .WaitForCompletion();
            selectHiglightBorder.type = Image.Type.Sliced;
            selectHiglightBorder.raycastTarget = false;

            // choose color: white (vanilla) or your tint for “colored corners”
            selectHiglightBorder.color = Color.white; // or = tint;

            selectHiglightBorder.enabled = false; // only on selected

            // Label
            var tmp = FactoryUtils.CreateTMP("Label", rt, 22, FontStyles.Bold, Color.white);
            tmp.text = label;
            tmp.alignment = TextAlignmentOptions.Center;

            // Button + selection behaviour
            var btn = go.GetComponent<Button>();


            var dtb = go.AddComponent<DraftTabButton>();
            dtb.BindHoverElements(hoverHighlight);
            dtb.BindSelectionElements(selectOverlay, selectHiglightBorder);
            dtb.BindButtonAction(itemTier, btn);

            dtb.BindColors(tabColors);
            dtb.BindBackground(buttonBackground);

            return dtb;
        }
    }
}
