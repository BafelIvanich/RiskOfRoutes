using RiskOfRoutes;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;


namespace SamplePlugin.StageModifiers.CombatModifiers
{
    //карты: iscVoidBarnacle, iscNullifier, iscVoidJailer, iscVoidMegaCrab
    public class VoidEnemiesModifier : StageModifier
    {
        public List<DirectorCard> voidEnemies;
        public VoidEnemiesModifier(int cost)
        {
            name = "Void invasion";
            reservedText = "<style=cIsVoid>Enemies from the void</style> might appear on the next stage.";
            activeText = "<style=cIsVoid>Enemies from the void</style> might appear on this stage.";
            var icon = Addressables.LoadAssetAsync<Sprite>("RoR2/DLC1/GameModes/InfiniteTowerRun/ITAssets/texITWaveVoidIcon.png").WaitForCompletion();
            if (!icon)
            {
                Log.Info("GFGG");
            }
            else
            {
                this.icon = icon;
            }
            negative = true;
            this.cost = cost;
        }

        public override void OnEnd()
        {
            Log.Info("Void invasion on end");
        }

        public override void OnStart()
        {
            Log.Info("Void invasion modifier start");

            int basicCost = 0;
            int miniBossCost = 0;
            int champCost = 0;
            //TODO breaks on loop
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
                if (ctg.name == "Champions")
                {
                    foreach (var card in ctg.cards)
                    {
                        champCost = Mathf.Max(champCost, card.cost);
                    }
                }
            }

            if (basicCost == 0) basicCost = 40;
            if (miniBossCost == 0) miniBossCost = 200;
            if (champCost == 0 || champCost > 800) champCost = 800;

            CreateVoidEnemiesSpawnCard(basicCost, miniBossCost, champCost);
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
                    HG.ArrayUtils.ArrayAppend(ref categories[i].cards, voidEnemies[2]);
                    Log.Info($"Succesfully added {voidEnemies[2].spawnCard.name} to category {categories[i].name}");
                    HG.ArrayUtils.ArrayAppend(ref categories[i].cards, voidEnemies[3]);
                    Log.Info($"Succesfully added {voidEnemies[3].spawnCard.name} to category {categories[i].name}");
                }
                else if (categories[i].name == "Minibosses")
                {
                    HG.ArrayUtils.ArrayAppend(ref categories[i].cards, voidEnemies[0]);
                    Log.Info($"Succesfully added {voidEnemies[0].spawnCard.name} to category {categories[i].name}");
                }
                else if (categories[i].name == "Champions")
                {
                    HG.ArrayUtils.ArrayAppend(ref categories[i].cards, voidEnemies[1]);
                    Log.Info($"Succesfully added {voidEnemies[1].spawnCard.name} to category {categories[i].name}");

                }

                //
                var ctg = categories[i];
                float num = SumAllWeightsInCategory(ctg);
                float num2 = categories[i].selectionWeight / num;
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


        public void CreateVoidEnemiesSpawnCard(int basicCost, int eliteCost, int championCost)
        {
            voidEnemies = new List<DirectorCard>();

            List<CharacterSpawnCard> csc = new List<CharacterSpawnCard>();
            var voidJailerOrig = Addressables.LoadAssetAsync<CharacterSpawnCard>("RoR2/DLC1/VoidJailer/cscVoidJailer.asset").WaitForCompletion();
            var voidMegaCrabOrig = Addressables.LoadAssetAsync<CharacterSpawnCard>("RoR2/DLC1/VoidMegaCrab/cscVoidMegaCrab.asset").WaitForCompletion();
            var voidReaverOrig = Addressables.LoadAssetAsync<CharacterSpawnCard>("RoR2/Base/Nullifier/cscNullifier.asset").WaitForCompletion();
            var voidBarnacleOrig = Addressables.LoadAssetAsync<CharacterSpawnCard>("RoR2/DLC1/VoidBarnacle/cscVoidBarnacle.asset").WaitForCompletion();

            var voidJailer = Object.Instantiate(voidJailerOrig);
            var voidMegaCrab = Object.Instantiate(voidMegaCrabOrig);
            var voidReaver = Object.Instantiate(voidReaverOrig);
            var voidBarnacle = Object.Instantiate(voidBarnacleOrig);
            voidJailer.directorCreditCost = eliteCost; //200
            voidMegaCrab.directorCreditCost = championCost; //200
            voidReaver.directorCreditCost = eliteCost; //200
            voidBarnacle.directorCreditCost = basicCost;

            csc.Add(voidJailer);
            csc.Add(voidMegaCrab);
            csc.Add(voidReaver);
            csc.Add(voidBarnacle);
            foreach (var ca in csc)
            {
                var dc = new DirectorCard()
                {
                    spawnCard = ca,
                    selectionWeight = stack,
                    minimumStageCompletions = 0,
                };
                voidEnemies.Add(dc);
            }

        }
    }



}
