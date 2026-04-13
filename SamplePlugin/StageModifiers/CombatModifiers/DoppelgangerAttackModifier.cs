using RoR2;
using RoR2.Artifacts;
using RoR2.CharacterAI;
using SamplePlugin.StageModifiers;
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace RiskOfRoutes.StageModifiers.CombatModifiers
{
    public class DoppelgangerAttackModifier : StageModifier
    {
        float coolDown = 0;

        float defaultTime = 240f;

        //TODO можно добавить стаки
        public DoppelgangerAttackModifier(int stack)
        {
            this.stack = stack;
        }

        public override void OnEnd()
        {
            On.RoR2.Run.FixedUpdate -= Run_FixedUpdate;
        }

        public override void OnStart()
        {
            coolDown = RoR2.Run.instance.time + defaultTime;
            On.RoR2.Run.FixedUpdate += Run_FixedUpdate;
        }

        private void Run_FixedUpdate(On.RoR2.Run.orig_FixedUpdate orig, Run self)
        {
            orig(self);
            if (!NetworkServer.active || self.isGameOverServer) return;

            //Log.Info($"{self.fixedTime}:{coolDown}");

            if (self.fixedTime > coolDown)
            {
                Log.Info("Here doppel");
                PerformInvasion();
                coolDown = self.fixedTime + Mathf.Max(defaultTime - (float)(RiskOfRoutes.doppelStackTime.Value), 60f);
            }

        }


        private void PerformInvasion()
        {

            var instances = PlayerCharacterMasterController.instances;
            Log.Info(instances.Count);
            foreach (var playerCharacterMaster in PlayerCharacterMasterController.instances)
            {

                if (!playerCharacterMaster || !playerCharacterMaster.master) continue;
                if (playerCharacterMaster.master.IsDeadAndOutOfLivesServer()) continue;

                CreateDoppelganger(playerCharacterMaster.master, Run.instance.stageRng);
            }
        }
        private static void CreateDoppelganger(CharacterMaster srcCharacterMaster, Xoroshiro128Plus rng)
        {
            Log.Info("Doppel");
            SpawnCard spawnCard = FromMaster(srcCharacterMaster);
            if (!spawnCard)
            {
                Log.Error("No spawn card");
                return;
            }
            Transform coreTransform;
            DirectorCore.MonsterSpawnDistance input;
            if ((bool)TeleporterInteraction.instance)
            {
                coreTransform = TeleporterInteraction.instance.transform;
                input = DirectorCore.MonsterSpawnDistance.Close;
            }
            else
            {
                coreTransform = srcCharacterMaster.GetBody().coreTransform;
                input = DirectorCore.MonsterSpawnDistance.Close;
            }
            DirectorPlacementRule directorPlacementRule = new DirectorPlacementRule
            {
                spawnOnTarget = coreTransform,
                placementMode = DirectorPlacementRule.PlacementMode.NearestNode
            };
            DirectorCore.GetMonsterSpawnDistance(input, out directorPlacementRule.minDistance, out directorPlacementRule.maxDistance);
            DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(spawnCard, directorPlacementRule, rng);
            directorSpawnRequest.teamIndexOverride = TeamIndex.Monster;
            directorSpawnRequest.ignoreTeamMemberLimit = true;
            CombatSquad combatSquad = null;
            directorSpawnRequest.onSpawnedServer = (Action<SpawnCard.SpawnResult>)Delegate.Combine(directorSpawnRequest.onSpawnedServer, (Action<SpawnCard.SpawnResult>)delegate (SpawnCard.SpawnResult result)
            {
                if (!combatSquad)
                {
                    combatSquad = UnityEngine.Object.Instantiate(LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/Encounters/ShadowCloneEncounter")).GetComponent<CombatSquad>();
                }
                combatSquad.AddMember(result.spawnedInstance.GetComponent<CharacterMaster>());
            });
            DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
            UnityEngine.Object.Destroy(spawnCard);
        }

        public static DoppelgangerSpawnCard FromMaster(CharacterMaster srcCharacterMaster)
        {
            if (!srcCharacterMaster || !srcCharacterMaster.GetBody())
            {
                Log.Error("Here");
                return null;
            }
            DoppelgangerSpawnCard doppelgangerSpawnCard = ScriptableObject.CreateInstance<DoppelgangerSpawnCard>();
            MasterCopySpawnCard.CopyDataFromMaster(doppelgangerSpawnCard, srcCharacterMaster, copyItems: true, copyEquipment: true);
            doppelgangerSpawnCard.GiveItem(RoR2Content.Items.InvadingDoppelganger);
            doppelgangerSpawnCard.onPreSpawnSetup = OnPreSpawnSetup;
            return doppelgangerSpawnCard;
            void OnPreSpawnSetup(CharacterMaster spawnedMaster)
            {
                BaseAI ai = spawnedMaster.GetComponent<BaseAI>();
                CharacterBody srcBody = srcCharacterMaster.GetBody();
                ai.onBodyDiscovered += SetEnemyToOriginator;
                void SetEnemyToOriginator(CharacterBody body)
                {
                    ai.currentEnemy.gameObject = srcBody.gameObject;
                    ai.onBodyDiscovered -= SetEnemyToOriginator;
                }
            }
        }
    }
}
