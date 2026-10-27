using Mono.Cecil.Cil;
using MonoMod.Cil;
using RiskOfRoutes;
using RoR2;
using System;
using UnityEngine;


namespace SamplePlugin.StageModifiers.BossRewardModifiers
{
    public class DroneBossModifier : StageModifier
    {
        public bool dropLegendary = false;
        public bool isAurelioniteActive = false;

        public DroneBossModifier(bool dropLegendary = false)
        {
            this.dropLegendary = dropLegendary;
        }

        public override void OnEnd()
        {
            IL.RoR2.BossGroup.DropRewards -= DropDroneReward;
            Log.Info("BossDroneReward on end");
        }

        public override void OnStart()
        {
            IL.RoR2.BossGroup.DropRewards += DropDroneReward;
            Log.Info("BossItemModifier on start");
        }

        private void DropDroneReward(ILContext il)
        {
            Log.Info("Dropping drone");
            var aurelioniteMod = StageModifierDirector.instance.IsModifierActive("Aurelionite");
            if (aurelioniteMod) return;
            ILCursor c = new ILCursor(il);
            bool noBossTables = true;

            if (noBossTables)
            {
                if (c.TryGotoNext(MoveType.Before,
                    x => x.MatchCallOrCallvirt(typeof(PickupDropletController), nameof(PickupDropletController.CreatePickupDroplet))))
                {
                    c.Remove();

                    c.Emit(OpCodes.Ldarg_0);

                    c.EmitDelegate<Action<UniquePickup, Vector3, Vector3, BossGroup>>((origPickup, position, vector, self) =>
                    {
                        UniquePickup pickup = origPickup;

                        RoR2.DroneIndex reward;
                        if (dropLegendary)
                            reward = Run.instance.stageRng.NextElementUniform(Run.instance.availableTier3DroneList);
                        else
                        {
                            var pool = Run.instance.availableTier2DroneList;
                            pool.Remove(RoR2.RoR2Content.DroneDefs.EquipmentDrone.droneIndex);
                            reward = Run.instance.stageRng.NextElementUniform(pool);
                        }
                        var pickupIndex = PickupCatalog.FindPickupIndex(reward);
                        if (pickupIndex == null)
                        {
                            Log.Error($"{nameof(DropDroneReward)}: Couldnt select drone to drop reward");
                        }
                        pickup = new UniquePickup(pickupIndex);
                        var pickupInfo = new GenericPickupController.CreatePickupInfo
                        {
                            pickup = pickup,
                            rotation = Quaternion.identity,
                            position = position,
                        };

                        PickupDropletController.CreatePickupDroplet(pickupInfo, position, vector);



                    });
                }

            }
        }



    }



}
