using RoR2;
using SamplePlugin.StageModifiers;
using System;
using UnityEngine;

namespace RiskOfRoutes.StageModifiers.CombatModifiers
{
    //not used yet
    public class BossSwarmModifier : StageModifier
    {
        public int spawnCount = 10;
        private static bool inSpawn = false;

        public override void OnEnd()
        {
            Log.Info("Boss swarm on end");
            RoR2.SpawnCard.onSpawnedServerGlobal -= OnSpawnCardOnSpawnedServerGlobal;
        }

        public override void OnStart()
        {
            Log.Info("Boss swarm on start");
            RoR2.SpawnCard.onSpawnedServerGlobal += OnSpawnCardOnSpawnedServerGlobal;
        }

        private void OnSpawnCardOnSpawnedServerGlobal(SpawnCard.SpawnResult result)
        {
            if (!result.success || !(result.spawnRequest.spawnCard as CharacterSpawnCard) || result.spawnRequest.teamIndexOverride == TeamIndex.Player)
            {
                return;
            }

            CharacterMaster component = result.spawnedInstance.gameObject.GetComponent<CharacterMaster>();

            if (component == null) return;

            var body = component.GetBody();

            if (body == null) return;
            if (!body.isBoss) return;

            Log.Info("Boss spawned:" + body.name);
            component.inventory.GiveItemPermanent(RoR2.RoR2Content.Items.CutHp, spawnCount / 2);

            if (inSpawn)
            {
                return;
            }
            for (int i = 1; i < spawnCount; i++)
            {
                inSpawn = true;
                try
                {
                    DirectorCore.instance.TrySpawnObject(result.spawnRequest);
                }
                catch (Exception message)
                {
                    Debug.LogError(message);
                }
                inSpawn = false;
            }
        }
    }
}
