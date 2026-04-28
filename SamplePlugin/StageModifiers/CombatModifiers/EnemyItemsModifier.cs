using RiskOfRoutes;
using RoR2;
using SamplePlugin.StageModifiers.SceneModifiers;


namespace SamplePlugin.StageModifiers.CombatModifiers
{
    //карты : 
    public class EnemyItemsModifier : StageModifier
    {
        ItemDef selectedItem;

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
            int itemStack = GetItemStack(selectedItem, 1);
            StageModifierDirector.instance.monsterTeamInventory.GiveItemPermanent(selectedItem, itemStack);

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

        //todo на будущее если сделаю стакинг предметов врагов
        public static int GetItemStack(ItemDef def, int modStack)
        {
            switch (def.tier)
            {
                case ItemTier.Tier1: return 5;
                case ItemTier.Tier2: return 3;
                case ItemTier.Tier3: return 1;
                case ItemTier.Boss: return 1;
                case ItemTier.VoidTier1: return 5;
                case ItemTier.VoidTier2: return 3;
                case ItemTier.VoidTier3: return 1;
                case ItemTier.VoidBoss: return 1;
                default: return 1;
            }

        }
    }



}
