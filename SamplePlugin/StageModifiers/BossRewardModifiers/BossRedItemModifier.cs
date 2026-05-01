using MonoMod.Cil;
using RiskOfRoutes;
using RoR2;
using System;
using UnityEngine;

namespace SamplePlugin.StageModifiers.BossRewardModifiers
{
    public class BossRedItemModifier : StageModifier
    {
        public BossRedItemModifier()
        {
        }

        public override void OnEnd()
        {
            Log.Info("RedItemReward on end");
            IL.RoR2.BossGroup.DropRewards -= BossGroup_DropRewards;
        }

        public override void OnStart()
        {
            Log.Info("RedItemReward on start");

            //On.RoR2.BossGroup.DropRewards += DropHalcyoniteReward;
            IL.RoR2.BossGroup.DropRewards += BossGroup_DropRewards;
        }

        private void BossGroup_DropRewards(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if (c.TryGotoNext(MoveType.Before,
                x => x.MatchCallOrCallvirt(typeof(PickupDropletController), nameof(PickupDropletController.CreatePickupDroplet))))
            {
                c.Remove();

                c.EmitDelegate<Action<UniquePickup, Vector3, Vector3>>((origPickup, position, vector) =>
                {


                    var pickupIndex = Run.instance.stageRng.NextElementUniform(Run.instance.availableTier3DropList);
                    var uniquePickup = new UniquePickup { pickupIndex = pickupIndex };
                    var pickup = new GenericPickupController.CreatePickupInfo
                    {
                        pickup = uniquePickup,
                        rotation = Quaternion.identity,
                        position = position,

                    };

                    PickupDropletController.CreatePickupDroplet(pickup, position, vector);
                    Log.Info("Created pickup");


                });
            }
        }
    }
}
