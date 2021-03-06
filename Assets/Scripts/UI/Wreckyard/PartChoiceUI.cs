﻿using System;
using StarSalvager.Factories;
using StarSalvager.Utilities.JsonDataTypes;
using StarSalvager.Utilities.Saving;
using StarSalvager.Values;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using StarSalvager.Utilities;
using StarSalvager.Utilities.Analytics.SessionTracking;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.Helpers;
using StarSalvager.Utilities.Inputs;
using StarSalvager.Utilities.Interfaces;
using StarSalvager.Utilities.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Console = System.Console;
using Random = UnityEngine.Random;
using StarSalvager.UI.Hints;

namespace StarSalvager.UI.Wreckyard
{
    public class PartChoiceUI : MonoBehaviour, IBuildNavigationProfile
    {
        [Serializable]
        public struct PartSelectionUI
        {
            public Button optionButton;
            public PartChoiceButtonHover PartChoiceButtonHover;
            public Image optionImage;
            [HideInInspector] public Image partBorderImage;
            public TMP_Text optionText;
            
            public Image categoryImage;
            public TMP_Text categoryText;
        }

        //Properties
        //====================================================================================================================//

        #region Properties

        public static PART_TYPE LastPicked { get; private set; }

        [SerializeField, Required]
        private TMP_Text titleText;
        [SerializeField]
        private GameObject partChoiceWindow;

        [SerializeField] private Button noPartSelectedOptionButton;
        private TMP_Text _noPartButtonText;

        /*[SerializeField]
        private DroneDesigner _droneDesigner;*/

        [SerializeField]
        private PartSelectionUI[] selectionUis;

        private PART_TYPE[] _partOptions;
        private PART_TYPE[] _partDiscardOptions;

        private PartAttachableFactory.PART_OPTION_TYPE _partOptionType;

        private PartDetailsUI PartDetailsUI
        {
            get
            {
                if (_partDetailsUI == null)
                    _partDetailsUI = FindObjectOfType<PartDetailsUI>();

                return _partDetailsUI;
            }
        }
        private PartDetailsUI _partDetailsUI;

        private bool _discardingPart;

        private bool _showingHint;
        private void IsShowingHint(bool showingHint) => _showingHint = showingHint;

        #endregion //Properties

        //Unity Functions
        //====================================================================================================================//

        // Start is called before the first frame update
        private void Start()
        {
            if(!_noPartButtonText) _noPartButtonText = noPartSelectedOptionButton.GetComponentInChildren<TMP_Text>();

            _partOptions = new PART_TYPE[2];
            InitButtons();
        }

        private void OnEnable()
        {
            HintManager.OnShowingHintAction += IsShowingHint;
        }

        private void OnDisable()
        {
            HintManager.OnShowingHintAction -= IsShowingHint;
        }

        //Init
        //============================================================================================================//

        #region Init

        public void Init(PartAttachableFactory.PART_OPTION_TYPE partOptionType)
        {
            InitButtons();
            
            titleText.text = "Pick a Part";
            
            noPartSelectedOptionButton.gameObject.SetActive(partOptionType != PartAttachableFactory.PART_OPTION_TYPE.InitialSelection);

            _partOptionType = partOptionType;

            var partFactory = FactoryManager.Instance.GetFactory<PartAttachableFactory>();
            var partProfiles = FactoryManager.Instance.PartsProfileData;
            var partRemoteData = FactoryManager.Instance.PartsRemoteData;

            void SetUI(in int index, in PART_TYPE partType)
            {
                var category = partRemoteData.GetRemoteData(partType).category;

                selectionUis[index].PartChoiceButtonHover.SetPartType(partType);
                selectionUis[index].optionImage.sprite = partProfiles.GetProfile(partType).Sprite;
                selectionUis[index].optionImage.color = Globals.UsePartColors ? category.GetColor() : Color.white;
                selectionUis[index].optionText.text = $"{partType}";
                
                selectionUis[index].categoryImage.color = category.GetColor();
                selectionUis[index].categoryText.text = category.GetCategoryName();
                var (sprite, color) = partType.GetBorderData();
                selectionUis[index].partBorderImage.sprite = sprite;
                selectionUis[index].partBorderImage.color = color;
            }

            Random.InitState(DateTime.Now.Millisecond);

            var partsOnBot = PlayerDataManager
                .GetBotBlockDatas()
                .OfType<PartData>()
                .Select(x => (PART_TYPE) x.Type)
                .Where(x => x != PART_TYPE.EMPTY)
                .ToList();
            var partsInStorage = PlayerDataManager
                .GetCurrentPartsInStorage()
                .OfType<PartData>()
                .Select(x => (PART_TYPE) x.Type);

            partsOnBot.AddRange(partsInStorage);

            try
            {
                partFactory.SelectPartOptions(ref _partOptions, partOptionType, partsOnBot.Distinct().ToArray());
            }
            catch (Exception)
            {
                SetActive(false);
                throw;
            }

            if (_partOptions[0] == _partOptions[1])
            {
                SetActive(false);
                throw new Exception($"Attempting to let the player choose two of the same part [{_partOptions[1]}]");
            }

            for (var i = 0; i < _partOptions.Length; i++)
            {
                SetUI(i, _partOptions[i]);
            }

            SetActive(true);
            
            UISelectHandler.SetBuildTarget(this);
            
        }

