using System;
using System.Collections.Generic;
using System.Text;
using RoR2.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArtifactsOfMight.UI.Utils
{
    public static class HierarchyDumper
    {

        public static string DumpActiveHierarchy(string title, int maxDepth = 6)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"==== {title} ====");

            void Recurse(Transform t, int depth)
            {
                if (t == null || depth > maxDepth) return;

                string Ind(int d) => new string(' ', d * 2);
                try
                {
                    var go = t.gameObject;
                    var rt = t as RectTransform;
                    var name = go ? go.name : "(null-go)";
                    var active = go ? go.activeInHierarchy : false;
                    var layer = go ? go.layer : -1;

                    string r;
                    if (rt != null)
                    {
                        var rrect = rt.rect;
                        r = $"RT aMin={rt.anchorMin} aMax={rt.anchorMax} piv={rt.pivot} pos={rt.anchoredPosition} size={rrect.size} oMin={rt.offsetMin} oMax={rt.offsetMax}";
                    }
                    else
                    {
                        r = $"T pos={t.position}";
                    }

                    sb.AppendLine($"{Ind(depth)}• {name}  (layer={layer}, active={active})  {r}");

                    // list critical components
                    var canvas = go.GetComponent<Canvas>();
                    var img = go.GetComponent<UnityEngine.UI.Image>();
                    var text = go.GetComponent<TMPro.TMP_Text>();
                    var mask = go.GetComponent<UnityEngine.UI.RectMask2D>();

                    if (canvas) sb.AppendLine($"{Ind(depth + 1)}Canvas sortingLayer={canvas.sortingLayerID} order={canvas.sortingOrder} override={canvas.overrideSorting} mode={canvas.renderMode}");
                    if (img) sb.AppendLine($"{Ind(depth + 1)}Image color={img.color} sprite={(img.sprite ? img.sprite.name : "null")} type={img.type}");
                    if (text) sb.AppendLine($"{Ind(depth + 1)}TMP text='{text.text}' color={text.color} size={text.fontSize}");
                    if (mask) sb.AppendLine($"{Ind(depth + 1)}RectMask2D PRESENT");

                    for (int i = 0; i < t.childCount; i++)
                        Recurse(t.GetChild(i), depth + 1);
                }
                catch (System.Exception ex)
                {
                    sb.AppendLine($"{Ind(depth)}(error on node): {ex.Message}");
                }
            }

            var rootCanvases = GameObject.FindObjectsOfType<Canvas>();
            foreach (var c in rootCanvases)
            {
                if (c.transform.parent == null) Recurse(c.transform, 0);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Logs the properties of a RectTransform
        /// </summary>
        /// <param name="name">the name of the object that owns the rect</param>
        /// <param name="rt">the rect transform</param>
        public static void LogRect(string name, RectTransform rt)
        {
            var r = rt.rect;
            Debug.Log($"[{name}] {r.width}x{r.height}  anchors {rt.anchorMin}->{rt.anchorMax}  pivot {rt.pivot}");
        }

        public static void TryDumpAllTooltips()
        {
            foreach (var ctrl in GameObject.FindObjectsOfType<TooltipController>(includeInactive: true))
            {
                Log.Info($"[TooltipSpy] ---- HOTKEY DUMP: {ctrl.name} activeSelf={ctrl.gameObject.activeSelf} ----");
                DumpRectTree(ctrl.transform, 0);
            }
        }

        static void DumpRectTree(Transform t, int depth)
        {
            var indent = new string(' ', depth * 2);
            var go = t.gameObject;
            var rt = go.GetComponent<RectTransform>();

            string line = $"{indent}• {go.name} (layer={go.layer}, active={go.activeSelf})";
            if (rt)
            {
                var size = rt.rect.size;
                line += $"  RT aMin=({rt.anchorMin.x:0.00}, {rt.anchorMin.y:0.00}) aMax=({rt.anchorMax.x:0.00}, {rt.anchorMax.y:0.00}) " +
                        $"piv=({rt.pivot.x:0.00}, {rt.pivot.y:0.00}) pos=({rt.anchoredPosition.x:0.00}, {rt.anchoredPosition.y:0.00}) " +
                        $"size=({size.x:0.00}, {size.y:0.00}) oMin=({rt.offsetMin.x:0.00}, {rt.offsetMin.y:0.00}) oMax=({rt.offsetMax.x:0.00}, {rt.offsetMax.y:0.00})";
            }
            Log.Info(line);

            // If it has an Image, print sprite and type
            if (go.TryGetComponent<Image>(out var img))
            {
                var spr = img.sprite ? img.sprite.name : "null";
                Log.Info($"{indent}  Image color=RGBA({img.color.r:0.###}, {img.color.g:0.###}, {img.color.b:0.###}, {img.color.a:0.###}) sprite={spr} type={img.type}" +
                         $" fillCenter={img.fillCenter} ppuMul={img.pixelsPerUnitMultiplier:0.###} raycast={img.raycastTarget}");
                if (img.sprite)
                {
                    var b = img.sprite.border;
                    Log.Info($"{indent}  Sprite.border(px)=({b.x:0.#}, {b.y:0.#}, {b.z:0.#}, {b.w:0.#}) ppu={img.sprite.pixelsPerUnit:0.#}");
                }
                if (img.material) Log.Info($"{indent}  Material={img.material.name}");
            }

            // If it has TMP, print a quick summary
            if (go.TryGetComponent<TextMeshProUGUI>(out var tmp))
            {
                Log.Info($"{indent}  TMP text=\"{tmp.text}\" font={tmp.font?.name} size={tmp.fontSize:0.#} wrap={tmp.enableWordWrapping} overflow={tmp.overflowMode}");
            }

            // Layout data (useful when ContentSizeFitter / LayoutGroups are involved)
            if (go.TryGetComponent<ContentSizeFitter>(out var fit))
            {
                Log.Info($"{indent}  ContentSizeFitter H={fit.horizontalFit} V={fit.verticalFit}");
            }
            if (go.TryGetComponent<LayoutElement>(out var le))
            {
                Log.Info($"{indent}  LayoutElement min=({le.minWidth:0.#},{le.minHeight:0.#}) pref=({le.preferredWidth:0.#},{le.preferredHeight:0.#}) flexible=({le.flexibleWidth:0.#},{le.flexibleHeight:0.#})");
            }
            if (go.TryGetComponent<HorizontalOrVerticalLayoutGroup>(out var lg))
            {
                Log.Info($"{indent}  LayoutGroup pad=({lg.padding.left},{lg.padding.top},{lg.padding.right},{lg.padding.bottom}) " +
                         $"childCtrl=({lg.childControlWidth},{lg.childControlHeight}) childExpand=({lg.childForceExpandWidth},{lg.childForceExpandHeight})");
            }

            // Recurse
            for (int i = 0; i < t.childCount; i++)
                DumpRectTree(t.GetChild(i), depth + 1);
        }

    }
}
