using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using StarSalvager.Utilities.Saving;
using StarSalvager.Values;
using TMPro;
using UnityEngine;

namespace StarSalvager.UI.Scrapyard
{
    public class LogisticsScreenUI : MonoBehaviour
    {
        [SerializeField] private FacilityItemUIElementScrollView facilityItemUIElements;

        [SerializeField] private FacilityBlueprintUIElementScrollView facilityBlueprintUIElements;

        [SerializeField]
        private CostUIElementScrollView costUIElementScrollView;

        [SerializeField, Required, BoxGroup("Details Window")]
        private GameObject detailsWindow;
        [SerializeField, Required, BoxGroup("Details Window")]
        private GameObject detailsCostSection;
        [SerializeField, Required, BoxGroup("Details Window")]
        private TMP_Text detailsTitle;
        [SerializeField, Required, BoxGroup("Details Window")]
        private TMP_Text detailsDescription;

        [SerializeField]
        private TMP_Text patchPointsText;
        [SerializeField]
        private TMP_Text patchPointProgressText;

        public static Action CheckFacilityBlueprintNewAlertUpdate;

        /*//TODO Need to add Resources
        [SerializeField]
        private ResourceUIElementScrollView resourceUIElementScrollView;
        
        //TODO Need to add Components
        [SerializeField]
        private ComponentResourceUIElementScrollView componentResourceUIElementScrollView;*/

        //====================================================================================================================//

        private void Start()
        {            
            SetupDetailsWindow((TEST_FacilityItem) null, false);
        }

        //Unity Functions
        //==============================================================================================================//

        private void OnEnable()
        {
            PlayerDataManager.OnValuesChanged += SetupScrollViews;

            SetupScrollViews();
        }

        private void OnDisable()
        {
            PlayerDataManager.OnValuesChanged -= SetupScrollViews;
        }

        //====================================================================================================================//

        private void SetupScrollViews()
        {
            //TODO Still need to setup the OnHover
            facilityItemUIElements.ClearElements();
            foreach (var facilityRemoteData in FactoryManager.Instance.FacilityRemote.GetRemoteDatas())
            {
                if (facilityRemoteData.hideInFacilityMenu)
                {
                    continue;
                }
                
                FACILITY_TYPE type = facilityRemoteData.type;

                if (!PlayerDataManager.GetFacilityRanks().ContainsKey(type))
                {
                    continue;
                }

                int level = PlayerDataManager.GetFacilityRanks()[type];

                string description = facilityRemoteData.displayDescription;
                description = description.Replace("*", facilityRemoteData.levels[level].increaseAmount.ToString());

                TEST_FacilityItem newItem = new TEST_FacilityItem
                {
                    name = facilityRemoteData.displayName + " " + (facilityRemoteData.levels[level].level + 1),
                    description = description
                };

                var element = facilityItemUIElements.AddElement(newItem, $"{newItem.name}_UIElement");
                element.Init(newItem, SetupDetailsWindow);
            }


            facilityBlueprintUIElements.ClearElements();
            foreach (var facilityRemoteData in FactoryManager.Instance.FacilityRemote.GetRemoteDatas())
            {
                if (facilityRemoteData.hideInFacilityMenu)
                {
                    continue;
                }

                FACILITY_TYPE type = facilityRemoteData.type;
                bool containsFacilityKey = PlayerDataManager.GetFacilityRanks().ContainsKey(type);
                bool containsFacilityBlueprintKey = PlayerDataManager.GetFacilityBlueprintRanks().ContainsKey(type);

                if (!containsFacilityBlueprintKey)
                {
                    continue;
                }

                for (int i = 0; i <= PlayerDataManager.GetFacilityBlueprintRanks()[type]; i++)
                //for (int i = 0; i < facilityRemoteData.levels.Count; i++)
                {
                    if (containsFacilityKey && PlayerDataManager.GetFacilityRanks()[type] >= i)
                    {
                        continue;
                    }

                    if (!containsFacilityKey && i > 0)
                    {
                        continue;
                    }

                    string description = facilityRemoteData.displayDescription;
                    description = description.Replace("*", facilityRemoteData.levels[i].increaseAmount.ToString());

                    FacilityBlueprint newBlueprint = new FacilityBlueprint
                    {
                        name = facilityRemoteData.displayName + " " + (facilityRemoteData.levels[i].level + 1),
                        description = description,
                        facilityType = type,
                        level = i,
                        patchCost = facilityRemoteData.levels[i].patchCost
                    };

                    bool hasPrereqs = true;
                    for (int k = 0; k < facilityRemoteData.levels[i].facilityPrerequisites.Count; k++)
                    {
                        if (PlayerDataManager.GetFacilityRanks().ContainsKey(facilityRemoteData.levels[i].facilityPrerequisites[k].facilityType) &&
                            PlayerDataManager.GetFacilityRanks()[facilityRemoteData.levels[i].facilityPrerequisites[k].facilityType] >= facilityRemoteData.levels[i].facilityPrerequisites[k].level)
                        {
                            continue;
                        }

                        hasPrereqs = false;
                        break;
                    }

                    bool craftButtonInteractable = hasPrereqs && 
                        ((containsFacilityKey && i == PlayerDataManager.GetFacilityRanks()[type] + 1) ||
                        (!containsFacilityKey && i == 0));

                    if (!craftButtonInteractable)
                    {
                        continue;
                    }

                    var element = facilityBlueprintUIElements.AddElement(newBlueprint);
                    element.Init(newBlueprint, PurchaseBlueprint, SetupDetailsWindow, craftButtonInteractable);
                }
            }

            /*SetupResourceScrollView();
            SetupComponentResourceScrollView();*/

            UpdatePatchPoints();
        }

