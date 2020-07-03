﻿using Sirenix.OdinInspector;
using StarSalvager.ScriptableObjects;
using StarSalvager.Utilities.UI;
using UnityEngine;
using UnityEngine.UI;

namespace StarSalvager.UI
{
    public class ScrapyardUI : MonoBehaviour
    {
        [SerializeField, Required, BoxGroup("Part UI")]
        private GameObject partElementPrefab;

        [SerializeField, Required, BoxGroup("Part UI")]
        private RectTransform partListContentTransform;
        
        [SerializeField, Required, BoxGroup("Part UI")]
        private RemotePartProfileScriptableObject _remotePartProfileScriptable;
        
        //============================================================================================================//
        
        [SerializeField, BoxGroup("View")]
        private SliderText zoomSliderText;

        [SerializeField, Required, BoxGroup("View")]
        private Button leftTurnButton;
        [SerializeField, Required, BoxGroup("View")]
        private Button rightTurnButton;
        
        //============================================================================================================//

        [SerializeField, Required, BoxGroup("Menu Buttons")]
        private Button SaveButton;
        [SerializeField, Required, BoxGroup("Menu Buttons")]
        private Button LoadButton;
        [SerializeField, Required, BoxGroup("Menu Buttons")]
        private Button ReadyButton;
        
        
        //============================================================================================================//

        
        private void Start()
        {
            zoomSliderText.Init();

            InitPartUI();

            InitButtons();
        }
        
        //============================================================================================================//

        private void InitButtons()
        {
            //Example:
            SaveButton.onClick.AddListener(() =>
            {
                Debug.Log("Save Pressed");
            });
        }

        private void InitPartUI()
        {
            foreach (var partRemoteData in _remotePartProfileScriptable.partRemoteData)
            {
                var partTemp = Instantiate(partElementPrefab).GetComponent<PartUIElement>();
                partTemp.gameObject.name = $"{partRemoteData.partType}_UIElement";
                partTemp.transform.SetParent(partListContentTransform, false);
                partTemp.transform.localScale = Vector3.one;
                
                partTemp.Init(partRemoteData);
            }
        }
        
        //============================================================================================================//

    }
}

