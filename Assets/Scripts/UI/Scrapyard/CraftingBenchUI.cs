using System;
using Sirenix.OdinInspector;
using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using StarSalvager.ScriptableObjects;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Values;
using System.Collections.Generic;
using StarSalvager.Prototype;
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

        [SerializeField, Required, FoldoutGroup("Cost Window")]
        private GameObject costWindowObject;

        [SerializeField, Required, FoldoutGroup("Cost Window")]
        private ResourceUIElementScrollView costView;

        [SerializeField, Required, FoldoutGroup("Cost Window")]
        private TMP_Text itemNameText;

        [SerializeField, Required, FoldoutGroup("Cost Window")]
        private Image itemIcon;

        /*[SerializeField]
        private Button craftButton;

        [SerializeField]
        private TMP_Text itemNameText;
        [SerializeField]
        private Image resultImage;*/

        [FormerlySerializedAs("blueprints")] [SerializeField]
        private BlueprintUIElementScrollView blueprintsContentScrollView;
        /*[SerializeField]
        private ResourceUIElementScrollView costContentView;
        [SerializeField]
        private ResourceUIElementScrollView resourceScrollView;*/

        [SerializeField, Required]
        private CraftingBench mCraftingBench;

        //[SerializeField, Required]
        private StorageUI storageUi;

        private TEST_Blueprint currentSelected;

        private bool scrollViewsSetup = false;

        //============================================================================================================//

        #region Unity Functions

        private void Start()
        {
            storageUi = FindObjectOfType<StorageUI>();

            costWindowObject.SetActive(false);

            InitButtons();

            InitUIScrollView();
            InitResourceScrollViews();
            scrollViewsSetup = true;
        }

        private void OnEnable()
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
            /*craftButton.onClick.AddListener(() =>
            {
                if (currentSelected == null)
                    return;

                mCraftingBench.CraftBlueprint(currentSelected);
                UpdateResources();
            });*/
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
                        //TODO Add these back in when we're ready!
                        switch (partRemoteData.partType)
                        {
                            //Still want to be able to upgrade the core, just don't want to buy new ones?
                            case PART_TYPE.CORE /*when i == 0*/:
                            case PART_TYPE.BOOST:
                                continue;
                        }

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
                temp.Init(blueprint, data =>
                {
                    Debug.Log("Craft button pressed");
                    mCraftingBench.CraftBlueprint(data);
                    storageUi.UpdateStorage();

                }, SetupBlueprintCosts);
            }
        }

        public void InitResourceScrollViews()
        {
            /*var resources = PlayerPersistentData.PlayerData.GetResources();

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
            }*/
        }

        public void RefreshScrollViews()
        {
            blueprintsContentScrollView.ClearElements<BlueprintUIElement>();
            InitUIScrollView();
            UpdateResources();
        }

        public void UpdateResources()
        {
            /*var resources = PlayerPersistentData.PlayerData.GetResources();

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
            }*/
        }

        #endregion //Scroll Views

        //============================================================================================================//

        #region Other

        private TEST_Blueprint lastBlueprint;

        private void SetupBlueprintCosts(TEST_Blueprint blueprint, bool showWindow, RectTransform buttonTransform)
        {
            costWindowObject.SetActive(showWindow);

            

            if (!showWindow)
            {
                PlayerData.OnValuesChanged -= UpdateCostUI;
                lastBlueprint = null;
                return;
            }

            PlayerData.OnValuesChanged += UpdateCostUI;

            lastBlueprint = blueprint;

            var windowTransform = costWindowObject.transform as RectTransform;

            windowTransform.position = buttonTransform.position/* +
                                       Vector3.left *
                                       (buttonTransform.sizeDelta.x / 2f + windowTransform.sizeDelta.x / 2f)*/;
            
            windowTransform.localPosition += Vector3.left * (buttonTransform.sizeDelta.x / 2f + windowTransform.sizeDelta.x / 2f);
            

            UpdateCostUI();
        }

        private void UpdateCostUI()
        {
            costView.ClearElements<CostUIElement>();
            
            var partProfileData = FactoryManager.Instance.GetFactory<PartAttachableFactory>()
                .GetProfileData(lastBlueprint.partType);
            
            itemIcon.sprite = partProfileData.Sprites[lastBlueprint.level];


            var partRemoteData = FactoryManager.Instance.GetFactory<PartAttachableFactory>()
                .GetRemoteData(lastBlueprint.partType);
            
            itemNameText.text = partRemoteData.name;

            var resources = partRemoteData.levels[lastBlueprint.level].cost;

            foreach (var resource in resources)
            {
                var element = costView.AddElement<CostUIElement>(resource, $"{resource.type}_UIElement");
                element.Init(resource);
            }
        }

        /*private void BlueprintPressed(TEST_Blueprint blueprint)
        {
            /*itemNameText.text = blueprint.name;
            PartProfile partProfile = FactoryManager.Instance.GetFactory<PartAttachableFactory>().GetProfileData(blueprint.remoteData.partType);
            resultImage.sprite = partProfile.Sprites[blueprint.level];#1#
            //SetupBlueprintCosts(blueprint);
            //currentSelected = blueprint;
        }*/

        #endregion //Other

        //============================================================================================================//
    }

    [System.Serializable]
    public class BlueprintUIElementScrollView: UIElementContentScrollView<TEST_Blueprint>
    {}
}
