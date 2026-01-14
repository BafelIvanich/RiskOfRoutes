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

        private static Dictionary<ArtifactDef, int> availableArtifactsAndPrices;

        private static bool ArtifactListCreated = false;

        public ArtifactModifier()
        {
            if (!ArtifactListCreated) CreateArtifactDictionary();
            currentArtifact = RoR2.RoR2Content.Artifacts.MixEnemy;
            if (currentArtifact == null)
            {
                Log.Error("WTF");
            }
            //здесь исключение
            activeText = currentArtifact.descriptionToken;
            if (activeText == null)
            {
                Log.Error($"{currentArtifact.cachedName} error with getting description");
            }
            Log.Info("2");
            reservedText = activeText;
            negative = true;
        }

        private void CreateArtifactDictionary()
        {
            availableArtifactsAndPrices = new Dictionary<ArtifactDef, int>()
            {
                [RoR2.RoR2Content.Artifacts.MixEnemy] = 30,
                [RoR2.RoR2Content.Artifacts.FriendlyFire] = 30,
                [RoR2.RoR2Content.Artifacts.WeakAssKnees] = 30,
                [RoR2.RoR2Content.Artifacts.EliteOnly] = 30,
                [RoR2.RoR2Content.Artifacts.Sacrifice] = 30,
                [RoR2.RoR2Content.Artifacts.WispOnDeath] = 30,
                [RoR2.RoR2Content.Artifacts.Bomb] = 30,
                [RoR2.RoR2Content.Artifacts.Swarms] = 30,
                [RoR2.RoR2Content.Artifacts.ShadowClone] = 30,
            };
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

        ////На время
        //private string GetRandomArtifactName()
        //{
        //    //if (Run.instance)
        //    //{
        //    //    int index = Run.instance.stageRng.RangeInt(0, ArtifactNames.Count - 1);
        //    //    Log.Info(index);
        //    //    return ArtifactNames[index];
        //    //}
        //    //else
        //    //{
        //    //    return ArtifactNames[0];
        //    //}
        //}

    }



}
