using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ArtifactsOfMight.UI.Drafting.TierTab
{
    /// <summary>
    /// Handles highlighting the dice icon on the shuffle tab button
    /// </summary>
    public class ShuffleTabDiceHighlighter : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler, IPointerUpHandler
    {
        private Image target;
        private Color baseColor = new(1, 1, 1, 0.12f);
        private Color hoverColor = new(1, 1, 1, 0.20f);
        private Color pressedColor = new(1, 1, 1, 0.18f);
        private float fadeSeconds = 0.12f;

        Coroutine _fade;

        public void Initialize(Image targetImg, Color baseCol, Color hoverCol, Color pressedCol, float fade = 0.12f)
        {
            target = targetImg;
            baseColor = baseCol;
            hoverColor = hoverCol;
            pressedColor = pressedCol;
            fadeSeconds = fade;

            if (target) target.color = baseColor;
        }

        void OnEnable()
        {
            if (target) target.color = baseColor;
        }

        public void OnPointerEnter(PointerEventData e) => FadeTo(hoverColor);
        public void OnPointerExit(PointerEventData e) => FadeTo(baseColor);
        public void OnPointerDown(PointerEventData e) => FadeTo(pressedColor);
        public void OnPointerUp(PointerEventData e) => FadeTo(hoverColor);

        void FadeTo(Color to)
        {
            if (!target) return;
            if (_fade != null) StopCoroutine(_fade);
            _fade = StartCoroutine(FadeRoutine(target.color, to, fadeSeconds));
        }

        IEnumerator FadeRoutine(Color from, Color to, float t)
        {
            float elapsed = 0f;
            while (elapsed < t)
            {
                elapsed += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(elapsed / t);
                target.color = Color.Lerp(from, to, k);
                yield return null;
            }
            target.color = to;
        }
    }
}
