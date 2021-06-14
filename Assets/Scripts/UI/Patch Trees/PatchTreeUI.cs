﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using StarSalvager.PatchTrees.Data;
using StarSalvager.UI.Hints;
using StarSalvager.Utilities;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.Helpers;
using StarSalvager.Utilities.JsonDataTypes;
using StarSalvager.Utilities.Saving;
using StarSalvager.Utilities.SceneManagement;
using StarSalvager.Utilities.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Console = System.Console;

namespace StarSalvager.UI.Scrapyard.PatchTrees
{
    public class PatchTreeUI : MonoBehaviour
    {
        //Properties
        //====================================================================================================================//

        #region Properties

        [SerializeField, Required]
        private Button menuButton;
        [SerializeField, Required]
        private Button launchButton;
        [SerializeField, Required]
        private TMP_Text currenciesText;
        [SerializeField, Required]
        private Slider healthSlider;

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
        
        [SerializeField, Required, FoldoutGroup("Patch Tree Window")]
        private Button scrapPartButton;
        [SerializeField, Required, FoldoutGroup("Patch Tree Window")]
        private Button swapPartButton;

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

        private Dictionary<BIT_TYPE, Image> _primaryPartImages;
        private Dictionary<BIT_TYPE, Image> _secondaryPartImages;

        private List<BitData> _bitsOnDrone;
        private List<PartData> _partOnDrone;
        private List<PartData> _partsInStorage;

        private (PART_TYPE type, bool inStorage) _selectedPart;


        //====================================================================================================================//




        #endregion //Properties

        //Unity Functions
        //====================================================================================================================//

        #region Unity Functions

        private void OnEnable()
        {
            PlayerDataManager.OnValuesChanged += OnValuesChanged;
            OnValuesChanged();
        }

        private void Start()
        {
            SetupButtons();
            SetupDroneUI();
        }

        private void OnDisable()
        {
            PlayerDataManager.OnValuesChanged -= OnValuesChanged;
        }

        #endregion //Unity Functions

        //Setup Wreck Screen
        //====================================================================================================================//

        #region Setup Wreck Screen

        private void SetupButtons()
        {
            menuButton.onClick.AddListener(MenuPressed);
            launchButton.onClick.AddListener(LaunchPressed);
            
            scrapPartButton.onClick.AddListener(ScrapPartPressed);
            swapPartButton.onClick.AddListener(SwapPartPressed);
        }

        private void SetupDroneUI()
        {

            //--------------------------------------------------------------------------------------------------------//
            
            Image CreatePartImage(in RectTransform container, in Vector2Int coordinate, in PART_TYPE partType, in bool storage)
            {
                var temp = new GameObject();
                var tempImage = temp.AddComponent<Image>();
                var tempButton = temp.AddComponent<Button>();
                var category = PlayerDataManager.GetCategoryAtCoordinate(coordinate);
                tempImage.sprite = partType.GetSprite();
                tempImage.color = category.GetColor();

                var tempData = (partType.GetCategory(), storage);
                tempButton.onClick.AddListener(() =>
                {
                    OnPartPressed(category, tempData.storage);
                });
                
                var tempTransform = (RectTransform)temp.transform;

                tempTransform.SetParent(container, false);
                tempTransform.sizeDelta = Vector2.one * partImageSize;
                tempTransform.anchoredPosition = (Vector2)coordinate * partImageSize;

                return tempImage;
            }

            //--------------------------------------------------------------------------------------------------------//

            var botLayout = PlayerDataManager.GetBotLayout();
            
            _primaryPartImages = new Dictionary<BIT_TYPE, Image>();
            _secondaryPartImages = new Dictionary<BIT_TYPE, Image>();
            
            //Setup 4 directions for Primary & Secondary
            foreach (var coordinate in botLayout)
            {
                var bitType = PlayerDataManager.GetCategoryAtCoordinate(coordinate);
                
                _primaryPartImages.Add(bitType, CreatePartImage(primaryPartsAreaTransform, coordinate, PART_TYPE.EMPTY, false));
                
                if (coordinate == Vector2Int.zero) continue;
                
                _secondaryPartImages.Add(bitType, CreatePartImage(secondaryPartsAreaTransform, coordinate, PART_TYPE.EMPTY, true));
            }
        }

