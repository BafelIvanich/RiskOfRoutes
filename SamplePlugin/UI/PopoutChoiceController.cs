using RoR2.UI;
using UnityEngine;
using UnityEngine.UI;

namespace RiskOfRoutes.UI
{
    public class PopoutChoiceController() : MonoBehaviour
    {
        public HGButton hgButton;
        public Image image;
        public TooltipProvider tooltipProvider;

        public GameObject[] checkboxStates;
        private CheckboxState _currentIndex;
        public CheckboxState currentIndex
        {
            get { return _currentIndex; }
            set
            {
                UpdateState(value);
            }
        }

        public GameObject[] usedStates;

        public PopoutCycleOption cycleOption;
        public ModifierTier tier;
        public bool isNegative;
        public string defName;

        public void UpdateState(CheckboxState newState)
        {
            if (_currentIndex != CheckboxState.DefaultDisabled)
            {
                var oldObj = checkboxStates[(int)_currentIndex];
                if (oldObj != null) { oldObj.SetActive(false); }

            }

            _currentIndex = newState;

            if (newState != CheckboxState.DefaultDisabled)
            {
                var newObj = checkboxStates[(int)_currentIndex];
                newObj.SetActive(true);
            }

            if (image != null)
                image.color = _currentIndex == CheckboxState.DefaultDisabled ? new Color(0.3f, 0.3f, 0.3f) : Color.white;

            switch (newState)
            {
                case CheckboxState.Positive_Tier1: { tier = ModifierTier.Tier1; isNegative = false; break; }
                case CheckboxState.Positive_Tier2: { tier = ModifierTier.Tier2; isNegative = false; break; }
                case CheckboxState.Positive_Tier3: { tier = ModifierTier.Tier3; isNegative = false; break; }
                case CheckboxState.Negative_Tier1: { tier = ModifierTier.Tier1; isNegative = true; break; }
                case CheckboxState.Negative_Tier2: { tier = ModifierTier.Tier2; isNegative = true; break; }
                case CheckboxState.Negative_Tier3: { tier = ModifierTier.Tier3; isNegative = true; break; }
                default: { break; }
            }
        }

        public static CheckboxState GetState(int tier, bool isNegative)
        {
            if (isNegative)
            {
                if (tier == 1) return CheckboxState.Negative_Tier1;
                if (tier == 2) return CheckboxState.Negative_Tier2;
                if (tier == 3) return CheckboxState.Negative_Tier3;
            }
            else
            {
                if (tier == 1) return CheckboxState.Positive_Tier1;
                if (tier == 2) return CheckboxState.Positive_Tier2;
                if (tier == 3) return CheckboxState.Positive_Tier3;
            }

            return CheckboxState.DefaultEnabled;
        }

        public CheckboxState GetEnabledState()
        {
            return GetState((int)tier, isNegative);
        }
    }

}
