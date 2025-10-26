using System.Collections.Generic;
using ArtifactsOfMight.Messages;
using R2API.Networking.Interfaces;
using RoR2;
using UnityEngine.Networking;
using System;
using ArtifactsOfMight.Loadout;
using ArtifactsOfMight.Loadout.Draft;

namespace ArtifactsOfMight
{
    /// <summary>
    /// Responsible for sending our loadout to the server
    /// </summary>
    public static class ClientLoadoutSender
    {
        public static void SendNow()
        {
            if (!NetworkClient.active)
            {
                return;
            }

            var local = LocalUserManager.GetFirstLocalUser();
            var user = local.currentNetworkUser;
            var netId = user ? user.netId : NetworkInstanceId.Invalid;

            Log.Info($"ClientLoadoutSender Sending loadout for {user.netId} | {user.name}");
         

            var loadout = DraftLoadout.Instance.ToPlayerLoadout();
            Log.Info($"ClientLoadoutSender loadout: {loadout}.");
            new LoadoutSyncMsg
            {
                senderNetId = netId,
                loadout = loadout
            }.Send(R2API.Networking.NetworkDestination.Server);
            Log.Info("ClientLoadoutSender Done sending loadout");
        }
    }
}
