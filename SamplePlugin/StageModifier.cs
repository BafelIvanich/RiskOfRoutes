using RiskOfRoutes;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using RoR2;
using UnityEngine.AddressableAssets;
using R2API.Utils;
using System.Reflection;

//whites: crowbar, bears, bleed, bombs, watches, gasoline, focus crystal, syringe, key
//greens: reload one, ukulele, active charge, wisps, band 1, band 2, injection, pinwheel, fin, star, rockets, energy shield, gasoline, wire


namespace SamplePlugin
{
    public abstract class StageModifier
    {
        public string activeText;
        public string reservedText;

        public int cost;

        public bool negative;

        public abstract void OnStart();

        public abstract void OnEnd();

    }


    //available artifacts would be: dissonance, frailty, honor,soul, spite, vengeance
    public class ArtifactModifier : StageModifier
    {
        private ArtifactDef currentArtifact;

        private static readonly List<string> ArtifactNames = new List<string>()
        {
            "MixEnemy", //Dissonance
            "WeakAssKnees", //Frailty
            "ElityOnly", //Honor
            "WispOnDeath", //Soul
            "Bomb", //Spite
            "ShadowClone", //Vengeance
            "FriendlyFire"
        };


        //TODO: сделать Struct артефактов может быть
        private static readonly Dictionary<string,string> ArtifactDesc = new Dictionary<string, string>()
        {
            { "ARTIFACT_MIXENEMY_DESCRIPTION","<style=cWorldEvent>Monsters can appear outside their usual environments.</style>" },
            { "ARTIFACT_WEAKASSKNEES_DESCRIPTION","<style=cWorldEvent>Fall damage is doubled and lethal.</style>" },
            { "ARTIFACT_ELITEONLY_DESCRIPTION","<style=cWorldEvent>Enemies can only spawn as elites.</style>" },
            { "ARTIFACT_WISPONDEATH_DESCRIPTION","<style=cWorldEvent>Wisps emerge from defeated monsters.</style>" },
            { "ARTIFACT_BOMB_DESCRIPTION","<style=cWorldEvent>Enemies drop multiple exploding bombs on death.</style>" },
            { "ARTIFACT_SHADOWCLONE_DESCRIPTION","<style=cWorldEvent>Your relentless doppelganger will invade on every 10th minute.</style>" },
            { "ARTIFACT_FRIENDLYFIRE_DESCRIPTION","<style=cWorldEvent>Friendly fire is enabled for both survivors and monsters alike.</style>" },
        };

        public ArtifactModifier()
        {
            currentArtifact = ArtifactCatalog.FindArtifactDef(GetRandomArtifactName()); 
            Log.Info("1");
            this.activeText = ArtifactDesc[currentArtifact.descriptionToken];
            Log.Info("2");
            this.reservedText = activeText;
            negative = true;
        }

        public override void OnEnd()
        {
            Log.Info("Mehod");
            if(currentArtifact)
            {
                RoR2.RunArtifactManager.instance.SetArtifactEnabled(currentArtifact, false);
                Log.Info("Succesfully false");
            }
            else
            {
                Log.Info("No current art");
            }
        }

        public override void OnStart()
        {
            
            Log.Info("ArtifactModifier start");
            if (currentArtifact)
            {
                RoR2.RunArtifactManager.instance.SetArtifactEnabled(currentArtifact, true);
                RoR2.Chat.AddMessage("Art enabled");
            }
            else
            {
                Log.Info("No such artifact");
            }
        }

        //На время
        private string GetRandomArtifactName()
        {
            if(Run.instance)
            {
                int index = Run.instance.stageRng.RangeInt(0,ArtifactNames.Count);
                return ArtifactNames[index];
            }
            else
            {
                return ArtifactNames[0];
            }
        }

    }

    public class BossItemModifier : StageModifier
    {

        public BossItemModifier()
        {
            this.activeText = "Boss will drop yellow item on this stage.";
            this.reservedText = "Boss is guaranteed to drop <style=cShrine>Yellow Item.</style>";
            negative = false;
            
        }
        public override void OnEnd()
        {
            On.RoR2.BossGroup.DropRewards -= BossGroup_DropRewards;
            Log.Info("BossItemModifier end");
        }

        public override void OnStart()
        {
            On.RoR2.BossGroup.DropRewards += BossGroup_DropRewards;
            Log.Info("BossItemModifier start");
            RoR2.Chat.AddMessage("Boss would drop yellow");

        }


        private void BossGroup_DropRewards(On.RoR2.BossGroup.orig_DropRewards orig, RoR2.BossGroup self)
        {
            Log.Info("Active");
            var bossDropTable = ScriptableObject.CreateInstance<RoR2.BasicPickupDropTable>();
            self.bossDropChance = 1.0f;
            bossDropTable.tier1Weight = -0f;  
            bossDropTable.tier2Weight = 0f; 
            bossDropTable.tier3Weight = 0f; 
            bossDropTable.bossWeight = 1f;  
            bossDropTable.lunarItemWeight = 0f;
            bossDropTable.voidTier1Weight = 0f;
            bossDropTable.voidTier2Weight = 0f;
            bossDropTable.voidTier3Weight = 0f;
            bossDropTable.voidBossWeight = 0f;

            var originalDropTable = self.dropTable;
            self.dropTable = bossDropTable;
            
            orig(self);

            Log.Info(self.dropTable.ToString());
        }



    }

