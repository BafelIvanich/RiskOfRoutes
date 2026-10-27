using RiskOfRoutes.StageModifiers;
using RoR2;
using UnityEngine;

namespace RiskOfRoutes
{
    public class DefInfo
    {
        public string name;
        public string nameToken;
        public string descriptionToken;
        public Sprite sprite;
        public Color tooltipColor;

        public ModifierTier tier = ModifierTier.Tier1;
        public bool isNegative = false;

        public DefInfo()
        {
        }

        public DefInfo(ItemDef def)
        {
            this.name = def.name;
            this.nameToken = def.nameToken;
            this.descriptionToken = def.descriptionToken;
            this.sprite = def.pickupIconSprite;
            this.tooltipColor = ColorCatalog.GetColor(def.darkColorIndex);
        }

        public DefInfo(ArtifactDef def)
        {
            this.name = def.cachedName;
            this.nameToken = def.nameToken;
            this.descriptionToken = def.descriptionToken;
            this.sprite = def.smallIconSelectedSprite;
            this.tooltipColor = ColorCatalog.GetColor(ColorCatalog.ColorIndex.Artifact);
        }

        public DefInfo(ModifierDef def)
        {
            this.name = def.name;
            this.nameToken = def.nameToken;
            this.descriptionToken = def.descriptionToken;
            this.sprite = def.spriteIcon;
            this.tooltipColor = ColorCatalog.GetColor(ColorCatalog.ColorIndex.Combat);
            this.tier = def.tier;
            this.isNegative = def.isNegative;
        }
    }


}
