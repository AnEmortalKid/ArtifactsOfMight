using System;
using System.Collections.Generic;
using System.Linq;
using ArtifactsOfMight.Loadout;
using Newtonsoft.Json;
using RoR2;
using UnityEngine.Networking;

namespace ArtifactsOfMight
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
