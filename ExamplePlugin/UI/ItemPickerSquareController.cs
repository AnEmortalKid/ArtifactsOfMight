using System;
using System.Diagnostics.CodeAnalysis;
using ExamplePlugin.Loadout;
using ExamplePlugin.Loadout.Draft;
using RoR2;
using UnityEngine;
using UnityEngine.UI;

namespace ExamplePlugin.UI
{
    /// <summary>
    /// Handles logic between our ItemPickerSquare display and its behavior
    /// </summary>
    public class ItemPickerSquareController : MonoBehaviour
    {
        private static Color DARK_GREY = new Color(0.25f, 0.25f, 0.25f, 1f);

        private Image background;
        private Image icon;
        private Outline outline;
        private GameObject lockOverlay;

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

        public void BindComponents(PickupDef pickupDef, Button button, Image background, Image icon, Outline uiOutline = null,
            GameObject lockOverlay = null)
        {
            this.pickupDef = pickupDef;
            this.button = button;
            this.background = background;
            this.icon = icon;
            this.outline = uiOutline;
            this.lockOverlay = lockOverlay;

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

        // TODO in restricted mode we need the outlines to call out selection
        // IN unrestricted mode we don't

        private void ReflectState()
        {
            if (DraftLoadout.Instance.IsLocked(pickupDef))
            {
                if (lockOverlay != null)
                {
                    lockOverlay.SetActive(true);
                }
            }
            else
            {
                lockOverlay.SetActive(false);
            }


            // Then display the pick styling
            // TODO we will have a styling state be returned and go off of that later

            // TODO styling lets have an outline for when selected of the color of the item
            // Then when its unselected we hide outline and gray out the icon
            var isAvailable = DraftLoadout.Instance.IsAvailable(pickupDef);
            if (isAvailable)
            {
                background.color = DARK_GREY;
                icon.color = Color.white;
                if (outline != null)
                {
                    outline.enabled = true;
                }
            }
            else
            {
                icon.color = DARK_GREY;
                background.color = DARK_GREY;
                outline.enabled = false;
            }
        }
    }
}
