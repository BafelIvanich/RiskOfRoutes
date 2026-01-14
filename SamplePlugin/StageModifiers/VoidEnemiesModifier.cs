using RiskOfRoutes;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;


namespace SamplePlugin.StageModifiers
{
    //карты: iscVoidBarnacle, iscNullifier, iscVoidJailer, iscVoidMegaCrab
    public class VoidEnemiesModifier : StageModifier
    {
        public List<DirectorCard> voidEnemies;
        public VoidEnemiesModifier()
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
        }

        public override void OnEnd()
        {
            Log.Info("Void invasion on end");
        }

        public override void OnStart()
        {
            int maxCost = 200;
            int minCost = 40;

            if (!ClassicStageInfo.instance || ClassicStageInfo.instance.monsterSelection == null)
            {
                Log.Error("No monster selection found");
                return;
            }

            var selection = ClassicStageInfo.instance.monsterSelection;
            for (int i = 0; i < selection.Count; i++)
            {
                var card = selection.GetChoice(i);
                if (card.value != null && card.value.spawnCard != null)
                {
                    Log.Info($"Card:{card.value.spawnCard.name}, Weight:{card.weight}, Cost:{card.value.cost}");
                }
            }

            for (int i = 0; i < selection.Count; i++)
            {
                var card = selection.GetChoice(i);
                var sc = card.value.spawnCard;
                Log.Info($"Card:{sc.name},weight:{card.weight},cost:{card.value.cost}");
                maxCost = Mathf.Max(card.value.cost, maxCost);
                minCost = Mathf.Min(card.value.cost, minCost);
            }
            //200 40 предельные значения
            int finalMaxCost = Mathf.Min(200, maxCost);
            int finalMinCost = Mathf.Max(40, minCost);
            CreateLunarEnemiesSpawnCards(finalMinCost, finalMaxCost);
            Log.Info("Moon modifier start");
            if (ClassicStageInfo.instance && ClassicStageInfo.instance.monsterSelection != null)
            {
                foreach (var voidEnemy in voidEnemies)
                {
                    ClassicStageInfo.instance.monsterSelection.AddChoice(voidEnemy, (float)(voidEnemy.selectionWeight * (1 + 0.2 * stack)));
                }
            }



            foreach (var director in CombatDirector.instancesList)
            {
                if (director.monsterCards == null) director.currentMonsterCard = null;
            }
        }

        public void CreateLunarEnemiesSpawnCards(int maxCost, int minCost)
        {
            voidEnemies = new List<DirectorCard>();

            List<CharacterSpawnCard> csc = new List<CharacterSpawnCard>();
            var voidJailerOrig = Addressables.LoadAssetAsync<CharacterSpawnCard>("RoR2/DLC1/Characters/VoidJailer/cscVoidJailer.asset").WaitForCompletion();
            var voidMegaCrabOrig = Addressables.LoadAssetAsync<CharacterSpawnCard>("RoR2/DLC1/Characters/VoidMegaCrab/cscVoidMegaCrab.asset").WaitForCompletion();
            var voidReaverOrig = Addressables.LoadAssetAsync<CharacterSpawnCard>("RoR2/Base/Nullifier/cscNullifier.asset").WaitForCompletion();

            var voidJailer = ScriptableObject.Instantiate(voidJailerOrig);
            var voidMegaCrab = ScriptableObject.Instantiate(voidMegaCrabOrig);
            var voidReaver = ScriptableObject.Instantiate(voidReaverOrig);

            voidJailer.directorCreditCost = maxCost; //200
            voidMegaCrab.directorCreditCost = maxCost; //200
            voidReaver.directorCreditCost= maxCost; //200
            csc.Add(voidJailer);
            csc.Add(voidMegaCrab);
            csc.Add(voidReaver);
            foreach (var ca in csc)
            {
                var dc = new DirectorCard()
                {
                    spawnCard = ca,
                    selectionWeight = 1,
                    minimumStageCompletions = 0,
                    
                };
                voidEnemies.Add(dc);
            }

        }
    }



}
