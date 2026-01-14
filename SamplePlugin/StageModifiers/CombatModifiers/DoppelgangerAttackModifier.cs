using SamplePlugin.StageModifiers;
using System;
using System.Collections.Generic;
using System.Text;

namespace RiskOfRoutes.StageModifiers.CombatModifiers
{
    public class DoppelgangerAttackModifier : StageModifier
    {
        public DoppelgangerAttackModifier()
        {
            reservedText = "Your relentless doppelganger will attack you after 3 minutes on a stage";
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