        /*private void SetupResourceScrollView()
        {
            foreach (BIT_TYPE _bitType in Enum.GetValues(typeof(BIT_TYPE)))
            {
                if (_bitType == BIT_TYPE.WHITE)
                    continue;

                PlayerResource playerResource = PlayerDataManager.GetResource(_bitType);

                var data = new ResourceAmount
                {
                    type = _bitType,
                    amount = playerResource.resource,
                    capacity = playerResource.resourceCapacity
                };

                var element = resourceUIElementScrollView.AddElement(data, $"{_bitType}_UIElement");
                element.Init(data);
            }
        }
        
        private void SetupComponentResourceScrollView()
        {
            var resources = PlayerDataManager.GetComponents();

            foreach (var resource in resources)
            {
                var data = new ComponentAmount
                {
                    type = resource.Key,
                    amount = resource.Value,
                };

                var element = componentResourceUIElementScrollView.AddElement(data, $"{resource.Key}_UIElement");
                element.Init(data);
            }
        }*/

        private void UpdatePatchPoints()
        {
            var (current, required) = PlayerDataManager.GetPatchPointProgress();
            patchPointsText.text = $"{PlayerDataManager.GetAvailablePatchPoints()}";
            patchPointProgressText.text = $"Next Patch Point: {current}/{required}";
        }

        //====================================================================================================================//
        
