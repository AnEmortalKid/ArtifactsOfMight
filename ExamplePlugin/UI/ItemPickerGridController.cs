using System;
using System.Collections.Generic;
using RoR2;
using UnityEngine;

namespace ArtifactsOfMight.UI
{
    /// <summary>
    /// Parent controller for operating on the picker squares
    /// </summary>
    public class ItemPickerGridController : MonoBehaviour
    {

        // Empty will be overridden
        private List<ItemPickerSquareController> squareControllers = new();

        public Action<PickupDef> OnSquareClicked;

        public void SetSquares(List<ItemPickerSquareController> squareControllers)
        {
            Log.Info($"Setting {squareControllers.Count} squares");

            this.squareControllers = squareControllers;

            foreach (var squareController in squareControllers)
            {
                squareController.OnSquareClicked += ForwardSquareClicked;
            }
        }

        public void RefreshSquares()
        {
            foreach (var squareController in squareControllers)
            {
                squareController.RefreshSquare();
            }
        }

        /// <summary>
        /// Do it this way so we don't take a reference to an unassigned action
        /// </summary>
        private void ForwardSquareClicked(PickupDef def)
        {
            OnSquareClicked?.Invoke(def);
        }
    }
}
