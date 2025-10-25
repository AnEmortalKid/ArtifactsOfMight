using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using ArtifactsOfMight.Loadout;
using RoR2;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArtifactsOfMight.UI
{
    /// <summary>
    /// Parent controller for operating on the picker squares
    /// </summary>
    public class ItemPickerGridController : MonoBehaviour
    {

        private List<ItemPickerSquareController> squareControllers;

        public Action<PickupDef> OnSquareClicked;

        public void SetSquares(List<ItemPickerSquareController> squareControllers)
        {
            this.squareControllers = squareControllers;

            foreach (var squareController in squareControllers)
            {
                squareController.OnSquareClicked += OnSquareClicked;
            }
        }

        public void RefreshSquares()
        {
            foreach (var squareController in squareControllers)
            {
                squareController.RefreshSquare();
            }
        }
    }
}
