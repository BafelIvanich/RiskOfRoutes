using SamplePlugin.StageModifiers;
using System;
using System.Collections.Generic;
using System.Text;

namespace RiskOfRoutes.StageModifiers.SceneModifiers
{
    //Каждое открытие сундука стакает на всех игроках проклятье на макс хп.
    public class StackingCurseModifier : StageModifier
    {
        public StackingCurseModifier()
        {
            reservedText = "Each purchase drains your max hp for an entire stage";
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
