using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using ArtifactsOfMight.UI.Drafting.TierTab;
using UnityEngine;
using UnityEngine.UI;

namespace ArtifactsOfMight.UI.Drafting.Summary
{



    /// <summary>
    /// Component that is just going to iterate over the colors of the dice
    /// </summary>
    public class GlobalRandomDiceColorFlicker : MonoBehaviour
    {

        private Image target;
        private float intervalSeconds = 1f;
        private float fadeSpeed = 3f;

        // internal state
        private int currentIndex;
        private int nextIndex;
        private float timer;
        private bool active;

        private Color[] palette = new[]
        {
          TierTabPalette.DiceColorsWhite.HighlightColor,
          TierTabPalette.DiceColorsGreen.HighlightColor,
          TierTabPalette.DiceColorsRed.HighlightColor,
          TierTabPalette.DiceColorsYellow.HighlightColor,
          TierTabPalette.DiceColorsPurple.HighlightColor
        };

        public void Initialize(Image target, float intervalSeconds = 1f, float fadeSpeed = 3f)
        {
            this.target = target;
            this.intervalSeconds = intervalSeconds;
            this.fadeSpeed = fadeSpeed;

            this.active = true;
        }

        [SuppressMessage("CodeQuality", "IDE0051", Justification = "MonoBehavior lifecycle")]
        void Update()
        {
            if (!active || !target)
            {
                return;
            }

            timer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(timer / intervalSeconds);

            // Smoothly fade between current and next color
            target.color = Color.Lerp(palette[currentIndex], palette[nextIndex], t);

            if (timer >= intervalSeconds)
            {
                timer = 0f;
                currentIndex = nextIndex;
                nextIndex = (nextIndex + 1) % palette.Length;
            }
        }

        [SuppressMessage("CodeQuality", "IDE0051", Justification = "MonoBehavior lifecycle")]
        void OnDestroy()
        {
            active = false;
        }
    }
}
