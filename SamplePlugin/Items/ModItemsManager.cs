using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace RiskOfRoutes.Items
{
    public class ModItemsManager
    {
        public static ItemDef soulCostCurse;
        public static ItemDef falseSonBless;

        public static void InitializeItems()
        {
            soulCostCurse = ScriptableObject.CreateInstance<ItemDef>();

            soulCostCurse.name = "SOULCOSTCURSE_NAME";
            soulCostCurse.nameToken = "SOULCOSTCURSE_NAME";
            soulCostCurse.pickupToken = "SOULCOSTCURSE_PICKUP";
            soulCostCurse.descriptionToken = "SOULCOSTCURSE_DESC";
            soulCostCurse.loreToken = "SOULCOSTCURSE_LORE";

            soulCostCurse.hidden = true;

            var displayRules = new ItemDisplayRuleDict(null);
            ItemAPI.Add(new CustomItem(soulCostCurse, displayRules));
            //


            falseSonBless = ScriptableObject.CreateInstance<ItemDef>();

            falseSonBless.name = "FALSESONBLESS_NAME";
            falseSonBless.nameToken = "FALSESONBLESS_NAME";
            falseSonBless.pickupToken = "FALSESONBLESS_PICKUP";
            falseSonBless.descriptionToken = "FALSESONBLESS_DESC";
            falseSonBless.loreToken = "FALSESONBLESS_LORE";

            falseSonBless.pickupIconSprite = AssetManager.falseSonHeartIcon;
            falseSonBless.pickupModelPrefab = AssetManager.falseSonHeartPrefab;
            falseSonBless.tags = new ItemTag[] { ItemTag.WorldUnique, ItemTag.AIBlacklist, ItemTag.CannotDuplicate, ItemTag.CannotCopy };

#pragma warning disable Publicizer001
            falseSonBless._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/BossTierDef.asset").WaitForCompletion();
#pragma warning restore Publicizer001

            falseSonBless.hidden = false;

            var falseSonDisplayRules = new ItemDisplayRuleDict(null);

            ItemAPI.Add(new CustomItem(falseSonBless, falseSonDisplayRules));

            //false son thging

            //EntityStates.FalseSonBoss.SkyJumpDeathState.falseSonDeathEvent += DropBless;
            On.EntityStates.FalseSonBoss.SkyJumpDeathState.GiveColossusItem += DropBless;

        }

        private static void DropBless(On.EntityStates.FalseSonBoss.SkyJumpDeathState.orig_GiveColossusItem orig, EntityStates.FalseSonBoss.SkyJumpDeathState self)
        {
            Log.Info("Dropping false son reward 1");

            orig(self);

            if (!NetworkServer.active)
            {
                return;
            }
            if(!RiskOfRoutes.enableFalseSonReward.Value)
            {
                return;
            }
            Log.Info("Dropping false son reward");

            float angle = 180f;
            Vector3 vector = Quaternion.AngleAxis(Random.Range(0, 360), Vector3.up) * (Vector3.up * 40f + Vector3.forward * 5f);
            Quaternion quaternion = Quaternion.AngleAxis(angle, Vector3.up);
            Vector3 position = self.cachedDeathPosition + self.rewardOffset;

            PickupDropletController.CreatePickupDroplet(new GenericPickupController.CreatePickupInfo
            {
                pickupIndex = PickupCatalog.FindPickupIndex(ModItemsManager.falseSonBless.itemIndex),
                rotation = Quaternion.identity,
            }, position, vector);
        }
    }
}
