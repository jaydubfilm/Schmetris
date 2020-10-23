using System;
using System.Collections.Generic;
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
        

        //TODO Need to add Resources
        [SerializeField]
        private ResourceUIElementScrollView resourceUIElementScrollView;
        
        //TODO Need to add Components
        [SerializeField]
        private ComponentResourceUIElementScrollView componentResourceUIElementScrollView;

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

                    string description = facilityRemoteData.displayDescription;
                    description = description.Replace("*", facilityRemoteData.levels[i].increaseAmount.ToString());

                    TEST_FacilityBlueprint newBlueprint = new TEST_FacilityBlueprint
                    {
                        name = facilityRemoteData.displayName + " " + (facilityRemoteData.levels[i].level + 1),
                        description = description,
                        facilityType = type,
                        level = i,
                        cost = facilityRemoteData.levels[i].craftCost
                    };

                    bool craftButtonInteractable =
                        (containsFacilityKey && i == PlayerDataManager.GetFacilityRanks()[type] + 1) ||
                        (!containsFacilityKey && i == 0);

                    var element = facilityBlueprintUIElements.AddElement(newBlueprint);
                    element.Init(newBlueprint, PurchaseBlueprint, SetupDetailsWindow, craftButtonInteractable);
                }
            }

            SetupResourceScrollView();
            SetupComponentResourceScrollView();
        }

        private void SetupResourceScrollView()
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
        }

        //====================================================================================================================//
        
        private void PurchaseBlueprint([CanBeNull] TEST_FacilityBlueprint item)
        {
            if (!PlayerDataManager.CanAffordFacilityBlueprint(item))
            {
                Debug.LogError($"Cannot afford {item.name}");
                return;
            }

            PlayerDataManager.SubtractCraftCostResources(item.cost);
            PlayerDataManager.SubtractComponents(item.cost);
            PlayerDataManager.UnlockFacilityLevel(item.facilityType, item.level);
        }

        private void SetupDetailsWindow([CanBeNull] TEST_FacilityItem item, bool active)
        {
            detailsWindow.SetActive(active);
            detailsCostSection.SetActive(false);

            if (!active) return;
            
            detailsTitle.text = item?.name;
            detailsDescription.text = item?.description;
        }
        
        private void SetupDetailsWindow([CanBeNull] TEST_FacilityBlueprint item, bool active)
        {
            detailsWindow.SetActive(active);
            detailsCostSection.SetActive(active);
            
            if (!active)
                return;
            
            detailsTitle.text = item?.name;
            detailsDescription.text = item?.description;
            DisplayCost(item?.cost);
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

        //====================================================================================================================//

    }

    [Serializable]
    public class FacilityItemUIElementScrollView : UIElementContentScrollView<FacilityItemUIElement, TEST_FacilityItem>
    {
    }

    [Serializable]
    public class FacilityBlueprintUIElementScrollView : UIElementContentScrollView<FacilityBlueprintUIElement,
            TEST_FacilityBlueprint>
    {
    }
}