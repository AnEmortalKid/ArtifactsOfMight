using System;
using System.Linq;
using BepInEx;
using R2API;
using RoR2;
using R2API.Networking;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using System.Collections.Generic;
using UnityEngine.XR;
using UnityEngine.UIElements.Collections;
using ArtifactsOfMight.UI;
using On.RoR2.UI;
using System.Diagnostics.CodeAnalysis;
using ArtifactsOfMight.Loadout.Corruption;
using ArtifactsOfMight.DraftArtifact.Game;
using static RoR2.Chat;
using ArtifactsOfMight.RunConfig;

namespace ArtifactsOfMight
{
    // This is an example plugin that can be put in
    // BepInEx/plugins/ExamplePlugin/ExamplePlugin.dll to test out.
    // It's a small plugin that adds a relatively simple item to the game,
    // and gives you that item whenever you press F2.

    // This attribute specifies that we have a dependency on a given BepInEx Plugin,
    // We need the R2API ItemAPI dependency because we are using for adding our item to the game.
    // You don't need this if you're not using R2API in your plugin,
    // it's just to tell BepInEx to initialize R2API before this plugin so it's safe to use R2API.
    [BepInDependency(ItemAPI.PluginGUID)]

    // This one is because we use a .language file for language tokens
    // More info in https://risk-of-thunder.github.io/R2Wiki/Mod-Creation/Assets/Localization/
    [BepInDependency(LanguageAPI.PluginGUID)]

    // We do things over the network
    [BepInDependency(NetworkingAPI.PluginGUID)]