        private void PurchaseBlueprint([CanBeNull] FacilityBlueprint item)
        {
            if (!PlayerDataManager.CanAffordFacilityBlueprint(item))
            {
                Debug.LogError($"Cannot afford {item.name}");
                return;
            }

            PlayerDataManager.SpendPatchPoints(item.patchCost);
            PlayerDataManager.UnlockFacilityLevel(item.facilityType, item.level);

            foreach (var facilityRemoteData in FactoryManager.Instance.FacilityRemote.GetRemoteDatas())
            {
                if (facilityRemoteData.hideInFacilityMenu)
                {
                    continue;
                }

                FACILITY_TYPE type = facilityRemoteData.type;
                bool containsFacilityKey = PlayerDataManager.GetFacilityRanks().ContainsKey(type);
                bool containsFacilityBlueprintKey = PlayerDataManager.GetFacilityBlueprintRanks().ContainsKey(type);

                if (!containsFacilityBlueprintKey)
                {
                    continue;
                }

                for (int i = 0; i <= PlayerDataManager.GetFacilityBlueprintRanks()[type]; i++)
                {
                    if (containsFacilityKey && PlayerDataManager.GetFacilityRanks()[type] >= i)
                    {
                        continue;
                    }

                    if (!containsFacilityKey && i > 0)
                    {
                        continue;
                    }

                    string description = facilityRemoteData.displayDescription;
                    description = description.Replace("*", facilityRemoteData.levels[i].increaseAmount.ToString());

                    if (!facilityRemoteData.levels[i].facilityPrerequisites.Any(f => f.facilityType == item.facilityType && f.level == item.level))
                    {
                        continue;
                    }

                    bool hasPrereqs = true;
                    for (int k = 0; k < facilityRemoteData.levels[i].facilityPrerequisites.Count; k++)
                    {
                        if (PlayerDataManager.GetFacilityRanks().ContainsKey(facilityRemoteData.levels[i].facilityPrerequisites[k].facilityType) &&
                            PlayerDataManager.GetFacilityRanks()[facilityRemoteData.levels[i].facilityPrerequisites[k].facilityType] >= facilityRemoteData.levels[i].facilityPrerequisites[k].level)
                        {
                            continue;
                        }

                        hasPrereqs = false;
                        break;
                    }

                    if (!hasPrereqs)
                    {
                        continue;
                    }

                    FacilityBlueprint newBlueprint = new FacilityBlueprint
                    {
                        name = facilityRemoteData.displayName + " " + (facilityRemoteData.levels[i].level + 1),
                        description = description,
                        facilityType = type,
                        level = i,
                        patchCost = facilityRemoteData.levels[i].patchCost
                    };

                    PlayerDataManager.AddNewFacilityBlueprintAlert(newBlueprint);
                }
            }

            CheckFacilityBlueprintNewAlertUpdate?.Invoke();
        }

        private void SetupDetailsWindow([CanBeNull] TEST_FacilityItem item, bool active)
        {
            detailsWindow.SetActive(active);
            detailsCostSection.SetActive(false);

            if (!active) return;
            
            detailsTitle.text = item?.name;
            detailsDescription.text = item?.description;
        }
        
        private void SetupDetailsWindow([CanBeNull] FacilityBlueprint item, bool active)
        {
            detailsWindow.SetActive(active);
            detailsCostSection.SetActive(active);
            
            if (!active)
                return;
            
            detailsTitle.text = item?.name;
            detailsDescription.text = item?.description;

            string prereqText = "\nPrerequisite for ";
            int numAdded = 0;
            for (int i = 0; i < FactoryManager.Instance.FacilityRemote.FacilityRemoteData.Count; i++)
            {
                FacilityRemoteData facilityRemoteData = FactoryManager.Instance.FacilityRemote.FacilityRemoteData[i];
                for (int k = 0; k < facilityRemoteData.levels.Count; k++)
                {
                    if (facilityRemoteData.levels[k].facilityPrerequisites.Any(f => f.facilityType == item.facilityType && f.level == item.level))
                    {
                        if (numAdded > 0)
                        {
                            prereqText += ", ";
                        }

                        prereqText += $"{facilityRemoteData.displayName} {k + 1}";
                        numAdded++;
                    }
                }
            }
            prereqText += ".";

            if (numAdded > 0)
            {
                detailsDescription.text += prereqText;
            }

            DisplayCost(item.patchCost);
        }

        private void DisplayCost(IEnumerable<CraftCost> costs)
        {
            costUIElementScrollView.ClearElements();
            
            foreach (var cost in costs)
            {
                var element = costUIElementScrollView.AddElement(cost);
                element.Init(cost);
            }
        }

        private void DisplayCost(int patchCost)
        {
            costUIElementScrollView.ClearElements();

            CraftCost patchCraftCost = new CraftCost();
            patchCraftCost.resourceType = CraftCost.TYPE.PatchPoint;
            patchCraftCost.amount = patchCost;


            var element = costUIElementScrollView.AddElement(patchCraftCost);
            element.Init(patchCraftCost);
        }

        //====================================================================================================================//

    }

    [Serializable]
    public class FacilityItemUIElementScrollView : UIElementContentScrollView<FacilityItemUIElement, TEST_FacilityItem>
    {
    }

    [Serializable]
    public class FacilityBlueprintUIElementScrollView : UIElementContentScrollView<FacilityBlueprintUIElement,
            FacilityBlueprint>
    {
    }
}