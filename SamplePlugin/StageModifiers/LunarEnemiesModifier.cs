using HarmonyLib;
using R2API;
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
        public List<DirectorCard> lunarEnemies;

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
            int maxCost = 200;
            int minCost = 40;

            if(!ClassicStageInfo.instance || ClassicStageInfo.instance.monsterSelection == null)
            {
                Log.Error("No monster selection found");
                return;
            }

            var selection = ClassicStageInfo.instance.monsterSelection;
            for(int i = 0; i < selection.Count; i++)
            {
                var card = selection.GetChoice(i);
                if(card.value != null && card.value.spawnCard!=null)
                {
                    Log.Info($"Card:{card.value.spawnCard.name}, Weight:{card.weight}, Cost:{card.value.cost}");
                }
            }

            for(int i = 0; i<selection.Count;i++)
            {
                var card = selection.GetChoice(i);
                var sc = card.value.spawnCard;
                Log.Info($"Card:{sc.name},weight:{card.weight},cost:{card.value.cost}");
                maxCost = Mathf.Max(card.value.cost,maxCost);
                minCost = Mathf.Min(card.value.cost,minCost);
            }
            //200 40 предельные значения
            int finalMaxCost = Mathf.Min(100,maxCost);
            int finalMinCost = Mathf.Max(40,minCost);
            CreateLunarEnemiesSpawnCards(finalMinCost,finalMaxCost);
            Log.Info("Moon modifier start");
            if(ClassicStageInfo.instance && ClassicStageInfo.instance.monsterSelection != null)
            {
                foreach(var lunar in lunarEnemies)
                {
                    ClassicStageInfo.instance.monsterSelection.AddChoice(lunar, (float)(lunar.selectionWeight * (1+0.2*stack)));
                }
            }



            foreach (var director in CombatDirector.instancesList)
            {
                if (director.monsterCards == null) director.currentMonsterCard = null;
            }
        }

        public void CreateLunarEnemiesSpawnCards(int maxCost, int minCost)
        {
            lunarEnemies = new List<DirectorCard>();

            List<CharacterSpawnCard> csc = new List<CharacterSpawnCard>();
            var lunarWispOrig = Addressables.LoadAssetAsync<CharacterSpawnCard>("RoR2/Base/LunarWisp/cscLunarWisp.asset").WaitForCompletion();
            var lunarGolemOrig = Addressables.LoadAssetAsync<CharacterSpawnCard>("RoR2/Base/LunarGolem/cscLunarGolem.asset").WaitForCompletion();
            var lunarExploderOrig = Addressables.LoadAssetAsync<CharacterSpawnCard>("RoR2/Base/LunarExploder/cscLunarExploder.asset").WaitForCompletion();

            var lunarWisp = ScriptableObject.Instantiate(lunarWispOrig);
            var lunarGolem = ScriptableObject.Instantiate(lunarGolemOrig);
            var lunarExploder = ScriptableObject.Instantiate(lunarExploderOrig);
            csc.Add(lunarWisp);
            csc.Add(lunarGolem);
            csc.Add(lunarExploder);
            foreach(var ca in csc)
            {
                var dc = new DirectorCard()
                {
                    spawnCard = ca,
                    selectionWeight = 1,
                    minimumStageCompletions = 0,
                };
                lunarEnemies.Add(dc);
            }
            
        }
    }



}
