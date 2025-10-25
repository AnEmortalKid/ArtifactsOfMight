using System;
using ExamplePlugin.Loadout.Draft;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using static UnityEngine.ParticleSystem;

namespace ExamplePlugin.UI.Drafting.Summary
{


    public static class DraftSummaryBarFactory
    {

        public static DraftSummaryBar CreateBar(RectTransform parentTransform)
        {
            var barGO = new GameObject("DraftSummaryBar", typeof(RectTransform), typeof(LayoutElement), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter));
            FactoryUtils.ParentToRectTransform(barGO, parentTransform);

            // take the full space
            var barGORT = barGO.GetComponent<RectTransform>();
            barGORT.anchorMin = Vector2.zero;
            barGORT.anchorMax = Vector2.one;
            barGORT.offsetMin = Vector2.zero;
            barGORT.offsetMax = Vector2.zero;

            // insets and padding, align things middle
            var hlg = barGO.GetComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(10, 10, 6, 6);
            hlg.spacing = 18f;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;

            var fitter = barGO.GetComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var draftSummaryBar = barGO.AddComponent<DraftSummaryBar>();

            bool isFirst = true;
            foreach (DraftItemTier draftTier in Enum.GetValues(typeof(DraftItemTier)))
            {
                // append separator from prior element
                //if (isFirst)
                //{
                //    isFirst = false;
                //}
                //else
                //{
                //    MakeSeparatorStep(barGORT);
                //}

                var tmp = MakeTierLabel(barGORT, draftTier);
                draftSummaryBar.BindTextArea(draftTier, tmp, SummaryBarPalette.GetTierTint(draftTier));
            }

            FactoryUtils.CreateSpacer("SummarySpacer", barGORT);

            //var diceIcon = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texRandomizeIcon.png").WaitForCompletion();
            //var diceGO = FactoryUtils.CreateImageButton("Summary" + "_Dice", new Vector2(36, 36), diceIcon);
            //FactoryUtils.ParentToRectTransform(diceGO, barGORT);

            var randomAll = CreateGlobalRandomAllButton(barGORT);
            draftSummaryBar.BindRandomizeButton(randomAll.GetComponent<Button>());

            return draftSummaryBar;
        }

        private static TextMeshProUGUI MakeTierLabel(RectTransform barGORT, DraftItemTier draftTier)
        {
            // each label takes how much it wants
            var textGO = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            var textRT = (RectTransform)textGO.transform;
            FactoryUtils.ParentToRectTransform(textGO, barGORT);

            var tmp = textGO.GetComponent<TextMeshProUGUI>();
            tmp.text = DraftTierLabels.GetUIName(draftTier);
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.fontSize = 20;
            tmp.fontStyle = FontStyles.Normal;
            tmp.color = Color.white;
            tmp.enableWordWrapping = false;

            return tmp;
        }
        public static GameObject CreateGlobalRandomAllButton(RectTransform parent, float size = 40f)
        {
            // Parent clickable + background raycast hitbox
            var go = new GameObject("GlobalRandomAllButton",
                typeof(RectTransform), typeof(Button), typeof(Image), typeof(VerticalLayoutGroup));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.sizeDelta = new Vector2(size, size);

            var borderImg = go.GetComponent<Image>();
            borderImg.sprite = Addressables.LoadAssetAsync<Sprite>(
                "RoR2/Base/UI/texUICleanButton.png"
            ).WaitForCompletion();
            borderImg.type = Image.Type.Sliced;
            // subtle bright
            borderImg.color = new Color(0.18f, 0.18f, 0.20f, 1f); 
            // where we click
            borderImg.raycastTarget = true;        

            // stretch with padding
            var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup)).GetComponent<RectTransform>();
            Stretch(content, rt, new Vector2(5f, 5f)); 

            // Layout
            var layout = content.GetComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = layout.childControlHeight = false;
            layout.childForceExpandWidth = layout.childForceExpandHeight = false;
            layout.spacing = 2f;
            layout.padding = new RectOffset(4, 4, 4, 4); // internal padding

            // Dice (top)
            var diceGO = new GameObject("Dice", typeof(RectTransform), typeof(Image));
            var diceRT = (RectTransform)diceGO.transform;
            diceRT.SetParent(content, false);
            diceRT.sizeDelta = new Vector2(28f, 28f);          // fits 40×40 nicely
            var diceImg = diceGO.GetComponent<Image>();
            diceImg.sprite = Addressables.LoadAssetAsync<Sprite>(
                "RoR2/Base/Common/MiscIcons/texRandomizeIcon.png").WaitForCompletion();
            diceImg.preserveAspect = true;
            diceImg.raycastTarget = false;                     // parent handles clicks

            // Label (bottom)
            var txtGO = new GameObject("Label", typeof(RectTransform), typeof(TMPro.TextMeshProUGUI));
            var txtRT = (RectTransform)txtGO.transform;
            txtRT.SetParent(content, false);
            txtRT.sizeDelta = new Vector2(size, 12f);
            var tmp = txtGO.GetComponent<TMPro.TextMeshProUGUI>();
            tmp.text = "ALL";
            // give a little breathing room
            tmp.fontSize = 11;
            tmp.fontStyle = TMPro.FontStyles.Bold;
            tmp.alignment = TMPro.TextAlignmentOptions.Center;
            tmp.color = new Color(1f, 1f, 1f, 0.95f);
            tmp.enableWordWrapping = false;
            tmp.raycastTarget = false;

            // --- BaseOutline (stretched, ignores layout) ---
            var baseOutline = new GameObject("BaseOutline",
                typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            var baseRT = (RectTransform)baseOutline.transform;
            baseRT.SetParent(rt, false);
            baseRT.anchorMin = Vector2.zero; baseRT.anchorMax = Vector2.one;
            baseRT.offsetMin = Vector2.zero; baseRT.offsetMax = Vector2.zero;
            baseOutline.GetComponent<LayoutElement>().ignoreLayout = true;
            var baseImg = baseOutline.GetComponent<Image>();
            baseImg.sprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/UI/texUIOutlineOnly.png").WaitForCompletion();
            baseImg.type = Image.Type.Sliced;
            baseImg.color = new Color(1f, 1f, 1f, 0.286f); // sample alpha
            baseImg.raycastTarget = false;

            // nice _polish_
            var colorFlicker = go.AddComponent<GlobalRandomDiceColorFlicker>();
            colorFlicker.Initialize(diceImg, 1.5f, 2f);

            return go;
        }

        static void Stretch(RectTransform rt, RectTransform parent, Vector2 pad)
        {
            rt.SetParent(parent, false);
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = pad; rt.offsetMax = -pad;
            rt.pivot = new Vector2(0.5f, 0.5f);
        }
    }
}
