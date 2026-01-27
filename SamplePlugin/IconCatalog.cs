using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace RiskOfRoutes
{
    public static class IconCatalog
    {
        public static Dictionary<string, Sprite> spriteNames = new Dictionary<string, Sprite>();
        public static AssetBundle bundle;

        public static void Init(string bundlePath)
        {
            bundle = AssetBundle.LoadFromFile(bundlePath);


        }

        public static Sprite ModifierIconFromDef(string def)
        {
            return RiskOfRoutes.placeholderIcon;
        }
    }
}
