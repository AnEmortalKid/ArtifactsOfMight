using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ExamplePlugin.UI.Drafting
{
    public static class FactoryUtils
    {
        private static Vector2 CENTER = new Vector2(0.5f, 0.5f);

        /// <summary>
        /// Parents the given gameObject to the desired rectTransform, with world position stays
        /// </summary>
        /// <param name="child"></param>
        /// <param name="parentRect"></param>
        public static void ParentToRectTransform(GameObject child, RectTransform parentRect)
        {
            var parentGO = parentRect.gameObject;
            child.layer = parentGO.layer;

            var childRT = child.GetComponent<RectTransform>();
            childRT.SetParent(parentRect, worldPositionStays: false);
        }

        /// <summary>
        /// Stretches the child to the full bounds of the parent (no borders/margins,etc)
        /// </summary>
        /// <param name="childRT"></param>
        public static void StretchToFillParent(RectTransform childRT)
        {
            // full stretch in parent
            childRT.anchorMin = Vector2.zero;
            childRT.anchorMax = Vector2.one;
            childRT.pivot = CENTER;
            childRT.anchoredPosition = Vector2.zero;
            childRT.sizeDelta = Vector2.zero;
            childRT.offsetMin = Vector2.zero;
            childRT.offsetMax = Vector2.zero;
        }

        /// <summary>
        /// Use this when you want the child to have a specific size
        /// </summary>
        /// <param name="childRT"></param>
        /// <param name="desiredSize"></param>
        public static void CenterWithinParent(RectTransform childRT, Vector2 desiredSize)
        {
            childRT.sizeDelta = desiredSize;
            childRT.pivot = CENTER;
            childRT.anchorMin = CENTER;
            childRT.anchorMax = CENTER;
        }

        public static void SetLayerRecursively(GameObject go, int layer)
        {
            go.layer = layer;
            for (int i = 0; i < go.transform.childCount; i++)
                SetLayerRecursively(go.transform.GetChild(i).gameObject, layer);
        }

        /// <summary>
        /// Creates a component that acts as a spacer element designed to stretch between 2 components
        /// with fixed sizes in order to keep them in the same spot
        /// </summary>
        /// <param name="spacerName"></param>
        /// <param name="parentRect"></param>
        /// <returns></returns>
        public static GameObject CreateSpacer(string spacerName, RectTransform parentRect)
        {
            var spacerGO = new GameObject(spacerName, typeof(RectTransform), typeof(LayoutElement));
            ParentToRectTransform(spacerGO, parentRect);

            var spacerLE = spacerGO.GetComponent<LayoutElement>();
            spacerLE.flexibleWidth = 1;
            spacerLE.preferredWidth = 0;
            spacerLE.minWidth = 0;

            return spacerGO;
        }

        public static GameObject CreateImageButton(string componentName,
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

       public static TMP_Text CreateTMP(string name, RectTransform parent, int size, FontStyles style, Color fontColor)
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

        public static Vector4 GetSpriteBorderAsUIPadding(Sprite s)
        {
            if (!s) return Vector4.zero;
            float ppu = s.pixelsPerUnit > 0 ? s.pixelsPerUnit : 100f;
            var b = s.border; // (L,B,R,T) in pixels
            return new Vector4(b.x / ppu, b.w / ppu, b.z / ppu, b.y / ppu); // (L,T,R,B)
        }
    }
}
