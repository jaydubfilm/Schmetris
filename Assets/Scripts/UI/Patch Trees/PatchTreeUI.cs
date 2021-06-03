using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using StarSalvager.PatchTrees.Data;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.Helpers;
using StarSalvager.Utilities.JsonDataTypes;
using StarSalvager.Utilities.Saving;
using StarSalvager.Utilities.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

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
        private GameObject patchOptionPrefab;
        [SerializeField, Required, FoldoutGroup("Wreck Window")]
        private RectTransform patchOptionsContainer;
        
        //Patch Tree Data
        //====================================================================================================================//
        
        [SerializeField, Required, FoldoutGroup("Patch Tree Window")]
        private RectTransform patchTreeTierContainer;

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
        private RectTransform[] _activeElements;
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

        private Dictionary<DIRECTION, Image> _primaryPartImages;
        private Dictionary<DIRECTION, Image> _secondaryPartImages;

        //FIXME I might need to actually select the Part Mono object, not just the type
        private PART_TYPE _selectedPart = PART_TYPE.EMPTY;


        //====================================================================================================================//




        #endregion //Properties

        //Unity Functions
        //====================================================================================================================//

        private void OnEnable()
        {
            PlayerDataManager.OnValuesChanged += OnValuesChanged;
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

        //Setup Wreck Screen
        //====================================================================================================================//

        private void SetupButtons()
        {
            menuButton.onClick.AddListener(MenuPressed);
            launchButton.onClick.AddListener(LaunchPressed);
        }

        private void SetupDroneUI()
        {

            //--------------------------------------------------------------------------------------------------------//
            
            Image CreatePartImage(in RectTransform container, in DIRECTION direction, in PART_TYPE partType)
            {
                var temp = new GameObject();
                var tempImage = temp.AddComponent<Image>();
                tempImage.sprite = partType.GetSprite();
                tempImage.color = PlayerDataManager.GetCategoryAtCoordinate(direction.ToVector2Int()).GetColor();
                var tempTransform = temp.transform as RectTransform;

                tempTransform.SetParent(container, false);
                tempTransform.sizeDelta = Vector2.one * partImageSize;
                tempTransform.anchoredPosition = direction.ToVector2() * partImageSize;

                return tempImage;
            }

            //--------------------------------------------------------------------------------------------------------//
            
            var directions = new[]
            {
                DIRECTION.UP,
                DIRECTION.RIGHT,
                DIRECTION.DOWN,
                DIRECTION.LEFT
            };
            
            _primaryPartImages = new Dictionary<DIRECTION, Image>();
            _secondaryPartImages = new Dictionary<DIRECTION, Image>();
            
            //Setup 4 directions for Primary & Secondary
            foreach (var direction in directions)
            {
                _primaryPartImages.Add(direction, CreatePartImage(primaryPartsAreaTransform, direction, PART_TYPE.EMPTY));
                _secondaryPartImages.Add(direction, CreatePartImage(secondaryPartsAreaTransform, direction, PART_TYPE.EMPTY));
            }
        }
        

        //Wreck Data Functions
        //====================================================================================================================//

        public void InitWreck(in string wreckName, in Sprite wreckSprite, in PartData[] patchOptions)
        {
            wreckNameText.text = wreckName;
            wreckImage.sprite = wreckSprite;
        }
        
        //Patch Tree Functions
        //====================================================================================================================//

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

            RectTransform CreatePartNodeElement(in RectTransform container, in PART_TYPE type)
            {
                var temp = Instantiate(patchNodeElementPrefab, container, false);
                temp.Init(type);
                //TODO Fill with patchData

                return (RectTransform) temp.transform;
            }

            RectTransform CreatePatchNodeElement(in RectTransform container, in PART_TYPE type, in PatchData patchData,
                in bool unlocked)
            {
                var temp = Instantiate(patchNodeElementPrefab, container, false);
                temp.Init(type, patchData, unlocked);
                //TODO Fill with patchData

                return (RectTransform) temp.transform;
            }

            RectTransform CreateUILine(in RectTransform startTransform, in RectTransform endTransform)
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

                var image = UILineCreator.DrawConnection(_lineContainer, startTransform, endTransform, Color.white);
                return image.transform as RectTransform;
            }

            bool HasUnlockedPatch(in PartData data, in PatchData patchData)
            {
                throw new NotImplementedException();
            }

            //--------------------------------------------------------------------------------------------------------//

            var partType = (PART_TYPE) partData.Type;
            CleanPatchTree();

            yield return null;

            var patchTreeData = partType.GetPatchTree();
            var maxTier = patchTreeData.Max(x => x.Tier);

            //Add one to account for the part Tier
            _activeTiers = new RectTransform[maxTier + 1];
            //Add one to account for the part Element
            _activeElements = new RectTransform[patchTreeData.Count + 1];

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
                var unlocked = HasUnlockedPatch(partData, patchData);

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
                Destroy(_activeTiers[i].gameObject);
            }
        }

        //Drone Functions
        //====================================================================================================================//

        private void SwapSelectedPart()
        {
            if (_selectedPart == PART_TYPE.EMPTY)
                throw new Exception();
            
            var direction = _selectedPart.GetCoordinateForCategory().ToDirection();
            //TODO This is likely going to be stored in the player data, so I would also be communicating with that here
            var primary = _primaryPartImages[direction].sprite;
            var secondary = _secondaryPartImages[direction].sprite;

            _primaryPartImages[direction].sprite = secondary;
            _secondaryPartImages[direction].sprite = primary;
        }
        
        public void SetPrimaryPart(in PART_TYPE partType) => SetPartImage(_primaryPartImages, partType);

        public void SetSecondaryPart(in PART_TYPE partType) => SetPartImage(_secondaryPartImages, partType);

        private static void SetPartImage(in Dictionary<DIRECTION, Image> partImages, in PART_TYPE partType)
        {
            var direction = partType.GetCoordinateForCategory().ToDirection();
            partImages[direction].sprite = partType.GetSprite();
        }



        //Extra Functions
        //====================================================================================================================//

        private void LaunchPressed()
        {
            throw new NotImplementedException();
        }

        private void MenuPressed()
        {
            throw new NotImplementedException();
        }

        private void OnValuesChanged()
        {
            currenciesText.text =
                $"{TMP_SpriteHelper.SILVER_ICON} {PlayerDataManager.GetSilver()} " +
                $"{TMP_SpriteHelper.GEAR_ICON} {PlayerDataManager.GetGears()}";
            
            //TODO Need to update the parts storage here
        }

        //Unity Editor
        //====================================================================================================================//

        #region Unity Editor

#if UNITY_EDITOR

        [Button, DisableInEditorMode]
        private void TestGunPatchTree()
        {
            var gunPartData = new PartData
            {
                Type = (int) PART_TYPE.GUN,
                Coordinate = Vector2Int.zero,
                Patches = new[]
                {
                    new PatchData {Type = (int) PATCH_TYPE.POWER, Level = 0}
                }
            };
            //GeneratePatchTree(PART_TYPE.GUN);
            StartCoroutine(GeneratePatchTreeCoroutine(gunPartData));
        }

#endif

        #endregion //Unity Editor
    }
}
