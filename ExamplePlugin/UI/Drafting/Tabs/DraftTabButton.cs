using System;
using System.Collections.Generic;
using System.Text;
using ExamplePlugin.Loadout.Draft;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ExamplePlugin.UI.Drafting.Tabs
{
    /// <summary>
    /// Handles the mouse events and selection state for our buttons that serve as tab switching buttons
    /// 
    /// This closely mirrors the behavior of the survivor info panel (Overview, Skills, Loadout) and our buttons have:
    /// 
    /// - An outline and color tint on hover (brighten)
    /// - A selected state outline, color tint and text tint
    /// 
    /// This button does not track its selection state, instead it relies on something marking it as Selected/Unselected
    /// </summary>
    public class DraftTabButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {

        #region Components
        /// <summary>
        /// The image that represents the background for the button
        /// </summary>
        private Image buttonBackground;

        private Image onHoverOutline;

        /// <summary>
        /// The box that adds an overlay
        /// </summary>
        private Image selectOverlay;
        /// <summary>
        /// The external outline
        /// </summary>
        private Image selectBorder;

        /// <summary>
        /// The colors to use
        /// </summary>
        private DraftTabColors tabColors;
        #endregion

        /// <summary>
        /// Tracks whether we are the selected element or not, before we show the hover incorrectly
        /// </summary>
        private bool isSelected;

        private bool isHovered;

        private DraftItemTier itemTier;

        public Action<DraftItemTier> OnTabButtonClicked;

        #region BindComponents
        public void BindHoverElements(Image hoverOutline)
        {
            this.onHoverOutline = hoverOutline;
        }

        /// <summary>
        /// Binds the elements that should be controled through selection
        /// </summary>
        /// <param name="selectOverlay">the overlay that darkens the image</param>
        /// <param name="selectBorder">The nine slice that goes outside of the button</param>
        public void BindSelectionElements(Image selectOverlay, Image selectBorder)
        {
            this.selectOverlay = selectOverlay;
            this.selectBorder = selectBorder;
        }

        public void BindButtonAction(DraftItemTier tier, Button button)
        {
            this.itemTier = tier;

            // TODO deregister
            button.onClick.AddListener(FireClickEvent);
        }

        public void BindBackground(Image buttonBackground)
        {
            this.buttonBackground = buttonBackground;
        }

        public void BindColors(DraftTabColors tabColors)
        {
            this.tabColors = tabColors;
        }
        #endregion


        public void MarkSelected()
        {
            isSelected = true;

            selectOverlay.enabled = true;
            selectBorder.enabled = true;

            buttonBackground.color = this.tabColors.SelectedTint;

            // disable hover outline
            onHoverOutline.enabled = false;
        }

        public void MarkUnselected()
        {
            isSelected = false;

            selectOverlay.enabled = false;
            selectBorder.enabled = false;

            buttonBackground.color = this.tabColors.NormalTint;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isHovered = true;

            if (!isSelected)
            {
                onHoverOutline.enabled = true;
                buttonBackground.color = this.tabColors.HoverTint;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHovered = false;

            if (!isSelected)
            {
                onHoverOutline.enabled = false;
                buttonBackground.color = this.tabColors.NormalTint;
            }
        }

        private void FireClickEvent()
        {
            OnTabButtonClicked?.Invoke(itemTier);
        }
    }
}
