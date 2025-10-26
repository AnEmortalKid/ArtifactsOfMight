using System;
using System.Collections.Generic;
using System.Text;
using R2API.Networking.Interfaces;
using UnityEngine.Networking;

namespace ArtifactsOfMight.Messages
{
    /// <summary>
    /// Server to clients request to ask for loadouts
    /// </summary>
    public class RequestLoadoutSyncMsg : INetMessage
    {
       

        public void OnReceived()
        {
            Log.Info("RequestLoadoutSyncMsg.OnReceived");

            if(!NetworkClient.active)
            {
                Log.Info("Received request for loadout, but client not active");
                return;
            }

            // Server should loopback
            ClientLoadoutSender.SendNow();
        }

        public void Serialize(NetworkWriter writer)
        {
            // intentionally write nothing
        }

        public void Deserialize(NetworkReader reader)
        {
           // intentionally read nothing
        }
    }
}
