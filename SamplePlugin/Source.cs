using BepInEx;
using BepInEx.Configuration;
using R2API;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using RiskOfRoutes.StageModifiers;
using RiskOfRoutes.StageModifiers.SceneModifiers;
using RoR2;
using SamplePlugin;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;
using UnityHotReloadNS;

//TODO
//Особые модификаторы для стейджей
//(Подкорректировать)Добавить больше предметов в пулл зеленых и белых, добавить возможность для такого же с предметами любого тира через конфиг
//Сделать, чтобы всё работало в мультиплеере
//(Возможно) фиксированные места поялвения шефа
//(Подкорректировать)Назначение exitController и purchase в RoutePortalManager нужно делать вручную, стоит сделать awake()
//Конфликт с артефакт модификат
//анти прок?
//реген меньше
//артефакт лимит модификаторов
//атрефакт стейджи в разном порядке
//(Возможно) более ровный спавн порталов
//!ROUTE PORTAL MANAGER хуки пересмотреть и почти все с обновлением текста
//почитать
//на улучшение дронов
//artifact of fallacy
//проверить появление шефа
//more drones с плюсиком


//Визуал:
//Текстура локи в портале


//(Возможно) Скрытые модификаторы и предметы их раскрывающие
//(Возможно) Модификатор исключающий категорию предметов из пулла
//(Возможно) Покупка всего за хп
//(Возможно) Меньше сундуков, но больше магазинов
//(Возможно) Портал, очищающий лунные предметы
//(Возможно) категории порталов?
//Утилити урон хил
//Категории порталов
//Эклипсовые

//Пост релиз
//(Сложно) UI карты на манер шпиля
//(Возможно) Можно ли сделать так, чтобы любые мобы могли воровать предметы?
//(Возможно) Бродячий дрифтер или можно рюкзак
//(Возможно) рюзкак дрифтера позволяет поделиться предметами
//(Возможно) Void Stage
//(Возможно) Святилище, что рероллит все предметы, лежащие на земле
//(Возможно)стейдж где алтари удачи заменяют сундуки но другой дроп голды
//(Возможно)Модификаторы длящиеся несколько стейджей
//(Возможно)Если надоедать продавцу на базаре то гарантирует сложный портал


[assembly: HG.Reflection.SearchableAttribute.OptIn]

namespace RiskOfRoutes
{
    // https://risk-of-thunder.github.io/R2Wiki/Mod-Creation/Assets/Localization/
    [BepInDependency(ItemAPI.PluginGUID)]
    [BepInDependency(LanguageAPI.PluginGUID)]
    [BepInDependency(PrefabAPI.PluginGUID)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(RecalculateStatsAPI.PluginGUID)]
    [BepInDependency("com.rune580.riskofoptions")]
    [BepInDependency(R2API.LanguageAPI.PluginGUID)]
    [BepInDependency(R2API.PrefabAPI.PluginGUID)]



    public class RiskOfRoutes : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "burboni4";
        public const string PluginName = "RiskOfRoutes";
        public const string PluginVersion = "1.0.0";

        //tweaks
        public static ConfigEntry<bool> usePortalMaterial { get; set; }
        public static ConfigEntry<bool> usePositiveNegativeColors { get; set; }
        public static ConfigEntry<bool> useUniqueDestinations { get; set; }
        public static ConfigEntry<bool> showPortalDestination { get; set; }
        public static ConfigEntry<bool> teleporterUseRandomMods { get; set; }
        public static ConfigEntry<bool> modifiersCanBeBanned { get; set; }
        public static ConfigEntry<bool> negativeStackPunishment { get; set; }
        public static ConfigEntry<int> punishmentTimeMinutes { get; set; }