    public class PrinterItemModifier : StageModifier
    {
        public PrinterItemModifier()
        {
            this.activeText = "PrinterItemModifier";
            negative = false;
        }
        public override void OnEnd()
        {
            Log.Info("PrinterItemModifier end");
        }

        public override void OnStart()
        {
            Log.Info("PrinterItemModifier start");
        }
    }

    public class MoreDronesModifier : StageModifier
    {
        public MoreDronesModifier()
        {
            this.activeText = "MoreDronesModifier";
            this.reservedText = "More <style=cIsUtility>drones</style> will appear on next stage.";
            negative = false;
        }
        public override void OnEnd()
        {
            Log.Info("MoreDronesModifier end");
        }

        public override void OnStart()
        {
            Log.Info("MoreDronesModifier start");
            On.RoR2.SceneDirector.Start += SceneDirector_Start;
        }

        private void SceneDirector_Start(On.RoR2.SceneDirector.orig_Start orig, SceneDirector self)
        {
            //отсюда все возможные для спавна интерктаблы
            var stageInfo = ClassicStageInfo.instance;

            var categories = stageInfo.interactableCategories.categories;

            for (int i = 0;i < categories.Length;i++)
            {
                //TODO stageInfo.interactableDccsPool. --- interactableCategories помечен как deprecated стоит посмотреть что за DCCS
                //еще есть             stageInfo.GetInteractableDccsPool
                var category = stageInfo.interactableCategories.categories[i];
                Log.Info($"{category.name}:{category.selectionWeight}");
                if (categories[i].name == "Drones")
                {
                    Log.Info("Drones category available");
                    categories[i].selectionWeight = 1000f;
                    foreach(var card in category.cards)
                    {
                        string initialCost = card.spawnCard.directorCreditCost.ToString();
                        string initialWeight = card.selectionWeight.ToString();

                        card.spawnCard.directorCreditCost = 0;
                        card.selectionWeight = 1000;
                        var isc = card.spawnCard as InteractableSpawnCard;
                        if (isc)
                        {
                            isc.maxSpawnsPerStage = -1;
                            isc.occupyPosition = false;
                        }
                        else
                        {
                            Log.Error("GO");
                        }
                            Log.Info($"Succesfully set price of {card.spawnCard.name} from {initialCost} to {0}");
                            Log.Info($"Succesfully set weight of {card.spawnCard.name} from {initialWeight} to {1000}");
                    }
                }
            }

            //TODO проверить возвращаются ли цены по умолчанию или стоит это делать вручную

            Log.Info("Done modifying drone cost");

            orig(self);
        }
    }

    public class YellowPrinterModifier : StageModifier
    {
        public static InteractableSpawnCard yellowPrinterSpawnCard;

        public YellowPrinterModifier()
        {
            CreateYellowPrinterSpawnCard();
            this.reservedText = "<style=cShrine>Yellow printer</style> is guaranteed to appear somewhere on the next stage.";
            this.activeText = "<style=cShrine>Yellow printer</style> has appeared somewhere on the stage.";
            negative = false;
        }
        public override void OnEnd()
        {
            Log.Info("YellowPrinterModifier end");
        }

        public override void OnStart()
        {
            Log.Info("YellowPrinterModifier start");
            SpawnYellowPrinter();
        }

        private void SpawnYellowPrinter()
        {
            Log.Info("Trying to place Yellow Printer");

            if (!NetworkServer.active)
            {
                Log.Error("SERVER IS NOT ACTIVE");
                return;
            }

            if (yellowPrinterSpawnCard == null)
            {
                Log.Error("Something wrong with card");
                return;
            }

            RoR2.DirectorPlacementRule placementRule = new RoR2.DirectorPlacementRule
            {
                placementMode = RoR2.DirectorPlacementRule.PlacementMode.Random
            };

            RoR2.DirectorSpawnRequest spawnRequest = new RoR2.DirectorSpawnRequest(
                yellowPrinterSpawnCard,
                placementRule,
                RoR2.Run.instance.stageRng
            );

            GameObject redPrinter = RoR2.DirectorCore.instance.TrySpawnObject(spawnRequest);
            if (redPrinter)
            {
                Log.Info($"Yellow printer appeared at {redPrinter.transform.position} on stage");
            }
            else
            {
                Log.Error($"Couldn't place yellow printer on scene");
            }
        }

        private void CreateYellowPrinterSpawnCard()
        {
            InteractableSpawnCard card = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/Base/DuplicatorWild/iscDuplicatorWild.asset").WaitForCompletion();

            if (card != null)
            {
                Log.Info("Yellow printer spawncard created succesfully");
                yellowPrinterSpawnCard = card;
            }
            else
            {
                Log.Info("Asset path is invalid");
            }
        }


    }

    public class RedPrinterModifier : StageModifier
    {
        public static InteractableSpawnCard redPrinterSpawnCard;

