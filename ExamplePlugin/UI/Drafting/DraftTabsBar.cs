using System;
using System.Collections.Generic;
using System.Text;
using ExamplePlugin.Loadout.Draft;
using UnityEngine;
using UnityEngine.UI;

namespace ExamplePlugin.UI.Drafting
{
    /// <summary>
    /// Component that handles the select tab buttons and notifies the listener of the draft request
    /// </summary>
    public class DraftTabsBar : MonoBehaviour
    {

        public Action<DraftItemTier> OnTabButtonClicked;

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
