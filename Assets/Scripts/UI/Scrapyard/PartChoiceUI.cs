using System;
using StarSalvager.Factories;
using StarSalvager.Utilities.JsonDataTypes;
using StarSalvager.Utilities.Saving;
using StarSalvager.Values;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
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
            public Image optionImage;
            public TMP_Text optionText;
        }

        //====================================================================================================================//

        public static PART_TYPE LastPicked { get; private set; }


        [SerializeField]
        private GameObject partChoiceWindow;

        [SerializeField]
        private DroneDesigner _droneDesigner;

        [SerializeField]
        private PartSelectionUI[] selectionUis;

        private PART_TYPE[] _partOptions;

        private PartAttachableFactory.PART_OPTION_TYPE _partOptionType;

        // Start is called before the first frame update
        private void Start()
        {
            _partOptions = new PART_TYPE[2];
            InitButtons();
        }

        //============================================================================================================//

        #region Init

        public void Init(PartAttachableFactory.PART_OPTION_TYPE partOptionType)
        {
            _partOptionType = partOptionType;

            var partFactory = FactoryManager.Instance.GetFactory<PartAttachableFactory>();
            var partProfiles = FactoryManager.Instance.PartsProfileData;
            var partRemoteData = FactoryManager.Instance.PartsRemoteData;
            var bitProfiles = FactoryManager.Instance.BitProfileData;

            void SetUI(in int index, in PART_TYPE partType)
            {
                var category = partRemoteData.GetRemoteData(partType).category;
                
                selectionUis[index].optionImage.sprite = partProfiles.GetProfile(partType).Sprite;
                selectionUis[index].optionImage.color = bitProfiles.GetProfile(category).color;
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

                if (_partOptionType == PartAttachableFactory.PART_OPTION_TYPE.Any)
                {
                    PlayerDataManager.AddPartToStorage(partData);
                }
                else
                {
                    var attachable = FactoryManager.Instance.GetFactory<PartAttachableFactory>().CreateScrapyardObject<ScrapyardPart>(partData);
                    _droneDesigner._scrapyardBot.AttachNewBit(PlayerDataManager.GetCoordinateForCategory(FactoryManager.Instance.PartsRemoteData.GetRemoteData(partType).category), attachable);
                }

                _droneDesigner.SaveBlockData();

                /*if (_partOptionType == PartAttachableFactory.PART_OPTION_TYPE.BasicWeapon)
                {
                    PlayerDataManager.SetStarted(true);
                    Init(PartAttachableFactory.PART_OPTION_TYPE.PowerWeapon);
                    return;
                }*/
                
                PlayerDataManager.SetStarted(true);

                PlayerDataManager.SetCanChoosePart(false);
                partChoiceWindow.SetActive(false);

                FindObjectOfType<ScrapyardUI>().CheckForPartOverage();
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
        }

        #endregion //Init

        //============================================================================================================//
    }
}