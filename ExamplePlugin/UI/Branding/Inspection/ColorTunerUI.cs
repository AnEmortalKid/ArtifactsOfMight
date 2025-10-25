using UnityEngine.UI;
using UnityEngine;

namespace ArtifactsOfMight.UI.Branding.Inspection
{
    /// <summary>
    /// Useful for tweaking colors live instead of having to change, screenshot, tweak, reubild, restart
    /// </summary>
    public static class ColorTunerUI
    {

        /// <summary>
        /// Creates a UI that lets you tinker with the RGBA values for an image
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="targetImage"></param>
        public static void AttachTunerUI(RectTransform parent, Image targetImage)
        {
            // --- Holder ---
            var root = New("ColorTuner", parent, out var rootRT);
            var bg = root.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 1f);

            var v = root.AddComponent<VerticalLayoutGroup>();
            v.childForceExpandHeight = false; v.childForceExpandWidth = true;
            v.childAlignment = TextAnchor.UpperLeft; v.spacing = 6; v.padding = new RectOffset(8, 8, 8, 8);

            // --- Sliders (0-255 for RGB, 0-100 for A) ---
            var start = targetImage.color;
            int r = Mathf.RoundToInt(start.r * 255f);
            int g = Mathf.RoundToInt(start.g * 255f);
            int b = Mathf.RoundToInt(start.b * 255f);
            int aPct = Mathf.RoundToInt(start.a * 100f);

            var rUI = Row(v.transform, "R", 0, 255, r);
            var gUI = Row(v.transform, "G", 0, 255, g);
            var bUI = Row(v.transform, "B", 0, 255, b);
            var aUI = Row(v.transform, "A%", 0, 100, aPct);

            // --- Readout ---
            var readout = NewText("Readout", v.transform, "");
            readout.fontSize = 12; readout.color = new Color(0.9f, 0.95f, 1f, 0.95f);

            // --- Buttons ---
            var buttons = New("Buttons", v.transform, out _);
            var h = buttons.AddComponent<HorizontalLayoutGroup>(); h.spacing = 6; h.childForceExpandWidth = false;

            var resetBtn = SmallBtn(buttons.transform, "Reset", () => {
                rUI.slider.value = r; gUI.slider.value = g; bUI.slider.value = b; aUI.slider.value = aPct;
            });

            var copyBtn = SmallBtn(buttons.transform, "Copy C#",
                () => GUIUtility.systemCopyBuffer = BuildCodeLine((int)rUI.slider.value, (int)gUI.slider.value, (int)bUI.slider.value, (int)aUI.slider.value));

            // --- Wire updates ---
            void OnAnyChanged(float _)
            {
                int R = (int)rUI.slider.value, G = (int)gUI.slider.value, B = (int)bUI.slider.value, A = (int)aUI.slider.value;
                rUI.value.text = R.ToString(); gUI.value.text = G.ToString(); bUI.value.text = B.ToString(); aUI.value.text = A.ToString();

                targetImage.color = new Color(R / 255f, G / 255f, B / 255f, A / 100f);

                var hex = $"#{R:X2}{G:X2}{B:X2}  {A}%";
                readout.text = $"{hex}\n{BuildCodeLine(R, G, B, A)}";
            }

            rUI.slider.onValueChanged.AddListener(OnAnyChanged);
            gUI.slider.onValueChanged.AddListener(OnAnyChanged);
            bUI.slider.onValueChanged.AddListener(OnAnyChanged);
            aUI.slider.onValueChanged.AddListener(OnAnyChanged);
            OnAnyChanged(0f); // initial populate
        }

        // ---------- UI helpers ----------
        private static GameObject New(string name, Transform parent, out RectTransform rt)
        {
            var go = new GameObject(name, typeof(RectTransform));
            rt = (RectTransform)go.transform; rt.SetParent(parent, false);
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = rt.offsetMax = Vector2.zero;
            return go;
        }

        private static Text NewText(string name, Transform parent, string txt)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<Text>();
            t.text = txt; t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            t.color = Color.white; t.alignment = TextAnchor.MiddleLeft;
            return t;
        }

        private static (Slider slider, Text label, Text value) Row(Transform parent, string label, int min, int max, int initial)
        {
            var row = new GameObject(label + "Row", typeof(RectTransform));
            row.transform.SetParent(parent, false);
            var h = row.AddComponent<HorizontalLayoutGroup>(); h.spacing = 6; h.childForceExpandWidth = true; h.childAlignment = TextAnchor.MiddleLeft;

            var labelTxt = NewText(label, row.transform, label); labelTxt.rectTransform.sizeDelta = new Vector2(32, 20);

            var sGO = new GameObject(label + "Slider", typeof(RectTransform), typeof(Slider));
            sGO.transform.SetParent(row.transform, false);
            var sRT = (RectTransform)sGO.transform; sRT.sizeDelta = new Vector2(220, 20);
            var slider = sGO.GetComponent<Slider>(); slider.minValue = min; slider.maxValue = max; slider.wholeNumbers = true; slider.value = initial;

            // Slider background/fill (super minimal)
            var bg = new GameObject("BG", typeof(RectTransform), typeof(Image)); bg.transform.SetParent(sGO.transform, false);
            var bgRT = (RectTransform)bg.transform; bgRT.anchorMin = Vector2.zero; bgRT.anchorMax = Vector2.one; bgRT.offsetMin = bgRT.offsetMax = Vector2.zero;
            bg.GetComponent<Image>().color = new Color(1, 1, 1, 0.08f);
            slider.targetGraphic = bg.GetComponent<Image>();

            var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image)); fill.transform.SetParent(bg.transform, false);
            var fillRT = (RectTransform)fill.transform; fillRT.anchorMin = new Vector2(0, 0); fillRT.anchorMax = new Vector2(0, 1); fillRT.offsetMin = Vector2.zero; fillRT.offsetMax = Vector2.zero;
            var fillImg = fill.GetComponent<Image>(); fillImg.color = new Color(1, 1, 1, 0.35f);

            var fillArea = new GameObject("FillArea", typeof(RectTransform)); fillArea.transform.SetParent(sGO.transform, false);
            var handleSlideArea = new GameObject("HandleSlideArea", typeof(RectTransform)); handleSlideArea.transform.SetParent(sGO.transform, false);
            slider.fillRect = fillRT; // minimal hookup

            var valueTxt = NewText("Value", row.transform, initial.ToString()); valueTxt.rectTransform.sizeDelta = new Vector2(40, 20);

            return (slider, labelTxt, valueTxt);
        }

        private static Button SmallBtn(Transform parent, string text, System.Action onClick)
        {
            var go = new GameObject(text + "Btn", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform; rt.sizeDelta = new Vector2(86, 24);
            go.GetComponent<Image>().color = new Color(1, 1, 1, 0.08f);
            var b = go.GetComponent<Button>(); b.onClick.AddListener(() => onClick());

            var txt = NewText("Txt", go.transform, text); txt.alignment = TextAnchor.MiddleCenter;
            txt.rectTransform.anchorMin = Vector2.zero; txt.rectTransform.anchorMax = Vector2.one; txt.rectTransform.offsetMin = txt.rectTransform.offsetMax = Vector2.zero;
            return b;
        }

        private static string BuildCodeLine(int r, int g, int b, int aPct)
            => $"primaryImg.color = new Color({r}/255f, {g}/255f, {b}/255f, {aPct}/100f); // #{r:X2}{g:X2}{b:X2}  {aPct}%";
    }
}
