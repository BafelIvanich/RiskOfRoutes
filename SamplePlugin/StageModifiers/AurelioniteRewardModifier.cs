using MonoMod.Cil;
using RiskOfRoutes;
using RoR2;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SamplePlugin.StageModifiers
{
    public class AurelioniteRewardModifier : StageModifier
    {
        public AurelioniteRewardModifier()
        {
            reservedText = "Get <color=#E4C415>aurelionite fragment</color> as a teleporter event reward.";
            activeText = "Get <color=#E4C415>aurelionite fragment</color> as a teleporter event reward.";
            name = "Aurelionite Fragment";
            tooltipColor = new Color(228f / 255f, 196f / 255f, 21f / 255f);
            stackable = false;

            var ic = RiskOfRoutes.RiskOfRoutes.bundle.LoadAsset<Sprite>("texBuffAurelioniteBlessingIcon");
            if (ic != null)
            {
                icon = ic;
            }
            else
            {
                Log.Error("failed to load aurelionite modifier");
            }
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

        private void BossGroup_DropRewards(MonoMod.Cil.ILContext il)
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
                    var pickup = new GenericPickupController.CreatePickupInfo
                    {
                        pickup = new RoR2.UniquePickup(PickupCatalog.FindPickupIndex(ItemTier.Tier1)),
                        pickerOptions = PickupPickerController.GenerateOptionsFromDropTablePlusForcedStorm(3, halcyoniteDropTableTier3, halcyoniteDropTableTier2, Run.instance.stageRng),
                        rotation = Quaternion.identity,
                        position = position,
                        prefabOverride = rewardPickupPrefab
                    };

                    PickupDropletController.CreatePickupDroplet(pickup, position, vector);
                    Log.Info("Created pickup");


                });
            }
        }

        private void DropHalcyoniteReward(On.RoR2.BossGroup.orig_DropRewards orig, RoR2.BossGroup self)
        {
            //if (!NetworkServer.active)
            //{
            //    Debug.LogWarning("[Server] function 'System.Void RoR2.HalcyoniteShrineInteractable::DropRewards()' called on client");
            //    return;
            //}
            //if (!Run.instance)
            //{
            //    Debug.LogError("No valid run instance!");
            //    return;
            //}
            //if (Run.instance.stageRng == null)
            //{
            //    Debug.LogError("RNG is null!");
            //    return;
            //}
            //int num = Run.instance.AsValidOrNull()?.participatingPlayerCount ?? 0;
            //if (num <= 0 || !self.dropTable)
            //{
            //    return;
            //}
            //int num2 = num * quantityIncreaseFromBuyIn;
            //float angle = 360f / (float)num2;
            //Vector3 vector = Quaternion.AngleAxis(Random.Range(0, 360), Vector3.up) * (Vector3.up * 40f + Vector3.forward * 5f);
            //Quaternion quaternion = Quaternion.AngleAxis(angle, Vector3.up);
            //Vector3 position = base.gameObject.transform.position + rewardOffset;
            //int num3 = 0;
            //while (num3 < num2)
            //{
            //    if (isCommandEnabled)
            //    {
            //        int num4 = rewardOptionCount - 2;
            //        for (int i = 0; i < num4; i++)
            //        {
            //            UniquePickup pickup = rewardDropTable.GeneratePickup(rng);
            //            GenericPickupController.CreatePickupInfo pickupInfo = new GenericPickupController.CreatePickupInfo
            //            {
            //                pickup = pickup,
            //                rotation = Quaternion.identity,
            //                position = position
            //            };
            //            PickupDropletController.CreatePickupDroplet(pickupInfo, pickupInfo.position, vector);
            //        }
            //    }
            //    else
            //    {
            //        PickupDropletController.CreatePickupDroplet(new GenericPickupController.CreatePickupInfo
            //        {
            //            pickup = new UniquePickup(PickupCatalog.FindPickupIndex(rewardDisplayTier)),
            //            pickerOptions = PickupPickerController.GenerateOptionsFromDropTablePlusForcedStorm(rewardOptionCount, halcyoniteDropTableTier3, halcyoniteDropTableTier2, rng),
            //            rotation = Quaternion.identity,
            //            position = position,
            //            prefabOverride = rewardPickupPrefab
            //        }, position, vector);
            //    }
            //    num3++;
            //    vector = quaternion * vector;
            //}



        }
    }
}
