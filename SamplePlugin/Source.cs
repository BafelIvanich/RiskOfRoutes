using BepInEx;
using HarmonyLib;
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
using UnityEngine.Networking;
using UnityEngine.UI;
using static Rewired.UI.ControlMapper.ControlMapper;

//TODO
//Включение артефактов
//Гарантированное появление шефа (баг сейчас)
//UI при наведении
//Модификаторы для других порталов
//Разный цвет у порталов
//Кредиты
//Нажатие на портал закрывает другие
//Вынести метод спавна объекта рандомно на сцене (для шефа, принтеров и другого)
//Директор модификатор


[assembly: HG.Reflection.SearchableAttribute.OptIn]

namespace RiskOfRoutes
{
    // https://risk-of-thunder.github.io/R2Wiki/Mod-Creation/Assets/Localization/
    [BepInDependency(ItemAPI.PluginGUID)]
    [BepInDependency(LanguageAPI.PluginGUID)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(RecalculateStatsAPI.PluginGUID)]
    
    public class RiskOfRoutes : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "burboni4";
        public const string PluginName = "RiskOfRoutes";
        public const string PluginVersion = "1.0.0";

        private GameObject portalPrefab;
        private GameObject ui;

        private HUD hud = null;

        private InteractableSpawnCard portalSpawnCard;

        public static GameObject chefPrefab;

        public const int RouteCount = 3;

        public void Awake()
        {

            Log.Init(Logger);

            chefPrefab = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/DLC3/MealPrep/MealPrep.prefab").WaitForCompletion(), "mealPrep");

            ui = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/Tooltip.prefab").WaitForCompletion(), "tooltipUI");
            
            
            
            //statue
            CreateInteractable();

            TeleporterInteraction.onTeleporterChargedGlobal += teleporterSpawnHologram;
            gameObject.AddComponent<StageModifierManager>();
            RoR2.Stage.onServerStageBegin += Stage_onServerStageBegin;
            On.RoR2.UI.HUD.Awake += HUD_Awake;
            On.RoR2.Networking.NetworkManagerSystemSteam.OnClientConnect += (s, u, t) => { };
        }


        private void HUD_Awake(On.RoR2.UI.HUD.orig_Awake orig, HUD self)
        {
            orig(self);
            hud = self;

            GameObject newUi = UnityEngine.Object.Instantiate(LegacyResourcesAPI.Load<GameObject>("Prefabs/UI/Tooltip"));
            newUi.transform.SetParent(hud.mainContainer.transform);
            RectTransform rectTransform = newUi.GetComponent<RectTransform>();
            rectTransform.pivot = new Vector2(1.0f, 1.0f);
            rectTransform.anchorMin = new Vector2(0.9f,0.5f);
            rectTransform.anchorMax = new Vector2(0.9f, 0.5f);
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;


            var images = newUi.GetComponentsInChildren<Image>();
            foreach(var img in images)
            {
                if(img.name == "TitleRect")
                {
                    img.color = new Color(0.5f,0.8f,0.9f,1.0f);
                }
            }

            tooltipSetText(newUi,"Unknown");

            newUi.SetActive(false);

            ui = newUi;
        }

        private void tooltipSetText(GameObject tooltip,string text)
        {
            var tc = tooltip.GetComponent<TooltipController>();

            var label = tc.titleLabel;
            var body = tc.bodyLabel;

            label.text = "Route Portal";
            body.text = text;
        }

        private void HUD_shouldHudDisplay(HUD hud, ref bool shouldDisplay)
        {
            var tc = ui.GetComponent<TooltipController>();
            var rc = ui.GetComponent<RectTransform>();
            tc.titleLabel.text = "frrrrrrrf";
            tc.bodyLabel.text = "gfefefef";
            ui.SetActive(true);
        }

        private void Stage_onServerStageBegin(Stage obj)
        {
            Log.Info($"Stage {SceneCatalog.mostRecentSceneDef.cachedName} started");
            return;
        }



        

        private void CreateInteractable()
        {
            GameObject routePortal = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/PortalShop/PortalShop.prefab").WaitForCompletion(), "BeebleMemorialStatue");
            Log.Info("Invoked");

            Destroy(routePortal.GetComponent<GenericInteraction>());
            Destroy(routePortal.GetComponent<SceneExitController>());

            routePortal.name = "RoutePortal";
            routePortal.transform.localScale = new Vector3(1f, 1f, 1f);

            //создаем интеракцию
            RoutePortalManager mgr = routePortal.AddComponent<RoutePortalManager>();
            PurchaseInteraction interaction = routePortal.AddComponent<PurchaseInteraction>();
            SceneExitController sceneExitController = routePortal.AddComponent<SceneExitController>();
            interaction.contextToken = "Enter the portal <sprite name=Utility>";
            interaction.NetworkdisplayNameToken = "Route Portal to a next stage";


            mgr.purchaseInteraction = interaction;
            mgr.sceneExitController = sceneExitController;
            mgr.modifierCredits = 1;

            routePortal.GetComponent<Highlight>().targetRenderer = routePortal.GetComponentInChildren<SkinnedMeshRenderer>();
            GameObject trigger = Instantiate(new GameObject("Trigger"), routePortal.transform);
            trigger.AddComponent<BoxCollider>().isTrigger = true;
            trigger.AddComponent<EntityLocator>().entity = routePortal;

            portalPrefab = routePortal;



        }

