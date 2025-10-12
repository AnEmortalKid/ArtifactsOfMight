using System;
using System.Collections.Generic;
using System.Text;
using ExamplePlugin.Loadout.Draft;

namespace ExamplePlugin.UI.Drafting
{
    public static class DraftTierLabels
    {


        /// <summary>
        /// Returns a user friendly display version of the draft tier
        /// </summary>
        /// <param name="draftItemTier"></param>
        /// <returns></returns>
        public static string GetUIName(DraftItemTier draftItemTier)
        {
            switch (draftItemTier)
            {
                case DraftItemTier.White:
                    return "Common";
                case DraftItemTier.Green:
                    return "Uncommon";
                case DraftItemTier.Red:
                    return "Legendary";
                case DraftItemTier.Yellow:
                    return "Boss";
                case DraftItemTier.Purple:
                    return "Void";
                default:
                    return nameof(draftItemTier);
            }
        }
    }
}
