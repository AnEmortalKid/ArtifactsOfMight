using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ArtifactsOfMight.UI.Branding.Panel
{
    /// <summary>
    /// Encompasses the options for our glass panel
    /// </summary>
    public sealed class BorderOptions
    {
        public BorderStyle Style = BorderStyle.Panel;

        // Common
        public Color Color = new Color(0.363f, 0.376f, 0.472f, 1f); // Panel default tint
        public bool RespectSpritePadding = true;  // if true, use sprite.border as content padding

        public readonly string PanelSpritePath = "RoR2/Base/UI/texUIHighlightHeader.png";
        public readonly string TooltipSpritePath = "RoR2/Base/UI/texUIOutlineOnly.png";

        // Some outline sprites have a transparent center; we keep it off by default.
        public bool FillCenter = false;
    }
}
