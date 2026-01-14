using RiskOfRoutes;
using RoR2;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;


namespace SamplePlugin.StageModifiers
{
    public class MoreDronesModifier : StageModifier
    {

        public MoreDronesModifier()
        {
            name = "More drones";
            activeText = "More <style=cIsUtility>drones</style> will appear on this stage.";
            reservedText = "More <style=cIsUtility>drones</style> will appear on next stage.";
            icon = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texDroneIconOutlined.png").WaitForCompletion();
            negative = false;
        }

        public override void OnEnd()
        {
            Log.Info("MoreDronesModifier end");
        }

        public override void OnStart()
        {
            Log.Info("MoreDronesModifier start");
            PopulateDrones();
        }

        public void PopulateDrones()
        {
            var credits = ClassicStageInfo.instance.sceneDirectorInteractibleCredits;
            int droneCredits = (int)(credits * stack*0.15);


            //tood nullcheck
            Log.Info("Spawning more drones");

            var stageInfo = ClassicStageInfo.instance;
            WeightedSelection<DirectorCard> availableDrones = new WeightedSelection<DirectorCard>();
            var droneCategory = stageInfo.interactableCategories.categories.FirstOrDefault(c => c.name == "Drones");
            foreach(var card in droneCategory.cards)
            {
                availableDrones.AddChoice(card,card.selectionWeight);
                //Log.Info($"Added {card.spawnCard.name}");
            }

            while (droneCredits > 0)
            {
                DirectorCard directorCard = SelectCard(availableDrones, droneCredits);
                if(directorCard == null)
                {
                    //Log.Info("Breal");
                    break;
                }
                if(!directorCard.IsAvailable())
                {
                    continue;
                }
                droneCredits -= directorCard.cost;
                if(!Run.instance)
                {
                    continue;
                }
                for (int i = 0; i < 10; i++)
                {
                    //Log.Info("Seucceeeeeding");
                    DirectorPlacementRule placementRule = new DirectorPlacementRule
                    {
                        placementMode = DirectorPlacementRule.PlacementMode.Random
                    };
                    GameObject gameObject = DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(directorCard.GetSpawnCard(), placementRule, Run.instance.stageRng));
                    if ((bool)gameObject)
                    {
                        PurchaseInteraction component = gameObject.GetComponent<PurchaseInteraction>();
                        if ((bool)component && component.costType == CostTypeIndex.Money)
                        {
                            component.Networkcost = Run.instance.GetDifficultyScaledCost(component.cost);
                        }
                        Log.Info($"DroneModifier: Succesfully placed drone {gameObject.name} at {gameObject.transform.position}");
                        break;
                    }
                    else
                    {
                        Log.Info($"Failed to place drone {directorCard.spawnCard.name}");
                    }
                }
            }


        }

        private static DirectorCard SelectCard(WeightedSelection<DirectorCard> deck, int maxCost)
        {
            var cardSelector = new WeightedSelection<DirectorCard>(maxCost);
            int i = 0;
            for (int count = deck.Count; i < count; i++)
            {
                WeightedSelection<DirectorCard>.ChoiceInfo choice = deck.GetChoice(i);
                Log.Info($"Ratio:{choice.value.cost} {maxCost}");
                if (choice.value.cost <= maxCost)
                {
                    cardSelector.AddChoice(choice);
                    Log.Info("ADDED CHOICE");
                }
            }
            Log.Info("SEELCTOR " + cardSelector.Count);
            if (cardSelector.Count == 0)
            {
                return null;
            }
            return cardSelector.Evaluate(Run.instance.stageRng.nextNormalizedFloat);
        }

        //private void DecreaseDronePlacementWeight()
        //{
        //    var stageInfo = ClassicStageInfo.instance;
        //    var categories = stageInfo.interactableCategories.categories;
        //    float temp = 0;
        //    for (int i = 0; i < categories.Length; i++)
        //    {
        //        var category = stageInfo.interactableCategories.categories[i];
        //        Log.Info($"{category.name}:{category.selectionWeight}");
        //        temp = category.selectionWeight;
        //        if (categories[i].name == "Drones")
        //        {
        //            Log.Info("Drones category available");
        //            categories[i].selectionWeight /= 3;
        //            Log.Info($"Done decreasing drone weight:{temp} -> {category.selectionWeight}");
        //            return;
        //        }
        //    }

        //}

        //первый вариант реализации вес категории дронов увеличивается
        //private void IncreaseDronePlacementWeight()
        //{
        //    //отсюда все возможные для спавна интерктаблы
        //    var stageInfo = ClassicStageInfo.instance;
        //    var categories = stageInfo.interactableCategories.categories;
        //    float temp = 0;
        //    for (int i = 0; i < categories.Length; i++)
        //    {
        //        //TODO stageInfo.interactableDccsPool. --- interactableCategories помечен как deprecated стоит посмотреть что за DCCS
        //        //еще есть             stageInfo.GetInteractableDccsPool
        //        var category = stageInfo.interactableCategories.categories[i];
        //        Log.Info($"{category.name}:{category.selectionWeight}");
        //        temp = category.selectionWeight;
        //        if (categories[i].name == "Drones")
        //        {
        //            Log.Info("Drones category available");
        //            categories[i].selectionWeight *= 30;
        //            Log.Info($"Done increasing drone weight:{temp} -> {category.selectionWeight}");
        //            return;
        //            //Удешевляет каждого отдельного дрона, не факт что нужно, сверху просто повышается приоритет покупки дронов
        //            //foreach (var card in category.cards)
        //            //{
        //            //    string initialCost = card.spawnCard.directorCreditCost.ToString();
        //            //    string initialWeight = card.selectionWeight.ToString();

        //            //    card.spawnCard.directorCreditCost = 0;
        //            //    card.selectionWeight = 1000;
        //            //    var isc = card.spawnCard as InteractableSpawnCard;
        //            //    if (isc)
        //            //    {
        //            //        isc.maxSpawnsPerStage = -1;
        //            //        isc.occupyPosition = false;
        //            //    }
        //            //    else
        //            //    {
        //            //        Log.Error("GO");
        //            //    }
        //            //    Log.Info($"Succesfully set price of {card.spawnCard.name} from {initialCost} to {0}");
        //            //    Log.Info($"Succesfully set weight of {card.spawnCard.name} from {initialWeight} to {1000}");
        //            //}
        //        }
        //    }
        //}

        private void SceneDirector_Start(On.RoR2.SceneDirector.orig_Start orig, SceneDirector self)
        {

        }
    }

    

}
