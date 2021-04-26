using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using StarSalvager.Cameras;
using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using StarSalvager.Parts.Data;
using StarSalvager.ScriptableObjects;
using StarSalvager.UI.Hints;
using StarSalvager.Utilities;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.Helpers;
using StarSalvager.Utilities.JsonDataTypes;
using StarSalvager.Utilities.Saving;
using StarSalvager.Utilities.SceneManagement;
using StarSalvager.Utilities.UI;
using StarSalvager.Values;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace StarSalvager.UI.Scrapyard
 {
    public class DroneDesignUI : MonoBehaviour
    {
        public bool CanAffordRepair { get; private set; }

        [SerializeField, Required] private PointerEvents repairButtonPointerEvents;

        //====================================================================================================================//

        [SerializeField] private PurchasePatchUIElementScrollView purchasePatchUIElementScrollView;
        
        

        //============================================================================================================//

        [Serializable]
        public struct PatchUI
        {
            [HorizontalGroup("Row1"), LabelWidth(75)]
            public Image backgroundImage;

            [HorizontalGroup("Row1"), LabelWidth(50)]
            public TMP_Text text;
        }

        [Serializable]
        public struct GradeUI
        {
            [HorizontalGroup("Row1"), LabelWidth(75)]
            public Image bitImage;

            [HorizontalGroup("Row1"), LabelWidth(50)]
            public TMP_Text text;
        }

        //====================================================================================================================//

        [SerializeField, FoldoutGroup("Part Upgrade UI"), Required]
        private GameObject partUpgradeWindow;
        
        [SerializeField, FoldoutGroup("Part Upgrade UI"), Required]
        private TMP_Text upgradeWindowHeaderText;

        [SerializeField, FoldoutGroup("Part Upgrade UI"), Required]
        private Button closePartUpgradeWindowButton;
        
        [SerializeField, FoldoutGroup("Part Upgrade UI")] 
        private UpgradeUIElementScrollView partUpgradeUIElementScrollView;

        //====================================================================================================================//
        
        [SerializeField, FoldoutGroup("Part Details Window")]
        private RectTransform partDetailsContainerRectTransform;

        [SerializeField, FoldoutGroup("Part Details Window")]
        private Image partImage;

        [SerializeField, FoldoutGroup("Part Details Window")]
        private TMP_Text partNameText;
        [SerializeField, FoldoutGroup("Part Details Window")]
        private TMP_Text partUseTypeText;
        [SerializeField, FoldoutGroup("Part Details Window")]
        private TMP_Text partDescriptionText;

        //[SerializeField, FoldoutGroup("Part Details Window")]
        //private TMP_Text otherPartDetailsText;

        [FormerlySerializedAs("PatchUis")] [SerializeField, FoldoutGroup("Part Details Window")]
        private PatchUI[] patchUis;

        [SerializeField, FoldoutGroup("Part Details Window")]
        private TMP_Text partDetailsText;

        /*[FormerlySerializedAs("GradeUis")] [SerializeField, FoldoutGroup("Part Details Window")]
        private GradeUI[] gradeUis;*/

        //====================================================================================================================//

        [SerializeField, Required, BoxGroup("Repairs Buttons")]
        private Button repairButton;
        [SerializeField, Required, BoxGroup("Repairs Buttons")]
        private TMP_Text repairButtonText;

        [SerializeField, Required, BoxGroup("Repairs Buttons")]
        private FadeUIImage repairButtonGlow;

        [SerializeField, Required, BoxGroup("Health UI")]
        private SliderText healthSliderText;
        

        [SerializeField, Required, BoxGroup("Menu Buttons")]
        private Button launchButton;

        

        [SerializeField] private CameraController CameraController;


        public static Action CheckBlueprintNewAlertUpdate;

        //============================================================================================================//

        public bool HoveringStoragePartUIElement { get; private set; }

        private DroneDesigner DroneDesigner
        {
            get
            {
                if (_droneDesigner == null)
                    _droneDesigner = FindObjectOfType<DroneDesigner>();

                return _droneDesigner;
            }
        }

        [SerializeField, Required] private DroneDesigner _droneDesigner;

        private ScrapyardLayout currentSelected;

        public bool UpgradeWindowOpen => partUpgradeWindow.activeInHierarchy;

        public bool IsPopupActive => Alert.Displayed;
        private bool _currentlyOverwriting;

        private bool _scrollViewsSetup;

        float cameraScaleOnEnter = 71;

        //Unity Functions
        //============================================================================================================//

        #region Unity Functions

        private void Start()
        {
            InitButtons();
            InitHealthBar();

            _scrollViewsSetup = true;

            _currentlyOverwriting = false;

            HidePartDetails();
            SetUpgradeWindowActive(false);
        }

        private void OnEnable()
        {
            Camera.onPostRender += _droneDesigner.DrawGL;
            PlayerDataManager.OnValuesChanged += CheckCanRepair;

            UpdateHealthBar();
            CheckCanRepair();
            
            InitPurchasePatches();
        }

        private void OnDisable()
        {
            Camera.onPostRender -= _droneDesigner.DrawGL;
            PlayerDataManager.OnValuesChanged -= CheckCanRepair;
            
            if(_droneDesigner)
                _droneDesigner.RecycleDrone();

            Globals.ScaleCamera(Globals.CameraScaleSize);
        }

        #endregion //Unity Functions

        //============================================================================================================//

        #region Init

        private void InitButtons()
        {
            repairButton.onClick.AddListener(() =>
            {
                DroneDesigner.RepairDrone();
                healthSliderText.value = PlayerDataManager.GetBotHealth();

                CheckCanRepair();
            });

            launchButton.onClick.AddListener(() =>
            {
                SceneLoader.ActivateScene(SceneLoader.UNIVERSE_MAP, SceneLoader.SCRAPYARD);
            });
            
            closePartUpgradeWindowButton.onClick.AddListener(() =>
            {
                SetUpgradeWindowActive(false);
            });

            //--------------------------------------------------------------------------------------------------------//

        }

        private void InitHealthBar()
        {
            var startingHealth = FactoryManager.Instance.PartsRemoteData
                .GetRemoteData(PART_TYPE.CORE)
                .GetDataValue<float>(PartProperties.KEYS.Health);
            
            healthSliderText.Init(true);
            healthSliderText.SetBounds(0f, startingHealth);
        }

        #endregion //Init

        //============================================================================================================//

        #region Scroll Views

        public void InitPurchasePatches()
        {
            purchasePatchUIElementScrollView.ClearElements();
            
            var patchRemoteData = FactoryManager.Instance.PatchRemoteData;
            var patches = PlayerDataManager.Patches;

            if (patches.IsNullOrEmpty())
                return;

            var purchasePatchData = new List<Purchase_PatchData>();
            for (var i = 0; i < patches.Count; i++)
            {
                var patchData = patches[i];
                var patchType = (PATCH_TYPE) patchData.Type;
                var remoteData = patchRemoteData.GetRemoteData(patchType);
                var silver = remoteData.Levels[patchData.Level].silver;
                var gears = remoteData.Levels[patchData.Level].gears *
                           PlayerDataManager.GetCurrentUpgradeValue(UPGRADE_TYPE.PATCH_COST);

                purchasePatchData.Add(new Purchase_PatchData
                {
                    index = i,
                    silver = silver,
                    gears = Mathf.RoundToInt(gears),
                    PatchData = patchData
                });
            }

            foreach (var t in purchasePatchData)
            {
                var element = purchasePatchUIElementScrollView.AddElement(t);
                element.Init(t, ShowPartUpgradeSelectionWindow);
            }
            
        }

        /// <summary>
        /// Gets a collection of all parts that the player currently has, then lays them out on a grid to allow the
        /// player to pick a part to be upgraded.
        /// </summary>
        /// <param name="purchasePatchData"></param>
        /// <exception cref="ArgumentException"></exception>
        private void ShowPartUpgradeSelectionWindow(Purchase_PatchData purchasePatchData)
        {
            //--------------------------------------------------------------------------------------------------------//
            
            //FIXME I need to sort this list By Enabled & Name
            void AddPartsToView(in IReadOnlyList<IBlockData> blockDatas, in bool isAttached = false)
            {
                for (var i = 0; i < blockDatas.Count; i++)
                {
                    if (!(blockDatas[i] is PartData partData && partData.Type != (int)PART_TYPE.EMPTY))
                        continue;

                    //var storageBlockData = storedParts[i];
                    var type = (PART_TYPE) partData.Type;

                    var blockIndex = i;
                    var partUpgrd = new TEST_PartUpgrd
                    {
                        PartData = partData,
                        itemIndex = blockIndex,
                        isAttached = isAttached,
                    
                        PurchasePatchData = purchasePatchData
                    };

                    var temp = partUpgradeUIElementScrollView.AddElement(partUpgrd,
                        $"{type}_UIElement",
                        allowDuplicate: true);
                
                    temp.Init(partUpgrd, OnPurchasedPatch);
                }
            }

            //--------------------------------------------------------------------------------------------------------//
            
            var patchType = (PATCH_TYPE)purchasePatchData.PatchData.Type;
            
            partUpgradeUIElementScrollView.ClearElements();

            //Parts in Storage
            //--------------------------------------------------------------------------------------------------------//
            
            //TODO Need to include the attached parts
            var storedParts = PlayerDataManager.GetCurrentPartsInStorage();
            AddPartsToView(storedParts);

            
            //Parts On Bot
            //--------------------------------------------------------------------------------------------------------//
            
            var attachedParts = PlayerDataManager.GetBlockDatas();
            AddPartsToView(attachedParts, true);

            //--------------------------------------------------------------------------------------------------------//
            
            partUpgradeUIElementScrollView.SortList();
            
            var patchData = FactoryManager.Instance.PatchRemoteData.GetRemoteData(patchType);

            SetUpgradeWindowActive(true, $"{patchData.name} {purchasePatchData.PatchData.Level + 1}", patchData.description);
        }



        #endregion //Scroll Views

        //====================================================================================================================//

        #region Upgrade Parts

        private void SetUpgradeWindowActive(in bool state, in string title, in string description)
        {
            upgradeWindowHeaderText.text = $"{title}\n{description}";
            
            SetUpgradeWindowActive(state);

        }
        private void SetUpgradeWindowActive(in bool state)
        {
            partUpgradeWindow.SetActive(state);
        }

        private void OnPurchasedPatch(TEST_PartUpgrd partUpgrd)
        {
            //Remove Components
            //--------------------------------------------------------------------------------------------------------//
            
            var patchData = partUpgrd.PurchasePatchData;
            var currentGears = PlayerDataManager.GetGears();
            var currentSilver = PlayerDataManager.GetSilver();
            if (currentGears < patchData.gears || currentSilver < patchData.silver)
                return;

            PlayerDataManager.SubtractGears(patchData.gears);
            PlayerDataManager.SubtractSilver(patchData.silver);
            
            //Add Patch to Selected Part
            //--------------------------------------------------------------------------------------------------------//

            if (partUpgrd.isAttached)
            {
                if (!(DroneDesigner._scrapyardBot.AttachedBlocks[partUpgrd.itemIndex] is ScrapyardPart scrapyardPart))
                    throw new Exception(
                        $"{nameof(ScrapyardPart)} was expected at {nameof(DroneDesigner._scrapyardBot.AttachedBlocks)}[{partUpgrd.itemIndex}]");
                
                scrapyardPart.AddPatch(partUpgrd.PurchasePatchData.PatchData);
            }
            else
            {
                var partsInStorage = PlayerDataManager.GetCurrentPartsInStorage().ToList();
                if(!(partsInStorage[partUpgrd.itemIndex] is PartData))
                    throw new Exception(
                        $"{nameof(PartData)} was expected at {nameof(PlayerDataManager.GetCurrentPartsInStorage)}[{partUpgrd.itemIndex}]");
                
                ((PartData)partsInStorage[partUpgrd.itemIndex]).AddPatch(partUpgrd.PurchasePatchData.PatchData);

                PlayerDataManager.SetCurrentPartsInStorage(partsInStorage);
            }
            
            //Once its been purchased it should be removed
            //purchasePatchUIElementScrollView.RemoveElementAtIndex(partUpgrd.PurchasePatchData.index);
            PlayerDataManager.RemovePatchAtIndex(partUpgrd.PurchasePatchData.index);
            InitPurchasePatches();

            //Refresh Data
            //--------------------------------------------------------------------------------------------------------//
            
            //TODO Will need to reload the
            
            SetUpgradeWindowActive(false);
        }
        
        #endregion //Upgrade Parts

        //====================================================================================================================//
        

        #region Other

        public void DisplayInsufficientResources()
        {
            Alert.ShowAlert("Alert!",
                "You do not have enough resources to purchase this part!", "Okay", null);
        }

        private void UpdateHealthBar()
        {
            var health = PlayerDataManager.GetBotHealth();
            healthSliderText.value = health;
        }

        private void CheckCanRepair()
        {
            var currentHealth = PlayerDataManager.GetBotHealth();
            var startingHealth = FactoryManager.Instance.PartsRemoteData
                .GetRemoteData(PART_TYPE.CORE)
                .GetDataValue<float>(PartProperties.KEYS.Health);
            
            var canRepair = currentHealth < startingHealth;

            repairButton.gameObject.SetActive(canRepair);

            if (!canRepair)
                return;

            var cost = Mathf.CeilToInt(startingHealth - currentHealth);
            var components = PlayerDataManager.GetGears();

            var finalCost = components > 0 ? Mathf.Min(cost, components) : cost;

            repairButtonText.text = $"Repair {finalCost}{TMP_SpriteHelper.GEAR_ICON}";
            repairButton.interactable = !(finalCost > components);
        }

        #endregion //Other

        //============================================================================================================//

        public void HidePartDetails()
        {
            ShowPartDetails(false, null);
            HoveringStoragePartUIElement = false;
        }
        public void ShowPartDetails(bool show, in ScrapyardPart scrapyardPart)
        {
            var screenPoint = show
                ? CameraController.Camera.WorldToScreenPoint(scrapyardPart.transform.position + Vector3.right)
                : Vector3.zero;
            
            var partData = show ? scrapyardPart.ToBlockData() : new PartData();

            ShowPartDetails(show, partData, screenPoint);
        }
        
        public void ShowPartDetails(bool show, in PartData partData, in RectTransform rectTransform)
        {
            HoveringStoragePartUIElement = show;
            
            var screenPoint = show ? RectTransformUtility.WorldToScreenPoint(null,
                (Vector2) rectTransform.position + Vector2.right * rectTransform.sizeDelta.x)
                    : Vector2.zero;

            ShowPartDetails(show, partData, screenPoint);
        }

        private void ShowPartDetails(in bool show, in PartData partData, in Vector2 screenPoint)
        {

            //--------------------------------------------------------------------------------------------------------//

            void SetRectSize(in TMP_Text tmpText, in float multiplier = 1.388f)
            {
                tmpText.ForceMeshUpdate();

                var lineCount = tmpText.GetTextInfo(tmpText.text).lineCount;
                var lineSize = tmpText.fontSize * multiplier;
                var rectTrans = (RectTransform)tmpText.transform;
                var sizeDelta = rectTrans.sizeDelta;

                if (tmpText.GetComponent<LayoutElement>() is LayoutElement layoutElement)
                {
                    sizeDelta.y = Mathf.Max(layoutElement.minHeight, lineSize * lineCount);
                    layoutElement.preferredHeight = sizeDelta.y;
                }
                else
                {
                    sizeDelta.y = lineSize * lineCount;
                }
                
                
                rectTrans.sizeDelta = sizeDelta;       
            }
            
            IEnumerator ResizeDelayedCoroutine(params TMP_Text[] args)
            {
                foreach (var tmpText in args)
                {
                    tmpText.ForceMeshUpdate();
                }
                
                yield return new WaitForEndOfFrame();

                foreach (var tmpText in args)
                {
                    SetRectSize(tmpText);
                }
            }
            
            //--------------------------------------------------------------------------------------------------------//
            
            partDetailsContainerRectTransform.gameObject.SetActive(show);

            if (!show)
                return;
            
            var canvasRect = GetComponentInParent<Canvas>().transform as RectTransform;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, null,
                out var localPoint);

            partDetailsContainerRectTransform.anchoredPosition = localPoint;

            //====================================================================================================================//

            var partType = (PART_TYPE) partData.Type;
            var partRemote = partType.GetRemoteData();
            var partProfile = partType.GetProfileData();
            var patchRemoteData = FactoryManager.Instance.PatchRemoteData;

            //====================================================================================================================//

            partNameText.text = partRemote.name;
            partUseTypeText.text = partRemote.isManual ? "Manually Triggered" : "Automatic";
            partDescriptionText.text = partRemote.description;
            
            partImage.sprite = partProfile.Sprite;
            partImage.color = partRemote.category.GetColor();

            partDetailsText.text = partData.GetPartDetails(partRemote);

            for (var i = 0; i < partData.Patches.Length; i++)
            {
                if (i >= patchUis.Length)
                    break;
                
                var patchData = partData.Patches[i];
                var type = (PATCH_TYPE) patchData.Type;

                patchUis[i].backgroundImage.enabled = type != PATCH_TYPE.EMPTY;

                patchUis[i].text.text = type == PATCH_TYPE.EMPTY
                    ? string.Empty
                    : $"{patchRemoteData.GetRemoteData(type).name} {patchData.Level + 1}";

            }

            //====================================================================================================================//

            //Resize the details text to accomodate the text
            StartCoroutine(ResizeDelayedCoroutine(partDetailsText, partDescriptionText));
            
            partDetailsContainerRectTransform.TryFitInScreenBounds(canvasRect, 20f);
            
        }
        //====================================================================================================================//

    }
}
