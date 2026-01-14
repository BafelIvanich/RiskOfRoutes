

namespace SamplePlugin.StageModifiers
{
    public class NoOneShotProtectionModifier : StageModifier
    {
        public NoOneShotProtectionModifier()
        {
            name = "Fragility";
            reservedText = "<style=cDeath>One shot protection</style> will be disabled on the next stage.";
            activeText = "<style=cDeath>One shot protection</style> will be disabled on this stage.";
            stackable = false;
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
