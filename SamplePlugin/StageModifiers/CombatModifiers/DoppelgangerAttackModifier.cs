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
        float kd = 0;
        public DoppelgangerAttackModifier()
        {
            reservedText = "Your relentless doppelganger will attack you each 3 minutes on a stage";
            name = "Doppelganger";
            activeText = reservedText;
        }

        public override void OnEnd()
        {
            On.RoR2.Run.FixedUpdate -= Run_FixedUpdate;
        }

        public override void OnStart()
        {
            kd = RoR2.Run.instance.time + 180f;
            On.RoR2.Run.FixedUpdate += Run_FixedUpdate;
        }

        private void Run_FixedUpdate(On.RoR2.Run.orig_FixedUpdate orig, Run self)
        {
            orig(self);
            if (!NetworkServer.active || self.isGameOverServer) return;

            Log.Info($"{self.fixedTime}:{kd}");

            if (self.fixedTime > kd)
            {
                Log.Info("Here doppel");
                PerformInvasion();
                kd = self.fixedTime + 180f;
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
