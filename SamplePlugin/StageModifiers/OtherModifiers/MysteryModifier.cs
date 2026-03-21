using RiskOfRoutes;
using UnityEngine;

namespace SamplePlugin.StageModifiers.OtherModifiers
{
    //test one
    //todo networking
    //пока что убран
    public class MysteryModifier : StageModifier
    {
        public MysteryModifier()
        {
            name = "Mystery";
            reservedText = "You cannot see items";
            activeText = "You cannot see items";
            negative = true;
            stackable = false;
            iconName = "mystery";
        }

        public override void OnEnd()
        {
            Log.Info("On mystery modifier end");

            On.RoR2.PickupDisplay.RebuildModel -= PickupDisplay_RebuildModel;
            On.RoR2.GenericPickupController.GetDisplayName -= GenericPickupController_GetDisplayName;
            On.RoR2.GenericPickupController.GetInspectInfoProvider -= GenericPickupController_GetInspectInfoProvider;
            On.RoR2.ShopTerminalBehavior.Start -= ShopTerminalBehavior_Start;
            On.RoR2.PickupDisplay.Update -= PickupDisplay_Update;
        }

        private void PickupDisplay_Update(On.RoR2.PickupDisplay.orig_Update orig, RoR2.PickupDisplay self)
        {
            orig(self);

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

        public override void OnStart()
        {

            //GameObject mysteryPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/MiscPickups/Mystery/mdlQuestionMark.prefab").WaitForCompletion();
            //Log.Info("QuestionMark");
            Log.Info("On mystery modfiier start");

            On.RoR2.PickupDisplay.RebuildModel += PickupDisplay_RebuildModel;
            On.RoR2.GenericPickupController.GetDisplayName += GenericPickupController_GetDisplayName;
            On.RoR2.GenericPickupController.GetInspectInfoProvider += GenericPickupController_GetInspectInfoProvider;
            On.RoR2.ShopTerminalBehavior.Start += ShopTerminalBehavior_Start;
            On.RoR2.PickupDisplay.Update += PickupDisplay_Update;

        }

        private void ShopTerminalBehavior_Start(On.RoR2.ShopTerminalBehavior.orig_Start orig, RoR2.ShopTerminalBehavior self)
        {
            self.hidden = true;
            orig(self);
        }

        private RoR2.IInspectInfoProvider GenericPickupController_GetInspectInfoProvider(On.RoR2.GenericPickupController.orig_GetInspectInfoProvider orig, RoR2.GenericPickupController self)
        {
            return null;
        }

        private string GenericPickupController_GetDisplayName(On.RoR2.GenericPickupController.orig_GetDisplayName orig, RoR2.GenericPickupController self)
        {
            return "???";
        }

        private void PickupDisplay_RebuildModel(On.RoR2.PickupDisplay.orig_RebuildModel orig, RoR2.PickupDisplay self, GameObject modelObjectOverride)
        {
            Log.Info($"Gameobject display name:{self.modelPrefab}");
            self.hidden = true;

            orig(self, modelObjectOverride);
        }


    }
}
