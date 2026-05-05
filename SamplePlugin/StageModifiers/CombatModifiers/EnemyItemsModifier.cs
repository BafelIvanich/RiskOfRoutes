using RiskOfRoutes;
using RoR2;
using SamplePlugin.StageModifiers.SceneModifiers;
using UnityEngine;


namespace SamplePlugin.StageModifiers.CombatModifiers
{
    //todo needs to be made separate 'EnemyItems modifier' so it wont overweight others in catalog
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

        //todo extra stack calculating logic for portal panel doesnt work on clients
        public static int GetItemStack(ItemDef def, int modStack)
        {
            int commonItemStack = StageModifierDirector.instance?.commonDefaultStack ?? 5;
            int uncommonItemStack = StageModifierDirector.instance?.uncommonDefaultStack ?? 3;
            int legendaryItemStack = StageModifierDirector.instance?.legendaryDefaultStack ?? 1;
            int bossItemStack = StageModifierDirector.instance?.bossDefaultStack ?? 1;

            int commonExtraStack = StageModifierDirector.instance?.commonExtraRange ?? 3;
            int uncommonExtraStack = StageModifierDirector.instance?.uncommonExtraRange ?? 2;
            int legendaryExtraStack = StageModifierDirector.instance?.legendaryExtraRange ?? 1;
            int bossExtraStack = StageModifierDirector.instance?.bossExtraRange ?? 1;

            int chanceForExtra = StageModifierDirector.instance?.chanceForExtra ?? 0;
            bool shouldAddExtra = false;

            if(Run.instance!=null && Run.instance.stageRng!=null)
            {
                shouldAddExtra = Run.instance.stageRng.nextNormalizedFloat < chanceForExtra;
            }

            switch (def.tier)
            {
                case ItemTier.Tier1: return (commonItemStack * modStack) + (shouldAddExtra ? commonExtraStack : 0);
                case ItemTier.Tier2: return (uncommonItemStack * modStack) + (shouldAddExtra ? uncommonExtraStack : 0);
                case ItemTier.Tier3: return (legendaryItemStack * modStack) + (shouldAddExtra ? legendaryExtraStack : 0);
                case ItemTier.Boss: return (bossItemStack * modStack) + (shouldAddExtra ? bossItemStack : 0);
                case ItemTier.VoidTier1: return (commonItemStack * modStack) + (shouldAddExtra ? commonExtraStack : 0);
                case ItemTier.VoidTier2: return (uncommonItemStack * modStack) + (shouldAddExtra ? uncommonExtraStack : 0);
                case ItemTier.VoidTier3: return (legendaryItemStack * modStack) + (shouldAddExtra ? legendaryExtraStack : 0);
                case ItemTier.VoidBoss: return (bossItemStack * modStack) + (shouldAddExtra ? bossItemStack : 0);
                default: return 1;
            }

        }
    }



}
