using System;
using System.Collections.Generic;
using System.Text;

namespace SamplePlugin
{
    public abstract class StageModifier
    {
        public string text;
        public bool negative;

        public abstract void OnStart();

        public abstract void OnEnd();


    }

    public class ArtifactModifier : StageModifier
    {

    }
}
