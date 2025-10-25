using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

namespace ArtifactsOfMight.UI.Drafting
{
    public class DraftTabsBarFactory
    {

        // TODO this is where our new fancy button would g

        public static GameObject CreateTextButton(
            string componentName,
            Vector2 buttonDimensions,
            string buttonText,
            FontStyles fontStyle = FontStyles.Normal,
            int fontSize = 24,
            Color? textColor = null,
            Color? backgroundColor = null)
        {
            // root object
            var go = new GameObject(componentName,
                typeof(RectTransform),
                typeof(Button),
                typeof(Image),
                typeof(LayoutElement));

            var rt = go.GetComponent<RectTransform>();

            // layout element sizing
            var le = go.GetComponent<LayoutElement>();
            le.preferredWidth = buttonDimensions.x;
            le.preferredHeight = buttonDimensions.y;
            le.minWidth = buttonDimensions.x;
            le.minHeight = buttonDimensions.y;
            le.flexibleWidth = 0;
            le.flexibleHeight = 0;

            // background
            var img = go.GetComponent<Image>();
            img.color = backgroundColor ?? new Color(0.15f, 0.15f, 0.15f, 1f); // default dark gray

            // button component
            var btn = go.GetComponent<Button>();
            btn.targetGraphic = img;
            btn.transition = Selectable.Transition.ColorTint;

            // text child
            var textGO = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            var textRT = (RectTransform)textGO.transform;
            textRT.SetParent(rt, false);
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = Vector2.zero;
            textRT.offsetMax = Vector2.zero;

            var tmp = textGO.GetComponent<TextMeshProUGUI>();
            tmp.text = buttonText;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = fontSize;
            tmp.fontStyle = fontStyle;
            tmp.color = textColor ?? Color.white;
            tmp.enableWordWrapping = false;

            return go;
        }
    }
}
