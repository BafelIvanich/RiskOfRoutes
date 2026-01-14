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
            name = "Boss item modifier";
            activeText = "Boss will drop <style=cShrine>yellow item</style> on this stage.";
            reservedText = "Boss is guaranteed to drop <style=cShrine>Yellow Item.</style>";
            stackable = false;
            negative = false;
            var ic = RiskOfRoutes.RiskOfRoutes.bundle.LoadAsset<Sprite>("yellowBossItem");
            if (ic != null)
            {
                icon = ic;
            }
            else
            {
                Log.Error("failed to load yyellowBossItem modifier");
            }

        }
        public override void OnEnd()
        {
            IL.RoR2.BossGroup.DropRewards -= BossGroup_DropRewards;
            Log.Info("BossItemModifier end");
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

                        bool hasDropTables = self.bossDropTables != null && self.bossDropTables.Count > 0;
                        bool hasUniqueDrops = self.bossDrops != null && self.bossDrops.Count > 0;

                        //1 уникальный босс
                        if (hasDropTables)
                        {

                            PickupDropTable table = self.rng.NextElementUniform(self.bossDropTables);
                            Log.Info($"Boss has dropTable:{table}");
                            if (table != null)
                            {
                                pickup = table.GeneratePickup(self.rng);
                            }
                        }
                        //2 имеет прописанный лут может быть не боссом
                        else if (hasUniqueDrops)
                        {
                            Log.Info($"Boss has bossDrops:{self.bossDrops}");
                            pickup = new UniquePickup(self.rng.NextElementUniform(self.bossDrops).pickupIndex);
                        }
                        //3 босс группа врагов
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
