using System;
using System.Collections.Generic;
using System.Text;
using ExamplePlugin.Loadout.Draft;
using TMPro;
using UnityEngine;

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


        public void BindTextArea(DraftItemTier draftTier, TextMeshProUGUI textArea)
        {
            textAreasByTier[draftTier] = textArea;
        }
        
        public void UpdateSummary()
        {
            // TODO make each pair of label do this but for now this is fine
            foreach(var tier  in textAreasByTier.Keys)
            {
                var mode = DraftLoadout.Instance.GetMode(tier);

                var textUpdate = DraftTierLabels.GetUIName(tier) + ": ";
                //if(mode == DraftLimitMode.None)
                //{
                //    textUpdate += INFINITY;
                //}
                //if(mode != DraftLimitMode.None)
                //{

                // mode is always restricted

                    var limit = DraftLoadout.Instance.GetLimit(tier);
                    var currCount = DraftLoadout.Instance.GetCount(tier);

                    textUpdate += currCount.ToString() + " / " + limit;
                //}

               textAreasByTier[tier].text = textUpdate;
            }
        }
    }
}
