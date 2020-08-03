﻿using Sirenix.OdinInspector;
using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StarSalvager.UI
{
    public class CostUIElement : UIElement<CraftCost>
    {
        private static BitAttachableFactory _bitAttachableFactory;
        
        //============================================================================================================//

        [SerializeField, Required]
        private Image resourceImage;

        [SerializeField, Required]
        private TMP_Text costText;
        
        //============================================================================================================//
        
        public override void Init(CraftCost data)
        {
            if (_bitAttachableFactory == null)
                _bitAttachableFactory = FactoryManager.Instance.GetFactory<BitAttachableFactory>();
            
            this.data = data;

            resourceImage.sprite = _bitAttachableFactory.GetBitProfile((BIT_TYPE)data.type).Sprites[1];

            costText.text = $"{data.amount}";
        }
        
        //============================================================================================================//
    } 
}


