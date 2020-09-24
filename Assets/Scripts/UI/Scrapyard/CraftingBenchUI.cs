﻿using System;
using System.Collections;
using Sirenix.OdinInspector;
using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using StarSalvager.ScriptableObjects;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Values;
using System.Collections.Generic;
using StarSalvager.Prototype;
using StarSalvager.Utilities.UI;
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

        private CanvasGroup costWindowCanvasGroup;
        private VerticalLayoutGroup costWindowVerticalLayoutGroup;

        [SerializeField, Required, FoldoutGroup("Cost Window")]
        private CostUIElementScrollView costView;

        [SerializeField, Required, FoldoutGroup("Cost Window")]
        private TMP_Text itemNameText;
        [SerializeField, Required, FoldoutGroup("Cost Window")]
        private TMP_Text itemDescriptionText;
        
        [SerializeField, Required, FoldoutGroup("Cost Window")]
        private TMP_Text itemPowerUsage;

        [SerializeField, Required, FoldoutGroup("Cost Window")]
        private Image itemIcon;

        [FormerlySerializedAs("blueprints")] [SerializeField]
        private BlueprintUIElementScrollView blueprintsContentScrollView;

        [SerializeField, Required]
        private CraftingBench mCraftingBench;

        private StorageUI _storageUi;
        private DroneDesignUI _droneDesignUI;

        private Blueprint _currentSelected;

        private bool _scrollViewsSetup;

        //============================================================================================================//

        #region Unity Functions

        private void Start()
        {
            _storageUi = FindObjectOfType<StorageUI>();
            _droneDesignUI = FindObjectOfType<DroneDesignUI>();

            costWindowVerticalLayoutGroup = costWindowObject.GetComponent<VerticalLayoutGroup>();
            costWindowCanvasGroup = costWindowObject.GetComponent<CanvasGroup>();

            InitUIScrollView();
            _scrollViewsSetup = true;
        }

        private void OnEnable()
        {
            if (_scrollViewsSetup)
                RefreshScrollViews();

            blueprintsContentScrollView.ClearElements();
            InitUIScrollView();
            
            costWindowObject.SetActive(false);

        }


        #endregion //Unity Functions

        //============================================================================================================//

        #region Scroll Views

        private void InitUIScrollView()
        {
            //FIXME This needs to move to the Factory
            if (!Globals.DisableTestingFeatures)
            {
                PlayerPersistentData.PlayerData.UnlockAllBlueprints();
            }

            foreach (var blueprint in PlayerPersistentData.PlayerData.unlockedBlueprints)
            {
                var temp = blueprintsContentScrollView.AddElement(blueprint, $"{blueprint.name}_UIElement");
                temp.Init(blueprint, data =>
                {
                    Debug.Log("Craft button pressed");
                    mCraftingBench.CraftBlueprint(data);
                    _storageUi.UpdateStorage();

                }, TryShowBlueprintCost);
            }
        }

        public void RefreshScrollViews()
        {
            blueprintsContentScrollView.ClearElements();
            InitUIScrollView();
        }

        #endregion //Scroll Views

        //============================================================================================================//

        #region Other

        private Blueprint lastBlueprint;

        private void TryShowBlueprintCost(Blueprint blueprint, bool showWindow, RectTransform buttonTransform)
        {
            costWindowObject.SetActive(showWindow);


            costWindowVerticalLayoutGroup.enabled = false;
            costWindowCanvasGroup.alpha = 0;

            _droneDesignUI.PreviewCraftCost(showWindow, blueprint);
            
            if (!showWindow)
            {
                PlayerData.OnValuesChanged -= UpdateCostUI;
                lastBlueprint = null;
                return;
            }

            PlayerData.OnValuesChanged += UpdateCostUI;

            lastBlueprint = blueprint;

            UpdateCostUI();

            //FIXME This is just a temp setup to ensure the functionality
            StartCoroutine(ResizeRepositionCostWindowCoroutine(buttonTransform));
        }

        private IEnumerator ResizeRepositionCostWindowCoroutine(RectTransform buttonTransform)
        {
            Canvas.ForceUpdateCanvases();
            costWindowVerticalLayoutGroup.enabled = true;
            
            yield return new WaitForEndOfFrame();
            
            var windowTransform = costWindowObject.transform as RectTransform;
            windowTransform.position = buttonTransform.position;
            windowTransform.localPosition += Vector3.left * (buttonTransform.sizeDelta.x / 2f + windowTransform.sizeDelta.x / 2f);

            costWindowCanvasGroup.alpha = 1;
        }

        private void UpdateCostUI()
        {
            costView.ClearElements();
            
            var partProfileData = FactoryManager.Instance.GetFactory<PartAttachableFactory>()
                .GetProfileData(lastBlueprint.partType);
            
            itemIcon.sprite = partProfileData.Sprites[lastBlueprint.level];


            var partRemoteData = FactoryManager.Instance.GetFactory<PartAttachableFactory>()
                .GetRemoteData(lastBlueprint.partType);
            
            itemNameText.text = partRemoteData.name;
            itemDescriptionText.text = partRemoteData.description;

            var powerDraw = partRemoteData.levels[lastBlueprint.level].powerDraw;
            itemPowerUsage.gameObject.SetActive(powerDraw > 0);

            if(powerDraw > 0)
                itemPowerUsage.text = $"Power: {powerDraw} {TMP_SpriteMap.MaterialIcons[BIT_TYPE.YELLOW]}/s";

            var resources = partRemoteData.levels[lastBlueprint.level].cost;

            foreach (var resource in resources)
            {
                var element = costView.AddElement(resource, $"{resource.type}_UIElement");
                element.Init(resource);
            }
        }

        #endregion //Other

        //============================================================================================================//
    }

    [System.Serializable]
    public class BlueprintUIElementScrollView: UIElementContentScrollView<BlueprintUIElement, Blueprint>
    {}
}
