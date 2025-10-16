using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ExamplePlugin.UI.Utils
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
                        r = $"RT aMin={rt.anchorMin} aMax={rt.anchorMax} piv={rt.pivot} pos={rt.anchoredPosition} size={rrect.size}";
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
    }
}
