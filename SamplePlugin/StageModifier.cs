using RiskOfRoutes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using BepInEx.Configuration;
using BepInEx;
using JetBrains.Annotations;
using R2API;
using RoR2;
using RoR2.UI;
using SamplePlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;



namespace SamplePlugin
{
    public abstract class StageModifier
    {
        public string activeText;
        public string reservedText;

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
            "Dissonance",
            "Frailty",
            "Honor",
            "Soul",
            "Spite",
            "Vengeance"
        };

        public ArtifactModifier()
        {
            this.activeText = "ArtifactModifier";
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
            ArtifactDef art = ArtifactCatalog.FindArtifactDef("Devotion");
            if (art)
            {
                RoR2.RunArtifactManager.instance.SetArtifactEnabled(art, true);
                RoR2.Chat.AddMessage("Art enabled");
                currentArtifact = art;
            }
            else
            {
                Log.Info("No such artifact");
            }
        }

    }

    public class BossItemModifier : StageModifier
    {

        public BossItemModifier()
        {
            this.activeText = "Boss will drop yellow item on this stage.";
            this.reservedText = "Boss is guaranteed to drop <style=cIsDamage>Yellow Item.</style>";
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
            self.bossDropChance = 1.0f;

            orig(self);

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

    public class WanderingChefModifier : StageModifier
    {

        public static RoR2.InteractableSpawnCard chefSpawnCard;

        public WanderingChefModifier()
        {
            CreateChefSpawnCard();
            this.activeText = "WanderingChefModifier";
            negative = false;
        }

        public override void OnEnd()
        {
        }

        public override void OnStart()
        {
            SpawnWanderingChef();
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

                wanderingChef.transform.position += wanderingChef.transform.up * 1.2f;
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
}