        private void InitButtons()
        {
            void RecordSelectedParts(in int index)
            {
                var outDict = new Dictionary<PART_TYPE, bool>();
                for (int i = 0; i < selectionUis.Length; i++)
                {
                    var partType = GetPartType(i);
                    
                    if(outDict.ContainsKey(partType))
                        continue;
                    
                    outDict.Add(GetPartType(i), i == index);
                }

                AnalyticsManager.PickedPartEvent(outDict);
            }
            PART_TYPE GetPartType(in int index)
            {
                return _partOptions[index];
            }

            void CreatePart(PART_TYPE partType)
            {
                var partRemoteData = partType.GetRemoteData();
                //var patchCount = partRemoteData.PatchSockets;

                var partData = new PartData
                {
                    Type = (int)partType,
                    Patches = new List<PatchData>()
                };

                var category = partRemoteData.category;
                var botCoordinate = PlayerDataManager.GetCoordinateForCategory(category);
                var botParts = PlayerDataManager.GetBotBlockDatas()?.OfType<PartData>().ToList();
                
                //If the player has an empty part at the location, auto equip it
                if (!botParts.Any(x => x.Type != (int)PART_TYPE.EMPTY && x.Coordinate == botCoordinate))
                {
                    partData.Coordinate = botCoordinate;
                    
                    PlayerDataManager.SetDroneBlockDataAtCoordinate(botCoordinate, partData, true);
                }
                else
                {
                    //Should I switch with what's currently equipped
                    PlayerDataManager.AddPartToStorage(partData);
                }

                
                SessionDataProcessor.Instance.RecordPartSelection(partType, new List<PART_TYPE>(_partOptions).ToArray());

                if (HasOverage(out var parts))
                {
                    _discardingPart = true;
                    PresentPartOverage(parts);
                    return;
                }
                
                
                PlayerDataManager.OnValuesChanged?.Invoke();
                PlayerDataManager.NewPartPicked?.Invoke(_partOptionType, partType);
                

                CloseWindow();
            }

            //--------------------------------------------------------------------------------------------------------//

            for (int i = 0; i < selectionUis.Length; i++)
            {
                var index = i;
                selectionUis[i].optionButton.onClick.RemoveAllListeners();
                selectionUis[i].optionButton.onClick.AddListener(() =>
                {
                    if (_showingHint) return;

                    var partType = GetPartType(index);

                    LastPicked = partType;
                    CreatePart(partType);
                    RecordSelectedParts(index);
                });
            }

            _noPartButtonText.text = $"No Part +{10}{TMP_SpriteHelper.GEAR_ICON}";
            noPartSelectedOptionButton.onClick.RemoveAllListeners();
            noPartSelectedOptionButton.onClick.AddListener(() =>
            {
                if (_showingHint) return;

                CloseWindow();
                PlayerDataManager.AddGears(10);
                RecordSelectedParts(-1);
                PlayerDataManager.NewPartPicked?.Invoke(_partOptionType, PART_TYPE.EMPTY);
            });
            
            for (var i = 0; i < selectionUis.Length; i++)
            {
                if (selectionUis[i].partBorderImage != null)
                    continue;
                
                selectionUis[i].partBorderImage =
                    PartAttachableFactory.CreateUIPartBorder(
                        (RectTransform)selectionUis[i].optionButton.transform,
                        BIT_TYPE.WHITE);
            }

            
        }

        private void CloseWindow()
        {
            PlayerDataManager.SetRunStarted(true);
            PlayerDataManager.SetCanChoosePart(false);
            partChoiceWindow.SetActive(false);

            PartDetailsUI.ShowPartDetails(false, new PartData(), null);
        }

        #endregion //Init

        //Discard Part
        //====================================================================================================================//

        #region Discard Parts

