using RoR2;
using SamplePlugin;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RiskOfRoutes
{
    public class ModifierHologramController : MonoBehaviour
    {
        private GameObject droneIcon;
        private GameObject rewardIcon;
        private GameObject aurelioniteIcon;

        public Texture2D droneTex;
        public Texture2D rewardTex;
        public Texture2D aurelioniteTex;

        public Color uncommon = new Color(36f / 255f, 191f / 255f, 0f, 1f);
        public Color legendary = new Color(255f / 255f, 56f / 255f, 50f / 255f, 1f);
        public Color boss = new Color(255f / 255f, 235f / 255f, 49f / 255f, 1f);
        public Color aurelionite = new Color(255f / 255f, 144f / 255f, 0, 1f);
        public Color mountain = new Color(61f / 255f, 220f / 255f, 255f / 255f, 1f);

        private void Start()
        {
            Transform bossIndicator = transform.Find("TeleporterBaseMesh/HologramPivot/ShrineBossIndicator");
            if (bossIndicator == null) return;

            var bossSymbol = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ShrineBoss/ShrineBossSymbol.prefab").WaitForCompletion();

            droneIcon = CreateIcon(bossSymbol, bossIndicator, droneTex, "Hologram_Drone");
            rewardIcon = CreateIcon(bossSymbol, bossIndicator, rewardTex, "Hologram_Reward");
            aurelioniteIcon = CreateIcon(bossSymbol, bossIndicator, aurelioniteTex, "Hologram_Aurelionite");

            RefreshHologramIcons();

            RiskOfRoutes.hologramsAreColored.SettingChanged += OnConfigChanged;
        }

        private void OnDestroy()
        {
            RiskOfRoutes.hologramsAreColored.SettingChanged -= OnConfigChanged;
        }

        private void OnConfigChanged(object sender, System.EventArgs e)
        {
            Log.Info("Config changed, refreshing hologram icons");
            RefreshHologramIcons();
        }

        public void RefreshHologramIcons()
        {
            if (StageModifierDirector.instance != null && TeleporterInteraction.instance != null)
            {
                bool isAurelioniteActive = StageModifierDirector.instance.IsModifierActive("Aurelionite");
                bool isDroneGreenActive = StageModifierDirector.instance.IsModifierActive("DroneBossGreen");
                bool isDroneRedActive = StageModifierDirector.instance.IsModifierActive("DroneBossRed");
                bool isRewardYellowActive = StageModifierDirector.instance.IsModifierActive("BossYellowItem");
                bool isRewardRedActive = StageModifierDirector.instance.IsModifierActive("BossRedItem");

                //Log.Info($"isAurelioniteActive:{isAurelioniteActive}");

                if (isAurelioniteActive) SetAurelioniteIconActive(true);
                if (isDroneGreenActive) SetDroneIconActive(true);
                if (isDroneRedActive) SetDroneIconActive(true, true);
                if (isRewardYellowActive) SetRewardIconActive(true);
                if (isRewardRedActive) SetRewardIconActive(true, true);
            }
        }

        private GameObject CreateIcon(GameObject prefab, Transform parent, Texture2D texture, string iconName)
        {
            GameObject placeholder = new GameObject(iconName);
            placeholder.transform.SetParent(parent, false);

            var rect = placeholder.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(1f, 1f);
            rect.localScale = Vector3.one;

            GameObject obj = Instantiate(prefab, placeholder.transform, false);
            obj.name = iconName + "_Visual";

            var renderer = obj.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.material.SetTexture("_MainTex", texture);
            }

            placeholder.SetActive(false);

            return placeholder;
        }

        public void SetDroneIconActive(bool active, bool isLegendary = false)
        {
            if (droneIcon == null) return;

            var visualObj = droneIcon.transform.Find("Hologram_Drone_Visual");

            if (visualObj == null) return;

            Log.Info($"drone Icon is null {droneIcon == null} || {visualObj == null}");
            var renderer = visualObj.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                if (RiskOfRoutes.hologramsAreColored.Value)
                {
                    if (isLegendary)
                    {
                        renderer.material.SetColor("_TintColor", legendary);
                    }
                    else
                    {
                        renderer.material.SetColor("_TintColor", uncommon);
                    }
                }
                else
                {
                    renderer.material.SetColor("_TintColor", mountain);
                }
            }

            droneIcon.SetActive(active);
        }
        public void SetRewardIconActive(bool active, bool isLegendary = false)
        {
            if (rewardIcon == null) return;

            var visualObj = rewardIcon.transform.Find("Hologram_Reward_Visual");

            if (visualObj == null) return;

            var renderer = visualObj.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                if (RiskOfRoutes.hologramsAreColored.Value)
                {
                    if (isLegendary)
                    {
                        renderer.material.SetColor("_TintColor", legendary);
                    }
                    else
                    {
                        renderer.material.SetColor("_TintColor", boss);
                    }

                }
                else
                {
                    renderer.material.SetColor("_TintColor", mountain);
                }
            }
            rewardIcon.SetActive(active);
        }
        public void SetAurelioniteIconActive(bool active)
        {
            if (aurelioniteIcon == null) return;

            var visualObj = aurelioniteIcon.transform.Find("Hologram_Aurelionite_Visual");

            if (visualObj == null) return;

            var renderer = visualObj.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                if (RiskOfRoutes.hologramsAreColored.Value)
                {
                    renderer.material.SetColor("_TintColor", aurelionite);
                }
                else
                {
                    renderer.material.SetColor("_TintColor", mountain);
                }

            }
            aurelioniteIcon.SetActive(active);
        }

    }
}
