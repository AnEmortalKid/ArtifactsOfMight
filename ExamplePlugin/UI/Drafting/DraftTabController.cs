using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RoR2;
using ExamplePlugin.Loadout;
using System.Diagnostics.CodeAnalysis;
using ExamplePlugin.Loadout.Draft;

namespace ExamplePlugin.UI.Drafting
{
    /// <summary>
    /// Component responsible for briding:
    ///  - The controls for a tab (restriction mode, limits, reroll)
    ///  - The Grid of squares that represents the state
    /// </summary>
    public class DraftTabController : MonoBehaviour
    {
        /// <summary>
        /// The component that handles grid operations for us
        /// 
        /// </summary>
        private ItemPickerGridController gridController;

        /// <summary>
        /// Component that handles the control interactions
        /// </summary>
        private DraftTabControls tabControls;


        // The data portion
        private DraftItemTier tabTier;

        #region Events
        /// <summary>
        /// Fired when a click occurs on an item square
        /// </summary>
        public Action<DraftItemTier, PickupDef> OnItemClicked;

        /// <summary>
        /// Fired when the mode toggle is clicked
        /// </summary>
        public Action<DraftItemTier, bool> OnModeChangeRequest;

        /// <summary>
        /// Fired when the limit should be increased
        /// </summary>
        public Action<DraftItemTier> OnLimitIncreaseRequest;

        /// <summary>
        /// Fired when the limit should be decreased
        /// </summary>
        public Action<DraftItemTier> OnLimitDecreaseRequest;

        /// <summary>
        /// Fired when the randomize button is pressed
        /// </summary>
        public Action<DraftItemTier> OnRandomizeTabRequest;
        #endregion

        public void SetTabTier(DraftItemTier tabTier)
        {
            this.tabTier = tabTier;
        }

        public void SetGridController(ItemPickerGridController gridController)
        {
            this.gridController = gridController;
            this.gridController.OnSquareClicked += OnSquareClicked;
        }

        public void SetControls(DraftTabControls controls)
        {
            tabControls = controls;

            tabControls.OnRestrictModeChanged += PropagateChangeMode;
            tabControls.OnIncreaseItemLimit += PropagateIncreaseLimit;
            tabControls.OnDecreaseItemLimit += PropagateDecreaseLimit;
            tabControls.OnRandomizeTier += PropagateRandomizeTab;
        }

        /// <summary>
        /// Requests that this controller refreshes due to a limit change
        /// </summary>
        public void RefreshLimit()
        {
            UpdateItemLabels();
            this.gridController.RefreshSquares();
        }

        /// <summary>
        /// Requests that this controller refreshes its item state due to a change in items
        /// </summary>
        public void RefreshItems()
        {
            this.gridController.RefreshSquares();

            UpdateItemLabels();
        }

        #region EventHandlers
        private void OnSquareClicked(PickupDef pickup)
        {
            OnItemClicked?.Invoke(tabTier, pickup);
        }

        private void PropagateIncreaseLimit()
        {
            OnLimitIncreaseRequest?.Invoke(tabTier);
        }

        private void PropagateDecreaseLimit()
        {
            OnLimitDecreaseRequest?.Invoke(tabTier);
        }

        private void PropagateChangeMode(bool isRestricted)
        {
            OnModeChangeRequest?.Invoke(tabTier, isRestricted);
        }

        private void PropagateRandomizeTab()
        {
            OnRandomizeTabRequest?.Invoke(tabTier);
        }
        #endregion

        private void UpdateItemLabels()
        {
            var currLimit = DraftLoadout.Instance.GetLimit(tabTier);
            tabControls.SetItemLimitCount(currLimit);

            var currChosen = DraftLoadout.Instance.GetCount(tabTier);
            tabControls.SetSelectionState(currChosen, currLimit);
        }
    }
}
