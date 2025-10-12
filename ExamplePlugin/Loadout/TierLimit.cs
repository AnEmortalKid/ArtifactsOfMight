using System;
using System.Collections.Generic;
using System.Text;
using RoR2;

namespace ExamplePlugin.Loadout
{
    /// <summary>
    /// Defines the limit of items for a given tier
    /// </summary>
    [Serializable]
    public struct TierLimit
    {
        /// <summary>
        /// The tier of items for this limit
        /// </summary>
        public ItemTier tier;

        /// <summary>
        /// How this limit should behave
        /// </summary>
        public TierLimitMode mode;

        /// <summary>
        /// What items are allowed, only used when mode == TierLimitMode.restricted;
        /// </summary>
        public HashSet<ItemIndex> allowed;

        /// <summary>
        /// What items are restricted until we have acquired their corrupted/void version
        /// </summary>
        public HashSet<ItemIndex> restrictedByVoid;
    }
}
