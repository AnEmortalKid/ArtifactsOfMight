using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using UnityEngine;
using RoR2;
using UnityEngine.Networking;
using ArtifactsOfMight.Assets;
using ArtifactsOfMight.DraftArtifact.Game;
using System.Linq;
using ArtifactsOfMight.Loadout.Draft;
using ArtifactsOfMight.RunConfig;
using ArtifactsOfMight.Loadout.Corruption;
using UnityEngine.UIElements.Collections;
using On.RoR2.UI;
using ArtifactsOfMight.Logger;

namespace ArtifactsOfMight.Artifacts
{
    /// <summary>
    ///  TODO move logic down to DraftArtifact
    /// </summary>
    public class ArtifactOfChoice : ArtifactBase
    {
        public override string ArtifactName => "Artifact of Choice";

        public override string ArtifactLangTokenName => "ARTIFACT_OF_CHOICE";

        public override string ArtifactDescription => "When enabled, limits the choices available on the command palette based on what you've picked. Enables Command.";

        public override Sprite ArtifactEnabledIcon =>
            ArtifactsOfMightAsset.LoadSprite("Assets/Addressable/artifact_of_choice_enabled.png");

        public override Sprite ArtifactDisabledIcon =>
             ArtifactsOfMightAsset.LoadSprite("Assets/Addressable/artifact_of_choice_disabled.png");

        private static readonly ScoppedLogger.Scoped LOGGER = ScoppedLogger.For<ArtifactOfChoice>();

        public override void Init(ConfigFile config)
        {
            // no config 
            CreateLang();
            CreateArtifact();
            // propagate for external checkers
            //Definition = ArtifactDef;

            Hooks();
        }

        public override void Hooks()
        {
            Run.onRunStartGlobal += EnableCommandIfChoiceEnabled;
            Run.onRunStartGlobal += SendLoadoutOnRunStart;

            On.RoR2.PickupPickerController.OnInteractionBegin += PopulateDraftOptionsOnInteractionBegin;
            On.RoR2.PickupPickerController.SetOptionsServer += SaveOriginalOptionsOnSetOptionsServer;
            PickupPickerPanel.SetPickupOptions += PreventWeirdSizeOnClientSetPickupOptions;
        }

        public void OnDestroy()
        {
            Run.onRunStartGlobal -= EnableCommandIfChoiceEnabled;
            Run.onRunStartGlobal -= SendLoadoutOnRunStart;

            On.RoR2.PickupPickerController.OnInteractionBegin -= PopulateDraftOptionsOnInteractionBegin;
            On.RoR2.PickupPickerController.SetOptionsServer -= SaveOriginalOptionsOnSetOptionsServer;
            PickupPickerPanel.SetPickupOptions -= PreventWeirdSizeOnClientSetPickupOptions;
        }

        #region Hooks

        private void EnableCommandIfChoiceEnabled(Run run)
        {
            if (!ArtifactEnabled)
            {
                return;
            }

            // only runs on server
            if (!NetworkServer.active)
            {
                return;
            }

            LOGGER.Info("Enabling command since our artifact is enabled");
            RunArtifactManager.instance.SetArtifactEnabledServer(RoR2Content.Artifacts.Command, ArtifactEnabled);
        }

        private void SendLoadoutOnRunStart(Run run)
        {
            if (!ArtifactEnabled)
            {
                LOGGER.Info("disabled");
                return;
            }

            var serverReady = NetworkServer.active;
            var isClient = NetworkClient.active;

            if (isClient)
            {
                LOGGER.Info("client dispatching loadout");
                ClientLoadoutSender.SendNow();
            }

            if (serverReady)
            {
                LOGGER.Info("serverReady not dispatching loadout");
            }
        }


