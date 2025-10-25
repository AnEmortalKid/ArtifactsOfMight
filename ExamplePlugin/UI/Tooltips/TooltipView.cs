using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

namespace ArtifactsOfMight.UI.Tooltips
{
    public class TooltipView : MonoBehaviour
    {
        public RectTransform Rect => (RectTransform)transform;

        private Image headerBackground;
        private TMP_Text title;

        private TMP_Text body;
        

        public void Bind(Image headerBackground, TMP_Text title, TMP_Text body)
        {
            this.headerBackground = headerBackground;

            this.title = title;
            this.body = body;
        }

        public void SetData(TooltipData tooltipData)
        {

            if (headerBackground)
            {
                var desiredColor = tooltipData.HeaderColor;
                headerBackground.color = desiredColor;
            }

            if (title)
            {
                title.text = tooltipData.Title ?? "";
            }

            if (body)
            {
                body.text = string.IsNullOrEmpty(tooltipData.Body) ? "" : tooltipData.Body;
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(Rect);
        }
    }
}
