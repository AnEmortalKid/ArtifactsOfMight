using UnityEngine.AddressableAssets;
using UnityEngine;
using UnityEngine.UI;
using ExamplePlugin.UI.Drafting;

namespace ExamplePlugin.UI.Branding.Panel
{
    public static class GlassPanelFactory
    {
        /// <summary>
        /// Constructs a Panel/Container with a Glass/Transluscent like background
        /// 
        /// The panel is intended to be positioned via its anchor, and will be of the desired dimension
        /// </summary>
        /// <param name="parent">The parent that owns the panel</param>
        /// <param name="size">the desired size of the panel</param>
        /// <param name="useFlatRim">whether we use a border or not</param>
        /// <returns></returns>
        public static GlassPanelContainer BuildGlassBody(string panelName,
            RectTransform parent,
            Vector2 size,
            bool shouldBlockClicks = true,
            BorderOptions borderOptions = null)
        {
            // use default if not passed in
            borderOptions ??= new BorderOptions();

            // Root / Self Anchored
            var rootGO = new GameObject(panelName + "_GlassPanel", typeof(RectTransform));
            var root = (RectTransform)rootGO.transform;
            root.SetParent(parent, false);
            root.sizeDelta = size;
            root.anchorMin = root.anchorMax = new Vector2(0f, 1f);
            root.pivot = new Vector2(0f, 1f);

            // Our version of the glass paneling
            var primaryPanel = new GameObject("PrimaryPanel", typeof(RectTransform), typeof(Image));
            FactoryUtils.ParentToRectTransform(primaryPanel, root);
            var primaryPanelRT = primaryPanel.GetComponent<RectTransform>();
            FactoryUtils.StretchToFillParent(primaryPanelRT);

            // we do our effect with pure color and alpha
            var primaryPanelImg = primaryPanel.GetComponent<Image>();
            primaryPanelImg.sprite = null;
            // panel blocks
            primaryPanelImg.raycastTarget = shouldBlockClicks;
            primaryPanelImg.color = ColorPalette.GlassPaneBackgroundColor;

            Image borderImg = null;

            if (borderOptions.Style != BorderStyle.None)
            {
                var borderGO = new GameObject("BorderImage", typeof(RectTransform), typeof(Image));
                FactoryUtils.ParentToRectTransform(borderGO, root);
                var borderRT = (RectTransform)borderGO.transform;
                // aMin=(0,0) aMax=(1,1) oMin=(0,0) oMax=(0,0)
                borderRT.anchorMin = Vector2.zero;
                borderRT.anchorMax = Vector2.one;
                borderRT.pivot = new Vector2(0.5f, 0.5f);
                borderRT.offsetMin = Vector2.zero;
                borderRT.offsetMax = Vector2.zero;

                borderImg = borderGO.GetComponent<Image>();
                borderImg.type = Image.Type.Sliced;
                borderImg.fillCenter = borderOptions.FillCenter;
                borderImg.preserveAspect = false;
                borderImg.raycastTarget = false;

                switch (borderOptions.Style)
                {
                    case BorderStyle.Panel:
                        borderImg.sprite = Addressables
                            .LoadAssetAsync<Sprite>(borderOptions.PanelSpritePath).WaitForCompletion();
                        borderImg.color = borderOptions.Color; // blue-grey default
                        break;
                }
            }



            var contentGO = new GameObject("Contents", typeof(RectTransform));
            FactoryUtils.ParentToRectTransform(contentGO, primaryPanelRT);

            var contentRT = (RectTransform)contentGO.transform;

            contentRT.anchorMin = Vector2.zero;
            contentRT.anchorMax = Vector2.one;
            contentRT.pivot = new Vector2(0.5f, 0.5f);
            // left = x, top = y, right = z, bottom = w
            // the caller will padd with a utility
            contentRT.offsetMin = Vector2.zero;
            contentRT.offsetMax = Vector2.zero;


            var container = new GlassPanelContainer
            {
                glassPanelHolder = rootGO,
                contentRoot = contentRT,
                primaryImage = primaryPanelImg,
                borderImage = borderImg
            };

            return container;
        }

        public static void WithContentPadding(in GlassPanelContainer panel, float left, float right, float top, float bottom)
        {
            var rt = panel.contentRoot;
            rt.offsetMin = new Vector2(left, bottom);
            rt.offsetMax = new Vector2(-right, -top);
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

        static string TruncateKey(string s, int max)
            => s.Length <= max ? s : s.Substring(s.Length - max, max); // keep tail (file name)

        private static Vector4 GetSpriteBorderAsUIPadding(Sprite s)
        {
            // sprite.border = (L,B,R,T) in pixels
            float ppu = (s != null && s.pixelsPerUnit > 0f) ? s.pixelsPerUnit : 100f;
            var b = s ? s.border : Vector4.zero;
            float left = b.x / ppu;
            float bottom = b.y / ppu;
            float right = b.z / ppu;
            float top = b.w / ppu;
            // return (L,T,R,B) to match your Vector4 convention
            return new Vector4(left, top, right, bottom);
        }
    }
}
