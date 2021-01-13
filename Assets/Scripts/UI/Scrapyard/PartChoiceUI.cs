using StarSalvager.Factories;
using StarSalvager.Utilities.JsonDataTypes;
using StarSalvager.Utilities.Saving;
using StarSalvager.Values;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace StarSalvager.UI.Scrapyard
{
    public class PartChoiceUI : MonoBehaviour
    {
        [SerializeField]
        private PartChoice partChoice;


        [SerializeField]
        private GameObject partChoiceWindow;

        [SerializeField]
        private Button buttonOptionOne;
        [SerializeField]
        private Button buttonOptionTwo;
        [SerializeField]
        private Image buttonImageOptionOne;
        [SerializeField]
        private Image buttonImageOptionTwo;
        [SerializeField]
        private TMP_Text partTextOptionOne;
        [SerializeField]
        private TMP_Text partTextOptionTwo;

        private PART_TYPE partTypeOptionOne;
        private PART_TYPE partTypeOptionTwo;


        // Start is called before the first frame update
        private void Start()
        {
            InitButtons();
        }

        //============================================================================================================//

        #region Init

        public void Init()
        {
            Random.InitState(System.DateTime.Now.Millisecond);

            PartAttachableFactory partAttachableFactory = FactoryManager.Instance.GetFactory<PartAttachableFactory>();

            partTypeOptionOne = partAttachableFactory.GetWreckPartTypeOption();
            partTypeOptionTwo = partAttachableFactory.GetWreckPartTypeOption();

            while (partTypeOptionOne == partTypeOptionTwo)
            {
                partTypeOptionTwo = partAttachableFactory.GetWreckPartTypeOption();
            }

            buttonImageOptionOne.sprite = partAttachableFactory.GetProfileData(partTypeOptionOne).Sprite;
            buttonImageOptionTwo.sprite = partAttachableFactory.GetProfileData(partTypeOptionTwo).Sprite;

            partTextOptionOne.text = $"{partTypeOptionOne}";
            partTextOptionTwo.text = $"{partTypeOptionTwo}";
        }

        private void InitButtons()
        {
            void CreatePart(PART_TYPE partType)
            {
                var patchCount = FactoryManager.Instance.PartsRemoteData.GetRemoteData(partType).PatchSockets;
                
                var partData = new PartData
                {
                    Type = (int)partType,
                    Patches = new PatchData[patchCount]
                };
                
                PlayerDataManager.AddPartToStorage(partData);
                Globals.PartChoiceAvailable = false;
                partChoiceWindow.SetActive(false);
            }
            
            buttonOptionOne.onClick.AddListener(() =>
            {
                CreatePart(partTypeOptionOne);
            });

            buttonOptionTwo.onClick.AddListener(() =>
            {
                CreatePart(partTypeOptionTwo);
            });
        }

        #endregion //Init

        //============================================================================================================//
    }
}