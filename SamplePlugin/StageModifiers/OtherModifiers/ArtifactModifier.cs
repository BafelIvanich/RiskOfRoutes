using RiskOfRoutes;
using RoR2;

namespace SamplePlugin.StageModifiers.OtherModifiers
{
    public class ArtifactModifier : StageModifier
    {
        private ArtifactDef currentArtifact;
        //TODO: артефакт жертвы не вырубает спавн сундуков
        public ArtifactModifier(ArtifactDef artifact, bool isPositive)
        {
            currentArtifact = artifact;
            negative = !isPositive;
            stackable = false;
        }

        public ArtifactModifier(string name, bool isPositive)
        {
            currentArtifact = RoR2.ArtifactCatalog.FindArtifactDef(name);
            if (currentArtifact == null)
            {
                Log.Error($"Could not find artifact by the name {name}");
                return;
            }
            name = currentArtifact.cachedName;
            negative = !isPositive;
            stackable = false;
        }

        public ArtifactModifier()
        {
            currentArtifact = RoR2Content.Artifacts.MixEnemy;
            if (currentArtifact == null)
            {
                Log.Error("CurrentArtifact is null");
                return;
            }

            negative = true;
            stackable = false;
        }


        public override void OnEnd()
        {
            Log.Info("On artifactModifier end");
            if (currentArtifact)
            {
                if (Run.instance)
                {
                    RunArtifactManager.instance.SetArtifactEnabled(currentArtifact, false);
                    Log.Info($"Succesfully disabled artifact: {currentArtifact.cachedName}");

                }
                else
                {
                    Log.Info("Run already ended");
                }
            }
            else
            {
                Log.Info("No current artifact for some reason");
            }
        }

        public override void OnStart()
        {

            Log.Info("ArtifactModifier start");
            if (currentArtifact)
            {
                RunArtifactManager.instance.SetArtifactEnabled(currentArtifact, true);
                Chat.AddMessage($"Artifact {currentArtifact.cachedName} enabled");
            }
            else
            {
                Log.Info("No such artifact");
            }
        }

    }



}
