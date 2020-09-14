using System;
using System.Collections.Generic;
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

        //====================================================================================================================//

        private void Start()
        {
            InitScrollViews();
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
                facilityItemUIElements.AddElement(facilityItem);
            }
            
            foreach (var facilityBlueprint in TEST_facilityBlueprints)
            {
                facilityBlueprintUIElements.AddElement(facilityBlueprint);
            }

            SetupResourceScrollView();
            
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

        //====================================================================================================================//
        
        private void SetupDetailsWindow(bool active, TEST_FacilityItem item)
        {
            detailsWindow.SetActive(active);
            detailsCostSection.SetActive(false);

            if (!active) return;
            
            detailsTitle.text = item.name;
            detailsDescription.text = item.description;
        }
        
        private void SetupDetailsWindow(bool active, TEST_FacilityBlueprint item)
        {
            detailsWindow.SetActive(active);
            detailsCostSection.SetActive(active);
            
            if (!active)
                return;
            
            detailsTitle.text = item.name;
            detailsDescription.text = item.description;
            DisplayCost(item.cost);
        }

        private void DisplayCost(IEnumerable<CraftCost> costs)
        {
            foreach (var cost in costs)
            {
                costUIElementScrollView.AddElement(cost);
            }
        }

        //====================================================================================================================//

    }

    [System.Serializable]
    public class FacilityItemUIElementScrollView : UIElementContentScrollView<FacilityItemUIElement, TEST_FacilityItem>
    {
    }

    [System.Serializable]
    public class FacilityBlueprintUIElementScrollView : UIElementContentScrollView<FacilityBlueprintUIElement,
            TEST_FacilityBlueprint>
    {
    }
}