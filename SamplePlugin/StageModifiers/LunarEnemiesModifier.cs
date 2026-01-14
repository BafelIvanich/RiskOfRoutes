using HarmonyLib;
using RiskOfRoutes;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;


namespace SamplePlugin.StageModifiers
{
    //карты : 
    public class LunarEnemiesModifier : StageModifier
    {
        public List<CharacterSpawnCard> lunarEnemies;

        public LunarEnemiesModifier()
        {

            name = "Moon invasion";
            reservedText = "<style=cLunarObjective>Enemies from the moon</style> might appear on the next stage.";
            activeText = "<style=cLunarObjective>Enemies from the moon</style> might appear on this stage.";
            var icon = Addressables.LoadAssetAsync<Sprite>("RoR2/DLC1/GameModes/InfiniteTowerRun/ITAssets/texITWaveLunarIcon.png").WaitForCompletion();
            if (!icon)
            {
                Log.Error($"Could not load icon for StageModifier {name}");
            }
            else
            {
                this.icon = icon;
            }
        }


        public override void OnEnd()
        {
            Log.Info("Moon invasion on end");
        }

        public override void OnStart()
        {
            //Log.Info("Moon modifier start");

            //// 1. Load Lunar DCCS
            //var lunarDCCS = Addressables.LoadAssetAsync<DirectorCardCategorySelection>("RoR2/Base/Common/DirectorCardCategorySelections/dccsLunarFamily.asset").WaitForCompletion();
            //if (!lunarDCCS) return;

            //// 2. Update the "Global" info (for any future directors)
            //if (RoR2.ClassicStageInfo.instance)
            //{
            //    RoR2.ClassicStageInfo.instance.monsterCategories = lunarDCCS;
            //}

            //// 3. Update the "Active" director (because it probably already loaded the old one)
            //foreach (var director in UnityEngine.Object.FindObjectsOfType<RoR2.CombatDirector>())
            //{
            //    // Only update the main monster director (monsterCredit > 0 is a decent guess, or check properties)
            //    // Usually we just update all of them to be safe, or check if they are handling monsters.
            //    director.monsterCards = lunarDCCS;

            //    // Optional: Reset the current spawn intent so it doesn't try to spawn a beetle from the old list
            //    director.currentMonsterCard = null;
            //}
        }

        public void CreateLunarEnemiesSpawnCards()
        {
            //Log.Info("Trying to create CHEF");
            //InteractableSpawnCard card = ScriptableObject.CreateInstance<InteractableSpawnCard>();

            //card.prefab = RiskOfRoutes.RiskOfRoutes.chefPrefab;
            //card.directorCreditCost = 0;
            //card.sendOverNetwork = true;
            //card.hullSize = HullClassification.Human;
            //card.orientToFloor = true;
            //card.nodeGraphType = RoR2.Navigation.MapNodeGroup.GraphType.Ground;

            //chefSpawnCard = card;

            var c = Addressables.LoadAssetAsync<CharacterSpawnCard>("RoR2/Base/LunarWisp/cscLunarWisp.asset").WaitForCompletion();
            var g = Addressables.LoadAssetAsync<CharacterSpawnCard>("RoR2/Base/LunarGolem/cscLunarGolem.asset").WaitForCompletion();
            var e = Addressables.LoadAssetAsync<CharacterSpawnCard>("RoR2/Base/LunarExploder/cscLunarExploder.asset").WaitForCompletion();
            lunarEnemies.Add(c);
            lunarEnemies.Add(g);
            lunarEnemies.Add(e);
        }
    }

    

}
