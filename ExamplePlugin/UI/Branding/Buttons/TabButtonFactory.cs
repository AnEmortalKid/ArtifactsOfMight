using System;
using System.Collections.Generic;
using System.Text;
using ArtifactsOfMight.UI.Drafting;
using TMPro;
using UnityEngine.AddressableAssets;
using UnityEngine;
using UnityEngine.UI;

namespace ArtifactsOfMight.UI.Branding.Buttons
{
    internal class TabButtonFactory
    {


        public static TabButtonContainer CreateTabButton(RectTransform parent, string text, Color color)
        {
            var btnGO = new GameObject($"Tab_{text}", typeof(RectTransform), typeof(Button));
            var rt = btnGO.GetComponent<RectTransform>();
            FactoryUtils.ParentToRectTransform(btnGO, parent);

            // Base
            var bgGO = new GameObject("Background", typeof(Image));
            FactoryUtils.ParentToRectTransform(bgGO, rt);
            var bg = bgGO.GetComponent<Image>();
            bg.sprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/UI/texUICleanButton.png").WaitForCompletion();
            bg.type = Image.Type.Sliced;
            bg.color = color;

            // Border / Highlight
            var borderGO = new GameObject("HighlightBorder", typeof(Image));
            FactoryUtils.ParentToRectTransform(borderGO, rt);
            var border = borderGO.GetComponent<Image>();
            border.sprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/UI/texUIAnimateSliceNakedButton.png").WaitForCompletion();
            border.type = Image.Type.Sliced;
            border.color = new Color(1f, 1f, 1f, 0.8f);
            border.enabled = false;

            // Text
            var tmp = FactoryUtils.CreateTMP("Label", rt, 22, FontStyles.Bold, Color.white);
            tmp.text = text;
            tmp.alignment = TextAlignmentOptions.Center;

            //// Button behavior
            //var button = btnGO.GetComponent<Button>();
            //button.onClick.AddListener(() => onClick?.Invoke());
            //var btnAnim = btnGO.AddComponent<UIHighlightAnim>();
            //btnAnim.border = border;

            return new TabButtonContainer
            {
                buttonHolder = btnGO,
                button = btnGO.GetComponent<Button>(),
                selectedSheen = border,
            };
        }

    }
}
