using MonoMod.Cil;
using RiskOfRoutes;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SamplePlugin.StageModifiers.BossRewardModifiers
{
    public class AurelioniteRewardModifier : StageModifier
    {

        public AurelioniteRewardModifier()
        {
            stackable = false;
        }

        public override void OnEnd()
        {
            Log.Info("HalcyoniteReward on end");
            IL.RoR2.BossGroup.DropRewards -= BossGroup_DropRewards;
        }

        public override void OnStart()
        {
            Log.Info("HalcyoniteReward on start");

            //On.RoR2.BossGroup.DropRewards += DropHalcyoniteReward;
            IL.RoR2.BossGroup.DropRewards += BossGroup_DropRewards;
        }

        private void BossGroup_DropRewards(ILContext il)
        {
            var halcyoniteDropTableTier2 = Addressables.LoadAssetAsync<PickupDropTable>("RoR2/DLC2/dtShrineHalcyoniteTier2.asset").WaitForCompletion();
            var halcyoniteDropTableTier3 = Addressables.LoadAssetAsync<PickupDropTable>("RoR2/DLC2/dtShrineHalcyoniteTier3.asset").WaitForCompletion();
            var rewardPickupPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC2/FragmentPotentialPickup.prefab").WaitForCompletion();

            ILCursor c = new ILCursor(il);

            if (c.TryGotoNext(MoveType.Before,
                x => x.MatchCallOrCallvirt(typeof(PickupDropletController), nameof(PickupDropletController.CreatePickupDroplet))))
            {
                c.Remove();

                c.EmitDelegate<Action<UniquePickup, Vector3, Vector3>>((origPickup, position, vector) =>
                {
                    RoR2.GenericPickupController.CreatePickupInfo pickup;
                    var droneMod = StageModifierDirector.instance.activeModifiers.OfType<DroneBossRewardModifier>().FirstOrDefault();
                    if (droneMod != null)
                    {
                        List<UniquePickup> pickups = new List<UniquePickup>();

                        var all = Run.instance.availableTier2DroneList;
                        foreach (var drone in all)
                        {
                            var pickIndex = PickupCatalog.FindPickupIndex(drone);
                            if (pickIndex != null)
                            {
                                pickups.Add(new UniquePickup(PickupCatalog.FindPickupIndex(drone)));
                            }
                        }
                        Util.ShuffleList(pickups);
                        pickup = new GenericPickupController.CreatePickupInfo
                        {
                            pickup = new UniquePickup(PickupCatalog.FindPickupIndex(ItemTier.Tier1)),
                            pickerOptions = PickupPickerController.GenerateOptionsFromList(pickups.Take(5).ToList()),
                            rotation = Quaternion.identity,
                            position = position,
                            prefabOverride = rewardPickupPrefab
                        };
                    }
                    else
                    {
                        pickup = new GenericPickupController.CreatePickupInfo
                        {
                            pickup = new UniquePickup(PickupCatalog.FindPickupIndex(ItemTier.Tier1)),
                            pickerOptions = PickupPickerController.GenerateOptionsFromDropTablePlusForcedStorm(3, halcyoniteDropTableTier3, halcyoniteDropTableTier2, Run.instance.stageRng),
                            rotation = Quaternion.identity,
                            position = position,
                            prefabOverride = rewardPickupPrefab
                        };

                    }


                    PickupDropletController.CreatePickupDroplet(pickup, position, vector);
                    Log.Info("Created pickup");


                });
            }
        }

    }
}
