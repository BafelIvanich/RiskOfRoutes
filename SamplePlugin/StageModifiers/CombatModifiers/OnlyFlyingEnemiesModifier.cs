using RiskOfRoutes;
using RoR2;
using System.Linq;

namespace SamplePlugin.StageModifiers.CombatModifiers
{
    public class OnlyFlyingEnemiesModifier : StageModifier
    {
        public OnlyFlyingEnemiesModifier()
        {

        }

        public override void OnEnd()
        {

        }

        public override void OnStart()
        {
            Log.Info("OnlyFlying on start");

            var categories = ClassicStageInfo.instance.monsterCategories.categories;
            if (categories == null)
            {
                Log.Error("Could not access categories");
                return;
            }
            var dccs = ClassicStageInfo.instance.monsterCategories;

            ClassicStageInfo.instance.monsterSelection.Clear();
            ClassicStageInfo.instance.monsterSelection = GetSelectionFromCategories(categories);

            Log.Info("Available");
            for (int i = 0; i < ClassicStageInfo.instance.monsterSelection.Count; i++)
            {
                var c = ClassicStageInfo.instance.monsterSelection.GetChoice(i);
                Log.Info($"\t{c.value.spawnCard.name}");
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
                var oldCtgCards = categories[i].cards;

                var newCtgCards = oldCtgCards.Where(card => card.spawnCard.nodeGraphType == RoR2.Navigation.MapNodeGroup.GraphType.Air).ToArray();
                foreach (var flCard in newCtgCards)
                {
                    Log.Info($"Flying enemy from Category:{categories[i].name}:{flCard.spawnCard.name}");
                }
                categories[i].cards = newCtgCards;


                var ctg = categories[i];
                float num = SumAllWeightsInCategory(ctg);
                float num2 = ctg.selectionWeight / num;
                if (!(num > 0f))
                {
                    continue;
                }
                DirectorCard[] cards = ctg.cards;
                foreach (DirectorCard directorCard in cards)
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


    }
}
