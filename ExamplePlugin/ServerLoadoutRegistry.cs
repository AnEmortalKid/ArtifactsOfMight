using System.Collections.Generic;
using UnityEngine.Networking;
using RoR2;

namespace ArtifactsOfMight
{
    /// <summary>
    /// Stores loadouts for the players on the server
    /// </summary>
    public static class ServerLoadoutRegistry
    {

        private static readonly Dictionary<NetworkInstanceId, PlayerLoadout> loadoutByUser = new();

        public static void SetFor(NetworkUser user, PlayerLoadout playerLoadout)
        {
            loadoutByUser[user.netId] = playerLoadout;
            Log.Info($"Stored loadout for [netId={user.netId}]: {playerLoadout}");
        }

        public static bool TryGetFor(NetworkUser user, out PlayerLoadout loadout)
        {
            if(user != null && loadoutByUser.TryGetValue(user.netId, out loadout))
            {
                return true;
            }

            // fallbacks
            loadout = default;
            return false;
        }
    }
}
