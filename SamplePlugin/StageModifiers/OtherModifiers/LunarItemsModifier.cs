using SamplePlugin.StageModifiers;

namespace RiskOfRoutes.StageModifiers.OtherModifiers
{
    //На стейдже будет активно то количество артефактов горы, что были на том стейдже, где был выбран этот портал
    public class LunarItemsModifier : StageModifier
    {
        public LunarItemsModifier()
        {
            if (RoR2.TeleporterInteraction.instance)
            {
                stack = RoR2.TeleporterInteraction.instance.shrineBonusStacks;
            }
        }

        public override void OnEnd()
        {
            Log.Info("Shrine safe on end");
            RoR2.Stage.onServerStageBegin -= Stage_onServerStageBegin;
        }

        public override void OnStart()
        {
            RoR2.Stage.onServerStageBegin += Stage_onServerStageBegin;
        }

        private void Stage_onServerStageBegin(RoR2.Stage obj)
        {
            RoR2.TeleporterInteraction.instance.SetShrineStack(stack);
        }
    }
}
