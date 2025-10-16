using System;
using System.Collections.Generic;
using System.Text;
using ExamplePlugin.UI.Drafting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace ExamplePlugin.UI.Branding.Inspection
{

   

    /// <summary>
    /// For sprite inspection purposes, load any assets from the asset wiki 
    /// just so we can see what they are on a grid
    /// </summary>
    public static class AssetDebugGridFactory
    {

        // Addressable keys to preview
        static readonly string[] Keys =
        {
            "RoR2/Base/UI/texUIAnimateHeaderGradient.png",
            "RoR2/Base/UI/texUIAnimateHeaderGradientInverted.png",
            "RoR2/Base/UI/texUIAnimateSlice1Colored.png",
            "RoR2/Base/UI/texUIAnimateSlice3.png",
            "RoR2/Base/UI/texUIAnimateSlice4.png",
            "RoR2/Base/UI/texUIAnimateSliceNakedButton.png",
            "RoR2/Base/UI/texUIBackdrop.png",
            "RoR2/Base/UI/texUIBackdropFadedEnds.png",
            "RoR2/Base/UI/texUIBottomUpFade.tga",
            "RoR2/Base/UI/texUICleanButton.png",
            "RoR2/Base/UI/texUICleanPanel.png",
            "RoR2/Base/UI/texUICombatHealthbar.png",
            "RoR2/Base/UI/texUICorner.png",
            "RoR2/Base/UI/texUICutOffCorner.png",
            "RoR2/Base/UI/texUIDifficultySegment.png",
            "RoR2/Base/UI/texUIDifficultySegmentFade.png",
            "RoR2/Base/UI/texUIHeaderDouble.png",
            "RoR2/Base/UI/texUIHeaderSingle.png",
            "RoR2/Base/UI/texUICornerSquared.png",
            "RoR2/Base/UI/texUICornerRounded.png",
            "RoR2/Base/UI/texUILaunchButton.png",
            "RoR2/Base/UI/texUILaunchButtonDepressed.png"
        };

        public static RectTransform BuildGlassBody(RectTransform parent, Vector2 size)
        {
            // Root
            var rootGO = new GameObject("GlassPanel", typeof(RectTransform));
            var root = (RectTransform)rootGO.transform;
            root.SetParent(parent, false);
            root.sizeDelta = size;
            root.anchorMin = root.anchorMax = new Vector2(0f, 1f);
            root.pivot = new Vector2(0f, 1f);

            // ---- Shadow (soft offset) ----
            var shadow = NewImage(root, "Shadow",
                Addressables.LoadAssetAsync<Sprite>("RoR2/Base/UI/texUIBackdrop.png").WaitForCompletion(),
                new Color(0f, 0f, 0f, 0.35f));
            Stretch(shadow, pad: 2f);      // grow a touch for softness
            shadow.anchoredPosition = new Vector2(0, -2);

            // ---- Backdrop (translucent plate) ----
            var plate = NewImage(root, "Backdrop",
                Addressables.LoadAssetAsync<Sprite>("RoR2/Base/UI/texUIBackdrop.png").WaitForCompletion(),
                new Color(1f, 1f, 1f, 0.58f));
            Stretch(plate, pad: 0f);

            // ---- Bottom vignette / color wash (per tier) ----
            var vignette = NewImage(root, "VignetteBottom",
                Addressables.LoadAssetAsync<Sprite>("RoR2/Base/UI/texUIBottomUpFade.tga").WaitForCompletion(),
                /* Purple example */ new Color32(110, 90, 169, 76)); // ~30% alpha
            vignette.anchorMin = new Vector2(0, 0);
            vignette.anchorMax = new Vector2(1, 0);
            vignette.pivot = new Vector2(0.5f, 0);
            vignette.sizeDelta = new Vector2(0, 48);
            vignette.anchoredPosition = Vector2.zero;
            vignette.GetComponent<Image>().preserveAspect = false;

            // ---- Optional: subtle top stripe/gradient sheen ----
            var topStripe = NewImage(root, "HeaderStripe",
                Addressables.LoadAssetAsync<Sprite>("RoR2/Base/UI/texUIAnimateHeaderGradientInverted.png").WaitForCompletion(),
                new Color(1f, 1f, 1f, 0.20f));
            topStripe.anchorMin = new Vector2(0, 1);
            topStripe.anchorMax = new Vector2(1, 1);
            topStripe.pivot = new Vector2(0.5f, 1);
            topStripe.sizeDelta = new Vector2(0, 26);
            topStripe.anchoredPosition = Vector2.zero;
            topStripe.GetComponent<Image>().preserveAspect = false;

            // ---- Border (thin, low alpha) ----
            var border = NewImage(root, "Border",
                Addressables.LoadAssetAsync<Sprite>("RoR2/Base/UI/texUIAnimateSlice3.png").WaitForCompletion(),
                new Color(1f, 1f, 1f, 0.18f), type: Image.Type.Sliced);
            Stretch(border, pad: 0f);

            // Ensure proper sibling order (shadow at back)
            shadow.SetAsFirstSibling();

            return root;
        }

        private static RectTransform NewImage(RectTransform parent, string name, Sprite sprite, Color color, Image.Type type = Image.Type.Simple)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.sprite = sprite;
            img.type = type;
            img.color = color;
            img.raycastTarget = false;
            return rt;
        }

        private static void Stretch(RectTransform rt, float pad)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = new Vector2(pad, pad);
            rt.offsetMax = new Vector2(-pad, -pad);
        }

        public static GameObject BuildTestGrid(RectTransform parentTransform)
        {
            // Root overlay (darken)
            var root = new GameObject("UITextureGrid", typeof(RectTransform), typeof(Image));
            FactoryUtils.ParentToRectTransform(root, parentTransform);
            var rootRT = (RectTransform)root.transform;
            FactoryUtils.StretchToFillParent(rootRT);

            var rootBG = root.GetComponent<Image>();
            rootBG.color = new Color(0f, 0f, 0f, 0.45f); // dimmer overlay
            rootBG.raycastTarget = true; // so clicks don't pass through when open

            // Container
            var container = new GameObject("GridContainer", typeof(RectTransform), typeof(Image));
            var contRT = (RectTransform)container.transform;
            contRT.SetParent(rootRT, false);
            contRT.sizeDelta = new Vector2(1024, 640);
            contRT.anchorMin = contRT.anchorMax = new Vector2(0.5f, 0.5f);
            contRT.pivot = new Vector2(0.5f, 0.5f);
            contRT.anchoredPosition = Vector2.zero;

            var contBG = container.GetComponent<Image>();
            contBG.color = new Color(0.10f, 0.12f, 0.14f, 0.95f);
            contBG.raycastTarget = true;

            // Viewport + ScrollRect
            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(RectMask2D));
            var viewportRT = (RectTransform)viewport.transform;
            viewportRT.SetParent(contRT, false);
            Stretch(viewportRT, new Vector2(10, 10), new Vector2(10, 10));
            viewport.GetComponent<Image>().color = new Color(0, 0, 0, 0.12f);
            viewport.GetComponent<Image>().raycastTarget = true;

            var scroll = container.AddComponent<ScrollRect>();
            scroll.viewport = viewportRT;
            scroll.horizontal = true;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Elastic;
            scroll.inertia = true;

            // Content (grid)
            var content = new GameObject("Content", typeof(RectTransform), typeof(GridLayoutGroup), typeof(ContentSizeFitter));
            var contentRT = (RectTransform)content.transform;
            contentRT.SetParent(viewportRT, false);
            contentRT.anchorMin = new Vector2(0, 1);
            contentRT.anchorMax = new Vector2(0, 1);
            contentRT.pivot = new Vector2(0, 1);
            contentRT.anchoredPosition = new Vector2(8, -8);

            var grid = content.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(160, 160);
            grid.spacing = new Vector2(10, 10);
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.UpperLeft;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 5;

            var fitter = content.GetComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.content = contentRT;

            // Populate cells
            foreach (var key in Keys)
            {
                var sprite = Addressables.LoadAssetAsync<Sprite>(key).WaitForCompletion();

                var cell = new GameObject(key, typeof(RectTransform), typeof(LayoutElement), typeof(Image));
                var cellRT = (RectTransform)cell.transform;
                cellRT.SetParent(contentRT, false);
                cellRT.sizeDelta = grid.cellSize;

                // checker bg so transparent sprites are visible
                var cellBG = cell.GetComponent<Image>();
                cellBG.color = new Color(0.18f, 0.20f, 0.23f, 1f);
                cellBG.raycastTarget = false;

                // sprite image
                var imgGO = new GameObject("Sprite", typeof(RectTransform), typeof(Image));
                var imgRT = (RectTransform)imgGO.transform;
                imgRT.SetParent(cellRT, false);
                imgRT.anchorMin = imgRT.anchorMax = new Vector2(0.5f, 0.5f);
                imgRT.pivot = new Vector2(0.5f, 0.5f);
                imgRT.sizeDelta = new Vector2(grid.cellSize.x - 16, grid.cellSize.y - 32); // leave room for label
                imgRT.anchoredPosition = new Vector2(0, 8);

                var img = imgGO.GetComponent<Image>();
                img.sprite = sprite;
                img.preserveAspect = true;
                img.raycastTarget = false;

                // label
                var labelGO = new GameObject("Label", typeof(RectTransform), typeof(Text));
                var labelRT = (RectTransform)labelGO.transform;
                labelRT.SetParent(cellRT, false);
                labelRT.anchorMin = new Vector2(0, 0);
                labelRT.anchorMax = new Vector2(1, 0);
                labelRT.pivot = new Vector2(0.5f, 0);
                labelRT.sizeDelta = new Vector2(0, 18);
                labelRT.anchoredPosition = new Vector2(0, 4);

                var txt = labelGO.GetComponent<Text>();
                txt.text = TruncateKey(key, 28);
                txt.alignment = TextAnchor.MiddleCenter;
                txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                txt.fontSize = 12;
                txt.color = new Color(0.88f, 0.90f, 0.95f, 0.95f);
            }

            return root;
        }

        // === helpers ===
        static void Stretch(RectTransform rt, Vector2 padMin, Vector2 padMax)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = new Vector2(padMin.x, padMin.y);
            rt.offsetMax = new Vector2(-padMax.x, -padMax.y);
        }

        static string TruncateKey(string s, int max)
            => s.Length <= max ? s : s.Substring(s.Length - max, max); // keep tail (file name)
    }
}
