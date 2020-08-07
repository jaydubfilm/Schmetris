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

        [FormerlySerializedAs("blueprints")] [SerializeField]
        private BlueprintUIElementScrollView blueprintsContentScrollView;
        [SerializeField]
        private ResourceUIElementScrollView costContentView;
        [SerializeField]
        private ResourceUIElementScrollView resourceScrollView;

        [SerializeField, Required]
        private CraftingBench mCraftingBench;

        private TEST_Blueprint currentSelected;

        private bool scrollViewsSetup = false;

        //============================================================================================================//

        #region Unity Functions

        private void Start()
        {
            InitButtons();

            InitUIScrollView();
            InitResourceScrollViews();
            scrollViewsSetup = true;
        }

        void OnEnable()
        {
            if (scrollViewsSetup)
                RefreshScrollViews();

            blueprintsContentScrollView.ClearElements<BlueprintUIElement>();
            InitUIScrollView();
        }

        #endregion //Unity Functions

        //============================================================================================================//

        #region Init

        private void InitButtons()
        {
            craftButton.onClick.AddListener(() =>
            {
                if (currentSelected == null)
                    return;

                mCraftingBench.CraftBlueprint(currentSelected);
                UpdateResources();
            });
        }

        #endregion //Init

        //============================================================================================================//

        #region Scroll Views

        private void InitUIScrollView()
        {
            //FIXME This needs to move to the Factory
            if (PlayerPersistentData.PlayerData.unlockedBlueprints.Count == 0)
            {
                foreach (var partRemoteData in _remotePartProfileScriptable.partRemoteData)
                {
                    for (int i = 0; i < partRemoteData.levels.Count; i++)
                    {
                        if (partRemoteData.partType == PART_TYPE.CORE)
                            continue;

                        TEST_Blueprint blueprint = new TEST_Blueprint
                        {
                            name = partRemoteData.partType + " " + i,
                            partType = partRemoteData.partType,
                            level = i
                        };
                        PlayerPersistentData.PlayerData.UnlockBlueprint(blueprint);
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

        #endregion //Scroll Views

        //============================================================================================================//

        #region Other

        private void SetupBlueprintCosts(TEST_Blueprint blueprint)
        {
            var resources = FactoryManager.Instance.GetFactory<PartAttachableFactory>().GetRemoteData(blueprint.partType).levels[blueprint.level].cost;

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
            PartProfile partProfile = FactoryManager.Instance.GetFactory<PartAttachableFactory>().GetProfileData(blueprint.partType);
            resultImage.sprite = partProfile.Sprites[blueprint.level];
            SetupBlueprintCosts(blueprint);
            currentSelected = blueprint;
        }

        #endregion //Other

        //============================================================================================================//
    }

    [System.Serializable]
    public class BlueprintUIElementScrollView: UIElementContentScrollView<TEST_Blueprint>
    {}
}

