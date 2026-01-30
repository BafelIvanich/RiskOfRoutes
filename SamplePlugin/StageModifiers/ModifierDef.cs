using System;
using System.Collections.Generic;

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


        public ModifierDef(string name, int cost, float weight, bool stackable, string[] conflicts = null, int maxStack = -1)
        {
            this.name = name;
            this.cost = cost;
            this.selectionWeight = weight;
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

            this.maxStack = maxStack;
        }

        public ModifierDef(string name, int cost, float weight, Func<bool> isAvailable, bool stackable, string[] conflicts = null, int maxStack = -1)
        {
            this.name = name;
            this.cost = cost;
            this.selectionWeight = weight;
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

        public ModifierDef(string name, ModifierTier tier, float weight, bool stackable, string[] conflicts = null, int maxStack = -1)
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

        public ModifierDef(string name, ModifierTier tier, float weight, Func<bool> isAvailable, bool stackable, string[] conflicts = null, int maxStack = -1)
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
