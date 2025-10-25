using System;
using System.Collections.Generic;
using System.Text;
using ExamplePlugin.Loadout.Draft;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ExamplePlugin.UI.Drafting
{
    /// <summary>
    /// Responsible for displaying a summary of the item restrictions 
    /// across multiple tabs
    /// </summary>
    public class DraftSummaryBar : MonoBehaviour
    {
        /// <summary>
        ///  Const for no limit
        /// </summary>
        private const string INFINITY = "N/A";

        private Dictionary<DraftItemTier, TextMeshProUGUI> textAreasByTier = new();

        private Dictionary<DraftItemTier, Color> tintByTier = new();


        private Button randomizeButton;

        #region Events
        /// <summary>
        /// Fired when a click event is received on the randomize all button
        /// </summary>
        public Action OnRandomizeClick;
        #endregion

        public void BindTextArea(DraftItemTier draftTier, TextMeshProUGUI textArea, Color tierTint)
        {
            textAreasByTier[draftTier] = textArea;
            tintByTier[draftTier] = tierTint;
        }

        public void UpdateSummary()
        {
            // TODO make each pair of label do this but for now this is fine
            foreach (var tier in textAreasByTier.Keys)
            {
                var mode = DraftLoadout.Instance.GetMode(tier);

                var hex = ColorUtility.ToHtmlStringRGBA(tintByTier[tier]);
                var tierName = DraftTierLabels.GetUIName(tier);
                var colorBlock = $"<color=#{hex}>{tierName}:</color> <color=#FFFFFFFF>";

                var textUpdate = colorBlock;

                var limit = DraftLoadout.Instance.GetLimit(tier);
                var currCount = DraftLoadout.Instance.GetCount(tier);

                textUpdate += currCount.ToString() + " / " + limit;
                textUpdate += "</color>";

                textAreasByTier[tier].text = textUpdate;
            }
        }

        public void BindRandomizeButton(Button button)
        {
            this.randomizeButton = button;

            this.randomizeButton.onClick.AddListener(PropagateRandomizeClick);
        }

        private void PropagateRandomizeClick()
        {
            OnRandomizeClick?.Invoke();
        }
    }
}
