using SamplePlugin.StageModifiers;

namespace RiskOfRoutes.StageModifiers.OtherModifiers
{
    public class SaveMountainEffectModifier : StageModifier
    {
        public SaveMountainEffectModifier(int stack)
        {
            this.stack = stack;
        }

        public override void OnEnd()
        {
            Log.Info("SaveMountain on end");
            RoR2.Stage.onServerStageBegin -= Stage_onServerStageBegin;

        }

        public override void OnStart()
        {
            Log.Info("SaveMountain on start");
            RoR2.Stage.onServerStageBegin += Stage_onServerStageBegin;
        }

        private void Stage_onServerStageBegin(RoR2.Stage obj)
        {
            RoR2.TeleporterInteraction.instance.SetShrineStack(stack);
        }
    }
}
