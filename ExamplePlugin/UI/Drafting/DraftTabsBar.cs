using System;
using System.Collections.Generic;
using System.Text;
using ExamplePlugin.Loadout.Draft;
using ExamplePlugin.UI.Drafting.Tabs;
using UnityEngine;
using UnityEngine.UI;

namespace ExamplePlugin.UI.Drafting
{
    /// <summary>
    /// Component that handles the select tab buttons and notifies the listener of the draft request
    /// </summary>
    public class DraftTabsBar : MonoBehaviour
    {


        private Dictionary<DraftItemTier, DraftTabButton> buttonsForTier = new();

        public void RegisterButton(DraftItemTier draftItemTier, DraftTabButton button)
        {
            buttonsForTier[draftItemTier] = button;
        }

        public void SelectTab(DraftItemTier draftItemTier)
        {
            buttonsForTier[draftItemTier].MarkSelected();
        }

        public void UnselectTab(DraftItemTier draftItemTier)
        {
            buttonsForTier[draftItemTier].MarkUnselected();
        }

        /// <summary>
        /// OLD STUFF
        /// </summary>
        public Action<DraftItemTier> OnTabButtonClicked;

        /// <summary>
        ///  OLD stuff
        /// </summary>
        /// <param name="item"></param>
        /// <param name="button"></param>
        public void BindButton(DraftItemTier item, Button button)
        {
            // TODO clean this listener up
            button.onClick.AddListener(() =>
            {
                Log.Info($"[DraftTabsBar] {item}");
                OnTabButtonClicked?.Invoke(item);
            });
        }
    }
}