        private void teleporterSpawnHologram(TeleporterInteraction interaction)
        {
            spawnRoutePortals(interaction.transform.position);
        }


        private void spawnRoutePortals(Vector3 position)
        {
            List<SceneDef> scenes = GetNextStagesScenesList();
            List<StageModifier> modifiers = new List<StageModifier>();
            modifiers.Add(new ArtifactModifier());
            modifiers.Add(new BossItemModifier());
            modifiers.Add(new PrinterItemModifier());




            if (scenes.Count == 0)
            {
                Log.Error("No stages availble next");
                return;
            }

            Util.ShuffleList(scenes,Run.instance.stageRng);
            Util.ShuffleList(modifiers, Run.instance.stageRng);


            int lim = RouteCount > scenes.Count ? scenes.Count : RouteCount;
            
            for (int i = 0; i < lim;i++)
            {
                int index = scenes.Count - 1;
                int jndex = modifiers.Count - 1;
                SceneDef destination = scenes[index];
                StageModifier mod = modifiers[jndex];
                scenes.RemoveAt(index);
                modifiers.RemoveAt(jndex);
                if(mod==null)
                {
                    Log.Error("Fix needed");
                }

                SpawnCard routePortal = CreateRoutePortalSpawnCard(destination,mod);

                if(routePortal==null)
                {
                    Log.Error("PortalCard is null");
                    return;
                }

                DirectorSpawnRequest request = new DirectorSpawnRequest(routePortal, new DirectorPlacementRule
                {
                    minDistance = 10f,
                    maxDistance = 40f,
                    placementMode = DirectorPlacementRule.PlacementMode.Approximate,
                    position = position
                }, Run.instance.stageRng);

                

                GameObject portalObject = DirectorCore.instance.TrySpawnObject(request);

                Log.Info($"Got modifier {mod.reservedText}");

                if (portalObject)
                {
                    var m = portalObject.GetComponent<RoutePortalManager>();
                    if (m != null) {
                        Log.Info($"Trying to add modifier to portal");
                        m.AddStageModifier(mod);
                    }

                    Log.Info($"Success? {m.stageModifiers.Count()}");
                    Log.Info($"{i+1}th portal placed successfully");
                }
                Vector3 facingVector = position - portalObject.transform.position;
                facingVector.y = 0;
                if(facingVector != Vector3.zero)
                {
                    portalObject.transform.rotation = Quaternion.LookRotation(facingVector);
                }
                Vector3 rdir = portalObject.transform.right;
                
                var renderers = portalObject.GetComponentsInChildren<ParticleSystem>();

                Material mat = SceneInfo.instance.sceneDef.portalMaterial;

            }

        }

        private InteractableSpawnCard CreateRoutePortalSpawnCard(SceneDef destination,StageModifier mod)
        {
            var m = portalPrefab.GetComponent<RoutePortalManager>();
            m.destinationScene = destination;

            InteractableSpawnCard interactableSpawnCard = ScriptableObject.CreateInstance<InteractableSpawnCard>();

            interactableSpawnCard.name = "iscRoutePortal";
            interactableSpawnCard.prefab = portalPrefab;
            interactableSpawnCard.sendOverNetwork = true;
            interactableSpawnCard.hullSize = HullClassification.Human;
            interactableSpawnCard.nodeGraphType = RoR2.Navigation.MapNodeGroup.GraphType.Ground;
            interactableSpawnCard.requiredFlags = RoR2.Navigation.NodeFlags.None;
            interactableSpawnCard.forbiddenFlags = RoR2.Navigation.NodeFlags.NoShrineSpawn;
            interactableSpawnCard.directorCreditCost = 0;
            interactableSpawnCard.occupyPosition = true;
            interactableSpawnCard.orientToFloor = false;
            interactableSpawnCard.skipSpawnWhenSacrificeArtifactEnabled = false;

            portalSpawnCard = interactableSpawnCard;

            return interactableSpawnCard;
        }

        private void ClosePortals(GameObject obj)
        {
            GameObject.Destroy(obj);
        }

        private static List<SceneDef> GetNextStagesScenesList()
        {
            //TODO луп вариации проверить
            SceneDef nextStageScene = Run.instance.nextStageScene;
            List<SceneDef> list = new List<SceneDef>();
            if(nextStageScene != null)
            {
                int nextStageOrder = nextStageScene.stageOrder;
                var s = SceneCatalog.mostRecentSceneDef.destinationsGroup;
                
                foreach (RoR2.SceneCollection.SceneEntry se in s.sceneEntries)
                {
                    var scene = se.sceneDef;
                    if(scene.sceneType == SceneType.Stage &&
                        scene.mainTrack &&
                        scene.stageOrder == nextStageOrder && 
                        (scene.requiredExpansion == null || Run.instance.IsExpansionEnabled(scene.requiredExpansion)) && 
                        RoR2.Run.instance.CanPickStage(scene)  )
                    {
                        list.Add(scene);
                    }
                }
            }

            return list;

        }

