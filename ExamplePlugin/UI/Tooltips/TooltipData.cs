using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ArtifactsOfMight.UI.Tooltips
{
    [System.Serializable]
    public struct TooltipData
    {
        /// <summary>
        /// Desired color for the header
        /// </summary>
        public Color HeaderColor;

        /// <summary>
        /// Title for the tooltip
        /// </summary>
        public string Title;

        /// <summary>
        /// Text contents for the tootlip
        /// </summary>
        public string Body;
    }
}
