using RiskOfRoutes;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;


namespace SamplePlugin.StageModifiers.CombatModifiers
{
    //карты : 
    public class LunarEnemiesModifier : StageModifier
    {
        public List<DirectorCard> lunarEnemies;

        public LunarEnemiesModifier(int stack)
        {
            this.stack = stack;
        }


        public override void OnEnd()
        {
            Log.Info("Moon invasion on end");
        }

        public override void OnStart()
        {
            Log.Info("Moon invasion modifier start");

            int basicCost = 0;
            int miniBossCost = 0;

            if (ClassicStageInfo.instance.monsterCategories == null || ClassicStageInfo.instance.monsterCategories.categories == null)
            {
                Log.Error("No monster categories found. Returning");
                return;
            }
            foreach (var ctg in ClassicStageInfo.instance.monsterCategories.categories)
            {
                if (ctg.name == "Basic Monsters")
                {
                    foreach (var card in ctg.cards)
                    {
                        basicCost = Mathf.Max(basicCost, card.cost);
                    }
                }
                if (ctg.name == "Minibosses")
                {
                    foreach (var card in ctg.cards)
                    {
                        miniBossCost = Mathf.Max(miniBossCost, card.cost);
                    }
                }
            }

            if (basicCost == 0) basicCost = 40;
            if (miniBossCost == 0) miniBossCost = 200;

            CreateLunarEnemiesSpawnCards(basicCost, miniBossCost);
            if (ClassicStageInfo.instance && ClassicStageInfo.instance.monsterSelection != null)
            {
                ClassicStageInfo.instance.monsterSelection = GetSelectionFromCategories(ClassicStageInfo.instance.monsterCategories.categories);
            }

            foreach (var director in CombatDirector.instancesList)
            {
                if (director.monsterCards == null) director.currentMonsterCard = null;
            }
        }

        private WeightedSelection<DirectorCard> GetSelectionFromCategories(DirectorCardCategorySelection.Category[] categories)
        {
            WeightedSelection<DirectorCard> weightedSelection = new WeightedSelection<DirectorCard>();
            for (int i = 0; i < categories.Length; i++)
            {

                if (categories[i].name == "Basic Monsters")
                {
                    HG.ArrayUtils.ArrayAppend(ref categories[i].cards, lunarEnemies[2]);
                    Log.Info($"Succesfully added {lunarEnemies[2].spawnCard.name} to category {categories[i].name}");
                }
                else if (categories[i].name == "Minibosses")
                {
                    HG.ArrayUtils.ArrayAppend(ref categories[i].cards, lunarEnemies[0]);
                    Log.Info($"Succesfully added {lunarEnemies[0].spawnCard.name} to category {categories[i].name}");
                    HG.ArrayUtils.ArrayAppend(ref categories[i].cards, lunarEnemies[1]);
                    Log.Info($"Succesfully added {lunarEnemies[1].spawnCard.name} to category {categories[i].name}");

                }
                var ctg = categories[i];

                float num = SumAllWeightsInCategory(ctg);
                float num2 = ctg.selectionWeight / num;
                if (!(num > 0f))
                {
                    continue;
                }
                foreach (DirectorCard directorCard in ctg.cards)
                {
                    if (directorCard.IsAvailable())
                    {
                        float weight = directorCard.selectionWeight * num2;
                        weightedSelection.AddChoice(directorCard, weight);
                        if (directorCard == lunarEnemies[0]) Log.Debug("THEY WERE ACTUALLY ADDED");
                    }
                }
            }
            return weightedSelection;
        }

        public float SumAllWeightsInCategory(DirectorCardCategorySelection.Category category)
        {
            float num = 0f;
            for (int i = 0; i < category.cards.Length; i++)
            {
                if (category.cards[i] != null && category.cards[i].IsAvailable())
                {
                    num += category.cards[i].selectionWeight;
                }
            }
            return num;
        }


        public void CreateLunarEnemiesSpawnCards(int basicCost, int miniBossCost)
        {
            lunarEnemies = new List<DirectorCard>();

            List<CharacterSpawnCard> csc = new List<CharacterSpawnCard>();
            var lunarWispOrig = Addressables.LoadAssetAsync<CharacterSpawnCard>("RoR2/Base/LunarWisp/cscLunarWisp.asset").WaitForCompletion();
            var lunarGolemOrig = Addressables.LoadAssetAsync<CharacterSpawnCard>("RoR2/Base/LunarGolem/cscLunarGolem.asset").WaitForCompletion();
            var lunarExploderOrig = Addressables.LoadAssetAsync<CharacterSpawnCard>("RoR2/Base/LunarExploder/cscLunarExploder.asset").WaitForCompletion();

            var lunarWisp = Object.Instantiate(lunarWispOrig);
            var lunarGolem = Object.Instantiate(lunarGolemOrig);
            var lunarExploder = Object.Instantiate(lunarExploderOrig);
            lunarWisp.directorCreditCost = miniBossCost;
            lunarGolem.directorCreditCost = miniBossCost;
            lunarExploder.directorCreditCost = basicCost;
            csc.Add(lunarWisp);
            csc.Add(lunarGolem);
            csc.Add(lunarExploder);
            foreach (var ca in csc)
            {
                var dc = new DirectorCard()
                {
                    spawnCard = ca,
                    selectionWeight = stack,
                    minimumStageCompletions = 0,
                };
                lunarEnemies.Add(dc);
            }

        }
    }



}