        private void PopulateDraftOptionsOnInteractionBegin(On.RoR2.PickupPickerController.orig_OnInteractionBegin orig, PickupPickerController self, Interactor interactor)
        {
            if (!ArtifactEnabled)
            {
                LOGGER.Info("disabled");
                orig(self, interactor);
                return;
            }

            if (NetworkServer.active)
            {
                // For troubleshooting what gets sent where
                LOGGER.Info($"picker:{self.netId} for interactor: {interactor.netId}");
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

            LOGGER.Info($"default logic since client");

            // Client: just let vanilla open locally; options came from server.
            orig(self, interactor);
        }

        /// <summary>
        /// Copy the array of original options so we can use it as the set to filter from for different players,
        /// since we constantly override the options on the server for a picker (so all clients see the same options for the current player),
        /// that would override the original set on a 2nd retrieval and everyone gets the first persons options.
        /// </summary>
        /// <param name="orig">the original invocation</param>
        /// <param name="self">the controller</param>
        /// <param name="newOptions">the options to set on the server</param>
        private void SaveOriginalOptionsOnSetOptionsServer(On.RoR2.PickupPickerController.orig_SetOptionsServer orig, PickupPickerController self, PickupPickerController.Option[] newOptions)
        {
            if (!ArtifactEnabled)
            {
                LOGGER.Info("disabled");
                orig(self, newOptions);
                return;
            }

            if (NetworkServer.active)
            {
                // store our state for future re-evaluations
                if (!self.TryGetComponent<PickerState>(out var originalState))
                {
                    LOGGER.Debug($"Store original options for picker:{self.netId} count:{newOptions.Length}");
                    originalState = self.gameObject.AddComponent<PickerState>();
                    // make copy
                    originalState.originalOptions = newOptions.ToArray();
                }
            }

            orig(self, newOptions);
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
            if (!ArtifactEnabled)
            {
                LOGGER.Info("disabled");
                orig(self, options);
                return;
            }

            // the host does not have this problem
            bool isClientOnly = NetworkClient.active && !NetworkServer.active;
            if (!isClientOnly)
            {
                orig(self, options);
                return;
            }


            LOGGER.Info($"Invoked with {options.Length} items for client only session.");

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
                LOGGER.Debug($"Attaching picker protector to picker: {self.pickerController.netId}");

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
            LOGGER.Info($"Server sent {serverOptionsLength} but we expected {clientWanted}");


            // In practice this would only happen the first time, the server sends us 34
            // then we open the panel and immediately get the correct 8
            // the panel rendered with the original 34 so we are gonna try to avoid that
            var clientPredictedOptions = GetDraftOptionsForPlayer(options, localUser, DraftLoadout.Instance.ToPlayerLoadout());
            LOGGER.Info("Using client predicted set");

            orig(self, clientPredictedOptions);
        }

        #endregion

        #region BackingLogic
        private PickupPickerController.Option[] ServerBuildDraftOptionsFor(NetworkUser opener, PickupPickerController ppc)
        {
            if (ServerLoadoutRegistry.TryGetFor(opener, out PlayerLoadout playerLoadout))
            {
                LOGGER.Info($"Building options for: {opener.netId} using loadout: {playerLoadout}");

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

        private void ApplyOptionsServer(PickupPickerController self, PickupPickerController.Option[] options)
        {
            LOGGER.Info($"applying");
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
            var pickupTier = PickupCatalog.GetPickupDef(first.pickup.pickupIndex).itemTier;


            LOGGER.Info($"Building options for tier: {pickupTier}");


            // if its not a draftable tier, return the current options (equipment / lunar for example)
            if (!playerLoadout.byTier.TryGetValue(pickupTier, out var limitByTier))
            {

                LOGGER.Info($"Draft has no limit for tier: {pickupTier} returning fallback");

                return ppcOptions;
            }

            var limitDefByTier = playerLoadout.byTier.Get(pickupTier);
            LOGGER.Info($"Build filter for tier: {pickupTier}");


            // We're going to rebuild the list
            List<PickupPickerController.Option> filteredItems = new();

            var restrictedByCorrupt = limitDefByTier.restrictedByVoid;
            var tierAllowedItems = limitDefByTier.allowed;

            LOGGER.Info($"Tier {pickupTier} allows {tierAllowedItems.Count} items");


            // Loop through the original options to preserve order
            foreach (var ppcOption in ppcOptions)
            {
                var optionDef = PickupCatalog.GetPickupDef(ppcOption.pickup.pickupIndex);
                var optionItemIndex = optionDef.itemIndex;

                if (tierAllowedItems.Contains(optionItemIndex))
                {
                    filteredItems.Add(ppcOption);
                    continue;
                }

                // check if we add or disable
                if (restrictedByCorrupt.Contains(optionItemIndex))
                {

                    LOGGER.Info($"Checking restrict by corrupt for {optionDef.nameToken}");

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
                            pickup = ppcOption.pickup,
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
                        pickup = ppcOption.pickup,
                        available = false
                    };
                    filteredItems.Add(grayOption);
                }
            }


            LOGGER.Info($"Loadout filtered item count: {filteredItems.Count}");

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
            var pickupTier = PickupCatalog.GetPickupDef(first.pickup.pickupIndex).itemTier;

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
            return inventory.GetItemCountEffective(voidIndex) > 0;
        }
        #endregion
    }
}
