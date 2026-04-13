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

        public ModifierSync(ModifierDef mod)
        {
            name = mod.name;
            stack = 1;
            isArtifact = mod.isArtifact; //todo
        }

        public ModifierSync(ModifierDef mod, int stack)
        {
            name = mod.name;
            this.stack = stack;
            isArtifact = mod.isArtifact; //todo
        }

        public bool Equals(ModifierSync other)
        {
            return this.name == other.name;
        }
    }
}

