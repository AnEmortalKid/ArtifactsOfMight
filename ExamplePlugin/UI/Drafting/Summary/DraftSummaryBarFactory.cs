using System;
using ExamplePlugin.Loadout.Draft;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace ExamplePlugin.UI.Drafting.Summary
{


    public static class DraftSummaryBarFactory
    {

        public static DraftSummaryBar CreateBar(RectTransform parentTransform)
        {
            var barGO = new GameObject("DraftSummaryBar", typeof(RectTransform), typeof(LayoutElement), typeof(HorizontalLayoutGroup));
            FactoryUtils.ParentToRectTransform(barGO, parentTransform);

            var barGORT = barGO.GetComponent<RectTransform>();
            var draftSummaryBar = barGO.AddComponent<DraftSummaryBar>();

            foreach (DraftItemTier draftTier in Enum.GetValues(typeof(DraftItemTier)))
            {
                var textGO = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
                var textRT = (RectTransform)textGO.transform;
                FactoryUtils.ParentToRectTransform(textGO, barGORT);
                textRT.anchorMin = Vector2.zero;
                textRT.anchorMax = Vector2.one;
                textRT.offsetMin = Vector2.zero;
                textRT.offsetMax = Vector2.zero;

                var tmp = textGO.GetComponent<TextMeshProUGUI>();
                tmp.text = DraftTierLabels.GetUIName(draftTier);
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.fontSize = 14;
                tmp.fontStyle = FontStyles.Normal;
                tmp.color = Color.white;
                tmp.enableWordWrapping = false;

                draftSummaryBar.BindTextArea(draftTier, tmp);
            }

            FactoryUtils.CreateSpacer("SummarySpacer", barGORT);

            var diceIcon = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texRandomizeIcon.png").WaitForCompletion();
            var diceGO = FactoryUtils.CreateImageButton("Summary" + "_Dice", new Vector2(40, 40), diceIcon);
            FactoryUtils.ParentToRectTransform(diceGO, barGORT);

            // Testing
            var lockIconMaybe = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/VoidIcon_2.png").WaitForCompletion();
            var lockGO = FactoryUtils.CreateImageButton("Summary" + "_Lock", new Vector2(40, 40), lockIconMaybe);
            FactoryUtils.ParentToRectTransform(lockGO, barGORT);

            return draftSummaryBar;
        }
    }
}
