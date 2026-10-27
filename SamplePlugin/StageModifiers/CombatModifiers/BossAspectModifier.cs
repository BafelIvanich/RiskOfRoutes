using RoR2;
using SamplePlugin.StageModifiers;
using System.Collections.Generic;

namespace RiskOfRoutes.StageModifiers.CombatModifiers
{
    //not used yet
    public class BossAspectModifier : StageModifier
    {
        public List<EquipmentIndex> tier1EliteAspects = new List<EquipmentIndex>()
        {
            RoR2.RoR2Content.Equipment.AffixRed.equipmentIndex,
            RoR2.RoR2Content.Equipment.AffixRed.equipmentIndex,
            RoR2.RoR2Content.Equipment.AffixRed.equipmentIndex,
            RoR2.RoR2Content.Equipment.AffixRed.equipmentIndex,
            RoR2.RoR2Content.Equipment.AffixRed.equipmentIndex,
        };

        public List<EquipmentIndex> tier2EliteAspects = new List<EquipmentIndex>()
        {
            RoR2.RoR2Content.Equipment.AffixRed.equipmentIndex,
        };
        public List<EquipmentIndex> tier3EliteAspects = new List<EquipmentIndex>()
        {
            RoR2.RoR2Content.Equipment.AffixRed.equipmentIndex,
        };
        public override void OnEnd()
        {
            Log.Info("Boss aspect on end");
            RoR2.SpawnCard.onSpawnedServerGlobal -= OnSpawnCardOnSpawnedServerGlobal;
        }

        public override void OnStart()
        {
            Log.Info("Boss aspect on start");
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
            component.inventory.SetEquipmentIndex(RoR2.RoR2Content.Equipment.AffixRed.equipmentIndex, true);

        }
    }
}
