using System;
using System.Collections.Generic;
using System.Text;

namespace ArtifactsOfMight.Loadout.Draft
{
    /// <summary>
    /// Defines the restrictions (if any) for a particular tier
    /// </summary>
    public class DraftTierRestrictionSettings
    {
        /// <summary>
        /// The current mode
        /// </summary>
        public DraftLimitMode Mode {  get; set; }

        /// <summary>
        /// The current allowed max
        /// Should be ignored when Mode is none
        /// </summary>
        public int Max { get; set; } = 0;

        /// <summary>
        /// Tracks whether we have set a max value on the first mode switch or not
        /// We use this to avoid re-setting the max when someone flip flops from restricted/none
        /// </summary>
        public bool HasSeededMax = false;
    }
}
