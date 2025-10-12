using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExamplePlugin.Loadout;
using ExamplePlugin.Loadout.Draft;
using R2API.Networking.Interfaces;
using RoR2;
using UnityEngine.Networking;
using UnityEngine.UIElements.Collections;

namespace ExamplePlugin.Messages
{
    /// <summary>
    /// Message used by clients to send their loadout to the server
    /// 
    /// Note: Loopback optimization will bypass serialize/deserialize when you're the server
    /// </summary>
    public class LoadoutSyncMsg : INetMessage
    {

        public NetworkInstanceId senderNetId;
        public PlayerLoadout loadout;

        public void Serialize(NetworkWriter writer)
        {
            writer.Write(senderNetId);

            foreach (DraftItemTier draftTier in GetOrderedTiers())
            {
                var nativeTiers = DraftTierMaps.ToNativeSet(draftTier);
                foreach (var nativeTier in nativeTiers)
                {
                    var limitForNativeTier = loadout.byTier.Get(nativeTier);
                    SerializeTier(writer, limitForNativeTier);
                }
            }
        }

        public void Deserialize(NetworkReader reader)
        {
            senderNetId = reader.ReadNetworkId();

            var readLoadout = new PlayerLoadout();

            foreach (DraftItemTier draftTier in GetOrderedTiers())
            {
                var nativeTiers = DraftTierMaps.ToNativeSet(draftTier);
                foreach (var nativeTier in nativeTiers)
                {
                    var tierLimit = DeserializeTier(reader, nativeTier);
                    readLoadout.byTier.Add(nativeTier, tierLimit);
                }
            }

            // Re-assign
            this.loadout = readLoadout;
        }

        public void OnReceived()
        {
            if (!NetworkServer.active)
            {
                return;
            }


            var sender = NetworkUser.readOnlyInstancesList.FirstOrDefault(nu => nu != null && nu.netId == senderNetId);


            if (sender != null)
            {
                Log.Info($"[DraftArtifact] Stored loadout for {sender.userName}: {loadout}");
                ServerLoadoutRegistry.SetFor(sender, loadout);
            }
            else
            {
                Log.Warning($"[DraftArtifact] LoadoutSync: unknown sender netId={senderNetId}");
            }
        }

        private void SerializeTier(NetworkWriter writer, TierLimit nativeTier)
        {
            // first byte is our mode
            writer.Write((byte)nativeTier.mode);

            // nothing elsse is written
            if (nativeTier.mode == TierLimitMode.None)
            {
                return;
            }


            // selected count and data
            writer.Write((int)nativeTier.allowed.Count);

            // then write each item index probably need to sort or something
            foreach (ItemIndex ii in nativeTier.allowed)
            {
                writer.Write((int)ii);
            }

            // restricted count and data
            writer.Write((int)nativeTier.restrictedByVoid.Count);
            foreach (ItemIndex ii in nativeTier.restrictedByVoid)
            {
                writer.Write((int)ii);
            }
        }

        private TierLimit DeserializeTier(NetworkReader reader, ItemTier nativeTier)
        {
            var tierMode = (TierLimitMode)reader.ReadByte();


            var tierLimit = new TierLimit();
            tierLimit.tier = nativeTier;
            tierLimit.mode = tierMode;
            tierLimit.allowed.Clear();
            tierLimit.restrictedByVoid.Clear();
            
            // nothing else is written
            if (tierMode == TierLimitMode.None)
            {
                return tierLimit;
            }

            // handle restricted logic
            var allowedCount = reader.ReadInt32();
            for (int i = 0; i < allowedCount; i++)
            {
                tierLimit.allowed.Add((ItemIndex)reader.ReadInt32());
            }

            var restrictedCount = reader.ReadInt32();
            for (int r = 0; r < restrictedCount; r++)
            {
                tierLimit.restrictedByVoid.Add((ItemIndex)reader.ReadInt32());
            }

            return tierLimit;
        }

        /// <summary>
        /// A manually sorted array of DraftItemTiers for constant serialization/deserialization order
        /// 
        /// Opted for this instead of GetEnumValues so the order is consistent in case we ever change the enum
        /// </summary>
        /// <returns></returns>
        private static DraftItemTier[] GetOrderedTiers()
        {
            return [
            DraftItemTier.White,DraftItemTier.Green,DraftItemTier.Red,DraftItemTier.Yellow,DraftItemTier.Purple
        ];
        }
    }
}