        [ConCommand(commandName = "nextStages", flags = ConVarFlags.ExecuteOnServer, helpText = "Get next stages")]
        private static void GetNextStagesScenesCommand(ConCommandArgs args)
        {
            Log.Info("Next stages available:");
            foreach(var scene in GetNextStagesScenesList())
            {
                Log.Info(scene.cachedName);
            }
            Log.Info(Run.instance.nextStageScene.destinationsGroup);

        }

        [ConCommand(commandName = "activeModifiers", flags = ConVarFlags.ExecuteOnServer, helpText = "Get currently active modifiers")]
        private static void GetActiveModifiersCommand(ConCommandArgs args)
        {
            if (StageModifierManager.Instance != null)
            {
                var mods = StageModifierManager.Instance.activeModifiers;
                if (mods != null)
                {
                    if (mods.Count > 0)
                    {
                        foreach (var modifier in mods)
                        {
                            Log.Info(modifier.activeText);
                        }
                    }
                    else
                    {
                        Log.Info("There are no modifiers");
                    }
                }
                else
                {
                    Log.Error("Modifiers list is null");
                }
            }

        }

        [ConCommand(commandName = "reservedModifiers", flags = ConVarFlags.ExecuteOnServer, helpText = "Get modifiers that would be active on next stage")]
        private static void GetReservedModifiersCommand(ConCommandArgs args)
        {
            if (StageModifierManager.Instance != null)
            {
                var mods = StageModifierManager.Instance.reservedModifiers;
                if (mods != null)
                {
                    if(mods.Count > 0)
                    {
                        foreach (var modifier in mods)
                        {
                            Log.Info(modifier.reservedText);
                        }
                    }
                    else
                    {
                        Log.Info("There are no modifiers");
                    }

                }
                else
                {
                    Log.Error("Modifiers list is null");
                }
            }

        }



        static bool IsValidNextStage(SceneDef sceneDef)
        {
            SceneDef nextStageScene = RoR2.Run.instance.nextStageScene;
            if (nextStageScene != null && nextStageScene.baseSceneName == sceneDef.baseSceneName)
            {
                return false;
            }
            if (!sceneDef.hasAnyDestinations)
            {
                return false;
            }

            return sceneDef.validForRandomSelection;
        }

        //телпорт сразу
        [ConCommand(commandName = "charge", flags = ConVarFlags.ExecuteOnServer, helpText = "Charges TP and kills boss")]
        static void ChargeTeleporter(ConCommandArgs args)
            {
                if (!TeleporterInteraction.instance) return;

                var tp = TeleporterInteraction.instance;

                tp.holdoutZoneController.charge = 0.99f;

                if (tp.bossGroup && tp.bossGroup.combatSquad)
                {

                    List<CharacterMaster> bossMasters = new List<CharacterMaster>(tp.bossGroup.combatSquad.readOnlyMembersList);

                    foreach (CharacterMaster master in bossMasters)
                    {
                        CharacterBody body = master.GetBody();

                        if (body && body.healthComponent && body.healthComponent.alive)
                        {
                            body.healthComponent.Suicide(null, null, DamageType.Generic);
                        }
                    }
                }

                Log.Info("Teleporter charged and bosses eliminated.");
            }
        
        
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F2))
            {
                // Get the player body to use a position:
                var transform = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;

                // And then drop our defined item in front of the player.

                Log.Info($"Player pressed F2. Spawning our custom item at coordinates {transform.position}");
                spawnRoutePortals(transform.position);
            }

            if (Input.GetKeyDown(KeyCode.F3))
            {
                var transform = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;

                Log.Info($"{transform.position}");
            }
            var localUser = LocalUserManager.GetFirstLocalUser();
            //TODO: Сделать смену при смене интерактбла, не чекать каждый кадр
            if (localUser != null && localUser.cachedBody != null)
            {
                var dr = localUser.cachedBody.GetComponent<InteractionDriver>();
                if (dr != null)
                {
                    if (dr.currentInteractable != null)
                    {
                        if (dr.currentInteractable.name.Contains("RoutePortal"))
                        {
                            var manager = dr.currentInteractable.GetComponent<RoutePortalManager>();
                            Log.Info("Looking at route portal");
                            Log.Info(manager.destinationScene.cachedName);
                            //TODO: вроде можно переделать через TooltipProvider
                            if(manager.modifierTextSynced != null)
                            {
                            tooltipSetText(ui,manager.modifierTextSynced);
                            }
                            else
                            {
                                Log.Info("hiii");
                            }
                            ui.SetActive(true);

                        }
                        
                    }
                    else
                    {

                        ui.SetActive(false);

                    }
                }

            }

        }


    }
}
