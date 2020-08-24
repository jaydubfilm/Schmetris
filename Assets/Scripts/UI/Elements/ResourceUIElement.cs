using Sirenix.OdinInspector;
using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StarSalvager.UI
{
    public class ResourceUIElement : UIElement<ResourceAmount>
    {
        private static BitAttachableFactory _bitAttachableFactory;
        private static PartAttachableFactory _partAttachableFactory;
        private static ComponentAttachableFactory _componentAttachableFactory;

        //[SerializeField, Required] private TMP_Text resourceAmountText;
        [SerializeField, Required] private Image resourceImage;

        [SerializeField]
        private SliderText amountSliderText;

        //============================================================================================================//

        public void Init(ResourceAmount data, bool showMaxValue)
        {
            if (_bitAttachableFactory == null)
                _bitAttachableFactory = FactoryManager.Instance.GetFactory<BitAttachableFactory>();

            this.data = data;

            amountSliderText.Init(showMaxValue);
            amountSliderText.SetBounds(0f, data.capacity);
            amountSliderText.value = data.amount;
            
            resourceImage.sprite = _bitAttachableFactory.GetBitProfile((BIT_TYPE) data.type).refinedSprite;

        }
        
        public override void Init(ResourceAmount data)
        {
            Init(data, false);

        }



        //============================================================================================================//
    }
}