        #endregion //Setup Wreck Screen

        //Wreck Data Functions
        //====================================================================================================================//

        #region Init Wreck

        public void InitWreck(in string wreckName, in Sprite wreckSprite, in PartData[] partPatchOptions)
        {
            //--------------------------------------------------------------------------------------------------------//
            
            RectTransform CreatePartNodeElement(in RectTransform container, in PartData partData)
            {
                var temp = Instantiate(partPatchOptionPrefab, container, false);
                temp.Init(partData);

                return (RectTransform) temp.transform;
            }

            //--------------------------------------------------------------------------------------------------------//

            wreckNameText.text = wreckName;
            wreckImage.sprite = wreckSprite;

            if (!partPatchOptions.IsNullOrEmpty())
            {
                foreach (var partPatchOption in partPatchOptions)
                {
                    CreatePartNodeElement(partPatchOptionsContainer, partPatchOption);
                }
            }

            swapPartButton.gameObject.SetActive(false);
            DrawDroneStorage();
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
                temp.SetSiblingIndex(0);

                return (RectTransform) temp;
            }

            PatchNodeElement CreatePartNodeElement(in RectTransform container, in PART_TYPE type)
            {
                var temp = Instantiate(patchNodeElementPrefab, container, false);
                temp.Init(type);
                //TODO Fill with patchData

                return temp;
            }

            PatchNodeElement CreatePatchNodeElement(in RectTransform container, in PART_TYPE type, in PatchData patchData,
                in bool unlocked)
            {
                var temp = Instantiate(patchNodeElementPrefab, container, false);
                temp.Init(type, patchData, unlocked);
                //TODO Fill with patchData

                return temp;
            }

            RectTransform CreateUILine(in PatchNodeElement startElement, in PatchNodeElement endElement)
            {
                if (_lineContainer == null)
                {
                    var temp = new GameObject("Line Container");
                    var layoutElement = temp.gameObject.AddComponent<LayoutElement>();
                    layoutElement.ignoreLayout = true;

                    _lineContainer = (RectTransform) temp.transform;
                    _lineContainer.SetParent(patchTreeTierContainer, false);
                    _lineContainer.SetSiblingIndex(0);
                }

                var image = endElement.Unlocked
                    ? UILineCreator.DrawConnection(_lineContainer, startElement.transform, endElement.transform,
                        Color.white)
                    : UILineCreator.DrawConnection(_lineContainer, startElement.transform, endElement.transform,
                        dottedLinePrefab, Color.white);
                

                return image.transform as RectTransform;
            }

            bool HasUnlockedPatch(in List<PatchNodeJson> patchTree, in PartData data, in PatchData patchData)
            {
                
                
                
                
                return true;
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
                var patchData = new PatchData();
                var unlocked = HasUnlockedPatch(patchTreeData, partData, patchData);

                var tier = patchTreeData[i].Tier;
                _activeElements[i + 1] = CreatePatchNodeElement(_activeTiers[tier], partType, patchData, unlocked);

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

        }

        private void CleanPatchTree()
        {
            if (_activeTiers.IsNullOrEmpty())
                return;

            for (int i = _activeTiers.Length - 1; i >= 0; i--)
            {
                if(_activeTiers[i].gameObject) Destroy(_activeTiers[i].gameObject);
            }
        }

        #endregion //Patch Tree Functions

        //Drone Functions
        //====================================================================================================================//

        #region Drone Functions

        private void DrawDroneStorage()
        {
            if(_partOnDrone.IsNullOrEmpty()) return;
            foreach (var partData in _partOnDrone)
            {
                var partType = (PART_TYPE) partData.Type;
                if (partType == PART_TYPE.EMPTY) continue;
                
                var category = partType.GetCategory();
                _primaryPartImages[category].sprite = partType.GetSprite();
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
                _secondaryPartImages[category].sprite = partType.GetSprite();
            }


        }

        public void SetPrimaryPart(in PART_TYPE partType) => SetPartImage(_primaryPartImages, partType);

        public void SetSecondaryPart(in PART_TYPE partType) => SetPartImage(_secondaryPartImages, partType);

        private static void SetPartImage(in Dictionary<BIT_TYPE, Image> partImages, in PART_TYPE partType)
        {
            var category = partType.GetCategory();
            partImages[category].sprite = partType.GetSprite();
        }

