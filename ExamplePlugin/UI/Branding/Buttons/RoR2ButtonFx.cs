using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine;

namespace ExamplePlugin.UI.Branding.Buttons
{

    public class RoR2ButtonFx : MonoBehaviour,
     IPointerEnterHandler, IPointerExitHandler,
     IPointerDownHandler, IPointerUpHandler,
     ISelectHandler, IDeselectHandler
    {
        [Header("Targets")]
        public RectTransform fill;
        public UnityEngine.UI.Image fillImage;
        public CanvasGroup activeRim; // bright rim
        public CanvasGroup sheen;     // white overlay

        [Header("Insets (px)")]
        public float baseFillInset = 8f, hoverFillInset = 10f, pressedFillInset = 12f;

        [Header("Fill Colors")]
        public Color normalFill = Color.black;
        public Color hoverFill = new Color(0.12f, 0.12f, 0.12f);
        public Color pressedFill = new Color(0.20f, 0.20f, 0.20f);

        [Header("Sheen/Rim Alphas")]
        public float hoverAlpha = 0.28f;   // both sheen + active rim
        public float pressAlpha = 0.42f;

        [Header("Timing")]
        public float animTime = 0.07f;

        bool hovered, pressed, selected;
        Coroutine a, b, c, d;

        float GoalInset() => pressed ? pressedFillInset : (hovered || selected ? hoverFillInset : baseFillInset);
        float GoalAlpha() => pressed ? pressAlpha : (hovered || selected ? hoverAlpha : 0f);
        Color GoalFillCol() => pressed ? pressedFill : (hovered || selected ? hoverFill : normalFill);

        void SetInset(RectTransform rt, float px) { rt.offsetMin = new Vector2(px, px); rt.offsetMax = new Vector2(-px, -px); }

        IEnumerator LerpInset(RectTransform rt, float target)
        {
            var sMin = rt.offsetMin; var sMax = rt.offsetMax;
            var gMin = new Vector2(target, target); var gMax = new Vector2(-target, -target);
            float t = 0; while (t < 1)
            {
                t += Time.unscaledDeltaTime / animTime;
                float k = Mathf.SmoothStep(0, 1, t);
                rt.offsetMin = Vector2.Lerp(sMin, gMin, k); rt.offsetMax = Vector2.Lerp(sMax, gMax, k); yield return null;
            }
            rt.offsetMin = gMin; rt.offsetMax = gMax;
        }
        IEnumerator LerpAlpha(CanvasGroup cg, float target)
        {
            float s = cg.alpha, t = 0; while (t < 1)
            {
                t += Time.unscaledDeltaTime / animTime;
                cg.alpha = Mathf.Lerp(s, target, Mathf.SmoothStep(0, 1, t)); yield return null;
            }
            cg.alpha = target;
        }
        IEnumerator LerpColor(UnityEngine.UI.Image img, Color target)
        {
            var s = img.color; float t = 0; while (t < 1)
            {
                t += Time.unscaledDeltaTime / animTime;
                img.color = Color.Lerp(s, target, Mathf.SmoothStep(0, 1, t)); yield return null;
            }
            img.color = target;
        }

        void Animate()
        {
            if (fill) { if (a != null) StopCoroutine(a); a = StartCoroutine(LerpInset(fill, GoalInset())); }
            if (activeRim) { if (b != null) StopCoroutine(b); b = StartCoroutine(LerpAlpha(activeRim, GoalAlpha())); }
            if (sheen) { if (c != null) StopCoroutine(c); c = StartCoroutine(LerpAlpha(sheen, GoalAlpha())); }
            if (fillImage) { if (d != null) StopCoroutine(d); d = StartCoroutine(LerpColor(fillImage, GoalFillCol())); }
        }

        public void OnPointerEnter(PointerEventData e) {
            Log.Info("OnPointerEnter");
            hovered = true; Animate(); 
        }
        public void OnPointerExit(PointerEventData e) {
            Log.Info("OnPointerExit");
            hovered = false; Animate(); 
        }

        public void OnPointerDown(PointerEventData e) { pressed = true; Animate(); }
        public void OnPointerUp(PointerEventData e) { pressed = false; Animate(); }
        public void OnSelect(BaseEventData e) { selected = true; Animate(); }
        public void OnDeselect(BaseEventData e) { selected = false; Animate(); }
    }
}