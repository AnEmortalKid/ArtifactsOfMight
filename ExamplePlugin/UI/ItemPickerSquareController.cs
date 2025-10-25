using System;
using System.Diagnostics.CodeAnalysis;
using ExamplePlugin.Loadout;
using ExamplePlugin.Loadout.Draft;
using RoR2;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ExamplePlugin.UI
{
    /// <summary>
    /// Handles logic between our ItemPickerSquare display and its behavior
    /// </summary>
    public class ItemPickerSquareController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private static Color DARK_GREY = new Color(0.25f, 0.25f, 0.25f, 1f);

        private Image background;
        private Image icon;
        private Outline hoverOutline;
        private GameObject lockOverlay;

        private Image selectionOverlayImg;

        private Image dimmingOverlayImg;

        private Image hoverOverlayImg;

        private bool isSelectionOn;

        /// <summary>
        /// The pickup definition this square represents
        /// </summary>
        private PickupDef pickupDef;


        /// <summary>
        /// The main button we attach our click handler to
        /// </summary>
        private Button button;


        /// <summary>
        /// Event fired when this square is clicked
        /// </summary>
        public Action<PickupDef> OnSquareClicked;

        public void BindComponents(PickupDef pickupDef, Button button, Image background, Image icon, Outline hoverOutline = null,
            GameObject lockOverlay = null, Image selectionOverlayImg = null,
            Image dimmingOverlayImg = null,
            Image hoverOverlayImg = null)
        {
            this.pickupDef = pickupDef;
            this.button = button;
            this.background = background;
            this.icon = icon;
            this.hoverOutline = hoverOutline;
            this.lockOverlay = lockOverlay;

            this.selectionOverlayImg = selectionOverlayImg;
            this.dimmingOverlayImg = dimmingOverlayImg;
            this.hoverOverlayImg = hoverOverlayImg;

            this.button.onClick.AddListener(HandleOnClick);

            ReflectState();
        }

        [SuppressMessage("CodeQuality", "IDE0051", Justification = "MonoBehavior lifecycle")]
        void OnDestroy()
        {
            this.button.onClick.RemoveListener(HandleOnClick);
        }

        private void HandleOnClick()
        {
            Log.Info($"[DraftArtifact]ItemPickerSquareController.HandleOnClick: {pickupDef.nameToken}");
            OnSquareClicked?.Invoke(pickupDef);
        }

        /// <summary>
        /// Tells this square controller to refresh its visual state
        /// </summary>
        public void RefreshSquare()
        {
            ReflectState();
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            if (hoverOutline != null)
            {
                hoverOutline.enabled = true;
            }
            if(hoverOverlayImg != null)
            {
                hoverOverlayImg.enabled = true;
            }

            if (!isSelectionOn && !DraftLoadout.Instance.IsLocked(pickupDef))
            {
                // turn dimmer off 
                if(dimmingOverlayImg != null)
                {
                    dimmingOverlayImg.enabled = false;
                }
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if(hoverOutline != null)
            {
                hoverOutline.enabled = false;
            }
            if (hoverOverlayImg != null)
            {
                hoverOverlayImg.enabled = false;
            }

            if (!isSelectionOn && !DraftLoadout.Instance.IsLocked(pickupDef))
            {
                // turn dimmer back on 
                if (dimmingOverlayImg != null)
                {
                    dimmingOverlayImg.enabled = true;
                }
            }
        }

        private void ReflectState()
        {
            if (DraftLoadout.Instance.IsLocked(pickupDef))
            {
                if (lockOverlay != null)
                {
                    lockOverlay.SetActive(true);
                }

                isSelectionOn = false;

                // if it was previously selected before it gets locked, hide state
                if (selectionOverlayImg != null)
                {
                    selectionOverlayImg.enabled = false;
                }

                return;
            }
            
            // not locked so reflect possible state
            lockOverlay.SetActive(false);

            // Then when its unselected we hide outline and gray out the icon
            var currentlyPicked = DraftLoadout.Instance.IsPicked(pickupDef);
            if (currentlyPicked)
            {
                //background.color = Color.white;
                //icon.color = Color.white;
                if (hoverOutline != null)
                {
                    hoverOutline.enabled = true;
                }

                if (selectionOverlayImg != null)
                {
                    selectionOverlayImg.enabled = true;
                }

                if(dimmingOverlayImg != null)
                {
                    dimmingOverlayImg.enabled = false;
                }

                isSelectionOn = true;
            }
            else
            {
                if (dimmingOverlayImg != null)
                {
                    dimmingOverlayImg.enabled = true;
                }
                else
                {
                    icon.color = DARK_GREY;
                    background.color = DARK_GREY;
                }

                if (hoverOutline != null)
                {
                    hoverOutline.enabled = false;
                }

                if (selectionOverlayImg != null)
                {
                    selectionOverlayImg.enabled = false;
                }

                isSelectionOn = false;
            }
        }
    }
}
