﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using StarSalvager.Factories;
using StarSalvager.PatchTrees.Data;
using StarSalvager.UI.Hints;
using StarSalvager.Utilities;
using StarSalvager.Utilities.Analytics.SessionTracking;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.Helpers;
using StarSalvager.Utilities.Inputs;
using StarSalvager.Utilities.JsonDataTypes;
using StarSalvager.Utilities.Saving;
using StarSalvager.Utilities.SceneManagement;
using StarSalvager.Utilities.UI;
using StarSalvager.Values;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace StarSalvager.UI.Wreckyard.PatchTrees
{
    public class PatchTreeUI : MonoBehaviour, IHasHintElement, IBuildNavigationProfile, IReset
    {
        private const int PART_SCRAP_VALUE = 10;
        private const string NO_PART_TEXT = "No Part Selected";
        
        //Properties
        //====================================================================================================================//

        #region Properties

        [SerializeField, Required]
        private Button launchButton;
        [SerializeField, Required]
        private TMP_Text currenciesText;
        [SerializeField, Required]
        private Slider healthSlider;
        
        [SerializeField, Required]
        private Button menuButton;
        [SerializeField, Required]
        private Button repairButton;

        [SerializeField, Required]
        private GameObject baseBackground;
        //Wreck Data
        //====================================================================================================================//

        [SerializeField, Required, FoldoutGroup("Wreck Window")]
        private TMP_Text wreckNameText;
        [SerializeField, Required, FoldoutGroup("Wreck Window")]
        private Image wreckImage;

        [SerializeField, Required, BoxGroup("Wreck Window/Prefabs")]
        private PartPatchUIElement partPatchOptionPrefab;
       [SerializeField, Required, FoldoutGroup("Wreck Window")]
       [FormerlySerializedAs("patchOptionsContainer")] 
        private RectTransform partPatchOptionsContainer;

        private PartPatchUIElement[] _partPatchOptionElements;

        //Purchase Patch Window
        //====================================================================================================================//

        [SerializeField, Required, FoldoutGroup("Patch Purchase Window")]
        private GameObject purchasePatchWindow;
        [SerializeField, Required, FoldoutGroup("Patch Purchase Window")]
        private Button purchasePatchButton;
        [SerializeField, Required, FoldoutGroup("Patch Purchase Window")]
        private TMP_Text patchNameText;
        [SerializeField, Required, FoldoutGroup("Patch Purchase Window")]
        private TMP_Text patchDetailsText;

        //Patch Details Window
        //====================================================================================================================//

        [SerializeField, Required, FoldoutGroup("Patch Hover Window")]
        private GameObject patchDetailsWindow;
        private RectTransform _patchDetailsWindowTransform;
        [SerializeField, Required, FoldoutGroup("Patch Hover Window")]
        private TMP_Text patchHoverTitleText;
        [SerializeField, Required, FoldoutGroup("Patch Hover Window")]
        private TMP_Text patchHoverDetailsText;
        

        //Part Data
        //====================================================================================================================//
        [SerializeField, Required, FoldoutGroup("Part Window")]
        private TMP_Text partNameText;
        [SerializeField, Required, FoldoutGroup("Part Window")]
        private TMP_Text partDescriptionText;
        [SerializeField, Required, FoldoutGroup("Part Window")]
        private TMP_Text partDetailsText;
        [SerializeField, Required, FoldoutGroup("Part Window")]
        private RectTransform partWindowContainer;
        
        [SerializeField, Required, FoldoutGroup("Part Window")]
        private Button scrapPartButton;
        private TMP_Text _scrapPartButtonText;
        
        //Patch Tree Data
        //====================================================================================================================//
        
        [SerializeField, Required, FoldoutGroup("Patch Tree Window")]
        private RectTransform patchTreeTierContainer;

        [SerializeField, Required, BoxGroup("Patch Tree Window/Prefabs")]
        private Image dottedLinePrefab;
        [SerializeField, Required, BoxGroup("Patch Tree Window/Prefabs")]
        private GameObject tierElementPrefab;

        [FormerlySerializedAs("patchTreeElementPrefab")]
        [SerializeField, Required, BoxGroup("Patch Tree Window/Prefabs")]
        private PatchNodeElement patchNodeElementPrefab;

        private RectTransform[] _activeTiers;
        private PatchNodeElement[] _activeElements;
        private RectTransform[] _activeElementLinks;
        private RectTransform _lineContainer;
        
        //Drone Data
        //====================================================================================================================//

        [SerializeField, FoldoutGroup("Drone Window")]
        private float partImageSize = 100f;
        [SerializeField, Required, FoldoutGroup("Drone Window")]
        private RectTransform primaryPartsAreaTransform;
        [SerializeField, Required, FoldoutGroup("Drone Window")]
        private RectTransform secondaryPartsAreaTransform;
        
        [SerializeField, Required, FoldoutGroup("Drone Window")]
        private Button swapPartButton;

        private Dictionary<BIT_TYPE, Button> _primaryPartButtons;
        private Dictionary<BIT_TYPE, Button> _secondaryPartButtons;

        private List<BitData> _bitsOnDrone;
        private List<PartData> _partsOnDrone;
        private List<PartData> _partsInStorage;


        //====================================================================================================================//
        
        private (PART_TYPE partType, PatchData patchData) _selectedPatch;
        private (PART_TYPE type, bool inStorage) _selectedPart;


        //====================================================================================================================//

        private PartChoiceUI PartChoice
        {
            get
            {
                if (_partChoice == null)
                    _partChoice = FindObjectOfType<PartChoiceUI>();

                return _partChoice;
            }
        }
        private PartChoiceUI _partChoice;

        //====================================================================================================================//

        private MenuUI MenuUI
        {
            get
            {
                if (_menuUI == null)
                    _menuUI = FindObjectOfType<MenuUI>();

                return _menuUI;
            }
        }
        private MenuUI _menuUI;

        
        //====================================================================================================================//

        private Selectable _objectToSelect = null;

        //====================================================================================================================//
        
        #endregion //Properties

        //Unity Functions
        //====================================================================================================================//

        #region Unity Functions

        private bool _showingHint;
        private void IsShowingHint(bool showingHint) =>_showingHint = showingHint;

        private void OnEnable()
        {
            InputManager.OnCancelPressed += OnCancelPerformed;
            InputManager.OnPausePressed += OnPausePerformed;
            
            PlayerDataManager.NewPartPicked += OnNewPartSelected;
            PlayerDataManager.OnValuesChanged += OnValuesChanged;
            HintManager.OnShowingHintAction += IsShowingHint;

            MenuUI.OnMenuOpened += OnPauseMenuOpened;
            
            OnValuesChanged();
        }

        

        private void Start()
        {
            SetupButtons();
            SetupDroneUI();
            SetupPatchHoverUI();
            
            _partPatchOptionElements = new PartPatchUIElement[0];
        }

        private void OnDisable()
        {
            InputManager.OnCancelPressed -= OnCancelPerformed;
            InputManager.OnPausePressed -= OnPausePerformed;
            
            PlayerDataManager.NewPartPicked -= OnNewPartSelected;
            PlayerDataManager.OnValuesChanged -= OnValuesChanged;
            HintManager.OnShowingHintAction -= IsShowingHint;
            
            MenuUI.OnMenuOpened -= OnPauseMenuOpened;
        }

        


        #endregion //Unity Functions

        //IHasHintElement Functions
        //====================================================================================================================//

        #region IHasHintElement Functions

        public object[] GetHintElements(HINT hint)
        {
            switch (hint)
            {
                case HINT.LAYOUT:
                    return new object[]
                    {
                       primaryPartsAreaTransform.parent as RectTransform
                    };
                case HINT.PATCH_TREE:
                    return new object[]
                    {
                        patchTreeTierContainer,
                        partWindowContainer,
                    };
                case HINT.ENTER_WRECK:
                    return new object[]
                    {
                        new Bounds
                        {
                            center = Vector3.zero,
                            size = Vector3.zero
                        },
                        partPatchOptionsContainer,
                    };
                default:
                    throw new ArgumentOutOfRangeException(nameof(hint), hint, null);
            }
        }

        #endregion //IHasHintElement Functions

        //Setup Wreck Screen
        //====================================================================================================================//

        #region Setup Wreck Screen

        private void SetupButtons()
        {
            launchButton.onClick.AddListener(LaunchPressed);
            
            scrapPartButton.onClick.AddListener(ScrapPartPressed);
            _scrapPartButtonText = scrapPartButton.GetComponentInChildren<TMP_Text>();
            
            purchasePatchButton.onClick.AddListener(OnPurchasePatchPressed);
            
            swapPartButton.onClick.AddListener(SwapPartPressed);
        }

        private void SetupDroneUI()
        {

            //--------------------------------------------------------------------------------------------------------//
            
            Button CreatePartButton(in RectTransform container, in Vector2Int coordinate, in PART_TYPE partType, in bool storage)
            {
                var category = PlayerDataManager.GetCategoryAtCoordinate(coordinate);

                var temp = new GameObject($"{category}_Part_Icon");
                temp.AddComponent<UIElementSelectEvents>();
                var tempImage = temp.AddComponent<Image>();
                var tempButton = temp.AddComponent<Button>();
                tempImage.sprite = partType.GetSprite();

                var tempData = (partType.GetCategory(), storage);
                tempButton.onClick.AddListener(() =>
                {
                    OnPartPressed(category, tempData.storage);
                });
                
                var tempTransform = (RectTransform)temp.transform;

                tempTransform.SetParent(container, false);
                tempTransform.sizeDelta = Vector2.one * partImageSize;
                tempTransform.anchoredPosition = (Vector2)coordinate * partImageSize;

                //--------------------------------------------------------------------------------------------------------//
                
                PartAttachableFactory.CreateUIPartBorder(tempTransform, category);

                //--------------------------------------------------------------------------------------------------------//

                return tempButton;
            }

            //--------------------------------------------------------------------------------------------------------//

            var botLayout = PlayerDataManager.GetBotLayout();
            
            _primaryPartButtons = new Dictionary<BIT_TYPE, Button>();
            _secondaryPartButtons = new Dictionary<BIT_TYPE, Button>();
            
            //Setup 4 directions for Primary & Secondary
            foreach (var coordinate in botLayout)
            {
                var bitType = PlayerDataManager.GetCategoryAtCoordinate(coordinate);
                
                _primaryPartButtons.Add(bitType, CreatePartButton(primaryPartsAreaTransform, coordinate, PART_TYPE.EMPTY, false));
                
                if (coordinate == Vector2Int.zero) continue;
                
                _secondaryPartButtons.Add(bitType, CreatePartButton(secondaryPartsAreaTransform, coordinate, PART_TYPE.EMPTY, true));
            }
        }

        private void SetupPatchHoverUI()
        {
            _patchDetailsWindowTransform = (RectTransform)patchDetailsWindow.transform;
            OnPatchHovered(null, default, false);
        }

        #endregion //Setup Wreck Screen

        //Wreck Data Functions
        //====================================================================================================================//

        #region Init Wreck

        private void GenerateUIElements()
        {
            SetupPurchaseOptions(PlayerDataManager.CurrentPatchOptions);
            DrawDroneStorage();

            _objectToSelect = launchButton;
            UISelectHandler.RebuildNavigationProfile();
        }
        
        public void InitWreck(in string wreckName, in Sprite wreckSprite/*, in IEnumerable<PartData> partPatchOptions*/)
        {
            //--------------------------------------------------------------------------------------------------------//
            
            void TryShowPartChoice()
            {
                PartChoice.SetActive(PlayerDataManager.CanChoosePart);
            
                if (!PlayerDataManager.CanChoosePart) 
                    return;
            
                var notYetStarted = PlayerDataManager.HasRunStarted();

                if (!notYetStarted)
                {
                    PartChoice.Init(PartAttachableFactory.PART_OPTION_TYPE.InitialSelection);
                    PlayerDataManager.ClearAllPatches();
                }
                else
                {
                    PartChoice.Init(PartAttachableFactory.PART_OPTION_TYPE.Any);

                    if (HintManager.CanShowHint(HINT.PICK_PART))
                        HintManager.TryShowHint(HINT.PICK_PART, 1f,null);
                }
            }

            //--------------------------------------------------------------------------------------------------------//
            
            GameTimer.SetPaused(true);
            GameManager.SetCurrentGameState(GameState.Wreckyard);
            //PlayerDataManager.DowngradeAllBits(1, false);

            //--------------------------------------------------------------------------------------------------------//
            
            //Clean up anything before we show new data
            CleanPatchTreeUI();
            TryShowPartChoice();

            wreckNameText.text = wreckName;
            wreckImage.sprite = wreckSprite;

            if (PlayerDataManager.CanChoosePart) 
                return;

            UISelectHandler.SetBuildTarget(this, false);
            GenerateUIElements();
        }

        private void OnNewPartSelected(PartAttachableFactory.PART_OPTION_TYPE optionType, PART_TYPE partType)
        {
            void LaunchPressedHandler()
            {
                LaunchPressed();
                HintManager.onEndingHintAction -= LaunchPressedHandler;
            }
            switch (optionType)
            {
                case PartAttachableFactory.PART_OPTION_TYPE.InitialSelection:
                    if (HintManager.CanShowHint(HINT.LAYOUT))
                    {
                        HintManager.onEndingHintAction += LaunchPressedHandler;
                        HintManager.TryShowHint(HINT.LAYOUT);
                    }
                    baseBackground.SetActive(true);
                    break;
                case PartAttachableFactory.PART_OPTION_TYPE.Any:
                    if (HintManager.CanShowHint(HINT.ENTER_WRECK)) HintManager.TryShowHint(HINT.ENTER_WRECK);
                    PlayerDataManager.GeneratePartPatchOptions();
                    baseBackground.SetActive(false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(optionType), optionType, null);
            }

            CheckForPartPositionAvailability();


            UISelectHandler.SetBuildTarget(this, false);
            GenerateUIElements();
        }

        #endregion //Init Wreck
        
        //Patch Tree Functions
        //====================================================================================================================//

        #region Patch Tree Functions

        private void GeneratePatchTree(in PartData partData)
        {
            StartCoroutine(GeneratePatchTreeCoroutine(partData));
        }
        
        private IEnumerator GeneratePatchTreeCoroutine(PartData partData)
        {
            //Instantiate Functions
            //--------------------------------------------------------------------------------------------------------//

            RectTransform CreateTierElement()
            {
                var temp = Instantiate(tierElementPrefab, patchTreeTierContainer, false).transform;
                temp.SetSiblingIndex(1);

                return (RectTransform) temp;
            }

            PatchNodeElement CreatePartNodeElement(in RectTransform container, in PART_TYPE type)
            {
                var temp = Instantiate(patchNodeElementPrefab, container, false);
                temp.Init(type);
                //TODO Fill with patchData

                PartAttachableFactory.CreateUIPartBorder(temp.transform, type);
                
                return temp;
            }

            PatchNodeElement CreatePatchNodeElement(in RectTransform container, in PART_TYPE type, 
                in PatchData patchData,
                in bool purchased,
                in bool unlocked)
            {
                var temp = Instantiate(patchNodeElementPrefab, container, false);
                temp.Init(type, patchData, purchased, unlocked, OnPatchPressed, OnPatchHovered);

                return temp;
            }

            RectTransform CreateUILine(in PatchNodeElement startElement, in PatchNodeElement endElement)
            {
                bool ShouldDrawDashedLine(in PartData currentPart, in PatchData patchToCheck)
                {
                    //Determine if patch has been purchased
                    return !currentPart.Patches.Contains(patchToCheck);
                }
                
                if (_lineContainer == null)
                {
                    var temp = new GameObject("Line Container");
                    var layoutElement = temp.gameObject.AddComponent<LayoutElement>();
                    layoutElement.ignoreLayout = true;

                    _lineContainer = (RectTransform) temp.transform;
                    _lineContainer.SetParent(patchTreeTierContainer, false);
                    _lineContainer.SetSiblingIndex(0);
                }

                var drawDottedLine = ShouldDrawDashedLine(partData, endElement.patchData);

                var image = !drawDottedLine
                    ? UILineCreator.DrawConnection(_lineContainer, startElement.transform, endElement.transform,
                        Color.white)
                    : UILineCreator.DrawConnection(_lineContainer, startElement.transform, endElement.transform,
                        dottedLinePrefab, Color.grey);
                

                return image.transform as RectTransform;
            }

            (bool hasPurchased, bool hasUnlocked) HasUnlockedPatch(in List<PatchNodeJson> patchTree, in PartData currentPart, in PatchData patchToCheck)
            {
                var type = currentPart.Type;
                
                //Determine if patch has been purchased
                if (currentPart.Patches.Contains(patchToCheck)) return (true, true);

                var options = PlayerDataManager.CurrentPatchOptions;
                if (options.IsNullOrEmpty()) return (false, false);

                var part = options.FirstOrDefault(x => x.Type == type);
                if (part.Patches.IsNullOrEmpty()) return (false, false);
                
                //Determine if patch can be purchased
                var canPurchase = part.Patches.Contains(patchToCheck);

                return (false, canPurchase);
                //TODO Determine if patch is missing pre-reqs
            }
            

            //--------------------------------------------------------------------------------------------------------//

            var partType = (PART_TYPE) partData.Type;
            CleanPatchTree();

            yield return null;

            var patchTreeData = partType.GetPatchTree();
            if(patchTreeData.IsNullOrEmpty()) yield break;
            
            var maxTier = patchTreeData.Max(x => x.Tier);

            //Add one to account for the part Tier
            _activeTiers = new RectTransform[maxTier + 1];
            //Add one to account for the part Element
            _activeElements = new PatchNodeElement[patchTreeData.Count + 1];

            //Create the base Part Tier
            _activeTiers[0] = CreateTierElement();
            _activeElements[0] = CreatePartNodeElement(_activeTiers[0], partType);

            //Create the remaining upgrade Tiers
            for (var i = 1; i <= maxTier; i++)
            {
                _activeTiers[i] = CreateTierElement();
            }

            //Populate with Elements
            for (int i = 0; i < patchTreeData.Count; i++)
            {
                var patchData = new PatchData
                {
                    Type = patchTreeData[i].Type,
                    Level = patchTreeData[i].Level
                };
                var (hasPurchased, hasUnlocked) = HasUnlockedPatch(patchTreeData, partData, patchData);
                

                var tier = patchTreeData[i].Tier;
                _activeElements[i + 1] = CreatePatchNodeElement(_activeTiers[tier], partType, patchData, hasPurchased, hasUnlocked);

            }

            //--------------------------------------------------------------------------------------------------------//

            LayoutRebuilder.ForceRebuildLayoutImmediate(patchTreeTierContainer);

            //Wait one frame while elements reposition before drawing the lines
            yield return null;

            //Lines
            //--------------------------------------------------------------------------------------------------------//

            var activeLinks = new List<RectTransform>();
            //Connect Elements
            for (var i = 0; i < patchTreeData.Count; i++)
            {
                var endIndex = i + 1;
                var endElement = _activeElements[endIndex];
                
                //var patchNode = patchTreeData[i];
                var links = patchTreeData[i].PreReqs;

                if (links.IsNullOrEmpty())
                {
                    //Create a line between this node and the part
                    activeLinks.Add(CreateUILine(_activeElements[0], endElement));
                    continue;
                }

                for (var j = 0; j < links.Length; j++)
                {
                    var startIndex = links[j] + 1;
                    var startElement = _activeElements[startIndex];

                    activeLinks.Add(CreateUILine(startElement, endElement));
                }

            }

            _activeElementLinks = activeLinks.ToArray();

            //--------------------------------------------------------------------------------------------------------//
            
            _objectToSelect = UISelectHandler.CurrentlySelected;
            UISelectHandler.RebuildNavigationProfile();
        }

        private void CleanPatchTree()
        {
            if (_activeTiers.IsNullOrEmpty())
                return;

            for (var i = _activeTiers.Length - 1; i >= 0; i--)
            {
                if(_activeTiers[i].gameObject) Destroy(_activeTiers[i].gameObject);
            }
            
            for (var i = _activeElementLinks.Length - 1; i >= 0; i--)
            {
                if(_activeElementLinks[i].gameObject) Destroy(_activeElementLinks[i].gameObject);
            }

            _activeTiers = new RectTransform[0];
            _activeElementLinks = new RectTransform[0];
        }

        #endregion //Patch Tree Functions

        //Patch Purchase Functions
        //====================================================================================================================//

        #region Patch Purchase Functions

        private void SetupPurchaseOptions(in IEnumerable<PartData> partPatchOptions)
        {

            //--------------------------------------------------------------------------------------------------------//
            
            PartPatchUIElement CreatePartNodeElement(in RectTransform container, in PartData partData)
            {
                var temp = Instantiate(partPatchOptionPrefab, container, false);
                temp.Init(partData, OnPartPressed, OnPatchPressed, OnPatchHovered);

                return temp;
            }

            //--------------------------------------------------------------------------------------------------------//

            if (partPatchOptions.IsNullOrEmpty()) 
                return;
            
            var optionsArray = partPatchOptions.ToArray();
            _partPatchOptionElements = new PartPatchUIElement[optionsArray.Length];
            for (var i = 0; i < optionsArray.Length; i++)
            {
                var partOption = optionsArray[i];
                _partPatchOptionElements[i] = CreatePartNodeElement(partPatchOptionsContainer, partOption);
            }
        }
        
        

        private void CleanPurchaseOptions()
        {
            if (_partPatchOptionElements.IsNullOrEmpty())
                return;

            for (var i = _partPatchOptionElements.Length - 1; i >= 0; i--)
            {
                Destroy(_partPatchOptionElements[i].gameObject);
            }

            _partPatchOptionElements = new PartPatchUIElement[0];
        }

        #endregion //Patch Purchase Functions

        //Drone Functions
        //====================================================================================================================//

        #region Drone Functions

        private void DrawDroneStorage()
        {
            //Reset the drone Images
            //--------------------------------------------------------------------------------------------------------//
            
            var emptyPartSprite = PART_TYPE.EMPTY.GetSprite();
            foreach (var partImage in _primaryPartButtons)
            {
                partImage.Value.image.sprite = emptyPartSprite;
            }
            foreach (var partImage in _secondaryPartButtons)
            {
                partImage.Value.image.sprite = emptyPartSprite;
            }

            //--------------------------------------------------------------------------------------------------------//
            
            
            if(_partsOnDrone.IsNullOrEmpty()) return;
            
            foreach (var partData in _partsOnDrone)
            {
                var partType = (PART_TYPE) partData.Type;
                if (partType == PART_TYPE.EMPTY) continue;
                
                var category = partType.GetCategory();
                _primaryPartButtons[category].image.sprite = partType.GetSprite();
            }
            
            //Get Parts in Storage
            if(_partsInStorage.IsNullOrEmpty()) return;
            //_secondaryPartImages
            foreach (var partData in _partsInStorage)
            {
                var partType = (PART_TYPE) partData.Type;
                if (partType == PART_TYPE.EMPTY) continue;
                
                var category = partType.GetCategory();
                //We cannot currently swap out the core
                if (category == BIT_TYPE.GREEN) continue;
                _secondaryPartButtons[category].image.sprite = partType.GetSprite();
            }


        }

        public void SetPrimaryPart(in PART_TYPE partType) => SetPartImage(_primaryPartButtons, partType);

        public void SetSecondaryPart(in PART_TYPE partType) => SetPartImage(_secondaryPartButtons, partType);

        private static void SetPartImage(in Dictionary<BIT_TYPE, Button> partImages, in PART_TYPE partType)
        {
            var category = partType.GetCategory();
            partImages[category].image.sprite = partType.GetSprite();
        }

        #endregion //Drone Functions

        //On Button Pressed Functions
        //====================================================================================================================//

        #region On Button Pressed Functions

        private void LaunchPressed()
        {
            if (_showingHint)
                return;
            
            ScreenFade.Fade(() =>
            {
                SceneLoader.ActivateScene(SceneLoader.UNIVERSE_MAP, SceneLoader.WRECKYARD);
                AnalyticsManager.WreckEndEvent(AnalyticsManager.REASON.LEAVE);
            });

            if (HintManager.CanShowHint(HINT.MAP))
            {
                ScreenFade.WaitForFade(() =>
                {
                    HintManager.TryShowHint(HINT.MAP);
                });
            }
        }

        private void ScrapPartPressed()
        {
            if (_showingHint)
                return;
            //--------------------------------------------------------------------------------------------------------//

            void ScrapPart()
            {
                var partTypeInt = (int) _selectedPart.type;
                //Find the part on Bot/in Storage & Destroy Part
                if (_selectedPart.inStorage)
                {
                    var index = _partsInStorage.FindIndex(x => x.Type == partTypeInt);
                   
                    var partData = _partsInStorage[index];
                    partData.Type = (int)PART_TYPE.EMPTY;
                    partData.Patches = new List<PatchData>();
                    _partsInStorage[index] = partData;
                }
                else
                {
                    var index = _partsOnDrone.FindIndex(x => x.Type == partTypeInt);
                    
                    var partData = _partsOnDrone[index];
                    partData.Type = (int)PART_TYPE.EMPTY;
                    partData.Patches = new List<PatchData>();
                    _partsOnDrone[index] = partData;
                    
                    //If i have a part available in storage I should attempt to auto equip it
                }
                //Add currency to player account
                PlayerDataManager.AddGears(PART_SCRAP_VALUE, false);
                
                //If the player destroyed something on the Drone, try and add something from storage
                if(!_selectedPart.inStorage)
                    CheckForPartPositionAvailability(false);
                
                SetSelectedPart(PART_TYPE.EMPTY, false);
                SetPartText(NO_PART_TEXT, string.Empty, string.Empty);
                CleanPatchTree();
                
                //Check if there are any Patches available for the part just scrapped. Remove & Update the UI
                if (PlayerDataManager.CurrentPatchOptions.Any(x => x.Type == partTypeInt))
                {
                    PlayerDataManager.RemovePartPatchOption((PART_TYPE)partTypeInt);
                    CleanPurchaseOptions();
                    SetupPurchaseOptions(PlayerDataManager.CurrentPatchOptions);
                }

                
                //Update Values
                SaveBlockData();
                
                
            }

            //--------------------------------------------------------------------------------------------------------//
            
            //TODO Determine the value of the part
            var partName = _selectedPart.type.GetRemoteData().name;
            
            Alert.ShowAlert($"Scrap {partName}", 
                $"Are you sure you want to scrap {partName} for {PART_SCRAP_VALUE}{TMP_SpriteHelper.GEAR_ICON}?",
                "Scrap", "Cancel",
                answer =>
                {
                    if (!answer)
                    {
                        _objectToSelect = null;
                        UISelectHandler.SetBuildTarget(this);
                        return;
                    }

                    ScrapPart();
                    _objectToSelect = default;
                    UISelectHandler.SetBuildTarget(this);
                });
        }

        private void SwapPartPressed()
        {

            if (_showingHint)
                return;
            //--------------------------------------------------------------------------------------------------------//

            void ListShuffles(in int targetPartIndex, ref List<PartData> fromList, ref List<PartData> toList)
            {
                var category = _selectedPart.type.GetCategory();
                var coordinate = PlayerDataManager.GetCoordinateForCategory(category);

                //Get the mirrored part on the bot
                var otherPartIndex = toList.FindIndex(x => ((PART_TYPE) x.Type).GetCategory() == category);
                var targetPartData = fromList[targetPartIndex];
                var otherPartData = toList[otherPartIndex];
                //var tempContainer = targetPartData;

                targetPartData.Coordinate = coordinate;
                otherPartData.Coordinate = coordinate;

                //Store that part but also remove it from that list
                toList[otherPartIndex] = targetPartData;
                
                //Add the new part into the list
                fromList[targetPartIndex] = otherPartData;
                //fromList.Remove(_partData);

                //Add old part in old list
                //fromList.Add(otherPartData);
            }

            //--------------------------------------------------------------------------------------------------------//
            
            if (_selectedPart.type == PART_TYPE.EMPTY) throw new Exception();

            var (index, inStorage) = FindSelectedPartIndex(_selectedPart.type);

            if (inStorage)
            {
                ListShuffles(index, ref _partsInStorage, ref _partsOnDrone);
                _selectedPart.inStorage = false;
            }
            else
            {
                ListShuffles(index, ref _partsOnDrone, ref _partsInStorage);
                _selectedPart.inStorage = true;
            }

            SaveBlockData();
        }

        /// <summary>
        /// Callback alternative for the Part Patch Options elements, where they do not know the location of the part.
        /// </summary>
        /// <param name="partType"></param>
        /// <exception cref="Exception"></exception>
        private void OnPartPressed(PART_TYPE partType)
        {
            if (_showingHint)
                return;

            //Determine if the part is in storage or on the bot
            if (partType == PART_TYPE.EMPTY)
                throw new Exception();

            var category = partType.GetCategory();
            var temp = (int)partType;

            if (_partsOnDrone.Any(x => x.Type == temp))
                OnPartPressed(category, false);
            else if (_partsInStorage.Any(x => x.Type == temp))
                OnPartPressed(category, true);
            else
                throw new Exception();
        }
        private void OnPartPressed(in BIT_TYPE category, in bool inStorage)
        {
            if (_showingHint)
                return;

            SetSelectedPatch(PART_TYPE.EMPTY, default);
            
            var cat = category;
            var partType = PART_TYPE.EMPTY;
            

            var partData = inStorage
                ? _partsInStorage.FirstOrDefault(x => ((PART_TYPE) x.Type).GetCategory() == cat)
                : _partsOnDrone.FirstOrDefault(x => ((PART_TYPE) x.Type).GetCategory() == cat);

            //If the player hasn't actually selected the core, and the default value is providing a false positive, check
            if ((PART_TYPE) partData.Type == PART_TYPE.CORE && category == PART_TYPE.CORE.GetCategory() ||
                (PART_TYPE) partData.Type != PART_TYPE.CORE)
                partType = (PART_TYPE) partData.Type;

            if (partType == PART_TYPE.EMPTY)
            {
                SetSelectedPart(PART_TYPE.EMPTY, default);
                SetPartText(NO_PART_TEXT, string.Empty,string.Empty);
                CleanPatchTree();
                return;
            }

            if (HintManager.CanShowHint(HINT.PATCH_TREE)) HintManager.TryShowHint(HINT.PATCH_TREE);

            //Show this part on the PatchTree
            GeneratePatchTree(partData);

            SetSelectedPart(partType, inStorage);
            _scrapPartButtonText.text = $"Scrap Part {PART_SCRAP_VALUE}{TMP_SpriteHelper.GEAR_ICON}";

            SetPartText(partType.GetRemoteData().name, partType.GetRemoteData().description, partData.GetPartDetails());
        }

        private void OnPatchPressed(PART_TYPE partType, PatchData patchData)
        {
            if (_showingHint)
                return;
            
            //Present the purchase option window to the player
            SetSelectedPatch(partType, patchData);
            //Ensure that when a loose patch is selected, that we change which part its selected for
            SetSelectedPart(partType, FindSelectedPart(partType).inStorage);
            
            
            //TODO Highlight the patch selected on tree & on patch purchase 
            
            
            
            //Preview the changes for purchasing this patch
            var partData = FindSelectedPart(partType).partData;
            var partRemoteData = partType.GetRemoteData();
            
            SetPartText(partRemoteData.name,
                partRemoteData.description,
                partData.GetPartDetailsPatchPreview(partRemoteData, patchData));
            
            _objectToSelect = purchasePatchButton.interactable ? purchasePatchButton : UISelectHandler.CurrentlySelected;
            UISelectHandler.RebuildNavigationProfile();


            if (HintManager.CanShowHint(HINT.PATCH_TREE)) HintManager.TryShowHint(HINT.PATCH_TREE);
        }

        private void OnPurchasePatchPressed()
        {
            if (_showingHint)
                return;

            //Get the price of Patch
            var (gears, silver) = _selectedPatch.patchData.GetPatchCost();
            //Check that the player can afford the patch
            if (!PlayerDataManager.CanAfford(gears, silver))
                throw new Exception();
            
            //Remove the cost of Patch
            PlayerDataManager.PurchaseItem(gears, silver, false);
            
            //Add the patch to the Selected Part
            var (index, inStorage) = FindSelectedPartIndex(_selectedPatch.partType);
            
            if(inStorage)
                _partsInStorage[index].Patches.Add(_selectedPatch.patchData);
            else
                _partsOnDrone[index].Patches.Add(_selectedPatch.patchData);

            SessionDataProcessor.Instance.RecordPatchPurchase(new PartData
            {
                Type = (int)_selectedPatch.partType,
                Patches = new List<PatchData>
                {
                    _selectedPatch.patchData
                }
            });
                
            //Remove the PartPatchOption
            PlayerDataManager.RemovePartPatchOption(_selectedPatch.partType);

            //Redraw the patch tree without the purchased patch
            CleanPatchUI();
            var partData = FindSelectedPart(_selectedPart.type).partData;
            GeneratePatchTree(partData);

            SetupPurchaseOptions(PlayerDataManager.CurrentPatchOptions);

            //Update the Datas
            SaveBlockData();
            
            _objectToSelect = null;
            UISelectHandler.RebuildNavigationProfile();
        }

        #endregion //On Button Pressed Functions


        //IBuildNavigationProfile Functions
        //====================================================================================================================//
        
        public NavigationProfile BuildNavigationProfile()
        {
            var overrides = new List<NavigationOverride>();
            var exceptions = new List<NavigationRestriction>();
            
            //Base selectable Items
            //--------------------------------------------------------------------------------------------------------//

            var selectables = new List<Selectable>
            {
                launchButton,
                menuButton,

                swapPartButton,
                purchasePatchButton,
                
                scrapPartButton,
                repairButton
            };

            //--------------------------------------------------------------------------------------------------------//
            
            var leftMostBitType = PlayerDataManager.GetCategoryAtCoordinate(Vector2Int.left);

            var patchTreeSelectables = patchTreeTierContainer.GetComponentsInChildren<Selectable>();
            var firstPatchTreeSelectable =
                patchTreeSelectables.FirstOrDefault(x => x.enabled && x.interactable && x.gameObject.activeInHierarchy);
            
            //Part Patch Options
            //--------------------------------------------------------------------------------------------------------//

            //Get list of navigation options 
            foreach (var partPatchUIElement in _partPatchOptionElements)
            {
                var temp = partPatchUIElement.Selectables;
                selectables.AddRange(temp.Selectables);

                //Force navigation from right most element to scrapPartButton
                overrides.Add(new NavigationOverride
                {
                    FromSelectable = temp.RightMostSelectable,
                    RightTarget = firstPatchTreeSelectable
                        ? firstPatchTreeSelectable
                        : _secondaryPartButtons[leftMostBitType]
                });
            }

            //Drone Layout
            //--------------------------------------------------------------------------------------------------------//
            var firstPartPatchOption = _partPatchOptionElements.FirstOrDefault()?.Selectables.RightMostSelectable;

            //FIXME I think I should be able to combine these to loops
            foreach (var partButton in _primaryPartButtons)
            {
                selectables.Add(partButton.Value);
                
                if (partButton.Key != leftMostBitType)
                    continue;

                overrides.Add(new NavigationOverride
                {
                    FromSelectable = partButton.Value,
                    LeftTarget = firstPatchTreeSelectable
                        ? firstPatchTreeSelectable
                        : firstPartPatchOption
                });
            }
            
            foreach (var partButton in _secondaryPartButtons)
            {
                selectables.Add(partButton.Value);
                
                if (partButton.Key != leftMostBitType)
                    continue;

                overrides.Add(new NavigationOverride
                {
                    FromSelectable = partButton.Value,
                    LeftTarget = scrapPartButton.gameObject.activeInHierarchy
                        ? scrapPartButton
                        : firstPartPatchOption
                });
            }

            //--------------------------------------------------------------------------------------------------------//
            
            selectables.AddRange(patchTreeSelectables);
            
            //PurchasePatch Button
            //--------------------------------------------------------------------------------------------------------//
            
            overrides.Add(new NavigationOverride
            {
                FromSelectable = purchasePatchButton,
                LeftTarget = purchasePatchButton,
                UpTarget = purchasePatchButton,
                RightTarget = firstPatchTreeSelectable ? firstPatchTreeSelectable : null
            });

            //Menu & Launch Button exceptions
            //--------------------------------------------------------------------------------------------------------//
            
            exceptions.Add(new NavigationRestriction
            {
                Selectable = menuButton,
                FromDirection = NavigationRestriction.DIRECTION.RIGHT
            });
            exceptions.Add(new NavigationRestriction
            {
                Selectable = launchButton,
                FromDirection = NavigationRestriction.DIRECTION.RIGHT
            });

            return new NavigationProfile(_objectToSelect, selectables, overrides, exceptions);
        }

        //Extra Functions
        //====================================================================================================================//

        #region Extra Functions

        private bool PartCanBeSwapped(in PART_TYPE partType)
        {
            var category = partType.GetCategory();


            return _partsInStorage.Any(x => ((PART_TYPE) x.Type).GetCategory() == category) &&
                _partsOnDrone.Any(x => ((PART_TYPE) x.Type).GetCategory() == category);
        }

        /// <summary>
        /// Clear both the selected patch and and part. 
        /// </summary>
        private void CleanPatchTreeUI()
        {
            CleanPartUI();
            CleanPatchUI();
        }

        /// <summary>
        /// Clears specifically just the part data UI
        /// </summary>
        private void CleanPartUI()
        {
            //Deselect part
            SetSelectedPart(PART_TYPE.EMPTY, false);
            //Clear Text
            SetPartText(NO_PART_TEXT, string.Empty, string.Empty);

        }

        /// <summary>
        /// Clears specifically just the patch data
        /// </summary>
        private void CleanPatchUI()
        {
            //Deselect Patch
            SetSelectedPatch(PART_TYPE.EMPTY, default);
            //Clear Text
            SetPatchText(string.Empty, string.Empty);
            //Clean Patch Tree
            CleanPatchTree();
            //Clean the Patch Purchase Options
            CleanPurchaseOptions();
            SetupPatchHoverUI();
        }

        private void SetSelectedPart(in PART_TYPE partType, in bool inStorage)
        {
            _selectedPart = (partType, inStorage);

            var partIsEmpty = partType == PART_TYPE.EMPTY;
            //Check if part can be swapped, show button if true
            var canSwap = !partIsEmpty && PartCanBeSwapped(partType);
            
            swapPartButton.gameObject.SetActive(canSwap);
            //Can't scrap empty parts nor the Core
            scrapPartButton.gameObject.SetActive(partType != PART_TYPE.CORE && !partIsEmpty);
        }

        private void SetPartText(in string partName, in string partDescription, in string partDetails)
        {
            partNameText.text = partName;
            partDescriptionText.text = partDescription;
            partDetailsText.text = partDetails;
        }


        private void SetSelectedPatch(in PART_TYPE partType, in PatchData patchData)
        {
            _selectedPatch = (partType, patchData);
            
            var partIsEmpty = partType == PART_TYPE.EMPTY;
            purchasePatchWindow.SetActive(!partIsEmpty);

            if (partIsEmpty)
            {
                SetPatchText(string.Empty, string.Empty);
                return;
            }

            var patchRemoteData = FactoryManager.Instance.PatchRemoteData.GetRemoteData(patchData.Type);

            var (gears, silver) = patchData.GetPatchCost();

            //Determine whether or not the Patch is affordable
            var canAffordPatch = PlayerDataManager.CanAfford(gears, silver);
            
            //Set the purchase button active from there
            purchasePatchButton.interactable = canAffordPatch;

            SetPatchText(
                $"{patchRemoteData.name} {Mathfx.ToRoman(patchData.Level + 1)} - " +
                $"{gears}{TMP_SpriteHelper.GEAR_ICON}" +
                $"{(silver > 0 ? $" {silver}{TMP_SpriteHelper.SILVER_ICON}" : string.Empty)}",
                patchRemoteData.description);

            if (_selectedPart.type == partType)
                return;
            
            var partData = FindSelectedPart(partType).partData;
            GeneratePatchTree(partData);
        }

        private void SetPatchText(in string patchName, in string patchDetails)
        {
            patchNameText.text = patchName;
            patchDetailsText.text = patchDetails;
        }

        private (PartData partData, bool inStorage) FindSelectedPart(in PART_TYPE partType)
        {
            //Determine if the part is in storage or on the bot
            if (partType == PART_TYPE.EMPTY)
                throw new Exception();

            var temp = (int)partType;

            var index = _partsOnDrone.FindIndex(x => x.Type == temp);

            //Check the drone parts first
            if (index >= 0) return (_partsOnDrone[index], false);
            
            index = _partsInStorage.FindIndex(x => x.Type == temp);
            
            //Then check the parts in storage
            if (index >= 0) return (_partsInStorage[index], true);

            //If we couldn't find parts, something is wrong
            throw new Exception();
        }
        
        private (int index, bool inStorage) FindSelectedPartIndex(in PART_TYPE partType)
        {
            //Determine if the part is in storage or on the bot
            if (partType == PART_TYPE.EMPTY)
                throw new Exception();

            var temp = (int)partType;

            var index = _partsOnDrone.FindIndex(x => x.Type == temp);

            //Check the drone parts first
            if (index >= 0) return (index, false);
            
            index = _partsInStorage.FindIndex(x => x.Type == temp);
            
            //Then check the parts in storage
            if (index >= 0) return (index, true);

            //If we couldn't find parts, something is wrong
            throw new Exception();
        }
        private void OnPatchHovered(RectTransform rectTransform, PatchData patchData, bool hovering)
        {
            //If hovering false, close window, clean elements, return
            //--------------------------------------------------------------------------------------------------------//
            
            patchDetailsWindow.gameObject.SetActive(hovering);

            if (!hovering) return;

            if (patchData.PatchExists() == false)
            {
                patchDetailsWindow.gameObject.SetActive(false);
                return;
            }
            
            var canvasRect = GetComponentInParent<Canvas>().transform as RectTransform;
            var screenPoint = RectTransformUtility.WorldToScreenPoint(null,
                (Vector2) rectTransform.position + Vector2.right * rectTransform.sizeDelta.x);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, null,
                out var localPoint);

            _patchDetailsWindowTransform.anchoredPosition = localPoint;

            //====================================================================================================================//

            var patchType = (PATCH_TYPE) patchData.Type;
            var patchRemote = FactoryManager.Instance.PatchRemoteData.GetRemoteData(patchType);
            var (gears, silver) = patchData.GetPatchCost();

            //====================================================================================================================//

            patchHoverTitleText.text = $"{patchRemote.name} {Mathfx.ToRoman(patchData.Level + 1)}";
            patchHoverDetailsText.text = $"{patchRemote.description}\n" +
                                         $"{gears}{TMP_SpriteHelper.GEAR_ICON}" +
                                         $"{(silver > 0 ? $" {silver}{TMP_SpriteHelper.SILVER_ICON}":string.Empty)}";
            
            //====================================================================================================================//

            _patchDetailsWindowTransform.TryFitInScreenBounds(canvasRect, 20f);
        }

        //====================================================================================================================//
        
        private void OnCancelPerformed()
        {
            //When patch window is open, close. return.
            if (_selectedPatch.partType != PART_TYPE.EMPTY)
            {
                SetSelectedPatch(PART_TYPE.EMPTY, default);
                
                _objectToSelect = launchButton;
                UISelectHandler.RebuildNavigationProfile();
                return;
            }
            //When Part Window is open, close. return;
            if (_selectedPart.type != PART_TYPE.EMPTY)
            {
                SetSelectedPart(PART_TYPE.EMPTY, default);
                SetPartText(NO_PART_TEXT, string.Empty, string.Empty);
                CleanPatchTree();
                OnPatchHovered(null, default, false);
                
                _objectToSelect = launchButton;
                UISelectHandler.RebuildNavigationProfile();
                return;
            }


            if (InputManager.Instance.UsingController) 
                return;

            MenuUI.OpenMenu();
        }

        private void OnPausePerformed()
        {
            if (!InputManager.Instance.UsingController)
                return;
            
            MenuUI.OpenMenu();
        }
        
        private void OnPauseMenuOpened(bool opened)
        {
            if (opened || SceneLoader.CurrentScene != SceneLoader.WRECKYARD)
                return;
            
            _objectToSelect = launchButton;
            UISelectHandler.SetBuildTarget(this);
        }

        #endregion //Extra Functions

        //Data Functions
        //====================================================================================================================//
        
        #region Data Functions

        private void CheckForPartPositionAvailability(in bool updateValues = true)
        {
            bool changesMade = false;
            
            foreach (var bitType in Constants.BIT_ORDER)
            {
                if(bitType == BIT_TYPE.GREEN) continue;
                
                //Check if the drone has a part with category bitType
                if(_partsOnDrone.Any(x => ((PART_TYPE)x.Type).GetCategory() == bitType)) continue;
                
                //Check if there is an option in storage that could be equipped
                if(_partsInStorage.All(x => ((PART_TYPE) x.Type).GetCategory() != bitType)) continue;
                
                var partIndex = _partsInStorage.FindIndex(x => ((PART_TYPE)x.Type).GetCategory() == bitType);
                
                var partData = _partsInStorage[partIndex];
                _partsInStorage.RemoveAt(partIndex);

                partData.Coordinate = PlayerDataManager.GetCoordinateForCategory(bitType);

                var droneIndex = _partsOnDrone.FindIndex(x => x.Coordinate == partData.Coordinate);
                _partsOnDrone[droneIndex] = partData;
                changesMade = true;
            }

            if (changesMade && updateValues)
            {
                SaveBlockData();
            }
        }

        private void UpdateBlockData()
        {
            var droneBlockData = PlayerDataManager.GetBotBlockDatas();

            if (droneBlockData.IsNullOrEmpty()) return;
            
            _bitsOnDrone = new List<BitData>(droneBlockData.OfType<BitData>());
            _partsOnDrone = new List<PartData>(droneBlockData.OfType<PartData>());
            _partsInStorage = new List<PartData>(PlayerDataManager.GetCurrentPartsInStorage().OfType<PartData>());

            DrawDroneStorage();
        }

        private void SaveBlockData()
        {
            var droneBlockData = new List<IBlockData>(_partsOnDrone.OfType<IBlockData>());
            droneBlockData.AddRange(_bitsOnDrone.OfType<IBlockData>());

            PlayerDataManager.SetDroneBlockData(droneBlockData);
            PlayerDataManager.SetCurrentPartsInStorage(_partsInStorage.OfType<IBlockData>());
            PlayerDataManager.SavePlayerAccountData();
            PlayerDataManager.OnValuesChanged?.Invoke();
        }
        
        private void OnValuesChanged()
        {
            currenciesText.text =
                $"{TMP_SpriteHelper.SILVER_ICON} {PlayerDataManager.GetSilver()} " +
                $"{TMP_SpriteHelper.GEAR_ICON} {PlayerDataManager.GetGears()}";
            
            //TODO Need to update the parts storage here
            UpdateBlockData();
        }

        #endregion //Data Functions

        public void OnConsolePartAdded() => CheckForPartPositionAvailability();

        //Unity Editor
        //====================================================================================================================//

        #region Unity Editor

#if UNITY_EDITOR

        /*[Button, DisableInEditorMode]
        private void TestPartPatchOptions()
        {
            var partDatas = new[]
            {
                new PartData
                {
                    Type = (int)PART_TYPE.GUN,
                    Patches = new List<PatchData>
                    {
                        new PatchData {Type = (int)PATCH_TYPE.POWER, Level = 0},
                        new PatchData {Type = (int)PATCH_TYPE.EFFICIENCY, Level = 0},
                        new PatchData {Type = (int)PATCH_TYPE.FIRE_RATE, Level = 0},
                        new PatchData {Type = (int)PATCH_TYPE.RANGE, Level = 0},
                    }
                },
                new PartData
                {
                    Type = (int)PART_TYPE.CORE,
                    Patches = new List<PatchData>
                    {
                        new PatchData {Type = (int)PATCH_TYPE.EFFICIENCY, Level = 0},
                    }
                },
                new PartData
                {
                    Type = (int)PART_TYPE.GRENADE,
                    Patches = new List<PatchData>
                    {
                        new PatchData {Type = (int)PATCH_TYPE.POWER, Level = 0},
                        new PatchData {Type = (int)PATCH_TYPE.EFFICIENCY, Level = 0},
                        new PatchData {Type = (int)PATCH_TYPE.FIRE_RATE, Level = 0},
                    }
                },
            };

            InitWreck("Test Wreck", null, partDatas);
        }*/

        [Button, DisableInEditorMode]
        private void TestGunPatchTree()
        {
            var gunPartData = new PartData
            {
                Type = (int) PART_TYPE.GUN,
                Coordinate = Vector2Int.zero,
                Patches = new List<PatchData>
                {
                    new PatchData {Type = (int) PATCH_TYPE.POWER, Level = 0}
                }
            };

            GeneratePatchTree(gunPartData);
        }

#endif

        #endregion //Unity Editor

        //====================================================================================================================//


        public void Activate()
        {
            UISelectHandler.SetBuildTarget(this);
        }

        public void Reset()
        {
        }
    }
}
