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
using ArtifactsOfMight.Messages;
using R2API.Networking.Interfaces;
using ArtifactsOfMight.Artifacts;
using System.Reflection;
using UnityEngine.UI;
using UnityEngine.Rendering;
using System.Collections;
using ArtifactsOfMight.UI.Utils;
using ArtifactsOfMight.Loadout.Draft;
using System.IO.Pipes;

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
        public const string PluginVersion = "0.1.4";

        public List<ArtifactBase> Artifacts = new List<ArtifactBase>();

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

            if (DebugSettings.LOCAL_NETWORK_TEST)
            {
                Log.Warning("In Network Test Mode, gonna do the thing to the System Steam");
                On.RoR2.Networking.NetworkManagerSystemSteam.OnClientConnect += (s, u, t) =>
                {
                    Log.Info("Bypass OnClientConnect");
                };
            }

            // V 0.2.0 when we introduce the artifact
            // RegisterArtifacts();
        }

        private void RegisterArtifacts()
        {
            Log.Info("Registering Artifacts");
            var ArtifactTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(ArtifactBase)));
            foreach (var artifactType in ArtifactTypes)
            {
                Log.Info($"Registering artifact: {artifactType}");
                ArtifactBase artifact = (ArtifactBase)Activator.CreateInstance(artifactType);
                if (ValidateArtifact(artifact, Artifacts))
                {
                    artifact.Init(Config);
                }
            }
        }

        public bool ValidateArtifact(ArtifactBase artifact, List<ArtifactBase> artifactList)
        {
            // always add
            artifactList.Add(artifact);
            return true;
            // Sample in case we anted to have config based enabling
            //var enabled = Config.Bind<bool>("Artifact: " + artifact.ArtifactName, "Enable Artifact?", true, "Should this artifact appear for selection?").Value;
            //if (enabled)
            //{
            //    artifactList.Add(artifact);
            //}
            //return enabled;
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
        /// 
        /// I think this is overkill now since the server requests the messages
        /// but ima leave it in for now b4 ripping it out
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
            PickupPickerPanel.SetPickupOptions += PreventWeirdSizeOnClientSetPickupOptions;

            // Register our networked stuff
            NetworkingAPI.RegisterMessageType<LoadoutSyncMsg>();
            NetworkingAPI.RegisterMessageType<RequestLoadoutSyncMsg>();

            // Pre-Artifact hook
            On.RoR2.Stage.BeginServer += OnBeginServer;
        }

        public void OnDisable()
        {
            On.RoR2.PickupPickerController.OnInteractionBegin -= PopulateOnOpen;
            On.RoR2.PickupPickerController.SetOptionsServer -= OnSetOptionsServer;
            PickupPickerPanel.SetPickupOptions -= PreventWeirdSizeOnClientSetPickupOptions;
        }

        /// <summary>
        /// See ClientPickerPanelProtection for an explanation on what the heck
        /// </summary>
        /// <param name="orig"></param>
        /// <param name="self"></param>
        /// <param name="options"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void PreventWeirdSizeOnClientSetPickupOptions(PickupPickerPanel.orig_SetPickupOptions orig, RoR2.UI.PickupPickerPanel self, PickupPickerController.Option[] options)
        {
            // the host does not have this problem
            bool isClientOnly = NetworkClient.active && !NetworkServer.active;
            if (!isClientOnly)
            {
                orig(self, options);
                return;
            }

            if (LoggerSettings.LOG_CLIENT_PICKUP_OPTIONS)
            {
                Log.Info($"SetPickupOptions Invoked with {options.Length} items for client only session.");
            }

            // On the first send, we'll get the original X items and then the updated count before the panel can re-render
            // This happens on the initial OnDisplayBegin on the panel
            // So we're going to use some client logic to use our copy or trust the server to avoid a silly panel render bug (wrong size)
            var serverOptionsLength = options.Length;

            var localUser = RoR2.LocalUserManager.GetFirstLocalUser()?.currentNetworkUser;
            var localLoadout = DraftLoadout.Instance.ToPlayerLoadout();

            // check if its our second go around with this picker controller
            var referencedController = self.pickerController;
            if (!referencedController.TryGetComponent<ClientPickerPanelProtection>(out var pickerProtect))
            {
                if (LoggerSettings.LOG_CLIENT_PICKUP_OPTIONS)
                {
                    Log.Debug($"Attaching picker protector to picker: {self.pickerController.netId}");
                }

                pickerProtect = referencedController.gameObject.AddComponent<ClientPickerPanelProtection>();
                pickerProtect.ClientExpectedLength = GetClientExpectedCount(options, localUser, localLoadout);
            }

            if (serverOptionsLength == pickerProtect.ClientExpectedLength)
            {
                // trust 
                orig(self, options);
                return;
            }

            var clientWanted = pickerProtect.ClientExpectedLength;
            if (LoggerSettings.LOG_CLIENT_PICKUP_OPTIONS)
            {
                Log.Info($"Server sent {serverOptionsLength} but we expected {clientWanted}");
            }

            // In practice this would only happen the first time, the server sends us 34
            // then we open the panel and immediately get the correct 8
            // the panel rendered with the original 34 so we are gonna try to avoid that
            var clientPredictedOptions = GetDraftOptionsForPlayer(options, localUser, DraftLoadout.Instance.ToPlayerLoadout());
            if (LoggerSettings.LOG_CLIENT_PICKUP_OPTIONS)
            {
                Log.Info("Using client predicted set");
            }
            orig(self, clientPredictedOptions);
        }

        /// <summary>
        /// Copy the array of original options so we can use it as the set to filter from for different players,
        /// since we constantly override the options on the server for a picker (so all clients see the same options for the current player),
        /// that would override the original set on a 2nd retrieval and everyone gets the first persons options.
        /// </summary>
        /// <param name="orig">the original invocation</param>
        /// <param name="self">the controller</param>
        /// <param name="newOptions">the options to set on the server</param>
        private void OnSetOptionsServer(On.RoR2.PickupPickerController.orig_SetOptionsServer orig, PickupPickerController self, PickupPickerController.Option[] newOptions)
        {
            if (NetworkServer.active)
            {
                // store our state for future re-evaluations
                if (!self.TryGetComponent<PickerState>(out var originalState))
                {
                    Log.Debug($"OnSetOptionsServer Store original options for picker:{self.netId} count:{newOptions.Length}");
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
                // For troubleshooting what gets sent where
                Log.Info($"PopulateOnOpen picker:{self.netId} for interactor: {interactor.netId}");
                var user = ResolveUser(interactor); // Interactor -> CharacterBody -> NetworkUser
                if (user != null)
                {
                    var options = ServerBuildDraftOptionsFor(user, self);
                    ApplyOptionsServer(self, options); // server-authoritative push
                }

                // Now open the UI; clients will already have the right options
                orig(self, interactor);
                return;
            }

            Log.Info($"PopulateOnOpen Client");

            // Client: just let vanilla open locally; options came from server.
            orig(self, interactor);
        }


        /// <summary>
        /// I think this fires on both he client and server
        /// </summary>
        /// <param name="orig"></param>
        /// <param name="self"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void OnBeginServer(On.RoR2.Stage.orig_BeginServer orig, Stage self)
        {
            // do original behavior
            orig(self);

            var isServerActive = NetworkServer.active;
            Log.Info($"OnBeginServer serverIsActive: {isServerActive}");
            if (isServerActive)
            {
                // request clients send us stuff
                Log.Info("OnBeginServer asking clients for their loadouts");
                new RequestLoadoutSyncMsg().Send(R2API.Networking.NetworkDestination.Clients);
            }
        }


        private void ApplyOptionsServer(PickupPickerController self, PickupPickerController.Option[] options)
        {
            Log.Info($"ApplyOptionsServer");
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

        private PickupPickerController.Option[] ServerBuildDraftOptionsFor(NetworkUser opener, PickupPickerController ppc)
        {
            if (ServerLoadoutRegistry.TryGetFor(opener, out PlayerLoadout playerLoadout))
            {
                Log.Info($"ServerBuildDraftOptionsFor: {opener.netId} using loadout: {playerLoadout}");

                // this should exist
                var originalState = ppc.GetComponent<PickerState>();

                // The ppc options keep changing when we override them on the server
                // Always evaluate from the original set
                var originalPPCOptions = originalState.originalOptions;
                return GetDraftOptionsForPlayer(originalPPCOptions, opener, playerLoadout);
            }

            // fallback we shouldn't have mutated this
            return ppc.options;
        }


        /// <summary>
        /// Returns a filtered array of options based on a players loadout and our currently available options
        /// 
        /// This is used both for client side prediction on the first send and for the server to correctly build
        /// the options to send for a picker to a player
        /// </summary>
        /// <param name="ppcOptions">an array of options to filter through</param>
        /// <param name="opener">who the options are intended for</param>
        /// <param name="playerLoadout">the loadout for a player</param>
        /// <returns></returns>
        private PickupPickerController.Option[] GetDraftOptionsForPlayer(PickupPickerController.Option[] ppcOptions, NetworkUser opener, PlayerLoadout playerLoadout)
        {
            var first = ppcOptions[0];
            var pickupTier = PickupCatalog.GetPickupDef(first.pickupIndex).itemTier;

            if (LoggerSettings.LOG_BUILD_DRAFT_OPTIONS)
            {
                Log.Info($"Building options for tier: {pickupTier}");
            }

            // if its not a draftable tier, return the current options (equipment / lunar for example)
            if (!playerLoadout.byTier.TryGetValue(pickupTier, out var limitByTier))
            {
                if (LoggerSettings.LOG_BUILD_DRAFT_OPTIONS)
                {
                    Log.Info($"Draft has no limit for tier: {pickupTier} returning fallback");
                }
                return ppcOptions;
            }

            var limitDefByTier = playerLoadout.byTier.Get(pickupTier);
            if (LoggerSettings.LOG_BUILD_DRAFT_OPTIONS)
            {
                Log.Info($"Build filter for tier: {pickupTier}");
            }

            // We're going to rebuild the list
            List<PickupPickerController.Option> filteredItems = new();

            var restrictedByCorrupt = limitDefByTier.restrictedByVoid;
            var tierAllowedItems = limitDefByTier.allowed;

            if (LoggerSettings.LOG_BUILD_DRAFT_OPTIONS)
            {
                Log.Info($"Tier {pickupTier} allows {tierAllowedItems.Count} items");
            }

            // Loop through the original options to preserve order
            foreach (var ppcOption in ppcOptions)
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
                    if (LoggerSettings.LOG_BUILD_DRAFT_OPTIONS)
                    {
                        Log.Info($"Checking restrict by corrupt for {optionDef.nameToken}");
                    }
                    var hasRequired = HasRequiredCorruptItem(opener, optionDef);
                    if (hasRequired)
                    {
                        filteredItems.Add(ppcOption);
                    }
                    else
                    {
                        // add a gray
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
                    continue;
                }

                // if we have no voids in this category, add this void as an empty square
                // if we are a void tier and everything is filtered
                // add the original amount of grays so it doesn't feel like things are broken
                if (tierAllowedItems.Count == 0 && CorruptionMaps.IsVoidTier(pickupTier))
                {
                    var grayOption = new PickupPickerController.Option
                    {
                        pickupIndex = ppcOption.pickupIndex,
                        available = false
                    };
                    filteredItems.Add(grayOption);
                }
            }

            if (LoggerSettings.LOG_BUILD_DRAFT_OPTIONS)
            {
                Log.Info($"Loadout filtered item count: {filteredItems.Count}");
            }
            return filteredItems.ToArray();
        }


        /// <summary>
        /// Determines how many items the client would expect in the options
        /// 
        /// The options here are not used for filtering , they're merely used to find the tier
        /// 
        /// Since the server logic sends gray squares when items are locked, we will do the same logic here,
        /// we care about the total count (items + corrupt unlocked)
        /// 
        /// We will do the same void assumption when there are no void items for a tier, we just assume it will be
        /// the full set of locked voids (for display)
        /// </summary>
        /// <param name="sentOptions">the options sent from the server, may already be the filtered set</param>
        /// <returns></returns>
        private int GetClientExpectedCount(PickupPickerController.Option[] sentOptions, NetworkUser opener, PlayerLoadout playerLoadout)
        {
            var first = sentOptions[0];
            var pickupTier = PickupCatalog.GetPickupDef(first.pickupIndex).itemTier;

            Log.Info($"Predicting options for tier: {pickupTier}");

            // if its not a draftable tier, return the current options (equipment / lunar for example)
            if (!playerLoadout.byTier.TryGetValue(pickupTier, out var limitByTier))
            {
                Log.Info($"Draft has no limit for tier: {pickupTier} assuming all items are ok");
                return sentOptions.Length;
            }

            var limitDefByTier = playerLoadout.byTier.Get(pickupTier);
            Log.Info($"Build predicted for tier: {pickupTier}");

            // Could be a void tier with no items in it or just a tab we didnt pick stuff on
            // In any case we will assume the server's count is right here
            if (limitByTier.allowed.Count == 0)
            {
                return sentOptions.Length;
            }

            int regularExpect = limitByTier.allowed.Count;
            int corruptedExpect = limitByTier.restrictedByVoid.Count;

            // we would get gray squares but the count is what matters for rendering
            int total = regularExpect + corruptedExpect;
            return total;
        }

        private static bool HasRequiredCorruptItem(NetworkUser user, PickupDef normalDef)
        {
            // somehow not a void mapping so go on
            if (!CorruptionMaps.HasVoidMapping(normalDef.itemIndex, out ItemIndex voidIndex))
            {
                return false;
            }

            if (DebugSettings.LOG_DRAFT_POOLS_INFO)
            {
                Log.Info($"Checking for void {voidIndex}");
            }

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
                Log.Info("Tier 1 single");
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
                Log.Info("Tier 2 single");
                var playerTransform = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;
                if (CorruptedItemDefs.TryGetItemDef(CorruptedItem.VoidsentFlame, out var itemDef))
                {
                    PickupDropletController.CreatePickupDroplet(
                   PickupCatalog.FindPickupIndex(itemDef.itemIndex),
                   playerTransform.position, playerTransform.forward * 20f);
                }
            }

            if (Input.GetKeyDown(KeyCode.F5))
            {
                Log.Info("Trying to make tier2 full panel");
                var playerTransform = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;
                var tier2Voids = new CorruptedItem[] { CorruptedItem.VoidsentFlame, CorruptedItem.PlasmaShrimp,
                CorruptedItem.Tentabauble, CorruptedItem.SingularityBand, CorruptedItem.Polylute,
                CorruptedItem.LysateCell};

                var pickerOptions = new PickupPickerController.Option[tier2Voids.Length];
                for (int i = 0; i < tier2Voids.Length; i++)
                {
                    var lookupDef = tier2Voids[i];
                    if (CorruptedItemDefs.TryGetItemDef(lookupDef, out var itemDef))
                    {
                        pickerOptions[i] = new PickupPickerController.Option()
                        {
                            available = true,
                            pickupIndex = PickupCatalog.FindPickupIndex(itemDef.itemIndex)
                        };
                    }
                }

                GenericPickupController.CreatePickupInfo pickupInfo = new()
                {
                    pickerOptions = pickerOptions,
                    position = playerTransform.position,
                    rotation = playerTransform.rotation,
                    artifactFlag = GenericPickupController.PickupArtifactFlag.COMMAND,
                };

                GenericPickupController.CreatePickup(pickupInfo);
            }
        }
    }
}
