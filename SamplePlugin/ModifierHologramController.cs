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
        public Color boss = new Color(255f / 255f, 235f / 255f, 49f / 255f,1f);
        public Color aurelionite = new Color(255f / 255f, 144f / 255f, 0, 1f);

        private void Start()
        {
            Transform bossIndicator = transform.Find("TeleporterBaseMesh/HologramPivot/ShrineBossIndicator");
            if (bossIndicator == null) return;

            var bossSymbol = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ShrineBoss/ShrineBossSymbol.prefab").WaitForCompletion();

            droneIcon = CreateIcon(bossSymbol, bossIndicator, droneTex, "Hologram_Drone");
            rewardIcon = CreateIcon(bossSymbol, bossIndicator, rewardTex, "Hologram_Reward");
            aurelioniteIcon = CreateIcon(bossSymbol, bossIndicator, aurelioniteTex, "Hologram_Aurelionite");

            RefreshIcons();
        }

        public void RefreshIcons()
        {
            if (StageModifierDirector.instance != null)
            {
                bool isAurelioniteActive = StageModifierDirector.instance.IsModifierActive("Aurelionite Fragment");
                bool isDroneGreenActive = StageModifierDirector.instance.IsModifierActive("Uncommon drone reward");
                bool isDroneRedActive = StageModifierDirector.instance.IsModifierActive("Legendary drone reward");
                bool isRewardYellowActive = StageModifierDirector.instance.IsModifierActive("Red item");
                bool isRewardRedActive = StageModifierDirector.instance.IsModifierActive("Boss item modifier");

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
            var renderer = droneIcon?.GetComponent<MeshRenderer>();
            if (renderer != null)
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

            droneIcon?.SetActive(active);
        }
        public void SetRewardIconActive(bool active, bool isLegendary = false)
        {
            var renderer = droneIcon?.GetComponent<MeshRenderer>();
            if (renderer != null)
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
            rewardIcon?.SetActive(active);
        }
        public void SetAurelioniteIconActive(bool active)
        {
            var renderer = droneIcon?.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.material.SetColor("_TintColor", aurelionite);
            }
            aurelioniteIcon?.SetActive(active);
        }

    }
}
