using RiskOfRoutes;
using UnityEngine;

//whites: crowbar, bears, bleed, bombs, watches, gasoline, focus crystal, syringe, key
//greens: reload one, ukulele, active charge, wisps, band 1, band 2, injection, pinwheel, fin, star, rockets, energy shield, gasoline, wire


namespace SamplePlugin.StageModifiers
{
    public class BossItemModifier : StageModifier
    {

        public BossItemModifier()
        {
            name = "Boss item modifier";
            activeText = "Boss will drop <style=cShrine>yellow item</style> on this stage.";
            reservedText = "Boss is guaranteed to drop <style=cShrine>Yellow Item.</style>";
            stackable = false;
            negative = false;

        }
        public override void OnEnd()
        {
            //On.RoR2.BossGroup.DropRewards -= BossGroup_DropRewards;
            Log.Info("BossItemModifier end");
        }

        public override void OnStart()
        {
            //On.RoR2.BossGroup.DropRewards += BossGroup_DropRewards;
            Log.Info("BossItemModifier start");
            RoR2.Chat.AddMessage("Boss would drop yellow");

        }

        //todo ллогика если группа мобов заместо босса
        private void BossGroup_DropRewards(On.RoR2.BossGroup.orig_DropRewards orig, RoR2.BossGroup self)
        {
            Log.Info("Active");
            var bossDropTable = ScriptableObject.CreateInstance<RoR2.BasicPickupDropTable>();
            self.bossDropChance = 1.0f;
            bossDropTable.tier1Weight = 0f;
            bossDropTable.tier2Weight = 0f;
            bossDropTable.tier3Weight = 0f;
            bossDropTable.bossWeight = 1f;
            bossDropTable.lunarItemWeight = 0f;
            bossDropTable.voidTier1Weight = 0f;
            bossDropTable.voidTier2Weight = 0f;
            bossDropTable.voidTier3Weight = 0f;
            bossDropTable.voidBossWeight = 0f;

            var originalDropTable = self.dropTable;
            self.dropTable = bossDropTable;

            orig(self);

            Log.Info(self.dropTable.ToString());
        }



    }

    

}
