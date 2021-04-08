﻿using System;
using StarSalvager.Factories;
using StarSalvager.Utilities.JsonDataTypes;
using StarSalvager.Utilities.Saving;
using StarSalvager.Values;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;


namespace StarSalvager.UI.Scrapyard
{
    public class PartChoiceUI : MonoBehaviour
    {
        [Serializable]
        private struct PartSelectionUI
        {
            public Button optionButton;
            public PartChoiceButtonHover PartChoiceButtonHover;
            public Image optionImage;
            public TMP_Text optionText;
        }

        //====================================================================================================================//

        public static PART_TYPE LastPicked { get; private set; }


        [SerializeField]
        private GameObject partChoiceWindow;

        [SerializeField] private Button noPartSelectedOptionButton;
        private TMP_Text _noPartButtonText;

        [SerializeField]
        private DroneDesigner _droneDesigner;

        [SerializeField]
        private PartSelectionUI[] selectionUis;

        private PART_TYPE[] _partOptions;

        private PartAttachableFactory.PART_OPTION_TYPE _partOptionType;

        // Start is called before the first frame update
        private void Start()
        {
            if(!_noPartButtonText)
                _noPartButtonText = noPartSelectedOptionButton.GetComponentInChildren<TMP_Text>();

            _partOptions = new PART_TYPE[2];
            InitButtons();
        }

        //============================================================================================================//

        #region Init

        public void Init(PartAttachableFactory.PART_OPTION_TYPE partOptionType)
        {
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
                selectionUis[index].optionImage.color = category.GetColor();
                selectionUis[index].optionText.text = $"{partType}";
            }

            Random.InitState(DateTime.Now.Millisecond);

            var partsOnBot = PlayerDataManager
                .GetBlockDatas()
                .OfType<PartData>()
                .Select(x => (PART_TYPE) x.Type)
                .Where(x => x != PART_TYPE.EMPTY)
                .ToList();
            var partsInStorage = PlayerDataManager
                .GetCurrentPartsInStorage()
                .OfType<PartData>()
                .Select(x => (PART_TYPE) x.Type);

            partsOnBot.AddRange(partsInStorage);

            partFactory.SelectPartOptions(ref _partOptions, partOptionType, partsOnBot.Distinct().ToArray());

            if (_partOptions[0] == _partOptions[1])
                throw new Exception($"Attempting to let the player choose two of the same part [{_partOptions[1]}]");

            for (var i = 0; i < _partOptions.Length; i++)
            {
                SetUI(i, _partOptions[i]);
            }

        }

        private void InitButtons()
        {
            PART_TYPE GetPartType(in int index)
            {
                return _partOptions[index];
            }

            void CreatePart(PART_TYPE partType)
            {
                var patchCount = FactoryManager.Instance.PartsRemoteData.GetRemoteData(partType).PatchSockets;

                var partData = new PartData
                {
                    Type = (int)partType,
                    Patches = new PatchData[patchCount]
                };

                var category = FactoryManager.Instance.PartsRemoteData.GetRemoteData(partType).category;
                var botCoordinate = PlayerDataManager.GetCoordinateForCategory(category);

                //If the player has an empty part at the location, auto equip it
                if (!_droneDesigner._scrapyardBot.AttachedBlocks
                    .OfType<ScrapyardPart>()
                    .Any(x => x.Type != PART_TYPE.EMPTY && x.Coordinate == botCoordinate))
                {
                    var attachable = FactoryManager.Instance.GetFactory<PartAttachableFactory>().CreateScrapyardObject<ScrapyardPart>(partData);
                    _droneDesigner._scrapyardBot.AttachNewBit(botCoordinate, attachable);
                }
                else
                {
                    //Should I switch with what's currently equipped
                    PlayerDataManager.AddPartToStorage(partData);
                }


                FindObjectOfType<ScrapyardUI>().CheckForPartOverage();
                _droneDesigner.SaveBlockData();

                CloseWindow();
            }

            for (int i = 0; i < selectionUis.Length; i++)
            {
                var index = i;
                selectionUis[i].optionButton.onClick.AddListener(() =>
                {
                    var partType = GetPartType(index);

                    LastPicked = partType;
                    CreatePart(partType);
                });
            }

            _noPartButtonText.text = $"No Part +{10}{TMP_SpriteMap.GEAR_ICON}";
            noPartSelectedOptionButton.onClick.AddListener(() =>
            {
                CloseWindow();
                PlayerDataManager.AddGears(10);
            });
        }

        private void CloseWindow()
        {
            PlayerDataManager.SetStarted(true);
            PlayerDataManager.SetCanChoosePart(false);
            partChoiceWindow.SetActive(false);

            _droneDesigner.DroneDesignUi.ShowPartDetails(false, new PartData(), null);
        }

        #endregion //Init

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
