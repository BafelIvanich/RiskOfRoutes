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
        }

        public override void OnEnd()
        {
            Log.Info("AurelioniteReward on end");
            IL.RoR2.BossGroup.DropRewards -= BossGroup_DropRewards;
        }

        public override void OnStart()
        {
            Log.Info("AurelioniteReward on start");

            //On.RoR2.BossGroup.DropRewards += DropHalcyoniteReward;
            IL.RoR2.BossGroup.DropRewards += BossGroup_DropRewards;
        }

        private void BossGroup_DropRewards(ILContext il)
        {
            var halcyoniteDropTableTier2 = Addressables.LoadAssetAsync<PickupDropTable>("RoR2/DLC2/dtShrineHalcyoniteTier2.asset").WaitForCompletion();
            var halcyoniteDropTableTier3 = Addressables.LoadAssetAsync<PickupDropTable>("RoR2/DLC2/dtShrineHalcyoniteTier3.asset").WaitForCompletion();

            //todo обработка BossRedItem BossYellowItem

            var rewardPickupPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC2/FragmentPotentialPickup.prefab").WaitForCompletion();

            ILCursor c = new ILCursor(il);

            if (c.TryGotoNext(MoveType.Before,
                x => x.MatchCallOrCallvirt(typeof(PickupDropletController), nameof(PickupDropletController.CreatePickupDroplet))))
            {
                c.Remove();

                c.EmitDelegate<Action<UniquePickup, Vector3, Vector3>>((origPickup, position, vector) =>
                {
                    RoR2.GenericPickupController.CreatePickupInfo pickup;

                    var greenDrone = StageModifierDirector.instance.IsModifierActive("DroneBossGreen");
                    var redDrone = StageModifierDirector.instance.IsModifierActive("DroneBossRed");

                    var bossYellowItem = StageModifierDirector.instance.IsModifierActive("BossYellowItem");
                    var bossRedItem = StageModifierDirector.instance.IsModifierActive("BossRedItem");
                    if (greenDrone || redDrone)
                    {
                        Log.Info($"{nameof(BossGroup_DropRewards)}: Dropping drone reward");
                        List<UniquePickup> pickups = new List<UniquePickup>();

                        List<DroneIndex> droneRewardPool = new List<DroneIndex>();
                        if (greenDrone)
                        {
                            droneRewardPool = Run.instance.availableTier2DroneList;
                            droneRewardPool.Remove(RoR2.RoR2Content.DroneDefs.EquipmentDrone.droneIndex);
                        }
                        else
                            droneRewardPool = Run.instance.availableTier3DroneList;

                        foreach (var drone in droneRewardPool)
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
                    else if (bossYellowItem || bossRedItem)
                    {
                        Log.Info($"{nameof(BossGroup_DropRewards)}: Dropping boss or red reward");
                        List<UniquePickup> pickups = new List<UniquePickup>();

                        List<PickupIndex> itemRewardPool = new List<PickupIndex>();
                        if (bossYellowItem)
                        {
                            itemRewardPool = Run.instance.availableBossDropList;
                        }
                        else
                            itemRewardPool = Run.instance.availableTier3DropList;

                        foreach (var pickIndex in itemRewardPool)
                        {
                            if (pickIndex != null)
                            {
                                pickups.Add(new UniquePickup(pickIndex));
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
