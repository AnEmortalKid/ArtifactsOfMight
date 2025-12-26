using ArtifactsOfMight.Assets;
using BepInEx.Configuration;
using UnityEngine;
using RoR2;
using UnityEngine.Networking;
using ArtifactsOfMight.Logger;

namespace ArtifactsOfMight.Artifacts
{
    internal class ArtifactOfShelldivers : ArtifactBase
    {
        public override string ArtifactName => "Artifact of Shelldivers";

        public override string ArtifactLangTokenName => "ARTIFACT_OF_SHELLDIVERS";

        public override string ArtifactDescription => "You get one shell, you give them hell. Enables Honor, Sacrifice, Swarms.";

        public override Sprite ArtifactEnabledIcon => ArtifactsOfMightAsset.LoadSprite("Assets/Addressable/artifact_of_shelldivers_enabled.png");

        public override Sprite ArtifactDisabledIcon => ArtifactsOfMightAsset.LoadSprite("Assets/Addressable/artifact_of_shelldivers_disabled.png");

        private static readonly ScoppedLogger.Scoped LOGGER = ScoppedLogger.For<ArtifactOfRoyalty>();

        public override void Init(ConfigFile config)
        {
            // no config 
            CreateLang();
            CreateArtifact();

            Hooks();
        }

  

        public override void Hooks()
        {
            Run.onRunStartGlobal += EnableRelatedArtifacts;
            Run.onRunStartGlobal += GiveShellOnRunStart;
        }


        public void OnDestroy()
        {
            Run.onRunStartGlobal -= EnableRelatedArtifacts;
            Run.onRunStartGlobal -= GiveShellOnRunStart;
        }

        private void EnableRelatedArtifacts(Run run)
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

            LOGGER.Info("Enabling related artifacts since our artifact is enabled");
            RunArtifactManager.instance.SetArtifactEnabledServer(RoR2Content.Artifacts.Sacrifice, ArtifactEnabled);
            RunArtifactManager.instance.SetArtifactEnabledServer(RoR2Content.Artifacts.Swarms, ArtifactEnabled);
            RunArtifactManager.instance.SetArtifactEnabledServer(RoR2Content.Artifacts.EliteOnly, ArtifactEnabled);
        }

        private void GiveShellOnRunStart(Run run)
        {
            if (!ArtifactEnabled)
            {
                return;
            }

            // only the server can grant items
            if (!NetworkServer.active)
            {
                return;
            }

            LOGGER.Info("Shelling it out");

            foreach (var player in PlayerCharacterMasterController.instances)
            {
                if (player != null)
                {
                    var master = player.master;
                    if (master != null && master.inventory)
                    {
                        master.inventory.GiveItemPermanent(DLC2Content.Items.ItemDropChanceOnKill, 1);
                    }
                }
            }
        }
    }
}