        public RedPrinterModifier()
        {
            CreateRedPrinterSpawnCard();
            this.reservedText = "<style=cDeath>Red printer</style> is guaranteed to appear somewhere on the next stage.";
            this.activeText = "<style=cDeath>Red printer</style> has appeared somewhere on the stage.";
            negative = false;
        }
        public override void OnEnd()
        {
            Log.Info("RedPrinterModifier end");
        }

        public override void OnStart()
        {
            Log.Info("RedPrinterModifier start");
            SpawnRedPrinter();
        }

        private void SpawnRedPrinter()
        {
            Log.Info("Trying to place Red Printer");

            if (!NetworkServer.active)
            {
                Log.Error("SERVER IS NOT ACTIVE");
                return;
            }

            if (redPrinterSpawnCard == null)
            {
                Log.Error("Something wrong with card");
                return;
            }

            RoR2.DirectorPlacementRule placementRule = new RoR2.DirectorPlacementRule
            {
                placementMode = RoR2.DirectorPlacementRule.PlacementMode.Random
            };

            RoR2.DirectorSpawnRequest spawnRequest = new RoR2.DirectorSpawnRequest(
                redPrinterSpawnCard,
                placementRule,
                RoR2.Run.instance.stageRng
            );

            GameObject redPrinter = RoR2.DirectorCore.instance.TrySpawnObject(spawnRequest);
            if (redPrinter)
            {
                Log.Info($"Red printer appeared at {redPrinter.transform.position} on stage");
            }
            else
            {
                Log.Error($"Couldn't place red printer on scene");
            }
        }

        private void CreateRedPrinterSpawnCard()
        {
            InteractableSpawnCard card = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/Base/DuplicatorMilitary/iscDuplicatorMilitary.asset").WaitForCompletion();

            if (card != null)
            {
                Log.Info("Red printer spawncard created succesfully");
                redPrinterSpawnCard = card;
            }
            else
            {
                Log.Info("Asset path is invalid");
            }
        }


    }

    public class WanderingChefModifier : StageModifier
    {

        public static RoR2.InteractableSpawnCard chefSpawnCard;

        public WanderingChefModifier()
        {
            CreateChefSpawnCard();
            this.activeText = "WanderingChefModifier";
            this.reservedText = "<style=cIsHealth>Wandering CHEF</style> is guaranteed to appear on next stage.";
            negative = false;
        }

        public override void OnEnd()
        {
        }

        public override void OnStart()
        {
            SpawnWanderingChef();
            CombatDirector.baseEliteCostMultiplier = 10.1f;
        }

        private void SpawnWanderingChef()
        {
            Log.Info("Trying to place CHEF");

            if(!NetworkServer.active)
            {
                Log.Error("SERVER IS NOT ACTIVE");
                return;
            }

            RoR2.InteractableSpawnCard card = chefSpawnCard;

            if(card == null)
            {
                Log.Error("Something wrong with card");
                return;
            }

            RoR2.DirectorPlacementRule placementRule = new RoR2.DirectorPlacementRule
            {
                placementMode = RoR2.DirectorPlacementRule.PlacementMode.Random
            };

            RoR2.DirectorSpawnRequest spawnRequest = new RoR2.DirectorSpawnRequest(
                card,
                placementRule,
                RoR2.Run.instance.stageRng
            );

            GameObject wanderingChef = RoR2.DirectorCore.instance.TrySpawnObject( spawnRequest );
            if(wanderingChef)
            {
                Log.Info($"Wandering Chef appeared at {wanderingChef.transform.position} on stage");

                wanderingChef.transform.position += wanderingChef.transform.up * 1.10f;
            }
            else
            {
                Log.Error($"Couldn't place chef on scene");
            }

            

        }

        private void CreateChefSpawnCard()
        {
            Log.Info("Trying to create CHEF");
            RoR2.InteractableSpawnCard card = ScriptableObject.CreateInstance<RoR2.InteractableSpawnCard>();

            card.prefab = RiskOfRoutes.RiskOfRoutes.chefPrefab;
            card.directorCreditCost = 0;
            card.sendOverNetwork = true;
            card.hullSize = RoR2.HullClassification.Human;
            card.orientToFloor = true;
            card.nodeGraphType = RoR2.Navigation.MapNodeGroup.GraphType.Ground;

            chefSpawnCard = card;
        }
    }

    //не артефакт, элитки становятся дешевле для директора
    public class MoreElitesModifier : StageModifier
    {
        public MoreElitesModifier()
        {
            this.activeText = "MoreElitesModifier";
            negative = false;
        }
        public override void OnEnd()
        {
            Log.Info("MoreElitesModifier end");
        }

        public override void OnStart()
        {
            Log.Info("MoreElitesModifier start");
        }
    }

    public class NoOneShotProtectionModifier : StageModifier
    {
        public override void OnEnd()
        {
            throw new System.NotImplementedException();
        }

        public override void OnStart()
        {
            throw new System.NotImplementedException();
        }
    }

    public class VoidEnemiesModifier
    {

    }

    public class LunarEnemiesModifier
    {

    }

}
