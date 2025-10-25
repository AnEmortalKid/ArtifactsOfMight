using ArtifactsOfMight.UI.Drafting;
using UnityEngine;
using UnityEngine.UI;

namespace ArtifactsOfMight.UI.Branding.Panel
{
    public class GlassPanelContainer
    {
        /// <summary>
        /// Game object that contains the full panel hierarchy
        /// Contains a RectTransform
        /// </summary>
        public GameObject glassPanelHolder;

        /// <summary>
        /// The rect transform where content should be placed
        /// </summary>
        public RectTransform contentRoot;

        /// <summary>
        /// Reference to the background for the panel
        /// </summary>
        public Image primaryImage;

        /// <summary>
        /// Created when the Style is not none,
        /// incase you want to tint things
        /// </summary>
        public Image borderImage;        

        /// <summary>
        /// Remembers the computed padding after reflow
        /// </summary>
        
        public Vector4 AutoBorderPaddingLTBR = Vector4.zero;
        /// <summary>
        /// Remembers what our last padding passed in was
        /// </summary>
        public Vector4 CallerPaddingLTBR = Vector4.zero;

        /// <summary>
        /// Recalculates and applies padding offsets to the <see cref="contentRoot"/> based on
        /// the current border sprite and any caller-specified insets.
        ///
        /// This method combines the sprite’s intrinsic 9-slice border (if requested)
        /// with additional caller padding, then updates <see cref="RectTransform.offsetMin"/> 
        /// and <see cref="RectTransform.offsetMax"/> to inset or extend the content area.
        ///
        /// Typical usage:
        /// <code>
        /// glassPanel.Reflow(respectSpritePadding: true, callerLTBR: new Vector4(8, 8, 8, 10));
        /// </code>
        /// </summary>
        /// <param name="respectSpritePadding">
        /// If <c>true</c>, the method reads the current <see cref="borderImage"/>'s
        /// <see cref="Sprite.border"/> values (converted to UI units) and includes them
        /// as automatic insets. This respects 9-slice borders and ensures content does
        /// not overlap the rim.
        /// 
        /// If <c>false</c>, the sprite’s border values are ignored and only
        /// <paramref name="callerLTBR"/> is applied.
        /// </param>
        /// <param name="callerLTBR">
        /// Additional padding to apply on top of any sprite-derived border values,
        /// expressed as <c>(Left, Top, Right, Bottom)</c> in local UI units.
        /// Use this to add design-specific spacing between the border and content.
        /// </param>
        public void Reflow(bool respectSpritePadding, Vector4 callerLTBR)
        {
            AutoBorderPaddingLTBR = Vector4.zero;

            if (respectSpritePadding && borderImage && borderImage.sprite) { 
                // (L,T,R,B)
                AutoBorderPaddingLTBR = FactoryUtils.GetSpriteBorderAsUIPadding(borderImage.sprite);
            }

            CallerPaddingLTBR = callerLTBR;
            var total = AutoBorderPaddingLTBR + CallerPaddingLTBR;

            // Offsets: (L,B) / (R,T)
            contentRoot.offsetMin = new Vector2(+total.x, +total.w);
            contentRoot.offsetMax = new Vector2(-total.z, -total.y);
        }

    }
}