    // This attribute is required, and lists metadata for your plugin.
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]


    // This is the main declaration of our plugin class.
    // BepInEx searches for all classes inheriting from BaseUnityPlugin to initialize on startup.
    // BaseUnityPlugin itself inherits from MonoBehaviour,
    // so you can use this as a reference for what you can declare and use in your plugin class
    // More information in the Unity Docs: https://docs.unity3d.com/ScriptReference/MonoBehaviour.html
    public class ArtifactsOfMight : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "AnEmortalKid";
        public const string PluginName = "ArtifactsOfMight";
        public const string PluginVersion = "0.1.0";

        // We need our item definition to persist through our functions, and therefore make it a class field.
        private static ItemDef myItemDef;

        private static ItemDef HoofDef;

        private DraftPickerUI DraftPickerUI;

        /// <summary>
        /// Unity lifecycle function, that we will use to hook our UI
        /// </summary>
        [SuppressMessage("CodeQuality", "IDE0051", Justification = "MonoBehavior lifecycle")]
        public void Awake()
        {
            // Init our logging class so that we can properly log for debugging
            Log.Init(Logger);
            Log.Info("Initializing via Awake");

            On.RoR2.UI.CharacterSelectController.ClientSetReady += OnClientSetReady;
            On.RoR2.UI.CharacterSelectController.Awake += CSC_Awake;
        }

        private void CSC_Awake(CharacterSelectController.orig_Awake orig, RoR2.UI.CharacterSelectController self)
        {
            orig(self);

            // Find SafeArea under CharacterSelectUIMain
            var canvas = self.GetComponentInChildren<Canvas>(true);
            if (!canvas)
            {
                Log.Warning("No Canvas under CSC.");
                return;
            }

            var safeArea = canvas.transform.Find("SafeArea") as RectTransform;
            if (!safeArea)
            {
                Log.Warning("SafeArea not found.");
                return;
            }

            if (!self.GetComponent<DraftPickerUI>())
            {
                var ui = self.gameObject.AddComponent<DraftPickerUI>();
                ui.SafeArea = safeArea;
                Log.Warning("DraftPickerUI attached to CharacterSelectController GameObject.");
            }
        }

        /// <summary>
        /// Ensure we can send our loadout on set read
        /// </summary>
        /// <param name="orig"></param>
        /// <param name="self"></param>
        private void OnClientSetReady(On.RoR2.UI.CharacterSelectController.orig_ClientSetReady orig, RoR2.UI.CharacterSelectController self)
        {
            orig(self);

            // Client-side only: fire loadout after you click Ready
            if (NetworkClient.active)
            {
                // Small delay so menus settle
                self.StartCoroutine(SendAfterDelay());
            }
        }

        private System.Collections.IEnumerator SendAfterDelay()
        {
            yield return new WaitForSecondsRealtime(0.05f);
            ClientLoadoutSender.SendNow();
        }


        /// <summary>
        /// Here we attach to the Game/Run side of the game
        /// </summary>
        [SuppressMessage("CodeQuality", "IDE0051", Justification = "MonoBehavior lifecycle")]
        public void OnEnable()
        {
            On.RoR2.PickupPickerController.OnInteractionBegin += PopulateOnOpen;
            On.RoR2.PickupPickerController.SetOptionsServer += OnSetOptionsServer;
        }


        /// <summary>
        /// Copy the array of original options for further filtering, just once
        /// </summary>
        /// <param name="orig"></param>
        /// <param name="self"></param>
        /// <param name="newOptions"></param>
        private void OnSetOptionsServer(On.RoR2.PickupPickerController.orig_SetOptionsServer orig, PickupPickerController self, PickupPickerController.Option[] newOptions)
        {
            if (NetworkServer.active)
            {
                // store our state for future re-evaluations
                if (!self.TryGetComponent<PickerState>(out var originalState))
                {
                    Log.Debug($"[DraftArtifact] Store original options for {self.netId} count {newOptions.Length}");
                    originalState = self.gameObject.AddComponent<PickerState>();
                    // make copy
                    originalState.originalOptions = newOptions.ToArray();
                }
            }

            orig(self, newOptions);
        }

        private void PopulateOnOpen(On.RoR2.PickupPickerController.orig_OnInteractionBegin orig, PickupPickerController self, Interactor interactor)
        {
            if (NetworkServer.active)
            {
                Log.Info($"[DraftArtifact] PopulateOnOpen for {interactor.netId}");
                var user = ResolveUser(interactor); // Interactor -> CharacterBody -> NetworkUser
                if (user != null)
                {
                    var options = BuildDraftOptionsFor(user, self);
                    ApplyOptionsServer(self, options); // server-authoritative push
                }

                // Now open the UI; clients will already have the right options
                orig(self, interactor);
                return;
            }

            // Client: just let vanilla open locally; options came from server.
            orig(self, interactor);
        }

        private void ApplyOptionsServer(PickupPickerController self, PickupPickerController.Option[] options)
        {
            Log.Info($"[DraftArtifact] ApplyOptionsServer");
            self.SetOptionsServer(options);
        }

        private NetworkUser ResolveUser(Interactor interactor)
        {
            if (interactor.TryGetComponent<RoR2.CharacterBody>(out var body))
            {
                var user = body.master?.playerCharacterMasterController?.networkUser;
                return user;
            }

            return null;
        }

        private PickupPickerController.Option[] BuildDraftOptionsFor(NetworkUser opener, PickupPickerController ppc)
        {
            if (ServerLoadoutRegistry.TryGetFor(opener, out PlayerLoadout playerLoadout))
            {
                Log.Info($"[DraftArtifact] BuildDraftOptionsFor using loadout: {playerLoadout}");

                // this should exist
                var originalState = ppc.GetComponent<PickerState>();

                // The ppc options keep changing when we override them on the server
                // Always evaluate from the original set
                var originalPPCOptions = originalState.originalOptions;

                // find tier sure hope its command
                var first = originalPPCOptions[0];
                var pickupTier = PickupCatalog.GetPickupDef(first.pickupIndex).itemTier;

                var limitDefByTier = playerLoadout.byTier.Get(pickupTier);
                //if (limitDefByTier.mode == Loadout.TierLimitMode.None)
                //{
                //    // bypass filtering
                //    return originalPPCOptions;
                //}

                Log.Info($"[DraftArtifact] Build filter for tier: {limitDefByTier}");

                // We're going to rebuild the list
                List<PickupPickerController.Option> filteredItems = new();

                var restrictedByCorrupt = limitDefByTier.restrictedByVoid;
                var tierAllowedItems = limitDefByTier.allowed;

                // Loop through the original options to preserve order
                foreach (var ppcOption in originalPPCOptions)
                {
                    var optionDef = PickupCatalog.GetPickupDef(ppcOption.pickupIndex);
                    var optionItemIndex = optionDef.itemIndex;

                    if (tierAllowedItems.Contains(optionItemIndex))
                    {
                        filteredItems.Add(ppcOption);
                        continue;
                    }

                    // check if we add or disable
                    if (restrictedByCorrupt.Contains(optionItemIndex))
                    {
                        Log.Info($"Checking restrict by corrupt for {optionDef.nameToken}");

                        var hasRequired = HasRequiredCorruptItem(opener, optionDef);

                        if (hasRequired)
                        {
                            filteredItems.Add(ppcOption);
                        }
                        else
                        {
                            // build a grayed out option
                            var grayOption = new PickupPickerController.Option
                            {
                                pickupIndex = ppcOption.pickupIndex,
                                available = false
                            };
                            filteredItems.Add(grayOption);
                        }

                        continue;
                    }

                    // allowed 
                    if (PickupPools.IsUndraftablePickup(optionItemIndex))
                    {
                        filteredItems.Add(ppcOption);
                    }
                }

                Log.Info($"[DraftArtifact] Loadout filtered item count: {filteredItems.Count}");
                return filteredItems.ToArray();
            }

            // fallback we shoulnd't have mutated this
            return ppc.options;
        }

        private void PPC_OnInteractBegin(On.RoR2.PickupPickerController.orig_OnInteractionBegin orig, PickupPickerController self, Interactor activator)
        {
            if (!NetworkServer.active)
            {
                return;
            }

            var ni = self.GetComponent<NetworkIdentity>();
            Log.Info($"[DraftArtifact] PPC_OnInteractBegin selfGO={self.gameObject.name} inst={self.gameObject.GetInstanceID()} netId={(ni ? ni.netId.ToString() : "none")}");

            if (NetworkServer.active && activator)
            {
                if (activator.TryGetComponent<RoR2.CharacterBody>(out var body))
                {
                    var user = body.master?.playerCharacterMasterController?.networkUser; // ← correct chain
                    if (user != null)
                    {
                        var tag = self.GetComponent<PickerOwnerTag>() ?? self.gameObject.AddComponent<PickerOwnerTag>();
                        tag.openerNetUserId = user.netId;
                        Log.Info($"[DraftArtifact] PPC_OnInteractBegin opener = {user.userName} ({user.netId})");
                        Log.Info($"[DraftArtifact] Assigned tag to {self.netId}");
                    }
                    else
                    {
                        Log.Info("[DraftArtifact] No NetworkUser from activator body.");
                    }
                }
                else
                {
                    Log.Info("[DraftArtifact] No CharacterBody on activator.");
                }
            }


            orig(self, activator);
        }

        public void OnDisable()
        {
            // On.RoR2.PickupPickerController.SetOptionsFromPickupForCommandArtifact -= SetOptions;
            On.RoR2.PickupPickerController.OnInteractionBegin -= PopulateOnOpen;
        }

        private void SetOptions(On.RoR2.PickupPickerController.orig_SetOptionsFromPickupForCommandArtifact orig, PickupPickerController self, PickupIndex pickupIndex)
        {
            // chill
            if (!NetworkServer.active)
            {
                return;
            }

            var ni = self.GetComponent<NetworkIdentity>();
            Log.Info($"[DraftArtifact] SetOptions selfGO={self.gameObject.name} inst={self.gameObject.GetInstanceID()} netId={(ni ? ni.netId.ToString() : "none")} pickup={pickupIndex}");


            RoR2.NetworkUser opener = null;
            var tag = self.GetComponent<PickerOwnerTag>();
            if (tag != null)
                opener = RoR2.NetworkUser.readOnlyInstancesList.FirstOrDefault(nu => nu.netId == tag.openerNetUserId);

            if (opener == null)
                Log.Info("[DraftArtifact] No opener on picker; using host default.");
            else
                Log.Info($"[DraftArtifact] has opener {opener.netId}");

            // Here's where i'd get the loadout per user

            if (ServerLoadoutRegistry.TryGetFor(opener, out PlayerLoadout playerLoadout))
            {
                Log.Info($"[DraftArtifact] using loadout: {playerLoadout}");
                List<PickupPickerController.Option> allowedItems = new();

                // TODO check if its the same tier
                var pickupTier = PickupCatalog.GetPickupDef(pickupIndex).itemTier;
                //if (pickupTier == ItemTier.Tier1)
                //{

                //    allowedItems.Add(new PickupPickerController.Option
                //    {
                //        pickupIndex = PickupCatalog.FindPickupIndex(playerLoadout.randomWhite),
                //        available = true
                //    }
                //     );
                //}
                //if (pickupTier == ItemTier.Tier2)
                //{
                //    allowedItems.Add(new PickupPickerController.Option
                //    {
                //        pickupIndex = PickupCatalog.FindPickupIndex(playerLoadout.randomGreen),
                //        available = true
                //    });
                //}

                //if (pickupTier == ItemTier.Tier3)
                //{
                //    allowedItems.Add(new PickupPickerController.Option
                //    {
                //        pickupIndex = PickupCatalog.FindPickupIndex(playerLoadout.randomRed),
                //        available = true
                //    });
                //}

                self.SetOptionsServer(allowedItems.ToArray());
                return;
            }
            else
            {
                // do regular stuff
                orig(self, pickupIndex);
            }

            // TODO server needs to do stuff
            //PickupIndex[] limitedSelection = PickupTransmutationManager.GetGroupFromPickupIndex(pickupIndex);

            //foreach (var item in limitedSelection)
            //{
            //    Log.Info($"[DraftArtifact] currSelection {item.pickupDef.nameToken}");
            //}

            PickupPickerController.Option[] items =
            {
                new PickupPickerController.Option
                   {
                    available = true,
                    pickupIndex = PickupCatalog.FindPickupIndex(RoR2Content.Items.Hoof.itemIndex)
                }
            };

            self.SetOptionsServer(items);
        }

        private void GP_OnInteractionBegin(
          On.RoR2.GenericPickupController.orig_OnInteractionBegin orig,
         GenericPickupController self,
          Interactor activator)
        {
            if (NetworkServer.active)
            {
                Log.Info("[DraftArtifact] GP_OnInteractionBegin");
            }

            if (NetworkServer.active && activator)
            {
                // todo here we would track
                if (activator.TryGetComponent<CharacterBody>(out CharacterBody characterBody))
                {
                    var user = characterBody.master?.playerCharacterMasterController?.networkUser;
                    Log.Info($"Activated by {user.netId}");
                }
                else
                {
                    Log.Info("No Activator");
                }
            }

            // do the original behavior
            orig(self, activator);
        }

        private static bool HasRequiredCorruptItem(NetworkUser user, PickupDef normalDef)
        {
            // somehow not a void mapping so go on
            if (!CorruptionMaps.HasVoidMapping(normalDef.itemIndex, out ItemIndex voidIndex))
            {
                return false;

            }

            Log.Info($"[DraftArtifactPlugin] checking for void {voidIndex}");
            // get inventory check if we have it
            var inventory = user.master.inventory;
            return inventory.GetItemCount(voidIndex) > 0;
        }

        [SuppressMessage("CodeQuality", "IDE0051", Justification = "MonoBehavior lifecycle")]
        private void Update()
        {
            // dont let us unintentionally cheat
            if (!DebugSettings.IS_DEV_MODE)
            {
                return;
            }

            // This if statement checks if the player has currently pressed F2.
            if (Input.GetKeyDown(KeyCode.F2))
            {

                // Grab the first player’s master (works SP/host)
                var pcmc = PlayerCharacterMasterController.instances[0];
                if (pcmc != null && pcmc.master != null)
                {
                    // Give 50 gold
                    pcmc.master.GiveMoney(50);

                    Log.Info($"[DraftArtifact] Gave 50 gold to {pcmc.networkUser}");
                    // WIP so i dont leave this in
                    SendBroadcastChat(new SimpleChatMessage { baseToken = "<color=#e5eefc>{0}: Im a big cheating cheater</color>", paramTokens = new[] { pcmc.networkUser.name } });
                }
                else
                {
                    Log.Info("[DraftArtifact] No player master found.");
                }


            }


            // This if statement checks if the player has currently pressed F2.
            if (Input.GetKeyDown(KeyCode.F3))
            {
                // shoot a needletick option for testing
                var playerTransform = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;
                if (CorruptedItemDefs.TryGetItemDef(CorruptedItem.Needletick, out var itemDef))
                {
                    PickupDropletController.CreatePickupDroplet(
                   PickupCatalog.FindPickupIndex(itemDef.itemIndex),
                   playerTransform.position, playerTransform.forward * 20f);
                }
            }

            if (Input.GetKeyDown(KeyCode.F4))
            {
                var playerTransform = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;
                if (CorruptedItemDefs.TryGetItemDef(CorruptedItem.NewlyHatchedZoea, out var itemDef))
                {
                    PickupDropletController.CreatePickupDroplet(
                   PickupCatalog.FindPickupIndex(itemDef.itemIndex),
                   playerTransform.position, playerTransform.forward * 20f);
                }
            }
        }
    }
}