        public static ConfigEntry<int> routeMapSize { get; set; }
        public static ConfigEntry<int> routeMapIconSize { get; set; }
        public static ConfigEntry<bool> routePanelShowsOnlyNames { get; set; }
        public static ConfigEntry<MapOrientation> punishmentBarOrientation { get; set; }
        public static ConfigEntry<Color> punishmentBarColor { get; set; }
        //artifacts
        public static ConfigEntry<bool> allowMysteryArtifact;
        public static ConfigEntry<bool> allowChaosArtifact;
        public static ConfigEntry<bool> allowCommandArtifact;
        public static ConfigEntry<bool> allowDeathArtifact;
        public static ConfigEntry<bool> allowDelusionArtifact;
        public static ConfigEntry<bool> allowDevotionArtifact;
        public static ConfigEntry<bool> allowDissonanceArtifact;
        public static ConfigEntry<bool> allowEnigmaArtifact;
        public static ConfigEntry<bool> allowEvolutionArtifact;
        public static ConfigEntry<bool> allowFrailtyArtifact;
        public static ConfigEntry<bool> allowGlassArtifact;
        public static ConfigEntry<bool> allowHonorArtifact;
        public static ConfigEntry<bool> allowKinArtifact;
        public static ConfigEntry<bool> allowMetamorphosisArtifact;
        public static ConfigEntry<bool> allowRebirthArtifact;
        public static ConfigEntry<bool> allowSacrificeArtifact;
        public static ConfigEntry<bool> allowSoulArtifact;
        public static ConfigEntry<bool> allowSpiteArtifact;
        public static ConfigEntry<bool> allowSwarmsArtifact;
        public static ConfigEntry<bool> allowVengeanceArtifact;
        //other modifiers
        public static ConfigEntry<int> droneStackPercent;
        public static ConfigEntry<int> doppelStackTime;
        public static ConfigEntry<int> elitesStackPercent;
        public static ConfigEntry<int> lunarStackPercent;
        public static ConfigEntry<int> voidStackPercent;
        public static ConfigEntry<int> soulCostStackPercent;

        public static ConfigEntry<int> routeMapWidth;


        public static CombatDirector.EliteTierDef[] defaultEliteTiers;

        public static GameObject networkedInventoryPrefab;

        internal static GameObject CentralNetworkObject;
        private static GameObject _centralNetworkObjectSpawned;