        #endregion //Drone Functions

        //On Button Pressed Functions
        //====================================================================================================================//

        #region On Button Pressed Functions

        private static void LaunchPressed()
        {
            ScreenFade.Fade(() =>
            {
                SceneLoader.ActivateScene(SceneLoader.UNIVERSE_MAP, SceneLoader.SCRAPYARD);
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

        private void MenuPressed()
        {
            throw new NotImplementedException();
        }

        private void ScrapPartPressed()
        {
            throw new NotImplementedException();
        }

        private void SwapPartPressed()
        {
            //--------------------------------------------------------------------------------------------------------//
            
            void ListShuffles(in PartData _partData, ref List<PartData> fromList, ref List<PartData> toList)
            {
                var category = _selectedPart.type.GetCategory();

                //Get the mirrored part on the bot
                var otherPartData = toList.FirstOrDefault(x => ((PART_TYPE)x.Type).GetCategory() == category);
                //Store that part but also remove it from that list
                toList.Remove(otherPartData);
                
                //Add the new part into the list
                toList.Add(_partData);
                fromList.Remove(_partData);

                //Add old part in old list
                fromList.Add(otherPartData);
            }

            //--------------------------------------------------------------------------------------------------------//
            
            if (_selectedPart.type == PART_TYPE.EMPTY) throw new Exception();

            var intPartType = (int) _selectedPart.type;
            var partData = _selectedPart.inStorage
                ? _partsInStorage.FirstOrDefault(x => x.Type == intPartType)
                : _partOnDrone.FirstOrDefault(x => x.Type == intPartType);

            if (_selectedPart.inStorage)
            {
                ListShuffles(partData, ref _partsInStorage, ref _partOnDrone);
                _selectedPart.inStorage = false;
            }
            else
            {
                ListShuffles(partData, ref _partOnDrone, ref _partsInStorage);
                _selectedPart.inStorage = true;
            }

            SaveBlockData();
        }

        private void OnPartPressed(in BIT_TYPE category, in bool inStorage)
        {
            var cat = category;
            //var typeInt = (int)partType;
            var partData = inStorage
                ? _partsInStorage.FirstOrDefault(x => ((PART_TYPE)x.Type).GetCategory() == cat)
                : _partOnDrone.FirstOrDefault(x => ((PART_TYPE)x.Type).GetCategory() == cat);

            var partType = (PART_TYPE)partData.Type;
            
            Debug.Log($"Selected {partType}");
            
            //TODO Show this part on the PatchTree
            GeneratePatchTree(partData);
            
            //TODO Check if part can be swapped, show button if true
            var canSwap = PartCanBeSwapped(partType);
            swapPartButton.gameObject.SetActive(canSwap);

            _selectedPart = (partType, inStorage);
        }
        private void OnPatchPressed()
        {
            throw new NotImplementedException();
        }
        
        #endregion //On Button Pressed Functions

        //Extra Functions
        //====================================================================================================================//

        #region Extra Functions

        private bool PartCanBeSwapped(in PART_TYPE partType)
        {
            var category = partType.GetCategory();


            return _partsInStorage.Any(x => ((PART_TYPE) x.Type).GetCategory() == category) &&
                _partOnDrone.Any(x => ((PART_TYPE) x.Type).GetCategory() == category);
        }
        

        #endregion //Extra Functions

        private void UpdateBlockData()
        {
            var droneBlockData = PlayerDataManager.GetBotBlockDatas();

            if (droneBlockData.IsNullOrEmpty()) return;
            
            _bitsOnDrone = new List<BitData>(droneBlockData.OfType<BitData>());
            _partOnDrone = new List<PartData>(droneBlockData.OfType<PartData>());
            _partsInStorage = new List<PartData>(PlayerDataManager.GetCurrentPartsInStorage().OfType<PartData>());

            DrawDroneStorage();
        }

        private void SaveBlockData()
        {
            var droneBlockData = new List<IBlockData>(_partOnDrone.OfType<IBlockData>());
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
        
        //Unity Editor
        //====================================================================================================================//

        #region Unity Editor

#if UNITY_EDITOR

        [Button, DisableInEditorMode]
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
        }

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
        
    }
}
