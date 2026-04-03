using RiskOfRoutes.StageModifiers;
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
            name = mod.nameToken;
            description = mod.descriptionToken;
            stack = 1;
            isArtifact = mod.isArtifact; //todo
        }

        public ModifierSync(ModifierDef mod, int stack)
        {
            name = mod.nameToken;
            description = mod.descriptionToken;
            this.stack = stack;
            isArtifact = mod.isArtifact; //todo
        }

        public bool Equals(ModifierSync other)
        {
            return this.name == other.name;
        }
    }
}

