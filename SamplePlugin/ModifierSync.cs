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

        public ModifierSync(StageModifier mod)
        {
            name = mod.name;
            description = mod.activeText;
            stack = mod.stack;
            isArtifact = mod is ArtifactModifier ? true : false;
        }

        public bool Equals(ModifierSync other)
        {
            return this.name == other.name;
        }
    }
}

