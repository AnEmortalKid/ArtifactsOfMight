using System;
using System.Collections.Generic;
using System.Text;

namespace ArtifactsOfMight.Loadout
{
    /// <summary>
    /// Defines the possible tier limit options
    /// </summary>
    public enum TierLimitMode : byte
    {
        /// <summary>
        /// No limit has been set for this item tier
        /// </summary>
        //None = 0,

        /// <summary>
        /// Only a certain set of items is allowed
        /// </summary>
        Restricted = 1
    }
}
