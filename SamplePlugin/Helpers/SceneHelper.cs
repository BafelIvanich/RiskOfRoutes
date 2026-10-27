using RoR2;

namespace RiskOfRoutes.Helpers
{
    public static class SceneHelper
    {
        public static bool IsSceneColossus(SceneIndex index, bool includeFalseSonScene = true)
        {
            if (includeFalseSonScene)
                return index == SceneCatalog.FindSceneIndex("lemuriantemple") || index == SceneCatalog.FindSceneIndex("habitat") || index == SceneCatalog.FindSceneIndex("meridian");
            else
                return index == SceneCatalog.FindSceneIndex("lemuriantemple") || index == SceneCatalog.FindSceneIndex("habitat");
        }

        public static bool IsSceneIntermission(SceneIndex index)
        {
            var scene = SceneCatalog.GetSceneDef(index);
            if (scene == null) return false;
            return scene.sceneType == SceneType.Intermission;
        }

        public static int GetGreenPortalSceneIndex()
        {
            SceneIndex sceneIndex = SceneIndex.Invalid;
            int stageOrder = Stage.instance.sceneDef.stageOrder;
            int num = Run.instance.nextStageScene.stageOrder;
            if (num != stageOrder + 1 && stageOrder <= 5)
            {
                num = stageOrder + 1;
            }
            Log.Info(stageOrder + " " + num);
            switch (num)
            {
                case 1:
                    sceneIndex = SceneCatalog.FindSceneIndex("lemuriantemple");
                    break;
                case 2:
                    sceneIndex = SceneCatalog.FindSceneIndex("lemurianTemple");
                    break;
                case 3:
                    sceneIndex = SceneCatalog.FindSceneIndex("habitat");
                    break;
                case 4:
                    sceneIndex = SceneCatalog.FindSceneIndex("meridian");
                    break;
                case 5:
                    sceneIndex = SceneCatalog.FindSceneIndex("lemuriantemple");

                    break;
            }
            Log.Info($"{nameof(GetGreenPortalSceneIndex)}: Selected scene for colossus portat - {sceneIndex}");
            return (int)sceneIndex;
        }

        public static bool IsValidNextStage(SceneDef sceneDef)
        {
            SceneDef nextStageScene = Run.instance.nextStageScene;
            if (nextStageScene != null && nextStageScene.baseSceneName == sceneDef.baseSceneName)
            {
                return false;
            }
            if (!sceneDef.hasAnyDestinations)
            {
                return false;
            }

            return sceneDef.validForRandomSelection;
        }

        public static WeightedSelection<SceneDef> GetNextStagesScenesList()
        {
            SceneDef nextStageScene = Run.instance.nextStageScene;
            WeightedSelection<SceneDef> list = new WeightedSelection<SceneDef>();
            if (nextStageScene == null)
            {
                return list;
            }

            int nextStageOrder = nextStageScene.stageOrder;
            var s = SceneCatalog.mostRecentSceneDef.destinationsGroup;

            foreach (SceneCollection.SceneEntry se in s.sceneEntries)
            {
                var scene = se.sceneDef;
                if (scene.sceneType == SceneType.Stage &&
                    scene.mainTrack &&
                    scene.stageOrder == nextStageOrder &&
                    (scene.requiredExpansion == null || Run.instance.IsExpansionEnabled(scene.requiredExpansion)) &&
                    Run.instance.CanPickStage(scene))
                {
                    list.AddChoice(se.sceneDef, se.weight);
                }
            }

            return list;
        }

    }
}
