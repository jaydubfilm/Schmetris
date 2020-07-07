using Sirenix.OdinInspector;
using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StarSalvager.UI
{
    public class ResourceUIElement : UIElement<ResourceAmount>
    {
        private static BitAttachableFactory _bitAttachableFactory;
        
        [SerializeField, Required]
        private TMP_Text resourceAmountText;
        [SerializeField, Required]
        private Image resourceImage;
        
        public int Amount
        {
            set => resourceAmountText.text = $"{value}";
        }
        
        //============================================================================================================//
        
        public override void Init(ResourceAmount data)
        {
            if (_bitAttachableFactory == null)
                _bitAttachableFactory = FactoryManager.Instance.GetFactory<BitAttachableFactory>();
            
            this.data = data;

            Amount = data.amount;
            
            resourceImage.sprite = _bitAttachableFactory.GetBitProfile(data.type).Sprites[1];
        }


        
        //============================================================================================================//
    }
}