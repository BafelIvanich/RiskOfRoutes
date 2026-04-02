using RoR2;
using SamplePlugin;
using UnityEngine;
namespace RiskOfRoutes
{
    public class MysteryArtifact
    {
        public static ArtifactDef Mystery = ScriptableObject.CreateInstance<ArtifactDef>();
        public static void InitializeArtifact()
        {
            Mystery.nameToken = "Artifact of mystery";
            Mystery.descriptionToken = "All item pickups are obscured.";
            Mystery.smallIconDeselectedSprite = RiskOfRoutes.placeholderIcon;
            Mystery.smallIconSelectedSprite = RiskOfRoutes.placeholderIcon;
            Mystery.cachedName = "Mystery";
            R2API.ContentAddition.AddArtifactDef(Mystery);

            On.RoR2.PickupDisplay.RebuildModel += PickupDisplay_RebuildModel;
            On.RoR2.GenericPickupController.GetDisplayName += GenericPickupController_GetDisplayName;
            On.RoR2.GenericPickupController.GetInspectInfoProvider += GenericPickupController_GetInspectInfoProvider;
            On.RoR2.ShopTerminalBehavior.Start += ShopTerminalBehavior_Start;
            On.RoR2.PickupDisplay.Update += PickupDisplay_Update;
        }

        //TODO Looking glass может показать предмет
        private static void PickupDisplay_Update(On.RoR2.PickupDisplay.orig_Update orig, RoR2.PickupDisplay self)
        {
            orig(self);

            if (RunArtifactManager.instance != null && RunArtifactManager.instance.IsArtifactEnabled(Mystery))
            {
                if (self.hidden && self.modelObject)
                {
                    var parent = self.GetComponentInParent<RoR2.ShopTerminalBehavior>();
                    if (parent != null && parent.name.Contains("Duplicator"))
                    {
                        self.modelObject.transform.localPosition += Vector3.up * 1.0f;
                    }
                    else
                    {
                        self.modelObject.transform.localPosition += Vector3.up * 0.1f;
                    }
                }
            }

        }

        private static void ShopTerminalBehavior_Start(On.RoR2.ShopTerminalBehavior.orig_Start orig, RoR2.ShopTerminalBehavior self)
        {
            if (RunArtifactManager.instance.IsArtifactEnabled(Mystery))
                self.hidden = true;
            orig(self);
        }

        private static RoR2.IInspectInfoProvider GenericPickupController_GetInspectInfoProvider(On.RoR2.GenericPickupController.orig_GetInspectInfoProvider orig, RoR2.GenericPickupController self)
        {
            if (RunArtifactManager.instance.IsArtifactEnabled(Mystery))
                return null;
            else
                return orig(self);
        }

        private static string GenericPickupController_GetDisplayName(On.RoR2.GenericPickupController.orig_GetDisplayName orig, RoR2.GenericPickupController self)
        {
            if (RunArtifactManager.instance.IsArtifactEnabled(Mystery))
                return "???";
            else
                return orig(self);
        }

        private static void PickupDisplay_RebuildModel(On.RoR2.PickupDisplay.orig_RebuildModel orig, RoR2.PickupDisplay self, GameObject modelObjectOverride)
        {
            if (RunArtifactManager.instance.IsArtifactEnabled(Mystery))
            {
                Log.Info($"Gameobject display name:{self.modelPrefab}");
                self.hidden = true;
            }

            orig(self, modelObjectOverride);
        }
    }
}