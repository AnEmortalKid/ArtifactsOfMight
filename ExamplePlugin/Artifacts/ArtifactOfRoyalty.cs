using ArtifactsOfMight.Assets;
using ArtifactsOfMight.Logger;
using BepInEx.Configuration;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace ArtifactsOfMight.Artifacts
{
    /// <summary>
    /// Heavy is the crown
    /// Gives a lunar crown every state
    /// </summary>
    public class ArtifactOfRoyalty : ArtifactBase
    {
        public override string ArtifactName => "Artifact of Royalty";

        public override string ArtifactLangTokenName => "ARTIFACT_OF_ROYALTY";

        public override string ArtifactDescription => "Heavy is the crown. Grants a Brittle Crown every stage.";

        public override Sprite ArtifactEnabledIcon =>
            ArtifactsOfMightAsset.LoadSprite("Assets/Addressable/artifact_of_royalty_enabled.png");

        public override Sprite ArtifactDisabledIcon => 
            ArtifactsOfMightAsset.LoadSprite("Assets/Addressable/artifact_of_royalty_disabled.png");

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
            Stage.onStageStartGlobal += GiveCrownOnStageStart;
        }

        public void OnDestroy()
        {
            Stage.onStageStartGlobal -= GiveCrownOnStageStart;
        }


        private void GiveCrownOnStageStart(Stage stage)
        {
            if(!ArtifactEnabled)
            {
                return;
            }

            // only the server can grant items
            if (!NetworkServer.active)
            {
                return;
            }

            LOGGER.Info("Checking grants");

            if(stage.sceneDef.sceneType == SceneType.Intermission )
            {
                LOGGER.Info($"Intermission scene {stage.name} so skipping");
                return;
            }

            foreach (var player in PlayerCharacterMasterController.instances)
            {
                if (player != null)
                {
                    var master = player.master;
                    if(master != null && master.inventory)
                    {
                        master.inventory.GiveItemPermanent(RoR2Content.Items.GoldOnHit, 1);
                    }
                }
            }
        }

    }
}
