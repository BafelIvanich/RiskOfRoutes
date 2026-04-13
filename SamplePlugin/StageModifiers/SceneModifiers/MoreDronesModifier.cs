using RiskOfRoutes;
using RoR2;
using System.Linq;
using UnityEngine;


namespace SamplePlugin.StageModifiers.SceneModifiers
{
    public class MoreDronesModifier : StageModifier
    {

        public MoreDronesModifier(int stack)
        {
            this.stack = stack;
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
            int count = 0;
            var credits = ClassicStageInfo.instance.sceneDirectorInteractibleCredits;
            float percent = (RiskOfRoutes.RiskOfRoutes.droneStackPercent.Value) / 100f;
            Log.Info($"Drone stack percent is {percent}");
            int droneCredits = (int)(credits * stack * percent);

            Log.Info("Spawning more drones");

            var stageInfo = ClassicStageInfo.instance;
            WeightedSelection<DirectorCard> availableDrones = new WeightedSelection<DirectorCard>();
            var droneCategory = stageInfo.interactableCategories.categories.FirstOrDefault(c => c.name == "Drones");

            if (droneCategory.cards.Length == 0)
            {
                Log.Error("No drones available on stage");
            }

            foreach (var card in droneCategory.cards)
            {
                if (card.IsAvailable())
                    availableDrones.AddChoice(card, card.selectionWeight);
                //Log.Info($"Added {card.spawnCard.name}");
            }

            int num = 100;
            while (droneCredits > 0 && num > 0)
            {
                DirectorCard directorCard = SelectCard(availableDrones, droneCredits);
                if (directorCard == null)
                {
                    break;
                }
                if (!directorCard.IsAvailable())
                {
                    continue;
                }
                GameObject gameObject = TryPlaceObjectOnScene(directorCard.spawnCard, directorCard.spawnCard.name);
                if (gameObject != null)
                {
                    PurchaseInteraction component = gameObject.GetComponent<PurchaseInteraction>();
                    if ((bool)component && component.costType == CostTypeIndex.Money)
                    {
                        component.Networkcost = Run.instance.GetDifficultyScaledCost(component.cost);
                    }
                    droneCredits -= directorCard.cost;
                    count++;
                }
                num--;
            }
            Log.Info($"Spawned {count} more drones");


        }

        private static DirectorCard SelectCard(WeightedSelection<DirectorCard> deck, int maxCost)
        {
            var cardSelector = new WeightedSelection<DirectorCard>(maxCost);
            int i = 0;
            for (int count = deck.Count; i < count; i++)
            {
                WeightedSelection<DirectorCard>.ChoiceInfo choice = deck.GetChoice(i);
                //Log.Info($"Ratio:{choice.value.cost} {maxCost}");
                if (choice.value.cost <= maxCost)
                {
                    cardSelector.AddChoice(choice);
                    //Log.Info("ADDED CHOICE");
                }
            }
            Log.Info("SEELCTOR " + cardSelector.Count);
            if (cardSelector.Count == 0)
            {
                return null;
            }
            return cardSelector.Evaluate(Run.instance.stageRng.nextNormalizedFloat);
        }

    }



}
