using RiskOfRoutes;
using RoR2;
using System.Collections.Generic;

//whites: crowbar, bears, bleed, bombs, watches, gasoline, focus crystal, syringe, key
//greens: reload one, ukulele, active charge, wisps, band 1, band 2, injection, pinwheel, fin, star, rockets, energy shield, gasoline, wire


namespace SamplePlugin.StageModifiers
{
    //available artifacts would be: dissonance, frailty, honor,soul, spite, vengeance
    public class ArtifactModifier : StageModifier
    {
        private ArtifactDef currentArtifact;

        private static readonly List<string> ArtifactNames = new List<string>()
        {
            "MixEnemy", //Dissonance
            "WeakAssKnees", //Frailty
            "ElityOnly", //Honor
            "WispOnDeath", //Soul
            "Bomb", //Spite
            "ShadowClone", //Vengeance
            "FriendlyFire"
        };


        //TODO: сделать Struct артефактов может быть со стоимостью
        private static readonly Dictionary<string, string> ArtifactDesc = new Dictionary<string, string>()
        {
            { "ARTIFACT_MIXENEMY_DESCRIPTION","<style=cWorldEvent>Monsters can appear outside their usual environments.</style>" },
            { "ARTIFACT_WEAKASSKNEES_DESCRIPTION","<style=cWorldEvent>Fall damage is doubled and lethal.</style>" },
            { "ARTIFACT_ELITEONLY_DESCRIPTION","<style=cWorldEvent>Enemies can only spawn as elites.</style>" },
            { "ARTIFACT_WISPONDEATH_DESCRIPTION","<style=cWorldEvent>Wisps emerge from defeated monsters.</style>" },
            { "ARTIFACT_BOMB_DESCRIPTION","<style=cWorldEvent>Enemies drop multiple exploding bombs on death.</style>" },
            { "ARTIFACT_SHADOWCLONE_DESCRIPTION","<style=cWorldEvent>Your relentless doppelganger will invade on every 10th minute.</style>" },
            { "ARTIFACT_FRIENDLYFIRE_DESCRIPTION","<style=cWorldEvent>Friendly fire is enabled for both survivors and monsters alike.</style>" },
        };

        public ArtifactModifier()
        {
            currentArtifact = ArtifactCatalog.FindArtifactDef(GetRandomArtifactName());
            Log.Info("1");
            //здесь исключение
            activeText = ArtifactDesc[currentArtifact.descriptionToken];
            if (activeText == null)
            {
                Log.Error($"{currentArtifact.cachedName} error with getting description");
            }
            Log.Info("2");
            reservedText = activeText;
            negative = true;
        }

        public override void OnEnd()
        {
            Log.Info("Mehod");
            if (currentArtifact)
            {
                RunArtifactManager.instance.SetArtifactEnabled(currentArtifact, false);
                Log.Info("Succesfully false");
            }
            else
            {
                Log.Info("No current art");
            }
        }

        public override void OnStart()
        {

            Log.Info("ArtifactModifier start");
            if (currentArtifact)
            {
                RunArtifactManager.instance.SetArtifactEnabled(currentArtifact, true);
                Chat.AddMessage("Art enabled");
            }
            else
            {
                Log.Info("No such artifact");
            }
        }

        //На время
        private string GetRandomArtifactName()
        {
            if (Run.instance)
            {
                int index = Run.instance.stageRng.RangeInt(0, ArtifactNames.Count);
                return ArtifactNames[index];
            }
            else
            {
                return ArtifactNames[0];
            }
        }

    }



}
