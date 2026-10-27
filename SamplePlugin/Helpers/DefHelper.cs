using RoR2;
using RoR2.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RiskOfRoutes.Helpers
{
    public static class DefHelper
    {
        public static HashSet<ItemDef> vanillaItems;
        public static HashSet<ArtifactDef> vanillaArtifacts;
        public static string[] vanillaPacks = new string[] { "RoR2.BaseContent", "RoR2.Junk", "RoR2.DLC1", "RoR2.CU8", "RoR2.DLC2", "RoR2.DLC3", "burboni4.RiskOfRoutes" };

        public static void Init()
        {
            vanillaItems = new HashSet<ItemDef>();
            vanillaArtifacts = new HashSet<ArtifactDef>();

            foreach (var pack in ContentManager.allLoadedContentPacks)
            {
                if (vanillaPacks.Contains(pack.identifier))
                {
                    foreach (var def in pack.itemDefs)
                    {
                        if (def != null) vanillaItems.Add(def);
                    }
                    foreach (var def in pack.artifactDefs)
                    {
                        if (def != null) vanillaArtifacts.Add(def);
                    }
                }
            }
        }


        public static bool IsModded(ItemDef def)
        {
            return !vanillaItems.Contains(def);
        }

        public static bool IsModded(ArtifactDef def)
        {
            return !vanillaArtifacts.Contains(def);
        }
    }
}
