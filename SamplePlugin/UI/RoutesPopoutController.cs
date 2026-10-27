using BepInEx.Configuration;
using RoR2.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RiskOfRoutes.UI
{
    public enum CheckboxState
    {
        Positive_Tier1,
        Positive_Tier2,
        Positive_Tier3,
        Negative_Tier1,
        Negative_Tier2,
        Negative_Tier3,
        DefaultEnabled,
        DefaultDisabled
    }

    public enum PopoutCycleOption
    {
        Default, All, Positive, Negative
    }

    public class RoutesPopoutController() : MonoBehaviour
    {
        public GameObject defaultButton;
        public GameObject allButton;
        public GameObject noneButton;
        public RectTransform container;
        public PopoutCycleOption cycleOption;

        public LanguageTextMeshController title;
        public LanguageTextMeshController subtitle;

        public HashSet<DefInfo> defList;
        public HashSet<string> banList;
        public HashSet<string> defaultBanList;
        public HashSet<DefInfo> moddedList;
        public ConfigEntry<string> moddedConfig;
        public ConfigEntry<string> configString;

        public bool checkboxShowInfo;

        public void Start()
        {
            var hgDefault = defaultButton.GetComponent<HGButton>();
            hgDefault.onClick.AddListener(() =>
            {
                Log.Info("CLicked on default button");
                if (cycleOption == PopoutCycleOption.Default)
                {
                    banList.Clear();
                    if (defaultBanList != null)
                    {
                        foreach (var def in defList)
                        {
                            if (defaultBanList.Contains(def.name))
                                banList.Add(def.name);
                        }
                        configString.Value = string.Join(", ", banList);
                    }
                }
                else
                {
                    //конфигуринг
                    moddedList.Clear();
                    moddedConfig.Value = UpdateModded();
                }
                UpdateContainer();
            });

            var hgAll = allButton.GetComponent<HGButton>();
            hgAll.onClick.AddListener(() =>
            {
                Log.Info("CLicked on all button");
                if (cycleOption == PopoutCycleOption.Default)
                {
                    foreach (var def in defList)
                    {
                        banList.Remove(def.name);
                    }
                    configString.Value = string.Join(", ", banList);
                }
                else
                {
                    moddedList.Clear();

                    foreach (var def in defList)
                    {
                        moddedList.Add(new DefInfo { name = def.name, tier = ModifierTier.Tier1, isNegative = (cycleOption == PopoutCycleOption.Negative) });
                        banList.Remove(def.name);
                    }
                    moddedConfig.Value = UpdateModded();
                }
                UpdateContainer();
            });

            var hgNone = noneButton.GetComponent<HGButton>();
            hgNone.onClick.AddListener(() =>
            {
                Log.Info("Clicked on none button");
                if (cycleOption == PopoutCycleOption.Default)
                {
                    foreach (var def in defList)
                    {
                        banList.Remove(def.name);
                    }
                    foreach (var def in defList) banList.Add(def.name);
                    configString.Value = string.Join(", ", banList);

                }
                else
                {
                    moddedList.Clear();
                    moddedConfig.Value = UpdateModded();
                }
                UpdateContainer();
            });
        }

        public void UpdateContainer()
        {
            Log.Info("Updating container");
            foreach (Transform choiceTransform in container.transform)
            {
                var rc = choiceTransform.gameObject.GetComponent<PopoutChoiceController>();
                if (rc == null) continue;

                if (cycleOption == PopoutCycleOption.Default)
                {
                    if (checkboxShowInfo)
                        rc.currentIndex = banList.Contains(rc.defName) ? CheckboxState.DefaultDisabled : rc.GetEnabledState();
                    else
                        rc.currentIndex = banList.Contains(rc.defName) ? CheckboxState.DefaultDisabled : CheckboxState.DefaultEnabled;
                }
                else
                {
                    var configState = moddedList.FirstOrDefault(info => info.name == rc.defName);
                    if (configState != null)
                    {
                        rc.currentIndex = PopoutChoiceController.GetState((int)configState.tier, configState.isNegative);
                    }
                    else
                    {
                        rc.currentIndex = CheckboxState.DefaultDisabled;
                    }
                }
            }
        }

        public string UpdateModded()
        {
            List<string> moddedDefs = new List<string>();
            foreach (var info in moddedList)
            {
                string sign = info.isNegative ? "-" : "+";
                int tier = (int)info.tier;
                moddedDefs.Add($"{info.name}={sign}={tier}");
            }
            return string.Join(",", moddedDefs);
        }

        public void Populate()
        {
            var usedDefs = defList;

            foreach (var def in usedDefs)
            {
                if (def == null) continue;

                var choice = Object.Instantiate(AssetManager.CreatePopoutChoicePrefab(), container, false); //todo заменить
                choice.SetActive(true);

                //if (choice == null) Log.Info("NO CHOICE");
                //choice.transform.SetParent(container, true);

                var rc = choice.GetComponent<PopoutChoiceController>();

                if (rc == null || rc.image == null) return;

                rc.defName = def.name;

                rc.tier = def.tier;
                rc.isNegative = def.isNegative;


                CheckboxState enabled = checkboxShowInfo ? rc.GetEnabledState() : CheckboxState.DefaultEnabled;

                GameObject checkbox = null;
                checkbox = rc.checkboxStates[0];

                var checkboxImage = checkbox.transform.Find("Checkbox/CheckboxImage");


                var defaultAll = new CheckboxState[] { CheckboxState.DefaultDisabled, enabled };

                var arrayAll = new CheckboxState[] { CheckboxState.DefaultDisabled, CheckboxState.Positive_Tier1, CheckboxState.Positive_Tier2, CheckboxState.Positive_Tier3, CheckboxState.Negative_Tier1, CheckboxState.Negative_Tier2, CheckboxState.Negative_Tier3 };
                var negativeAll = new CheckboxState[] { CheckboxState.DefaultDisabled, CheckboxState.Negative_Tier1, CheckboxState.Negative_Tier2, CheckboxState.Negative_Tier3 };
                var positiveAll = new CheckboxState[] { CheckboxState.DefaultDisabled, CheckboxState.Positive_Tier1, CheckboxState.Positive_Tier2, CheckboxState.Positive_Tier3 };


                if (def.sprite != null)
                    rc.image.sprite = def.sprite;
                else
                    rc.image.sprite = AssetManager.placeholderIcon;


                if (rc.tooltipProvider != null)
                {
                    rc.tooltipProvider.titleToken = def.nameToken;
                    rc.tooltipProvider.bodyToken = def.descriptionToken;
                    rc.tooltipProvider.titleColor = def.tooltipColor;
                }

                if (rc.hgButton != null)
                {
                    rc.hgButton.onClick.RemoveAllListeners();
                    //rc.hgButton.onSelect.RemoveAllListeners(); в консоли исключение на OnSelect()

                    CheckboxState[] currentArray = new CheckboxState[] { };
                    if (cycleOption == PopoutCycleOption.Default) currentArray = defaultAll;
                    else if (cycleOption == PopoutCycleOption.Negative) currentArray = negativeAll;
                    else if (cycleOption == PopoutCycleOption.Positive) currentArray = positiveAll;
                    else if (cycleOption == PopoutCycleOption.All) currentArray = arrayAll;
                    else currentArray = defaultAll;

                    if (cycleOption == PopoutCycleOption.Default)
                    {
                        rc.currentIndex = banList.Contains(def.name) ? CheckboxState.DefaultDisabled : enabled;
                    }
                    else
                    {
                        var configState = moddedList.FirstOrDefault(info => info.name == def.name);
                        if (configState != null)
                        {
                            rc.currentIndex = PopoutChoiceController.GetState((int)configState.tier, configState.isNegative);
                        }
                        else
                        {
                            rc.currentIndex = CheckboxState.DefaultDisabled;
                        }
                    }

                    CheckboxState currentState = rc.currentIndex;
                    int foundIndex = System.Array.IndexOf(currentArray, currentState);

                    if (foundIndex == -1)
                    {
                        Log.Error($"option {currentState} is not present in array with values:");
                        foreach (var o in currentArray)
                        {
                            Log.Info($"\t{o}");
                        }
                        return;
                    }


                    rc.hgButton.onClick.AddListener(() =>
                    {
                        int length = currentArray.Length;
                        Log.Info($"Array length: {currentArray.Length}");
                        if (length == 0) return;

                        int currentIndex = System.Array.IndexOf(currentArray, rc.currentIndex);
                        if (currentIndex == -1)
                        {
                            Log.Error("fwck");
                            return;
                        }

                        var nextIndex = (currentIndex + 1) % length;
                        var nextState = currentArray[nextIndex];

                        rc.currentIndex = nextState;

                        if (cycleOption == PopoutCycleOption.Default)
                        {
                            //банлист
                            if (rc.currentIndex == CheckboxState.DefaultDisabled)
                                banList.Add(rc.defName);
                            else
                                banList.Remove(rc.defName);

                            configString.Value = string.Join(",", banList);
                            Log.Info($"Config got {configString.Value}");
                        }
                        else
                        {
                            //конфигуринг
                            moddedList.RemoveWhere(info => info.name == rc.defName);

                            if (rc.currentIndex != CheckboxState.DefaultDisabled)
                            {
                                moddedList.Add(new DefInfo { name = rc.defName, isNegative = rc.isNegative, tier = rc.tier });
                            }
                            moddedConfig.Value = UpdateModded();
                            Log.Info($"Config got {moddedConfig.Value}");
                        }
                    });
                }
            }
        }


    }
}
