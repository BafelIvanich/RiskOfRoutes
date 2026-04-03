namespace SamplePlugin.StageModifiers.OtherModifiers
{
    public class NoOneShotProtectionModifier : StageModifier
    {
        public NoOneShotProtectionModifier()
        {
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
