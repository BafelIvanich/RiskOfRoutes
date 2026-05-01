using RiskOfRoutes;
using RoR2;
using SamplePlugin.StageModifiers.SceneModifiers;


namespace SamplePlugin.StageModifiers.CombatModifiers
{
    //карты : 
    public class EnemyItemsModifier : StageModifier
    {
        ItemDef selectedItem;

        public EnemyItemsModifier(string internalName, int stack)
        {
            selectedItem = PrinterItemModifier.stringToItemDef(internalName);
            if (selectedItem == null) return;

            this.stack = stack;
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
            int itemStack = GetItemStack(selectedItem, stack);
            StageModifierDirector.instance.monsterTeamInventory.GiveItemPermanent(selectedItem, itemStack);
            //Log.Info($"MonsterItem modifier {itemStack}");
            //Log.Info($"Stack modifier {stack}");

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
                case ItemTier.Tier1: return (5 * modStack);
                case ItemTier.Tier2: return (3 * modStack);
                case ItemTier.Tier3: return (1 * modStack);
                case ItemTier.Boss: return (1 * modStack);
                case ItemTier.VoidTier1: return (5 * modStack);
                case ItemTier.VoidTier2: return (3 * modStack);
                case ItemTier.VoidTier3: return (1 * modStack);
                case ItemTier.VoidBoss: return (1 * modStack);
                default: return 1;
            }

        }
    }



}
