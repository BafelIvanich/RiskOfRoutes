using IL.RoR2;
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
        public string text;
        public bool negative;

        public abstract void OnStart();

        public abstract void OnEnd();


    }


    //available artifacts would be: devotion, dissonance, frailty, honor,soul, spite, vengeance
    public class ArtifactModifier : StageModifier
    {
        public ArtifactModifier()
        {
            this.text = "ArtifactModifier";
            negative = true;
        }

        public override void OnEnd()
        {
            Log.Info("ArtifactModifier end");
        }

        public override void OnStart()
        {
            Log.Info("ArtifactModifier start");
            //RoR2.RunArtifactManager.instance.SetArtifactEnabled();
            RoR2.Chat.AddMessage("Art enabled");
        }
    }

    public class BossItemModifier : StageModifier
    {
        public BossItemModifier()
        {
            this.text = "BossItemModifier";
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
            self.bossDropChance = 100.0f;
            orig(self);

        }



    }

    public class PrinterItemModifier : StageModifier
    {
        public PrinterItemModifier()
        {
            this.text = "PrinterItemModifier";
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
            this.text = "WanderingChefModifier";
            negative = false;
        }

        public override void OnEnd()
        {
            On.RoR2.SceneDirector.Start -= AddWanderingChefToScene;
        }

        public override void OnStart()
        {
            On.RoR2.SceneDirector.Start += AddWanderingChefToScene;
        }

        private void AddWanderingChefToScene(On.RoR2.SceneDirector.orig_Start orig, RoR2.SceneDirector self)
        {
            Log.Info("TRUING TO PLACE CHEF CREAET");
            orig(self);


            RoR2.InteractableSpawnCard card = chefSpawnCard;

            RoR2.DirectorPlacementRule placementRule = new RoR2.DirectorPlacementRule
            {
                placementMode = RoR2.DirectorPlacementRule.PlacementMode.Random
            };

            RoR2.DirectorSpawnRequest spawnRequest = new RoR2.DirectorSpawnRequest(
                card,
                placementRule,
                self.rng
            );

            GameObject wanderingChef = RoR2.DirectorCore.instance.TrySpawnObject( spawnRequest );
            if(wanderingChef)
            {
                Log.Info($"Wandering Chef appeared at {wanderingChef.transform.position} on stage");

                wanderingChef.transform.position += wanderingChef.transform.up * 1.55f;
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
