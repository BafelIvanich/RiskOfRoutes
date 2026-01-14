using RiskOfRoutes;
using UnityEngine;
using UnityEngine.AddressableAssets;


namespace SamplePlugin.StageModifiers
{
    //не артефакт, элитки становятся дешевле для директора
    public class MoreElitesModifier : StageModifier
    {
        public MoreElitesModifier()
        {
            name = "More elites";
            activeText = "More <style=cDeath>elite</style> enemies will appear.";
            reservedText = "More <style=cDeath>elite</style> enemies will appear on the next stage.";
            icon = Addressables.LoadAssetAsync<Sprite>("RoR2/DLC1/GameModes/InfiniteTowerRun/ITAssets/texITWaveBossIcon.png").WaitForCompletion();
            if (!icon)
            {
                Log.Info("GFGG");
            }
            negative = false;
        }
        public override void OnEnd()
        {
            Log.Info("MoreElitesModifier end");
        }

        public override void OnStart()
        {
            Log.Info("More elites modifies start");
            //On.RoR2.SceneDirector.Start += SceneDirector_Start;
        }

        private void SceneDirector_Start(On.RoR2.SceneDirector.orig_Start orig, RoR2.SceneDirector self)
        {
            Log.Info("Hello");
        }

        private bool CombatDirector_AttemptSpawnOnTarget(On.RoR2.CombatDirector.orig_AttemptSpawnOnTarget orig, RoR2.CombatDirector self, Transform spawnTarget, RoR2.DirectorPlacementRule.PlacementMode placementMode)
        {
            //Log.Info($"MoreElitesModifier:{self.currentActiveEliteTier.costMultiplier}");

            self.eliteBias = 0;

            return orig(self, spawnTarget, placementMode);
        }
    }

    

}
