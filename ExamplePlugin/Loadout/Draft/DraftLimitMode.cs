using System;
using System.Collections.Generic;
using System.Text;

namespace ExamplePlugin.Loadout.Draft
{
    /// <summary>
    /// What kind of limit we have for a particular DraftTier
    /// </summary>
    public enum DraftLimitMode
    {
        ///// <summary>
        ///// A limit does not exist to be enforced, could be any number of items
        ///// </summary>
        //None, 
        /// <summary>
        /// Restrict count of items
        /// </summary>
        Restricted
    }
}
