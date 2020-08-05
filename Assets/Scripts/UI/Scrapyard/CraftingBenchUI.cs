using Sirenix.OdinInspector;
using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using StarSalvager.ScriptableObjects;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Values;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace StarSalvager.UI.Scrapyard
{
    public class CraftingBenchUI : MonoBehaviour
    {
        [SerializeField, Required]
        private RemotePartProfileScriptableObject _remotePartProfileScriptable;

        [SerializeField]
        private Button craftButton;

        [SerializeField]
        private TMP_Text itemNameText;
        [SerializeField]
        private Image resultImage;

        private TEST_Blueprint selectedBlueprint;


        [FormerlySerializedAs("blueprints")] [SerializeField]
        private BlueprintUIElementScrollView blueprintsContentScrollView;
        [SerializeField]
        private ResourceUIElementScrollView costContentView;
        [SerializeField]
        private ResourceUIElementScrollView resourceScrollView;

        //============================================================================================================//

        [SerializeField, Required]
        private CraftingBench mCraftingBench;

        //============================================================================================================//

        private bool scrollViewsSetup = false;

        // Start is called before the first frame update
        private void Start()
        {
            InitButtons();

            InitUIScrollView();
            InitResourceScrollViews();
            scrollViewsSetup = true;
        }

        private void InitButtons()
        {
            craftButton.onClick.AddListener(() =>
            {
                if (selectedBlueprint == null)
                    return;

                mCraftingBench.CraftBlueprint(selectedBlueprint);
                UpdateResources();
            });

        }

        void OnEnable()
        {
            if (scrollViewsSetup)
                RefreshScrollViews();

            blueprintsContentScrollView.ClearElements<BlueprintUIElement>();
            InitUIScrollView();
        }

        //============================================================================================================//

        private void InitUIScrollView()
        {
            //FIXME This needs to move to the Factory
            if (PlayerPersistentData.PlayerData.unlockedBlueprints.Count == 0)
            {
                foreach (var partRemoteData in _remotePartProfileScriptable.partRemoteData)
                {
                    for (int i = 0; i < partRemoteData.levels.Count - 1; i++)
                    {
                        if (partRemoteData.partType == PART_TYPE.CORE && i == 0)
                            continue;

                        TEST_Blueprint blueprint = new TEST_Blueprint
                        {
                            name = partRemoteData.partType + " " + i,
                            remoteData = partRemoteData,
                            level = i
                        };
                        PlayerPersistentData.PlayerData.unlockedBlueprints.Add(blueprint);
                    }
                }
            }

            foreach (var blueprint in PlayerPersistentData.PlayerData.unlockedBlueprints)
            {
                var temp = blueprintsContentScrollView.AddElement<BlueprintUIElement>(blueprint, $"{blueprint.name}_UIElement");
                temp.Init(blueprint, BlueprintPressed);
            }
        }

        public void InitResourceScrollViews()
        {
            var resources = PlayerPersistentData.PlayerData.GetResources();

            foreach (var resource in resources)
            {
                var data = new CraftCost
                {
                    resourceType = CraftCost.TYPE.Bit,
                    type = (int)resource.Key,
                    amount = resource.Value
                };

                var element = resourceScrollView.AddElement<ResourceUIElement>(data, $"{resource.Key}_UIElement");
                element.Init(data);
            }

            var components = PlayerPersistentData.PlayerData.GetComponents();

            foreach (var component in components)
            {
                var data = new CraftCost
                {
                    resourceType = CraftCost.TYPE.Component,
                    type = (int)component.Key,
                    amount = component.Value
                };

                var element = resourceScrollView.AddElement<ResourceUIElement>(data, $"{component.Key}_UIElement");
                element.Init(data);
            }
        }

        public void RefreshScrollViews()
        {
            blueprintsContentScrollView.ClearElements<BlueprintUIElement>();
            InitUIScrollView();
            UpdateResources();
        }

        public void UpdateResources()
        {
            var resources = PlayerPersistentData.PlayerData.GetResources();

            foreach (var resource in resources)
            {
                var data = new CraftCost
                {
                    resourceType = CraftCost.TYPE.Bit,
                    type = (int)resource.Key,
                    amount = resource.Value
                };

                var element = resourceScrollView.FindElement<ResourceUIElement>(data);

                if (element == null)
                    continue;

                element.Init(data);
            }

            var components = PlayerPersistentData.PlayerData.GetComponents();

            foreach (var component in components)
            {
                var data = new CraftCost
                {
                    resourceType = CraftCost.TYPE.Component,
                    type = (int)component.Key,
                    amount = component.Value
                };

                var element = resourceScrollView.FindElement<ResourceUIElement>(data);

                if (element == null)
                    continue;

                element.Init(data);
            }
        }

        private void SetupBlueprintCosts(TEST_Blueprint blueprint)
        {
            var resources = blueprint.remoteData.levels[blueprint.level].cost;

            costContentView.ClearElements<ResourceUIElement>();
            foreach (var resource in resources)
            {
                var element = costContentView.AddElement<ResourceUIElement>(resource, $"{resource.type}_UIElement");
                element.Init(resource);
            }
        }

        private void BlueprintPressed(TEST_Blueprint blueprint)
        {
            itemNameText.text = blueprint.name;
            PartProfile partProfile = FactoryManager.Instance.GetFactory<PartAttachableFactory>().GetProfileData(blueprint.remoteData.partType);
            resultImage.sprite = partProfile.Sprites[blueprint.level];
            SetupBlueprintCosts(blueprint);
            selectedBlueprint = blueprint;
        }

        //============================================================================================================//
        }
    
    [System.Serializable]
    public class BlueprintUIElementScrollView: UIElementContentScrollView<TEST_Blueprint>
    {}
}

