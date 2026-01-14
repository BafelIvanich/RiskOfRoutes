using SamplePlugin.StageModifiers;
using System;
using System.Collections.Generic;
using System.Text;

namespace RiskOfRoutes.StageModifiers.OtherModifiers
{
    //На стейдже будет активно то количество артефактов горы, что были на том стейдже, где был выбран этот портал
    public class SaveMountainEffectModifier: StageModifier
    {
        public SaveMountainEffectModifier()
        {
            reservedText = "Save all mountain shrine effects from the current stage";
        }

        public override void OnEnd()
        {
            throw new NotImplementedException();
        }

        public override void OnStart()
        {
            throw new NotImplementedException();
        }
    }
}
