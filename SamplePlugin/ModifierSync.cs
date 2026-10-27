using RiskOfRoutes.StageModifiers;
using System;

namespace SamplePlugin
{
    [System.Serializable]
    public struct ModifierSync : IEquatable<ModifierSync>
    {
        public string name;
        public int stack;
        public bool isArtifact;

        //todo item extra stack for networking

        public ModifierSync(ModifierDef mod)
        {
            name = mod.name;
            stack = 1;
            isArtifact = mod.isArtifact;
        }

        public ModifierSync(ModifierDef mod, int stack)
        {
            name = mod.name;
            this.stack = stack;
            isArtifact = mod.isArtifact;
        }

        public bool Equals(ModifierSync other)
        {
            return this.name == other.name;
        }
    }
}

