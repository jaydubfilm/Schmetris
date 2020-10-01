using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using StarSalvager.Cameras;
using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using StarSalvager.ScriptableObjects;
using StarSalvager.Utilities.JsonDataTypes;
using StarSalvager.Utilities.UI;
using StarSalvager.Values;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StarSalvager.UI.Scrapyard
{
    public class DroneDesignUI : MonoBehaviour
    {
        [SerializeField, Required] 
        private TMP_Text flightDataText;
        
        [SerializeField, Required]
        private PointerEvents launchButtonPointerEvents;
        [SerializeField, Required]
        private PointerEvents repairButtonPointerEvents;

        //====================================================================================================================//
        
        [SerializeField, Required, BoxGroup("Part UI")]
        private GameObject partsWindow;
        [SerializeField, Required, BoxGroup("Part UI")]
        private RemotePartProfileScriptableObject remotePartProfileScriptable;
        [SerializeField, BoxGroup("Part UI")]
        private PartUIElementScrollView partsScrollView;

        //============================================================================================================//

        [SerializeField, BoxGroup("Resource UI")]
        private ResourceUIElementScrollView resourceScrollView;
        
        [SerializeField, BoxGroup("Resource UI")]
        private ResourceUIElementScrollView liquidResourceContentView;

        [SerializeField, BoxGroup("Load List UI")]
        private LayoutElementScrollView layoutScrollView;

        //============================================================================================================//

        [SerializeField, Required, BoxGroup("Menu Buttons")]
        private Button isUpgradingButton;
        [SerializeField, Required, BoxGroup("Menu Buttons")]
        private Button undoButton;
        [SerializeField, Required, BoxGroup("Menu Buttons")]
        private Button redoButton;
        [SerializeField, Required, BoxGroup("Menu Buttons")]
        private Button saveLayoutButton;
        [SerializeField, Required, BoxGroup("Menu Buttons")]
        private Button loadLayoutButton;
        
        [SerializeField, Required, BoxGroup("Menu Buttons")]
        private Button repairButton;
        private TMP_Text _repairButtonText;

        [SerializeField, Required, BoxGroup("Load Menu")]
        private GameObject loadMenu;
        [SerializeField, Required, BoxGroup("Load Menu")]
        private Button loadConfirm;
        [SerializeField, Required, BoxGroup("Load Menu")]
        private Button loadReturn;
        [SerializeField, Required, BoxGroup("Load Menu")]
        private Button loadReturn2;
        [SerializeField, Required, BoxGroup("Load Menu")]
        private TMP_Text loadName;

        [SerializeField, Required, BoxGroup("Save Menu")]
        private GameObject saveMenu;
        [SerializeField, Required, BoxGroup("Save Menu")]
        private GameObject saveOverwritePortion;
        [SerializeField, Required, BoxGroup("Save Menu")]
        private GameObject saveBasePortion;
        [SerializeField, Required, BoxGroup("Save Menu")]
        private TMP_InputField saveNameInputField;
        [SerializeField, Required, BoxGroup("Save Menu")]
        private Button saveConfirm;
        [SerializeField, Required, BoxGroup("Save Menu")]
        private Button saveReturn;
        [SerializeField, Required, BoxGroup("Save Menu")]
        private Button saveReturn2;
        [SerializeField, Required, BoxGroup("Save Menu")]
        private Button saveOverwrite;
        [SerializeField, Required, BoxGroup("Save Menu")]
        private Button saveAsNew;

        [SerializeField, Required, BoxGroup("UI Visuals")]
        private Image screenBlackImage;

        //============================================================================================================//

        private DroneDesigner DroneDesigner
        {
            get
            {
                if (_droneDesigner == null)
                    _droneDesigner = FindObjectOfType<DroneDesigner>();

                return _droneDesigner;
            }
        }
        [SerializeField, Required]
        private DroneDesigner _droneDesigner;

        private ScrapyardLayout currentSelected;

        public bool IsPopupActive => loadMenu.activeSelf || saveMenu.activeSelf || Alert.Displayed;
        private bool _currentlyOverwriting;

        private bool _scrollViewsSetup;

        //Unity Functions
        //============================================================================================================//

        #region Unity Functions

        private void Start()
        {
            InitButtons();

            InitUiScrollView();
            UpdateBotResourceElements();
            _scrollViewsSetup = true;

            _currentlyOverwriting = false;
        }

        private void OnEnable()
        {
            if (_scrollViewsSetup)
                RefreshScrollViews();

            PlayerData.OnValuesChanged += UpdateBotResourceElements;
            PlayerData.OnCapacitiesChanged += UpdateBotResourceElements;

            //TODO May want to setup some sort of Init function to merge these two setups
            launchButtonPointerEvents.PointerEntered += PreviewFillBotResources;
            repairButtonPointerEvents.PointerEntered += PreviewRepairCost;
        }

        private void OnDisable()
        {
            DroneDesigner?.ClearUndoRedoStacks();
            
            PlayerData.OnValuesChanged -= UpdateBotResourceElements;
            PlayerData.OnCapacitiesChanged -= UpdateBotResourceElements;
            
            
            launchButtonPointerEvents.PointerEntered -= PreviewFillBotResources;
            repairButtonPointerEvents.PointerEntered -= PreviewRepairCost;
        }

        #endregion //Unity Functions

        //============================================================================================================//

        #region Init

        private void InitButtons()
        {

            _repairButtonText = repairButton.GetComponentInChildren<TMP_Text>();
            
            repairButton.onClick.AddListener(() =>
            {
                DroneDesigner.RepairParts();
                PreviewRepairCost(false);
            });
            
            //--------------------------------------------------------------------------------------------------------//

            undoButton.onClick.AddListener(() =>
            {
                DroneDesigner.UndoStackPop();
            });

            redoButton.onClick.AddListener(() =>
            {
                DroneDesigner.RedoStackPop();
            });

            //--------------------------------------------------------------------------------------------------------//

            isUpgradingButton.onClick.AddListener(() =>
            {
                DroneDesigner.IsUpgrading = !DroneDesigner.IsUpgrading;
            });

            //--------------------------------------------------------------------------------------------------------//

            saveLayoutButton.onClick.AddListener(() =>
            {
                if (DroneDesigner.IsFullyConnected())
                {
                    saveMenu.SetActive(true);
                    bool isOverwrite = _currentlyOverwriting;
                    saveOverwritePortion.SetActive(isOverwrite);
                    saveBasePortion.SetActive(!isOverwrite);
                }
                else
                {
                    Alert.ShowAlert("Alert!", "There are blocks currently floating or disconnected.", "Okay", () =>
                    {
                        screenBlackImage.gameObject.SetActive(false);
                    });
                }
                screenBlackImage.gameObject.SetActive(true);
            });

            loadLayoutButton.onClick.AddListener(() =>
            {
                loadMenu.SetActive(true);
                partsWindow.SetActive(false);
                currentSelected = null;
                UpdateLoadListUiScrollViews();
                screenBlackImage.gameObject.SetActive(true);
            });

            //--------------------------------------------------------------------------------------------------------//

            loadConfirm.onClick.AddListener(() =>
            {
                if (currentSelected == null)
                    return;

                DroneDesigner.LoadLayout(currentSelected.Name);
                loadMenu.SetActive(false);
                partsWindow.SetActive(true);
                _currentlyOverwriting = true;
                screenBlackImage.gameObject.SetActive(false);
            });

            loadReturn.onClick.AddListener(() =>
            {
                loadMenu.SetActive(false);
                partsWindow.SetActive(true);
                screenBlackImage.gameObject.SetActive(false);
            });

            loadReturn2.onClick.AddListener(() =>
            {
                loadMenu.SetActive(false);
                partsWindow.SetActive(true);
                screenBlackImage.gameObject.SetActive(false);
            });

            //--------------------------------------------------------------------------------------------------------//

            saveConfirm.onClick.AddListener(() =>
            {
                DroneDesigner.SaveLayout(saveNameInputField.text);
                saveMenu.SetActive(false);
                _currentlyOverwriting = true;
                screenBlackImage.gameObject.SetActive(false);
            });

            saveReturn.onClick.AddListener(() =>
            {
                saveMenu.SetActive(false);
                screenBlackImage.gameObject.SetActive(false);
            });

            saveReturn2.onClick.AddListener(() =>
            {
                saveMenu.SetActive(false);
                screenBlackImage.gameObject.SetActive(false);
            });

            saveOverwrite.onClick.AddListener(() =>
            {
                DroneDesigner.SaveLayout(currentSelected.Name);
                saveMenu.SetActive(false);
                _currentlyOverwriting = true;
                screenBlackImage.gameObject.SetActive(false);
            });

            saveAsNew.onClick.AddListener(() =>
            {
                saveOverwritePortion.SetActive(false);
                saveBasePortion.SetActive(true);
                _currentlyOverwriting = false;
            });

            //--------------------------------------------------------------------------------------------------------//

        }

        #endregion //Init

        //============================================================================================================//

        #region Scroll Views

        private void InitUiScrollView()
        {
            foreach (var blockData in PlayerPersistentData.PlayerData.GetCurrentPartsInStorage())
            {
                var partRemoteData = remotePartProfileScriptable.GetRemoteData((PART_TYPE)blockData.Type);

                var element = partsScrollView.AddElement(partRemoteData, $"{partRemoteData.partType}_UIElement", allowDuplicate: true);
                element.Init(partRemoteData, BrickElementPressed, blockData.Level);
            }
        }

        public void AddToPartScrollView(BlockData blockData)
        {
            var partRemoteData = remotePartProfileScriptable.GetRemoteData((PART_TYPE)blockData.Type);

            var element = partsScrollView.AddElement(partRemoteData, $"{partRemoteData.partType}_UIElement", allowDuplicate: true);
            element.Init(partRemoteData, BrickElementPressed, blockData.Level);
        }

        public void RefreshScrollViews()
        {
            partsScrollView.ClearElements();
            InitUiScrollView();
            UpdateBotResourceElements();
        }

        public void UpdateBotResourceElements()
        {
            var resources = PlayerPersistentData.PlayerData.resources;
            var resourceCapacities = PlayerPersistentData.PlayerData.ResourceCapacities;

            foreach (var resource in resources)
            {
                var data = new ResourceAmount
                {
                    //resourceType = CraftCost.TYPE.Bit,
                    type = resource.Key,
                    amount = resource.Value,
                    capacity = resourceCapacities[resource.Key]
                };

                var element = resourceScrollView.AddElement(data, $"{resource.Key}_UIElement");
                element.Init(data);
            }

            //liquidResourceContentView
            var liquids = PlayerPersistentData.PlayerData.liquidResource;
            var liquidsCapacity = PlayerPersistentData.PlayerData.liquidCapacity;
            foreach (var liquid in liquids)
            {
                var bitType = liquid.Key;

                switch (bitType)
                {
                    case BIT_TYPE.BLUE:
                    case BIT_TYPE.WHITE:
                        continue;
                }
                
                var data = new ResourceAmount
                {
                    amount = (int)liquid.Value,
                    capacity = liquidsCapacity[bitType],
                    type = bitType,
                };
                
                if(bitType == BIT_TYPE.YELLOW)
                    Console.WriteLine("");

                var element = liquidResourceContentView.AddElement(data, $"{liquid.Key}_UIElement");
                element.Init(data, true);
            }


            UpdateFlightDataUI();

        }

        private void UpdateLoadListUiScrollViews()
        {
            foreach (var layoutData in DroneDesigner.ScrapyardLayouts)
            {
                if (layoutScrollView.FindElement(layoutData))
                    continue;

                var element = layoutScrollView.AddElement(layoutData, $"{layoutData.Name}_UIElement");
                element.Init(layoutData, LayoutPressed);
            }
            layoutScrollView.SetElementsActive(true);
        }

        #endregion //Scroll Views

        //============================================================================================================//

        #region Flight Data UI

        private void UpdateFlightDataUI()
        {
            //--------------------------------------------------------------------------------------------------------//
            if (_droneDesigner?._scrapyardBot is null)
            {
                flightDataText.text = "Flight Data:\nPower Draw: 0.0 KW/s\nTotal Power: Infinite";
                return;
            }
            
            var powerDraw = _droneDesigner._scrapyardBot.powerDraw;
            var availablePower =
                Mathf.Clamp(
                    PlayerPersistentData.PlayerData.liquidResource[BIT_TYPE.YELLOW] +
                    PlayerPersistentData.PlayerData.resources[BIT_TYPE.YELLOW], 0,
                    PlayerPersistentData.PlayerData.liquidCapacity[BIT_TYPE.YELLOW]);

            if (powerDraw == 0f)
            {
                flightDataText.text = $"Flight Data:\nPower Draw: {powerDraw:0.0} KW/s\nTotal Power: Infinite";
            }
            else
            {
                var powerTime = TimeSpan.FromSeconds(availablePower / powerDraw).ToString(@"mm\:ss");

                flightDataText.text = $"Flight Data:\nPower Draw: {powerDraw:0.0} KW/s\nTotal Power: {powerTime}s";
            }
            
        }

        #endregion //Flight Data UI

        //Preview Costs
        //====================================================================================================================//

        #region Preview Costs

        private void PreviewFillBotResources(bool showPreview)
        {
            BIT_TYPE[] types = {
                BIT_TYPE.RED,
                BIT_TYPE.GREY,
                BIT_TYPE.GREEN,
                BIT_TYPE.YELLOW
            };
            
            foreach (var bitType in types)
            {
                switch (bitType)
                {
                    case BIT_TYPE.GREEN:
                        //TODO Check for repair
                        if(!_droneDesigner.HasPart(PART_TYPE.REPAIR))
                            continue;
                        break;
                    case BIT_TYPE.GREY:
                        //TODO Check for a gun
                        if(!_droneDesigner.HasParts(PART_TYPE.GUN, PART_TYPE.TRIPLESHOT))
                            continue;
                        break;
                    case BIT_TYPE.YELLOW:
                        if(_droneDesigner._scrapyardBot.powerDraw <= 0)
                            continue;
                        break;
                    case BIT_TYPE.RED:
                        break;
                    default:
                        continue;
                }

                var botLiquidElement = liquidResourceContentView.FindElement(x => x.type == bitType);
                var storageLiquidElement = resourceScrollView.FindElement(x => x.type == bitType);
                

                if (!showPreview)
                {
                    botLiquidElement.PreviewChange(0);
                    storageLiquidElement.PreviewChange(0);
                    continue;
                }
                
                
                var currentAmount = PlayerPersistentData.PlayerData.liquidResource[bitType];
                var currentCapacity = PlayerPersistentData.PlayerData.liquidCapacity[bitType];

                var fillRemaining = currentCapacity - currentAmount;

                //If its already full, then we're good to move on
                if (fillRemaining <= 0f)
                    continue;

                var availableResources = PlayerPersistentData.PlayerData.resources[bitType];

                //If we have no resources available to refill the liquid, move onto the next
                if(availableResources <= 0)
                    continue;

                var movingAmount = Mathf.RoundToInt(Mathf.Min(availableResources, fillRemaining));

                botLiquidElement.PreviewChange(movingAmount);
                storageLiquidElement.PreviewChange(-movingAmount);
            }
        }

        private void PreviewRepairCost(bool showPreview)
        {
            var storageLiquidElement = resourceScrollView.FindElement(x => x.type == BIT_TYPE.GREEN);

            if (!showPreview || !repairButton.interactable)
            {
                storageLiquidElement.PreviewChange(0);
                return;
            }
            
            var finalRepairCost = GetRepairCost(DroneDesigner.GetRepairCostPair());
            storageLiquidElement.PreviewChange(-finalRepairCost);

        }

        public void PreviewCraftCost(bool showPreview, Blueprint blueprint)
        {
            //Get all the elements here
            var resourceUIElements = new Dictionary<BIT_TYPE, ResourceUIElement>();
            foreach (BIT_TYPE type in Enum.GetValues(typeof(BIT_TYPE)))
            {
                if (type == BIT_TYPE.WHITE)
                    continue;
                resourceUIElements.Add(type, resourceScrollView.FindElement(x => x.type == type));
            }

            if (!showPreview || !blueprint.CanAfford)
            {
                foreach (var resourceUIElement in resourceUIElements.Values)
                {
                    resourceUIElement?.PreviewChange(0f);
                }

                return;
            }

            var blueprintCraftCosts = FactoryManager.Instance.GetFactory<PartAttachableFactory>()
                .GetRemoteData(blueprint.partType).levels[blueprint.level].cost;

            foreach (var craftCost in blueprintCraftCosts.Where(craftCost =>
                craftCost.resourceType == CraftCost.TYPE.Bit))
            {
                resourceUIElements[(BIT_TYPE) craftCost.type].PreviewChange(-craftCost.amount);
            }

        }

        #endregion //Preview Costs

        //Repair Costs
        //====================================================================================================================//

        #region Repair Cost

        //FIXME This needs to be set up to better account for the weird things that come with Replacing destroyed parts
        public void ShowRepairCost(int repairCost, int replacementCost)
        {

            var finalRepairCost = GetRepairCost(repairCost, replacementCost);
            
            var show = finalRepairCost > 0;
            
            repairButton.gameObject.SetActive(show);

            if (!show)
                return;
            
            _repairButtonText.text = $"Repair {finalRepairCost} {TMP_SpriteMap.MaterialIcons[BIT_TYPE.GREEN]}";
            repairButton.interactable = PlayerPersistentData.PlayerData.resources[BIT_TYPE.GREEN] >= finalRepairCost;
            
            /*var totalCost = repairCost + replacementCost;
            
            
            var show = totalCost > 0;
            
            repairButton.gameObject.SetActive(show);

            if (!show)
                return;

            if (_repairButtonText == null)
                return;
            
            var available = PlayerPersistentData.PlayerData.resources[BIT_TYPE.GREEN];

            if (totalCost > available)
            {
                if (repairCost > 0)
                {
                    _repairButtonText.text = available < repairCost ? $"Repair {available}" : $"Repair all {repairCost}" + 
                        $" {TMP_SpriteMap.MaterialIcons[BIT_TYPE.GREEN]}";
                    repairButton.interactable = PlayerPersistentData.PlayerData.resources[BIT_TYPE.GREEN] > 0;
                }
                else
                {
                    _repairButtonText.text = $"Repair all {replacementCost} {TMP_SpriteMap.MaterialIcons[BIT_TYPE.GREEN]}";
                    repairButton.interactable = PlayerPersistentData.PlayerData.resources[BIT_TYPE.GREEN] >= replacementCost;
                }

                return;
            }
            

            _repairButtonText.text = $"Repair all {totalCost} {TMP_SpriteMap.MaterialIcons[BIT_TYPE.GREEN]}";
            repairButton.interactable = PlayerPersistentData.PlayerData.resources[BIT_TYPE.GREEN] >= totalCost;*/
        }

        private static int GetRepairCost(Vector2Int repairCostPair)
        {
            return GetRepairCost(repairCostPair.x, repairCostPair.y);
        }
        private static int GetRepairCost(int repairCost, int replacementCost)
        {
            var totalCost = repairCost + replacementCost;

            if (totalCost == 0)
                return 0;

            var available = PlayerPersistentData.PlayerData.resources[BIT_TYPE.GREEN];

            if (totalCost <= available || available == 0) 
                return totalCost;
            
            
            if (repairCost > 0)
            {
                return available < repairCost? available : repairCost;
            }
                
            return replacementCost;

        }

        #endregion //Repair Cost

        //====================================================================================================================//
        
        #region Other

        public void DisplayInsufficientResources()
        {
            Alert.ShowAlert("Alert!",
                "You do not have enough resources to purchase this part!", "Okay", null);
        }

        private void BrickElementPressed((Enum remoteDataType, int level) tuple)
        {
            var (remoteDataType, level) = tuple;

            string classType;
            int type;
            float health;
            
            switch (remoteDataType)
            {
                case PART_TYPE partType:
                    classType = nameof(Part);
                    type = (int) partType;
                    health = FactoryManager.Instance.PartsRemoteData.GetRemoteData(partType).levels[level].health;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(remoteDataType), remoteDataType, null);
            }
            
            var blockData = new BlockData
            {
                ClassType = classType,
                Type = type,
                Health = health
            };
            
            DroneDesigner.SelectPartFromStorage(blockData);
        }

        private void LayoutPressed(ScrapyardLayout botData)
        {
            currentSelected = botData;
            loadName.text = "Load " + botData.Name;
        }

        #endregion //Other

        //============================================================================================================//
    }
}
