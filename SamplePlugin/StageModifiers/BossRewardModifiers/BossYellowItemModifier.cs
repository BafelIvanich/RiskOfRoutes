using Mono.Cecil.Cil;
using MonoMod.Cil;
using RiskOfRoutes;
using RoR2;
using System;
using UnityEngine;

//whites: crowbar, bears, bleed, bombs, watches, gasoline, focus crystal, syringe, key
//greens: reload one, ukulele, active charge, wisps, band 1, band 2, injection, pinwheel, fin, star, rockets, energy shield, gasoline, wire


namespace SamplePlugin.StageModifiers.BossRewardModifiers
{
    public class BossYellowItemModifier : StageModifier
    {

        public BossYellowItemModifier()
        {
        }
        public override void OnEnd()
        {
            IL.RoR2.BossGroup.DropRewards -= BossGroup_DropRewards;
            Log.Info("BossYellowItem on end");
        }

        public override void OnStart()
        {
            IL.RoR2.BossGroup.DropRewards += BossGroup_DropRewards;
            Log.Info("BossYellowItem on start");
        }

        private void BossGroup_DropRewards(ILContext il)
        {
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

                        bool hasDropTables = self.bossDropTables != null && self.bossDropTables.Count > 0;
                        bool hasUniqueDrops = self.bossDrops != null && self.bossDrops.Count > 0;

                        //boss signature
                        if (hasDropTables)
                        {
                            PickupDropTable table = self.rng.NextElementUniform(self.bossDropTables);
                            Log.Info($"Boss has dropTable:{table}");
                            if (table != null)
                            {
                                pickup = table.GeneratePickup(self.rng);
                            }
                        }
                        //2 
                        else if (hasUniqueDrops)
                        {
                            Log.Info($"Boss has bossDrops:{self.bossDrops}");
                            pickup = new UniquePickup(self.rng.NextElementUniform(self.bossDrops).pickupIndex);
                        }
                        //enemies as a boss
                        else
                        {
                            Log.Info("Boss is group of enemies");
                            if (Run.instance && Run.instance.availableBossDropList.Count > 0)
                            {
                                var pickupIndex = Run.instance.stageRng.NextElementUniform(Run.instance.availableBossDropList);
                                pickup = new UniquePickup(pickupIndex);
                            }
                        }

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