        private void PresentPartOverage(IReadOnlyList<PartData> partDatas)
        {
            titleText.text = "Discard a Part";
            noPartSelectedOptionButton.gameObject.SetActive(false);
            
            //--------------------------------------------------------------------------------------------------------//

            void FindAndDestroyPart(in PART_TYPE partType)
            {
                var partDiscardOptions = partDatas.Select(x => (PART_TYPE)x.Type).ToArray();
                SessionDataProcessor.Instance.RecordPartDiscarding(partType, new List<PART_TYPE>(partDiscardOptions).ToArray());
                
                var type = partType;

                var storage = new List<IBlockData>(PlayerDataManager.GetCurrentPartsInStorage());
                var index = storage.FindIndex(x => x is PartData p && p.Type == (int) type);
                if (index >= 0)
                {
                    //TODO From the part from the storage
                    PlayerDataManager.RemovePartFromStorageAtIndex(index);
                    PlayerDataManager.OnValuesChanged?.Invoke();
                    return;
                }

                var botBlockDatas = new List<IBlockData>(PlayerDataManager.GetBotBlockDatas());
                index = botBlockDatas.FindIndex(x => x is PartData && x.Type == (int) type);

                if (index < 0)
                    throw new Exception();

                var partData = botBlockDatas[index];
                var coordinate = partData.Coordinate;
                
                botBlockDatas.RemoveAt(index);
                botBlockDatas.Add(new PartData
                {
                    Type = (int) PART_TYPE.EMPTY,
                    Coordinate = coordinate,
                    Patches = new List<PatchData>()
                });

                PlayerDataManager.SetDroneBlockData(botBlockDatas);
                PlayerDataManager.OnValuesChanged?.Invoke();
            }

            //--------------------------------------------------------------------------------------------------------//

            var partProfileScriptableObject = FactoryManager.Instance.PartsProfileData;
            for (int i = 0; i < partDatas.Count; i++)
            {
                var partData = partDatas[i];
                var partType = (PART_TYPE)partData.Type;
                var category = partType.GetCategory();
                var (sprite, color) = partProfileScriptableObject.GetPartBorder(category);

                selectionUis[i].optionText.text = partType.GetRemoteData().name;
                selectionUis[i].optionImage.sprite = partType.GetSprite();
                selectionUis[i].partBorderImage.sprite = sprite;
                selectionUis[i].partBorderImage.color = color;

                selectionUis[i].PartChoiceButtonHover.SetPartType(partType);
                    
                selectionUis[i].categoryImage.color = category.GetColor();
                selectionUis[i].categoryText.text = category.GetCategoryName();

                selectionUis[i].optionButton.onClick.RemoveAllListeners();
                selectionUis[i].optionButton.onClick.AddListener(() =>
                {
                    if (_showingHint) return;

                    FindAndDestroyPart(partType);
                    
                    PlayerDataManager.OnValuesChanged?.Invoke();
                    PlayerDataManager.NewPartPicked?.Invoke(_partOptionType, LastPicked);

                    _discardingPart = false;
                    CloseWindow();
                });
            }
            
            UISelectHandler.SetBuildTarget(this);
        }

        private bool HasOverage(out PartData[] partDatas)
        {
            partDatas = default;
            
            var currentParts = new List<PartData>(PlayerDataManager.GetCurrentPartsInStorage().OfType<PartData>());
            currentParts.AddRange(PlayerDataManager.GetBotBlockDatas().OfType<PartData>());

            foreach (BIT_TYPE bitType in Enum.GetValues(typeof(BIT_TYPE)))
            {
                if(bitType == BIT_TYPE.WHITE || bitType == BIT_TYPE.NONE)
                    continue;

                var parts = currentParts
                    .Where(x => ((PART_TYPE) x.Type).GetCategory() == bitType)
                    .ToList();

                if(parts.Count <= Globals.MaxPartTypeCount)
                    continue;
                
                var partOptions = parts
                    .Where(x => LastPicked != (PART_TYPE)x.Type)
                    .Take(2)
                    .ToArray();

                partDatas = partOptions;
                return true;
            }

            return false;
        }

        #endregion //Discard Parts
        
        //IBuildNavigationProfile Functions
        //====================================================================================================================//
        
        public NavigationProfile BuildNavigationProfile()
        {
            if (_discardingPart)
            {
                return new NavigationProfile(selectionUis[0].optionButton,
                    new []
                    {
                        selectionUis[0].optionButton,
                        selectionUis[1].optionButton,
                    }, null, null);
            }
            
            return new NavigationProfile(selectionUis[0].optionButton,
                new []
                {
                    selectionUis[0].optionButton,
                    selectionUis[1].optionButton,
                    noPartSelectedOptionButton
                }, null, null);
            
        }

        //====================================================================================================================//

        //Extra Functions
        //====================================================================================================================//
        
        public void SetActive(in bool state) => partChoiceWindow.SetActive(state);

        //Unity Editor
        //============================================================================================================//

#if UNITY_EDITOR

        [Button]
        private void ShowPartSelection()
        {
            partChoiceWindow.SetActive(true);
            Init(PartAttachableFactory.PART_OPTION_TYPE.Any);
        }

#endif

        
    }
}
