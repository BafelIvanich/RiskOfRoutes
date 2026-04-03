using RiskOfRoutes;
using RoR2;
using SamplePlugin.StageModifiers.SceneModifiers;


namespace SamplePlugin.StageModifiers.CombatModifiers
{
    //карты : 
    public class EnemyItemsModifier : StageModifier
    {
        ItemDef selectedItem;
        int commonCount = 5;
        int uncommonCount = 3;
        int legendaryCount = 1;

        public EnemyItemsModifier(string internalName)
        {
            selectedItem = PrinterItemModifier.stringToItemDef(internalName);
            if (selectedItem == null) return;

            stackable = false;
        }


        public override void OnEnd()
        {
            Log.Info("EnemyItems on end");
            SpawnCard.onSpawnedServerGlobal -= SpawnCard_onSpawnedServerGlobal;
            StageModifierDirector.instance.monsterTeamInventory.CleanInventory();
        }

        public override void OnStart()
        {
            Log.Info("EnemyItems on start");
            int count = 1;
            switch (selectedItem.tier)
            {
                case ItemTier.Tier1: count = commonCount; break;
                case ItemTier.Tier2: count = uncommonCount; break;
                case ItemTier.Tier3: count = legendaryCount; break;
                default: count = 1; break;
            }
            StageModifierDirector.instance.monsterTeamInventory.GiveItemPermanent(selectedItem, count);

            SpawnCard.onSpawnedServerGlobal += SpawnCard_onSpawnedServerGlobal;
        }

        private void SpawnCard_onSpawnedServerGlobal(SpawnCard.SpawnResult spawnResult)
        {
            CharacterMaster characterMaster = (spawnResult.spawnedInstance ? spawnResult.spawnedInstance.GetComponent<CharacterMaster>() : null);
            if ((bool)characterMaster && characterMaster.teamIndex == TeamIndex.Monster)
            {
                characterMaster.inventory.AddItemsFrom(StageModifierDirector.instance.monsterTeamInventory);
            }
        }
    }



}
