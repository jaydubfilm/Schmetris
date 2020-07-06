using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StarSalvager.UI
{
    public class PartUIElement : ButtonUIElement<PartRemoteData, PART_TYPE>
    {
        private static PartAttachableFactory _partAttachableFactory;
        private static BitAttachableFactory _bitAttachableFactory;
        
        //============================================================================================================//
        
        [SerializeField, Required]
        private Image logoImage;

        [SerializeField, Required]
        private TMP_Text partNameText;

        [SerializeField]
        private GameObject costPrefab;

        private List<CostUIElement> costElements;

        //============================================================================================================//
        
        public override void Init(PartRemoteData data, Action<PART_TYPE> OnPressed)
        {
            if (_partAttachableFactory == null)
                _partAttachableFactory = FactoryManager.Instance.GetFactory<PartAttachableFactory>();
            
            if (_bitAttachableFactory == null)
                _bitAttachableFactory = FactoryManager.Instance.GetFactory<BitAttachableFactory>();
            
            this.data = data;

            logoImage.sprite = _partAttachableFactory.GetProfileData(data.partType).Sprites[0];
            partNameText.text = data.name;

            costElements = new List<CostUIElement>();
            foreach (var cost in data.costs[0].levelCosts)
            {
                var costElement = Instantiate(costPrefab).GetComponent<CostUIElement>();
                costElement.gameObject.name = $"{cost.type}_UIElement";
                costElement.transform.SetParent(transform, false);
                costElement.transform.localScale = Vector3.one;
                
                costElement.Init(cost);
            }
            
            button.onClick.AddListener(() =>
            {
                OnPressed?.Invoke(data.partType);
            });
        }
        
        //============================================================================================================//
    }
}


