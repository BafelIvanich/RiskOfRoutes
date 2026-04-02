using RiskOfRoutes.StageModifiers;
using SamplePlugin.StageModifiers;
using SamplePlugin.StageModifiers.OtherModifiers;
using System;

namespace SamplePlugin
{
    [System.Serializable]
    public struct ModifierSync : IEquatable<ModifierSync>
    {
        public string name;
        public string description;
        public int stack;
        public bool isArtifact;

        public ModifierSync(ModifierDef mod)
        {
            name = mod.name;
            description = mod.name;
            stack = 1;
            isArtifact = mod.;
        }

        public bool Equals(ModifierSync other)
        {
            return this.name == other.name;
        }
    }
}

