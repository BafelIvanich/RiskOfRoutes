using RiskOfRoutes.StageModifiers;
using System;

namespace SamplePlugin
{
    [System.Serializable]
    public struct ModifierSync : IEquatable<ModifierSync>
    {
        public string name;

        public string nameToken;
        public string descriptionToken;

        public int stack;
        public bool isArtifact;

        public ModifierSync(ModifierDef mod)
        {
            name = mod.name;
            descriptionToken = mod.descriptionToken;
            stack = 1;
            isArtifact = mod.isArtifact; //todo
        }

        public ModifierSync(ModifierDef mod, int stack)
        {
            name = mod.name;
            descriptionToken = mod.descriptionToken;
            this.stack = stack;
            isArtifact = mod.isArtifact; //todo
        }

        public bool Equals(ModifierSync other)
        {
            return this.name == other.name;
        }
    }
}

