using RiskOfRoutes.StageModifiers;
using RoR2;
using SamplePlugin;
using SamplePlugin.StageModifiers.OtherModifiers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RiskOfRoutes
{
    //todo чтобы брал иконки напрямую из ModifierCatalog
    public static class IconCatalog
    {
        public static Dictionary<string, Sprite> spriteNames = new Dictionary<string, Sprite>();
        public static AssetBundle iconBundle;

        public static void Init(AssetBundle bundle)
        {
            Log.Info("Icon catalog init called");
            iconBundle = bundle;

            foreach (var modDef in StageModifierCatalog.AllDefs)
            {
                var mod = StageModifierCatalog.StageModifierFromDef(modDef);

                if (mod == null || string.IsNullOrEmpty(mod.iconName)) continue;

                if (spriteNames.ContainsKey(mod.iconName)) continue;

                Sprite sprite;
                if (mod.iconName.Contains("RoR2"))
                {
                    sprite = Addressables.LoadAssetAsync<Sprite>(mod.iconName).WaitForCompletion();
                }
                else
                {
                    if (mod is ArtifactModifier)
                    {
                        Log.Info("Loading artifact icon");
                        ArtifactDef art = RoR2.ArtifactCatalog.FindArtifactDef(mod.iconName);
                        if (art != null) sprite = art.smallIconSelectedSprite;
                        else
                        {
                            Log.Error($"Error loading sprite for artifact {mod.iconName}");
                            sprite = RiskOfRoutes.placeholderIcon;
                        }
                    }
                    else
                    {
                        sprite = bundle.LoadAsset<Sprite>(mod.iconName);

                    }
                }

                if (sprite != null)
                {
                    spriteNames.Add(mod.name, sprite);
                }
                else
                {
                    Log.Error($"Error loading sprite for {mod.name}");
                }
            }
            //предметы
            foreach (var itemInfo in StageModifierDirector.itemsInfo)
            {
                var name = itemInfo.internalName;
                ItemIndex itemIndex = RoR2.ItemCatalog.FindItemIndex(name);
                if (itemIndex != ItemIndex.None)
                {
                    var itemDef = RoR2.ItemCatalog.GetItemDef(itemIndex);
                    Sprite sprite = itemDef.pickupIconSprite;
                    if (sprite != null)
                    {
                        spriteNames.Add(name, sprite);
                    }

                }
                else
                {
                    Log.Error($"Item index not found for name {name}");
                }
            }



        }

        public static Sprite ModifierIconFromDef(string def)
        {
            if (def.Contains("Printer")) def = def.Split(" ")[0];
            if (spriteNames.ContainsKey(def))
            {
                return spriteNames[def];
            }
            else
            {
                //Log.Error($"Icon not found for modifier with name {def}");
                return RiskOfRoutes.placeholderIcon;
            }
        }
    }
}
