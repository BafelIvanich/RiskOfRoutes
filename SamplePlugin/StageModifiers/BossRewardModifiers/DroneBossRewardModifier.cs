using Mono.Cecil.Cil;
using MonoMod.Cil;
using RiskOfRoutes;
using RoR2;
using System;
using System.Linq;
using UnityEngine;

//whites: crowbar, bears, bleed, bombs, watches, gasoline, focus crystal, syringe, key
//greens: reload one, ukulele, active charge, wisps, band 1, band 2, injection, pinwheel, fin, star, rockets, energy shield, gasoline, wire


namespace SamplePlugin.StageModifiers.BossRewardModifiers
{
    public class DroneBossRewardModifier : StageModifier
    {
        public bool dropLegendary = false;
        public bool isAurelioniteActive = false;

        //todo может и зеленых
        //todo кроме equipment Дрона
        public DroneBossRewardModifier(bool dropLegendary = false)
        {
            if (dropLegendary)
            {
                reservedText = "Get <style=cDeath>random legendary drone</style> as a teleporter event reward.";
                activeText = "Get <style=cDeath>random legendary drone</style> as a teleporter event reward.";
                name = "Legendary drone reward";
                iconName = "legendaryDroneReward";
            }
            else
            {
                reservedText = "Get <style=cIsHealing>random uncommon drone</style> as a teleporter event reward.";
                activeText = "Get <style=cIsHealing>random uncommon drone</style> as a teleporter event reward.";
                iconName = "uncommonDroneReward";
                name = "Uncommon drone reward";
            }
            this.dropLegendary = dropLegendary;
            tooltipColor = new Color(246f / 255f, 90f / 255f, 58f / 255f);
            stackable = false;

        }

        public override void OnEnd()
        {
            IL.RoR2.BossGroup.DropRewards -= BossGroup_DropRewards;
            Log.Info("BossDroneReward end");
        }

        public override void OnStart()
        {
            // On.RoR2.BossGroup.DropRewards += BossGroup_DropRewards;
            IL.RoR2.BossGroup.DropRewards += BossGroup_DropRewards;
            Log.Info("BossItemModifier start");
            Chat.AddMessage("Boss would drop yellow");

        }

        private void BossGroup_DropRewards(ILContext il)
        {
            var aurelioniteMod = StageModifierDirector.instance.activeModifiers.OfType<AurelioniteRewardModifier>().FirstOrDefault();
            if (aurelioniteMod != null) return;
            ILCursor c = new ILCursor(il);
            bool noBossTables = true;

            if (noBossTables)
            {
                if (c.TryGotoNext(MoveType.Before,
                    x => x.MatchCallOrCallvirt(typeof(PickupDropletController), nameof(PickupDropletController.CreatePickupDroplet))))
                {
                    c.Remove();

                    //self (bossgroup)
                    c.Emit(OpCodes.Ldarg_0);

                    c.EmitDelegate<Action<UniquePickup, Vector3, Vector3, BossGroup>>((origPickup, position, vector, self) =>
                    {
                        UniquePickup pickup = origPickup;

                        //var droneDef = RoR2.DLC3Content.DroneDefs.JunkDrone.droneIndex;
                        //todo убрать дрона снаряжения
                        RoR2.DroneIndex reward;
                        if (dropLegendary)
                            reward = Run.instance.stageRng.NextElementUniform(Run.instance.availableTier3DroneList);
                        else
                            reward = Run.instance.stageRng.NextElementUniform(Run.instance.availableTier2DroneList);
                        var pickupIndex = PickupCatalog.FindPickupIndex(reward);
                        if (pickupIndex == null)
                        {
                            Log.Error("NONE DRONE");
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
