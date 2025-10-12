using System.Collections.Generic;
using ExamplePlugin.Messages;
using R2API.Networking.Interfaces;
using RoR2;
using UnityEngine.Networking;
using System;
using ExamplePlugin.Loadout;
using ExamplePlugin.Loadout.Draft;

namespace ExamplePlugin
{
    /// <summary>
    /// Responsible for sending our loadout to the server
    /// </summary>
    public static class ClientLoadoutSender
    {

        private static readonly Random rng = new Random();

        public static PlayerLoadout MakeRandomTriColor()
        {
            // Make dictionary by tier then rando pick
            Dictionary<ItemTier, List<ItemIndex>> itemsByCategory = new();
            foreach (var itemDef in ItemCatalog.allItemDefs)
            {
                Log.Info($"[DraftArtifact] Processing {itemDef}");
                if (itemDef != null)
                {
                    if (!itemsByCategory.TryGetValue(itemDef.tier, out var list))
                    {
                        list = new List<ItemIndex>();
                        itemsByCategory[itemDef.tier] = list;
                    }
                    list.Add(itemDef.itemIndex);
                }
            }

            Log.Info($"[DraftArtifact] Dictionary Keys: {itemsByCategory.Keys}");

            ItemIndex pick(ItemTier tier)
            {
                var itemList = itemsByCategory[tier];
                if (itemList != null)
                {
                    return itemList[rng.Next(itemList.Count)];
                }

                return ItemIndex.None;
            }

            var loadout = new PlayerLoadout();

            var whiteLimit = new Loadout.TierLimit
            {
                mode = Loadout.TierLimitMode.Restricted,
                tier = ItemTier.Tier1,
                allowed = new()
            };

            whiteLimit.allowed.Add(pick(ItemTier.Tier1));
            whiteLimit.allowed.Add(pick(ItemTier.Tier1));
            whiteLimit.allowed.Add(pick(ItemTier.Tier1));

            loadout.byTier.Add(whiteLimit.tier, whiteLimit);

            return loadout;
        }

        public static void SendNow()
        {
            if (!NetworkClient.active)
            {
                return;
            }

            var local = LocalUserManager.GetFirstLocalUser();
            var user = local.currentNetworkUser;
            var netId = user ? user.netId : NetworkInstanceId.Invalid;


            Log.Info($"[DraftArtifact] Client sending loadout for {user.netIdentity}");

            var loadout = DraftLoadout.Instance.ToPlayerLoadout();
            new LoadoutSyncMsg
            {
                senderNetId = netId,
                loadout = loadout
            }.Send(R2API.Networking.NetworkDestination.Server);

            Log.Info("[DraftArtifact] Client sent loadout (tri-color test).");
        }
    }
}
