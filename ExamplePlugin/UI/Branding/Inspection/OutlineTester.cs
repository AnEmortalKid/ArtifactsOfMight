using UnityEngine.UI;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ExamplePlugin.UI.Branding.Inspection
{
    /// <summary>
    /// Demo area where we figure out how to position the hover outline for it to fit correctly
    /// </summary>
    public static class OutlineTester
    {
        public static GameObject CreateOutlineTester(RectTransform safeArea)
        {
            // Root panel
            var root = new GameObject("OutlineAlignTester", typeof(RectTransform), typeof(Image));
            var rt = (RectTransform)root.transform;
            rt.SetParent(safeArea, false);
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = new Vector2(16, 16);
            rt.offsetMax = new Vector2(-16, -16);
            root.GetComponent<Image>().color = new Color(0.19f, 0.23f, 0.30f, 1f);

            // Grid container
            var gridGO = new GameObject("Grid", typeof(RectTransform), typeof(GridLayoutGroup));
            var gridRT = (RectTransform)gridGO.transform;
            gridRT.SetParent(rt, false);
            gridRT.anchorMin = new Vector2(0, 0); gridRT.anchorMax = new Vector2(1, 1);
            gridRT.offsetMin = new Vector2(16, 16);
            gridRT.offsetMax = new Vector2(-16, -16);

            var grid = gridGO.GetComponent<GridLayoutGroup>();
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.UpperLeft;
            grid.cellSize = new Vector2(72, 72);
            grid.spacing = new Vector2(10, 10);
            grid.padding = new RectOffset(10, 10, 10, 10);

            // Make cells
            const int cols = 8, rows = 4;
            RectTransform testCellRT = null;
            for (int i = 0; i < cols * rows; i++)
            {
                var cell = new GameObject($"Cell_{i}", typeof(RectTransform), typeof(Image));
                var cellRT = (RectTransform)cell.transform;
                cellRT.SetParent(gridRT, false);

                var cellImg = cell.GetComponent<Image>();
                cellImg.color = new Color(0.35f, 0.40f, 0.45f, 1f); // neutral tile

                if (i == 10) testCellRT = cellRT; // pick one to host the outline
            }

            // Outline overlay on test cell
            var outlineGO = new GameObject("HoverOutline", typeof(RectTransform), typeof(Image));
            var outlineRT = (RectTransform)outlineGO.transform;
            outlineRT.SetParent(testCellRT, false);
            outlineRT.anchorMin = Vector2.zero; outlineRT.anchorMax = Vector2.one;
            outlineRT.pivot = new Vector2(0.5f, 0.5f);
            outlineRT.anchoredPosition = Vector2.zero;
            outlineRT.offsetMin = Vector2.zero; outlineRT.offsetMax = Vector2.zero;

            var outlineImg = outlineGO.GetComponent<Image>();
            outlineImg.sprite = Addressables
                .LoadAssetAsync<Sprite>("RoR2/Base/UI/texUIHighlightBoxOutlineThick.png")
                .WaitForCompletion();
            outlineImg.preserveAspect = false;
            outlineImg.type = Image.Type.Sliced;
            outlineImg.color = Color.white;

            // ----- Compact controls toolbox (top-left) -----
            var box = new GameObject("ControlsBox", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
            var boxRT = (RectTransform)box.transform;
            boxRT.SetParent(rt, false);
            boxRT.anchorMin = new Vector2(0, 1);
            boxRT.anchorMax = new Vector2(0, 1);
            boxRT.pivot = new Vector2(0, 1);
            boxRT.anchoredPosition = new Vector2(16, -16);
            boxRT.sizeDelta = new Vector2(260, 0); // width fixed; height by layout
            var boxImg = box.GetComponent<Image>();
            boxImg.color = new Color(0.12f, 0.15f, 0.20f, 0.95f);

            var v = box.GetComponent<VerticalLayoutGroup>();
            v.childAlignment = TextAnchor.UpperLeft;
            v.childControlWidth = true;
            v.childControlHeight = true;
            v.childForceExpandWidth = false;
            v.childForceExpandHeight = false;
            v.spacing = 6;
            v.padding = new RectOffset(10, 10, 10, 10);

            // Row builder: [Label][Slider][Value]
            System.Action<string, float, float, float, System.Action<float>> AddRow =
            (label, min, max, start, onChanged) =>
            {
                var row = new GameObject(label, typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
                var rowRT = (RectTransform)row.transform;
                rowRT.SetParent(boxRT, false);
                var le = row.GetComponent<LayoutElement>();
                le.minHeight = 24; le.preferredHeight = 28;

                var h = row.GetComponent<HorizontalLayoutGroup>();
                h.childAlignment = TextAnchor.MiddleLeft;
                h.childControlWidth = true;
                h.childControlHeight = true;
                h.childForceExpandWidth = false;
                h.childForceExpandHeight = false;
                h.spacing = 8;
                h.padding = new RectOffset(0, 0, 0, 0);

                Text MakeText(string text, float w)
                {
                    var t = new GameObject("Text", typeof(RectTransform), typeof(Text), typeof(LayoutElement));
                    var tRT = (RectTransform)t.transform;
                    tRT.SetParent(rowRT, false);
                    t.GetComponent<LayoutElement>().preferredWidth = w;
                    var tx = t.GetComponent<Text>();
                    tx.text = text; tx.alignment = TextAnchor.MiddleLeft;
                    tx.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                    tx.color = Color.white;
                    tx.raycastTarget = false;
                    return tx;
                }

                // Label
                MakeText(label, 70);

                // Slider (fixed width)
                var sliderWrap = new GameObject("SliderWrap", typeof(RectTransform), typeof(LayoutElement));
                var swRT = (RectTransform)sliderWrap.transform;
                swRT.SetParent(rowRT, false);
                sliderWrap.GetComponent<LayoutElement>().preferredWidth = 150;

                var slider = BuildTightSlider(swRT, min, max, start, onChanged);

                // Value text (never blocked)
                var val = MakeText(start.ToString("0"), 36);

                slider.onValueChanged.AddListener(vv => val.text = vv.ToString("0"));
            };

            // Hook sliders to outlineRT
            AddRow("anchX", -20, 20, 3, v => outlineRT.anchoredPosition = new Vector2(v, outlineRT.anchoredPosition.y));
            AddRow("anchY", -20, 20, -6, v => outlineRT.anchoredPosition = new Vector2(outlineRT.anchoredPosition.x, v));
            AddRow("minX", -20, 20, -3, v => outlineRT.offsetMin = new Vector2(v, outlineRT.offsetMin.y));
            AddRow("minY", -20, 20, -6, v => outlineRT.offsetMin = new Vector2(outlineRT.offsetMin.x, v));
            AddRow("maxX", -20, 20, 6, v => outlineRT.offsetMax = new Vector2(v, outlineRT.offsetMax.y));
            AddRow("maxY", -20, 20, 3, v => outlineRT.offsetMax = new Vector2(outlineRT.offsetMax.x, v));

            return root;
        }

        // Builds a slider with proper, constrained child rects so the handle/fill never overlap labels.
        static Slider BuildTightSlider(RectTransform parent, float min, float max, float start, System.Action<float> onChanged)
        {
            var sliderGO = new GameObject("Slider", typeof(RectTransform), typeof(Slider));
            var sRT = (RectTransform)sliderGO.transform;
            sRT.SetParent(parent, false);
            sRT.sizeDelta = new Vector2(150, 22);

            // Background
            var bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
            var bgRT = (RectTransform)bg.transform;
            bgRT.SetParent(sRT, false);
            bgRT.anchorMin = new Vector2(0, 0.5f); bgRT.anchorMax = new Vector2(1, 0.5f);
            bgRT.pivot = new Vector2(0.5f, 0.5f);
            bgRT.sizeDelta = new Vector2(0, 8); // thickness
            bg.GetComponent<Image>().color = new Color(1, 1, 1, 0.15f);

            // Fill Area
            var fillArea = new GameObject("Fill Area", typeof(RectTransform));
            var faRT = (RectTransform)fillArea.transform;
            faRT.SetParent(sRT, false);
            faRT.anchorMin = new Vector2(0, 0.5f); faRT.anchorMax = new Vector2(1, 0.5f);
            faRT.pivot = new Vector2(0.5f, 0.5f);
            faRT.offsetMin = new Vector2(6, -4);
            faRT.offsetMax = new Vector2(-6, 4);

            // Fill
            var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            var fRT = (RectTransform)fill.transform;
            fRT.SetParent(faRT, false);
            fRT.anchorMin = new Vector2(0, 0); fRT.anchorMax = new Vector2(0, 1);
            fRT.pivot = new Vector2(0, 0.5f);
            fRT.sizeDelta = new Vector2(0, 0);
            fill.GetComponent<Image>().color = new Color(1, 1, 1, 0.6f);

            // Handle Slide Area
            var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
            var haRT = (RectTransform)handleArea.transform;
            haRT.SetParent(sRT, false);
            haRT.anchorMin = new Vector2(0, 0.5f); haRT.anchorMax = new Vector2(1, 0.5f);
            haRT.pivot = new Vector2(0.5f, 0.5f);
            haRT.offsetMin = new Vector2(6, -10);
            haRT.offsetMax = new Vector2(-6, 10);

            // Handle
            var handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
            var hRT = (RectTransform)handle.transform;
            hRT.SetParent(haRT, false);
            hRT.sizeDelta = new Vector2(14, 14);
            var hi = handle.GetComponent<Image>();
            hi.color = Color.white; hi.raycastTarget = true;

            // Slider wire-up
            var s = sliderGO.GetComponent<Slider>();
            s.minValue = min; s.maxValue = max; s.value = start; s.wholeNumbers = true;
            s.transition = Selectable.Transition.None;
            s.targetGraphic = hi;
            s.fillRect = fRT;
            s.handleRect = hRT;
            s.direction = Slider.Direction.LeftToRight;
            s.onValueChanged.AddListener(v => onChanged?.Invoke(v));

            return s;
        }
    }
}