        public void Awake()
        {
            Log.Init(Logger);
            BindConfig();

            var temp = new GameObject("tmpGo");
            temp.AddComponent<NetworkIdentity>();
            CentralNetworkObject = PrefabAPI.InstantiateClone(temp, "somethingUnique", false);

            GameObject.Destroy(temp);
            CentralNetworkObject.AddComponent<StageModifierDirector>();
            CentralNetworkObject.AddComponent<MyNetworkComponent>();
            PrefabAPI.RegisterNetworkPrefab(CentralNetworkObject);

            On.RoR2.Networking.NetworkManagerSystemSteam.OnClientConnect += (s, u, t) => { };

            string path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Info.Location), "riskofroutes");
            var bundle = AssetBundle.LoadFromFile(path);
            AssetManager.Initialize(bundle);

            FieldInfo eliteTiersField = typeof(CombatDirector).GetField("eliteTiers", BindingFlags.NonPublic | BindingFlags.Static);
            if (eliteTiersField != null)
            {
                defaultEliteTiers = (CombatDirector.EliteTierDef[])eliteTiersField.GetValue(null);
                Log.Info("Succesfully loaded elite tiers");
            }
            else
            {
                Log.Error("Could not load default elite tiers");
                defaultEliteTiers = new CombatDirector.EliteTierDef[] { };
            }

            ModUIManager.Initialize();

            RoR2.RoR2Application.onLoad += () =>
            {
                StageModifierCatalog.Init(bundle);
            };

            MysteryArtifact.InitializeArtifact();
            RoutesExpansion.InitializeExpansion(CentralNetworkObject);
            SoulCostController.Initialize();
        }

        //private void CreateNetworkObj(Run obj)
        //{
        //    if (NetworkServer.active)
        //    {
        //        if (!_centralNetworkObjectSpawned)
        //        {
        //            _centralNetworkObjectSpawned = GameObject.Instantiate(CentralNetworkObject);
        //            NetworkServer.Spawn(_centralNetworkObjectSpawned);
        //            Log.Info("StageModifierDirector spawned");
        //        }
        //    }
        //}

        private void BindConfig()
        {

            //GENERAL //////////////////////////////////////////////////////
            useUniqueDestinations = Config.Bind<bool>(
                "General", "Unique Destination", true, "Makes each route portal have unique scene as destination, otherwise they might repeat");
            ModSettingsManager.AddOption(new CheckBoxOption(useUniqueDestinations));

            teleporterUseRandomMods = Config.Bind<bool>(
                "General", "Teleporter Use Random mods", true, "Makes teleporter use random modifiers of spawned route portals after interacting with it, set false if you want it not to use any stage modifiers");
            ModSettingsManager.AddOption(new CheckBoxOption(teleporterUseRandomMods));

            //todo
            modifiersCanBeBanned = Config.Bind<bool>(
                "General", "Modifiers can be banned after being used", true, "Prevents some of stage modifiers appearing on the next stage, if they were active on the current stage");
            ModSettingsManager.AddOption(new CheckBoxOption(modifiersCanBeBanned));

            routeMapWidth = Config.Bind<int>(
                "General", "Route map width", 3, "Width of the map created at the start of each loop");
            ModSettingsManager.AddOption(new IntSliderOption(routeMapWidth, new IntSliderConfig() { min = 3, max = 5 }));

            negativeStackPunishment = Config.Bind<bool>(
                "General", "Time spent punishment", true, "Adds one negative stack per punishment, punishment is given by spending too much time on current stage, by default it's 8 minutes");
            ModSettingsManager.AddOption(new CheckBoxOption(negativeStackPunishment));

            punishmentTimeMinutes = Config.Bind<int>(
                "General", "Punishment time", 8, "Adds one negative stack per punishment, punishment is given by spending too much time on current stage, by default it's 8 minutes");
            ModSettingsManager.AddOption(new IntSliderOption(punishmentTimeMinutes, new IntSliderConfig() { min = 1, max = 20 }));

            /////////////////////////////////////////////////////////////////


            //VISUAL ///////////////////////////////////////////////////////
            usePortalMaterial = Config.Bind<bool>(
                "Visual", "WIP Use Scene Material", false, "[WIP] Sets destination scene portrait as a portal texture");
            ModSettingsManager.AddOption(new CheckBoxOption(usePortalMaterial));

            usePositiveNegativeColors = Config.Bind<bool>(
                "Visual", "Use Positive/Negative colors", false, "Makes bubbles on portals red and green, otherwise they are just blue");
            ModSettingsManager.AddOption(new CheckBoxOption(usePositiveNegativeColors));

            showPortalDestination = Config.Bind<bool>(
                "Visual", "Show Portal Destination", true, "Show destination scene in hud");
            ModSettingsManager.AddOption(new CheckBoxOption(showPortalDestination));

            routeMapSize = Config.Bind<int>(
                "Visual", "Route Map panel size", 90, "Size of Route Map panel in percents");
            ModSettingsManager.AddOption(new IntSliderOption(routeMapSize));

            routeMapIconSize = Config.Bind<int>(
                "Visual", "Route Map icons size", 100, "Size of icons on Route Map panel");
            ModSettingsManager.AddOption(new IntSliderOption(routeMapIconSize));

            routePanelShowsOnlyNames = Config.Bind<bool>(
                "Visual", "Route panel shows only names", false, "Makes it so route portal panel shows only names of modifiers, use if descriptions seem too clutered");
            ModSettingsManager.AddOption(new CheckBoxOption(routePanelShowsOnlyNames));

            punishmentBarOrientation = Config.Bind<MapOrientation>(
                "Visual", "Progress bar orientation", MapOrientation.Vertical, "Orientation of punishment bar");
            ModSettingsManager.AddOption(new ChoiceOption(punishmentBarOrientation));

            punishmentBarColor = Config.Bind<Color>(
                "Visual", "Punishment bar color", Color.white, "Color of progress bar");
            ModSettingsManager.AddOption(new ColorOption(punishmentBarColor));
            ////////////////////////////////////////////////////////////////



            //ARTIFACT MODIFIER
            allowMysteryArtifact = Config.Bind<bool>(
                "Artifacts", "Mystery", true, "Mystery artifact available as artifact stage modifier");
            ModSettingsManager.AddOption(new CheckBoxOption(allowMysteryArtifact));

            allowChaosArtifact = Config.Bind<bool>(
               "Artifacts", "Chaos", true, "Chaos artifact available as artifact stage modifier");
            ModSettingsManager.AddOption(new CheckBoxOption(allowChaosArtifact));

            allowDeathArtifact = Config.Bind<bool>(
                "Artifacts", "Death", true, "Death artifact available as artifact stage modifier");
            ModSettingsManager.AddOption(new CheckBoxOption(allowDeathArtifact));

            allowDissonanceArtifact = Config.Bind<bool>(
                "Artifacts", "Dissonance", true, "Dissonance artifact available as artifact stage modifier");
            ModSettingsManager.AddOption(new CheckBoxOption(allowDissonanceArtifact));

            allowFrailtyArtifact = Config.Bind<bool>(
                "Artifacts", "Frailty", true, "Frailty artifact available as artifact stage modifier");
            ModSettingsManager.AddOption(new CheckBoxOption(allowFrailtyArtifact));

            allowHonorArtifact = Config.Bind<bool>(
                "Artifacts", "Honor", true, "Honor artifact available as artifact stage modifier");
            ModSettingsManager.AddOption(new CheckBoxOption(allowHonorArtifact));

            allowSacrificeArtifact = Config.Bind<bool>(
                "Artifacts", "Sacrifice", true, "Sacrifice artifact available as artifact stage modifier");
            ModSettingsManager.AddOption(new CheckBoxOption(allowSacrificeArtifact));

            allowSoulArtifact = Config.Bind<bool>(
                "Artifacts", "Soul", true, "Soul artifact available as artifact stage modifier");
            ModSettingsManager.AddOption(new CheckBoxOption(allowSoulArtifact));

            allowSpiteArtifact = Config.Bind<bool>(
                "Artifacts", "Spite", true, "Spite artifact available as artifact stage modifier");
            ModSettingsManager.AddOption(new CheckBoxOption(allowSpiteArtifact));

            allowSwarmsArtifact = Config.Bind<bool>(
                "Artifacts", "Swarms", true, "Swarms artifact available as artifact stage modifier");
            ModSettingsManager.AddOption(new CheckBoxOption(allowSwarmsArtifact));

            allowVengeanceArtifact = Config.Bind<bool>(
                "Artifacts", "Vengeance", false, "Vengeance artifact available as artifact stage modifier. Not intended to be available");
            ModSettingsManager.AddOption(new CheckBoxOption(allowVengeanceArtifact));

            allowCommandArtifact = Config.Bind<bool>(
                "Artifacts", "Command", false, "Command artifact available as artifact stage modifier. Not intented to be available");
            ModSettingsManager.AddOption(new CheckBoxOption(allowCommandArtifact));

            allowDelusionArtifact = Config.Bind<bool>(
                "Artifacts", "Delusion", false, "Delusion artifact available as artifact stage modifier. Not intented to be available");
            ModSettingsManager.AddOption(new CheckBoxOption(allowDelusionArtifact));

            allowDevotionArtifact = Config.Bind<bool>(
                "Artifacts", "Devotion", false, "Devotion artifact available as artifact stage modifier. Not intented to be available");
            ModSettingsManager.AddOption(new CheckBoxOption(allowDevotionArtifact));

            allowEnigmaArtifact = Config.Bind<bool>(
                "Artifacts", "Enigma", false, "Enigma artifact available as artifact stage modifier. Not intented to be available");
            ModSettingsManager.AddOption(new CheckBoxOption(allowEnigmaArtifact));

            allowEvolutionArtifact = Config.Bind<bool>(
                "Artifacts", "Evolution", false, "Evolution artifact available as artifact stage modifier. Not intented to be available");
            ModSettingsManager.AddOption(new CheckBoxOption(allowEvolutionArtifact));

            allowGlassArtifact = Config.Bind<bool>(
                "Artifacts", "Glass", false, "Glass artifact available as artifact stage modifier. Not intented to be available");
            ModSettingsManager.AddOption(new CheckBoxOption(allowGlassArtifact));

            allowKinArtifact = Config.Bind<bool>(
                "Artifacts", "Kin", false, "Kin artifact available as artifact stage modifier. Not intented to be available");
            ModSettingsManager.AddOption(new CheckBoxOption(allowKinArtifact));

            allowMetamorphosisArtifact = Config.Bind<bool>(
                "Artifacts", "Metamorphosis", false, "Metamorphosis artifact available as artifact stage modifier. Not intented to be available");
            ModSettingsManager.AddOption(new CheckBoxOption(allowMetamorphosisArtifact));

            allowRebirthArtifact = Config.Bind<bool>(
                "Artifacts", "Rebirth", false, "Rebirth artifact available as artifact stage modifier. Not intented to be available");
            ModSettingsManager.AddOption(new CheckBoxOption(allowRebirthArtifact));

            /////////////////////////////////////////////////////////////////////

            //more drones modifier /////////////////////////////////////////////////////////////////
            droneStackPercent = Config.Bind<int>(
                "Stage modifier tweaks", "Percent", 15, "Percent per stack of initial scene director budget, used only to spawn additional drones");
            ModSettingsManager.AddOption(new IntFieldOption(droneStackPercent));
            ////////////////////////////////////////////////////////////////////////////////////////

            //more elites modifier /////////////////////////////////////////////////////////////////
            elitesStackPercent = Config.Bind<int>(
                "Stage modifier tweaks", "Elite increase percent", 33, "Percent per stack of modifier");
            ModSettingsManager.AddOption(new IntFieldOption(elitesStackPercent));
            ////////////////////////////////////////////////////////////////////////////////////////

            //doppelganger modifier

            doppelStackTime = Config.Bind<int>(
                "Stage modifier tweaks", "Doppelganger cooldown reduced by stack", 30, "Cooldown reducement with one stack in seconds");
            ModSettingsManager.AddOption(new IntFieldOption(doppelStackTime));
            //////////////////////////////////////////////////////////////////////////////////////

            //lunar enemies
            lunarStackPercent = Config.Bind<int>(
                "Stage modifier tweaks", "Lunar Enemies percent", 15, "Percent per stack of initial scene director budget, used only to spawn additional drones");
            ModSettingsManager.AddOption(new IntFieldOption(lunarStackPercent));
            /////////////////////////////////////////////////////////////////////////////////////

            //void enemies
            voidStackPercent = Config.Bind<int>(
                "Stage modifier tweaks", "Void Enemies percent", 15, "Percent per stack of initial scene director budget, used only to spawn additional drones");
            ModSettingsManager.AddOption(new IntFieldOption(voidStackPercent));
            /////////////////////////////////////////////////////////////////////////////////////

            //soul cost
            soulCostStackPercent = Config.Bind<int>(
                "Stage modifier tweaks", "Soul cost percent", 15, "Percent per stack of initial scene director budget, used only to spawn additional drones");
            ModSettingsManager.AddOption(new IntFieldOption(soulCostStackPercent));
            //////////////////////////////////////////////////////////////////////////////////////////
        }

        private void Update()
        {
            //if (Time.timeScale == 0f) return;

            if (Input.GetKeyDown(KeyCode.F2))
            {
                var transform = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;

                Log.Info($"Player pressed F2. Spawning route portals {transform.position}");
                RoutesExpansion.SpawnRoutePortals(transform.position);
            }

            if (Input.GetKeyDown(KeyCode.F3))
            {
                UnityHotReload.LoadNewAssemblyVersion(
                    typeof(RiskOfRoutes).Assembly, // The currently loaded assembly to replace.
                    "C:/Users/vit/AppData/Roaming/r2modmanPlus-local/RiskOfRain2/profiles/TEstling/BepInEx/plugins/RiskOfRoutes.dll"  // The path to the newly compiled DLL.
                );
                Log.Info("Done");
            }
            ////
        }


        [ConCommand(commandName = "debuglog_on_all", flags = ConVarFlags.ExecuteOnServer, helpText = "Logs a network message to all connected people.")]
        private static void CCNetworkLog(ConCommandArgs args)
        {
            if (NetworkServer.active)
            {
                if (!_centralNetworkObjectSpawned)
                {
                    _centralNetworkObjectSpawned = Object.Instantiate(CentralNetworkObject);
                    NetworkServer.Spawn(_centralNetworkObjectSpawned);
                    Log.Info("instantiated network ob");
                }
                foreach (NetworkUser user in NetworkUser.readOnlyInstancesList)
                {
                    MyNetworkComponent.Invoke(user, string.Join("Pognali", args.userArgs));
                }
            }
        }

        public enum MapOrientation { Vertical, Horizontal }


    }
}
