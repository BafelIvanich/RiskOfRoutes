using System;
using System.Collections.Generic;
using UnityEngine;

namespace RiskOfRoutes.StageModifiers
{
    public class ModifierDef
    {
        public string name;
        public ModifierTier tier;
        public int cost;
        public float weight;
        public Func<bool> isAvailable = () => true;
        public List<string> conflictList = new List<string>();
        public int maxStack = -1;
        public Sprite spriteIcon;

        public string nameToken;
        public string descriptionToken;

        public bool isNegative = false;
        public bool isArtifact = false;
        public bool isStackable = false;

        public ModifierDef(string name, ModifierTier tier, float weight, bool isStackable, bool isNegative, string[] conflicts = null, int maxStack = -1)
        {
            this.name = name;
            this.tier = tier;
            this.weight = weight;
            this.isNegative = isNegative;
            this.isStackable = isStackable;

            if (conflicts != null)
            {
                foreach (var c in conflicts)
                {
                    conflictList.Add(c);
                }
            }
        }

        public ModifierDef(string name, ModifierTier tier, float weight, Func<bool> isAvailable, bool isStackable, bool isNegative, string[] conflicts = null, int maxStack = -1)
        {
            this.name = name;
            this.tier = tier;
            this.weight = weight;
            this.isNegative = isNegative;
            this.isAvailable = isAvailable;
            this.isStackable = isStackable;
            if (conflicts != null)
            {
                foreach (var c in conflicts)
                {
                    conflictList.Add(c);
                }
            }
        }

    }
}

public enum ModifierTier
{
    None = 0, Tier1 = 1, Tier2 = 2, Tier3 = 3
}

public enum ModifierRarity
{
    Common, Rare
}
