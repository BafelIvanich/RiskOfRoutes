using RiskOfRoutes;
using UnityEngine;
using UnityEngine.AddressableAssets;


namespace SamplePlugin.StageModifiers
{
    //карты: iscVoidBarnacle, iscNullifier, iscVoidJailer, iscVoidMegaCrab
    public class VoidEnemiesModifier : StageModifier
    {
        public VoidEnemiesModifier()
        {
            name = "Void invasion";
            reservedText = "<style=cIsVoid>Enemies from the void</style> might appear on the next stage.";
            activeText = "<style=cIsVoid>Enemies from the void</style> might appear on this stage.";
            var icon = Addressables.LoadAssetAsync<Sprite>("RoR2/DLC1/GameModes/InfiniteTowerRun/ITAssets/texITWaveVoidIcon.png").WaitForCompletion();
            if (!icon)
            {
                Log.Info("GFGG");
            }
            else
            {
                this.icon = icon;
            }
        }

        public override void OnEnd()
        {
            Log.Info("Void invasion on end");
        }

        public override void OnStart()
        {
            Log.Info("Void invasion on start");
        }
    }

    

}
