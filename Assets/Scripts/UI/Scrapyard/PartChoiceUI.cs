﻿using System;
using StarSalvager.Factories;
using StarSalvager.Utilities.JsonDataTypes;
using StarSalvager.Utilities.Saving;
using StarSalvager.Values;
using System.Collections;
using System.Collections.Generic;
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


        [SerializeField]
        private GameObject partChoiceWindow;

        [SerializeField]
        private PartSelectionUI[] selectionUis;

        private PART_TYPE[] _partOptions;

        // Start is called before the first frame update
        private void Start()
        {
            _partOptions = new PART_TYPE[2];
            InitButtons();
        }

        //============================================================================================================//

        #region Init

        public void Init()
        {
            var partAttachableFactory = FactoryManager.Instance.GetFactory<PartAttachableFactory>();

            void SetUI(in int index, in PART_TYPE partType)
            {
                selectionUis[index].optionImage.sprite = partAttachableFactory.GetProfileData(partType).Sprite;
                selectionUis[index].optionText.text = $"{partType}";
            }
            
            Random.InitState(DateTime.Now.Millisecond);

            //Only use isBasic if the player is selecting the first Sector First Wave
            PartAttachableFactory.SelectPartOptions(ref _partOptions,
                Globals.CurrentSector == 0 && Globals.CurrentWave <= 1);


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
                
                PlayerDataManager.AddPartToStorage(partData);
                PlayerDataManager.SetCanChoosePart(false);
                
                partChoiceWindow.SetActive(false);
            }

            for (int i = 0; i < selectionUis.Length; i++)
            {
                var index = i;
                selectionUis[i].optionButton.onClick.AddListener(() =>
                {
                    CreatePart(GetPartType(index));
                });
            }
        }

        #endregion //Init

        //============================================================================================================//
    }
}