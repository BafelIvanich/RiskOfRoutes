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
        public float selectionWeight;
        public Func<bool> isAvailable = () => true;
        public List<string> conflictList = new List<string>();
        public int maxStack = -1;
        public Sprite icon;

        public string nameToken;
        public string descriptionToken;

        public bool isNegative = false;
        public bool isArtifact = false;




        public ModifierDef(string name, ModifierTier tier, float weight, bool stackable, bool isNegative, string[] conflicts = null, int maxStack = -1)
        {
            this.name = name;
            this.cost = tier switch
            {
                ModifierTier.Tier1 => 30,
                ModifierTier.Tier2 => 60,
                ModifierTier.Tier3 => 120,
                _ => 30,
            };
            this.selectionWeight = weight;
            this.isNegative = isNegative;
            if (!stackable)
            {
                conflictList.Add(name);
            }
            if (conflicts != null)
            {
                foreach (var c in conflicts)
                {
                    conflictList.Add(c);
                }
            }
        }

        public ModifierDef(string name, ModifierTier tier, float weight, Func<bool> isAvailable, bool stackable, bool isNegative, string[] conflicts = null, int maxStack = -1)
        {
            this.name = name;
            this.cost = tier switch
            {
                ModifierTier.Tier1 => 30,
                ModifierTier.Tier2 => 60,
                ModifierTier.Tier3 => 120,
                _ => 30,
            };
            this.selectionWeight = weight;
            this.isNegative = isNegative;
            this.isAvailable = isAvailable;
            if (!stackable)
            {
                conflictList.Add(name);
            }
            if (conflicts != null)
            {
                foreach (var c in conflicts)
                {
                    conflictList.Add(c);
                }
            }
        }

        public bool IsStackable()
        {
            return !conflictList.Contains(name);
        }
    }
}

public enum ModifierTier
{
    Tier1, Tier2, Tier3
}

public enum ModifierRarity
{
    Common, Rare
}
