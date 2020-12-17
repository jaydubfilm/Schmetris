using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using StarSalvager.Factories;
using StarSalvager.ScriptableObjects;
using StarSalvager.UI.Hints;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Values;
using StarSalvager.Utilities.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using StarSalvager.Utilities.Saving;

namespace StarSalvager.UI.Scrapyard
{
    //TODO Need to test the scroll sensitivity on the blueprint scrollview on other platforms. Works well for windows at 32.5
    public class CraftingBenchUI : MonoBehaviour
    {
        [SerializeField, Required] private RemotePartProfileScriptableObject _remotePartProfileScriptable;

        [SerializeField, Required, FoldoutGroup("Cost Window")]
        private GameObject costWindowObject;

        private CanvasGroup costWindowCanvasGroup;
        private VerticalLayoutGroup costWindowVerticalLayoutGroup;

        [SerializeField, Required, FoldoutGroup("Cost Window")]
        private RectTransform windowParent;

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

        [SerializeField, Required, FoldoutGroup("Cost Window")]
        private GameObject missingBannerObject;

        [SerializeField, Required, FoldoutGroup("Cost Window")]
        private TMP_Text missingFacilityText;

        [FormerlySerializedAs("blueprints")] [SerializeField]
        private BlueprintUIElementScrollView blueprintsContentScrollView;

        [SerializeField, Required] private CraftingBench mCraftingBench;

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
            costWindowObject.transform.SetParent(windowParent);
            costWindowObject.transform.SetAsLastSibling();

        }


        #endregion //Unity Functions

        //============================================================================================================//

        #region Scroll Views

        private void InitUIScrollView()
        {
            //FIXME This needs to move to the Factory
            if (Globals.TestingFeatures)
            {
                PlayerDataManager.UnlockAllBlueprints();
            }

            foreach (var blueprint in PlayerDataManager.GetUnlockedBlueprints())
            {
                var temp = blueprintsContentScrollView.AddElement(blueprint, $"{blueprint.name}_UIElement");
                temp.Init(blueprint, data =>
                {
                    Debug.Log("Craft button pressed");
                    mCraftingBench.CraftBlueprint(data);
                    _storageUi.UpdateStorage();
                    
                    if(HintManager.CanShowHint(HINT.CRAFT_PART))
                        HintManager.TryShowHint(HINT.CRAFT_PART, 0.25f);
                    
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

        public void HideBlueprintCostWindow()
        {
            TryShowBlueprintCost(null, false, null);
        }

        private void TryShowBlueprintCost(Blueprint blueprint, bool showWindow, RectTransform buttonTransform)
        {
            costWindowObject.SetActive(showWindow);


            costWindowVerticalLayoutGroup.enabled = false;
            costWindowCanvasGroup.alpha = 0;

            _droneDesignUI.PreviewCraftCost(showWindow, blueprint);

            if (!showWindow)
            {
                PlayerDataManager.OnValuesChanged -= UpdateCostUI;
                lastBlueprint = null;
                return;
            }

            PlayerDataManager.OnValuesChanged += UpdateCostUI;

            lastBlueprint = blueprint;

            UpdateCostUI();

            //FIXME This is just a temp setup to ensure the functionality
            StartCoroutine(ResizeRepositionCostWindowCoroutine(buttonTransform));
        }

        private IEnumerator ResizeRepositionCostWindowCoroutine(RectTransform buttonTransform)
        {
            //TODO Should also reposition the window relative to the screen bounds to always keep in window
            Canvas.ForceUpdateCanvases();
            costWindowVerticalLayoutGroup.enabled = true;

            yield return new WaitForEndOfFrame();

            var canvasSize = ((RectTransform)costWindowObject.GetComponentInParent<Canvas>().transform).sizeDelta;

            var windowTransform = (RectTransform) costWindowObject.transform;
            windowTransform.position = buttonTransform.position;

            //--------------------------------------------------------------------------------------------------------//

            var pos = windowTransform.localPosition;
            var sizeDelta = windowTransform.sizeDelta;
            var yDelta = sizeDelta.y / 2f;
            
            
            var yBoundAbs = canvasSize.y / 2f;

            if (pos.y + yDelta > yBoundAbs)
            {
                pos.y = yBoundAbs - yDelta;
                windowTransform.localPosition = pos;
            }
            else if (pos.y - yDelta < -yBoundAbs)
            {
                pos.y = -yBoundAbs + yDelta;
                windowTransform.localPosition = pos;
            }

            //--------------------------------------------------------------------------------------------------------//

            windowTransform.localPosition += Vector3.left * (buttonTransform.sizeDelta.x / 2f + sizeDelta.x / 2f);

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

            itemNameText.text = $"{partRemoteData.name} {lastBlueprint.level + 1}";
            itemDescriptionText.text = partRemoteData.description;

            var powerDraw = partRemoteData.levels[lastBlueprint.level].powerDraw;
            itemPowerUsage.gameObject.SetActive(powerDraw > 0);

            if (powerDraw > 0)
                itemPowerUsage.text = $"Power: {powerDraw} {TMP_SpriteMap.MaterialIcons[BIT_TYPE.YELLOW]}/s";

            var resources = partRemoteData.levels[lastBlueprint.level].cost;

            var hasIssue = CheckIfMissingFacility(resources, out var missingText);

            costView.SetActive(!hasIssue);
            missingBannerObject.SetActive(hasIssue);
            missingFacilityText.text = missingText;

            if (hasIssue)
                return;

            foreach (var resource in resources)
            {
                var element = costView.AddElement(resource, $"{resource.type}_UIElement");
                element.Init(resource);
            }
        }

        #endregion //Other

        private static bool CheckIfMissingFacility(IEnumerable<CraftCost> resources, out string missingText)
        { 
            var facilities = new Dictionary<COMPONENT_TYPE, FACILITY_TYPE>
            {
                [COMPONENT_TYPE.COIL] = FACILITY_TYPE.WORKBENCHCOIL,
                [COMPONENT_TYPE.CHIP] = FACILITY_TYPE.WORKBENCHCHIP,
                [COMPONENT_TYPE.FUSOR] = FACILITY_TYPE.WORKBENCHFUSOR
            };
            
            missingText = string.Empty;

            var condensed = resources.Where(x => x.resourceType == CraftCost.TYPE.Component).ToArray();

            if (condensed.IsNullOrEmpty())
                return false;

            foreach (var facility in from kvp in facilities
                let type = (int) kvp.Key
                let facility = kvp.Value
                where condensed.Any(x => x.type == type) && !PlayerDataManager.CheckHasFacility(facility)
                select facility)
            {
                missingText =
                    $"Missing {FactoryManager.Instance.FacilityRemote.GetRemoteData(facility).displayName}";

                return true;
            }

            return false;
        }

        //============================================================================================================//
    }

    [System.Serializable]
    public class BlueprintUIElementScrollView: UIElementContentScrollView<BlueprintUIElement, Blueprint>
    {}
}
