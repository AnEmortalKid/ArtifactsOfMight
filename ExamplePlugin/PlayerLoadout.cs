using System;
using System.Collections.Generic;
using System.Linq;
using ExamplePlugin.Loadout;
using Newtonsoft.Json;
using RoR2;
using UnityEngine.Networking;

namespace ExamplePlugin
{
    /// <summary>
    /// How the server understands our item selections and limits
    /// </summary>
    [Serializable]
    public struct PlayerLoadout
    {

        public Dictionary<ItemTier, TierLimit> byTier = [];

        public PlayerLoadout()
        {
        }

        public override string ToString()
        {
            return Summarize(this);
        }

        public static string Summarize(PlayerLoadout l)
        {
            var parts = new List<string>();
            foreach (KeyValuePair<ItemTier, TierLimit> keyValuePair in l.byTier)
            {

                var tier = keyValuePair.Key;
                var limit = keyValuePair.Value;
                switch (limit.mode)
                {
                    case TierLimitMode.None:
                        parts.Add($"{tier}=None");
                        break;
                    case TierLimitMode.Restricted:
                        parts.Add($"{tier}=[{string.Join(",", limit.allowed)}]");
                        parts.Add($",restricted=[{string.Join(",", limit.restrictedByVoid)}]");
                        break;
                }
            }

            return string.Join("; ", parts);
        }
    }
}
