using System;
using System.Collections.Generic;
using System.Text;

namespace RiskOfRoutes.StageModifiers
{
    public class ModifierDef
    {
        public string name;
        public int cost;
        public float selectionWeight;
        public Func<bool> isAvailable = () => true;


        public ModifierDef(string name, int cost, float weight)
        {
            this.name = name;
            this.cost = cost;
            this.selectionWeight = weight;
        }

        public ModifierDef(string name, int cost, float weight, Func<bool> isAvailable)
        {
            this.name = name;
            this.cost = cost;
            this.selectionWeight = weight;
            this.isAvailable = isAvailable;
        }
    }
}

public enum ModifierTier
{
    Tier1, Tier2, Tier3
}
