using System;
using System.Linq;
using BepInEx;
using R2API;
using RoR2;
using R2API.Networking;
using UnityEngine;
using System.Collections.Generic;
using ArtifactsOfMight.UI;
using On.RoR2.UI;
using System.Diagnostics.CodeAnalysis;
using ArtifactsOfMight.Loadout.Corruption;
using static RoR2.Chat;
using ArtifactsOfMight.RunConfig;
using ArtifactsOfMight.Messages;
using ArtifactsOfMight.Artifacts;
using System.Reflection;
using ArtifactsOfMight.Assets;
using ArtifactsOfMight.Logger;

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
        public const string PluginVersion = BuildInfo.Version;

        public List<ArtifactBase> Artifacts = new List<ArtifactBase>();

        public static PluginInfo PluginInfo { get; private set; }

        /// <summary>
        /// Unity lifecycle function, that we will use to hook our UI
        /// </summary>
        [SuppressMessage("CodeQuality", "IDE0051", Justification = "MonoBehavior lifecycle")]
        public void Awake()
        {
            // Init our logging class so that we can properly log for debugging
            Log.Init(Logger);
            ScoppedLogger.Init(Logger);
            EnableScopedLoggers();

            Log.Info("Initializing via Awake");

            // Addressable stuff
            PluginInfo = Info;
            RegisterAssetBundles();

            // CharacterSelectController.ClientSetReady += OnClientSetReady;
            CharacterSelectController.Awake += HookupUIDepsOnCharacterSelectAwake;

            if (DebugSettings.LOCAL_NETWORK_TEST)
            {
                Log.Warning("In Network Test Mode, gonna do the thing to the System Steam");
                On.RoR2.Networking.NetworkManagerSystemSteam.OnClientConnect += (s, u, t) =>
                {
                    Log.Info("Bypass OnClientConnect");
                };
            }

            // V 0.2.0 when we introduce the artifact
            RegisterArtifacts();
        }

        private void EnableScopedLoggers()
        {
            // Only let specific loggers
            ScoppedLogger.Only([
                "ArtifactsOfMight.Artifacts"
            ]);
        }

        private void RegisterAssetBundles()
        {
            ArtifactsOfMightAsset.Init();
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

        private void HookupUIDepsOnCharacterSelectAwake(CharacterSelectController.orig_Awake orig, RoR2.UI.CharacterSelectController self)
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
        /// Here we attach to the Game/Run side of the game
        /// </summary>
        [SuppressMessage("CodeQuality", "IDE0051", Justification = "MonoBehavior lifecycle")]
        public void OnEnable()
        {
            // Register our networked stuff
            NetworkingAPI.RegisterMessageType<LoadoutSyncMsg>();
            NetworkingAPI.RegisterMessageType<RequestLoadoutSyncMsg>();

            ScoppedLogger.LogEnabledScopes();
        }

        [SuppressMessage("CodeQuality", "IDE0051", Justification = "MonoBehavior lifecycle")]
        private void Update()
        {
            // dont let us unintentionally cheat
            if (!DebugSettings.IS_DEV_MODE)
            {
                return;
            }

            
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

            
            if (Input.GetKeyDown(KeyCode.F3))
            {
                Log.Info("Tier 1 single");
                // shoot a needletick option for testing
                var playerTransform = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;
                if (CorruptedItemDefs.TryGetItemDef(CorruptedItem.Needletick, out var itemDef))
                {
                    var uniquePick = new UniquePickup(
                       PickupCatalog.FindPickupIndex(itemDef.itemIndex));

                    PickupDropletController.CreatePickupDroplet(
                                      uniquePick
                                 ,
                                 playerTransform.position, playerTransform.forward * 20f, false);
                }
            }

            if (Input.GetKeyDown(KeyCode.F4))
            {
                Log.Info("Tier 2 single");
                var playerTransform = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;
                if (CorruptedItemDefs.TryGetItemDef(CorruptedItem.VoidsentFlame, out var itemDef))
                {
                    var uniquePick = new UniquePickup(
                        PickupCatalog.FindPickupIndex(itemDef.itemIndex));

                    PickupDropletController.CreatePickupDroplet(
                        uniquePick
                   ,
                   playerTransform.position, playerTransform.forward * 20f, false);
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
                            pickup = new UniquePickup(PickupCatalog.FindPickupIndex(itemDef.itemIndex))
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
