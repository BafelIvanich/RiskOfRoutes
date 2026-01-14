namespace SamplePlugin.StageModifiers.OtherModifiers
{
    public class NoOneShotProtectionModifier : StageModifier
    {
        public NoOneShotProtectionModifier()
        {
            name = "No one shot protection";
            reservedText = "<style=cDeath>One shot protection</style> will be disabled on the next stage.";
            activeText = "<style=cDeath>One shot protection</style> will be disabled on this stage.";
            stackable = false;
            negative = true;
            this.cost = cost;
        }

        public override void OnEnd()
        {
            throw new System.NotImplementedException();
        }

        public override void OnStart()
        {
            throw new System.NotImplementedException();
        }
    }



}
