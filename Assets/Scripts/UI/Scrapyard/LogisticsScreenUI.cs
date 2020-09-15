using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using StarSalvager.Factories.Data;
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
            InitScrollViews();
            
            SetupDetailsWindow(false, (TEST_FacilityItem) null);
        }

        //====================================================================================================================//

        private static readonly TEST_FacilityItem[] TEST_FacilityItems =
        {
            new TEST_FacilityItem {name = "Facility 1", description = "Does Thing"},
            new TEST_FacilityItem {name = "Facility 2", description = "Does Thing"},
            new TEST_FacilityItem {name = "Facility 3", description = "Does Thing"},
            new TEST_FacilityItem {name = "Facility 4", description = "Does Thing"},
        };

        private static readonly List<CraftCost> TEST_costs = new List<CraftCost>
        {
            new CraftCost
            {
                resourceType = CraftCost.TYPE.Bit,
                type = (int) BIT_TYPE.BLUE,
                amount = 100
            },
            new CraftCost
            {
                resourceType = CraftCost.TYPE.Bit,
                type = (int) BIT_TYPE.RED,
                amount = 100
            },
            new CraftCost
            {
                resourceType = CraftCost.TYPE.Component,
                type = (int) COMPONENT_TYPE.NUT,
                amount = 2
            }
        };

        private static readonly TEST_FacilityBlueprint[] TEST_facilityBlueprints =
        {
            new TEST_FacilityBlueprint {name = "Facility 1", description = "Does Other Things", cost = TEST_costs},
            new TEST_FacilityBlueprint {name = "Facility 2", description = "Does Other Things", cost = TEST_costs},
            new TEST_FacilityBlueprint {name = "Facility 3", description = "Does Other Things", cost = TEST_costs},
            new TEST_FacilityBlueprint {name = "Facility 4", description = "Does Other Things", cost = TEST_costs},
        };

        private void InitScrollViews()
        {
            //TODO Still need to setup the OnHover
            foreach (var facilityItem in TEST_FacilityItems)
            {
                var element = facilityItemUIElements.AddElement(facilityItem, $"{facilityItem.name}_UIElement");
                element.Init(facilityItem);
            }

            foreach (var facilityBlueprint in TEST_facilityBlueprints)
            {
                var element = facilityBlueprintUIElements.AddElement(facilityBlueprint);
                element.Init(facilityBlueprint);
            }

            SetupResourceScrollView();
            SetupComponentResourceScrollView();

        }

        private void SetupResourceScrollView()
        {
            var resources = PlayerPersistentData.PlayerData.resources;
            var capacities = PlayerPersistentData.PlayerData.resourceCapacities;

            foreach (var resource in resources)
            {
                var data = new ResourceAmount
                {
                    type = resource.Key,
                    amount = resource.Value,
                    capacity = capacities[resource.Key]
                };

                var element = resourceUIElementScrollView.AddElement(data, $"{resource.Key}_UIElement");
                element.Init(data);
            }
        }
        
        private void SetupComponentResourceScrollView()
        {
            var resources = PlayerPersistentData.PlayerData.components;

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
        
        private void SetupDetailsWindow(bool active, [CanBeNull] TEST_FacilityItem item)
        {
            detailsWindow.SetActive(active);
            detailsCostSection.SetActive(false);

            if (!active) return;
            
            detailsTitle.text = item?.name;
            detailsDescription.text = item?.description;
        }
        
        private void SetupDetailsWindow(bool active, [CanBeNull] TEST_FacilityBlueprint item)
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
                costUIElementScrollView.AddElement(cost);
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