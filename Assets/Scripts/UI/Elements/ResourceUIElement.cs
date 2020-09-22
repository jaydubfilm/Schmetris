using Sirenix.OdinInspector;
using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using StarSalvager.Utilities.UI;
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
        
        [SerializeField, Required]
        private Slider previewSlider;

        //============================================================================================================//

        public void Init(ResourceAmount data, bool showMaxValue)
        {
            if (_bitAttachableFactory == null)
                _bitAttachableFactory = FactoryManager.Instance.GetFactory<BitAttachableFactory>();

            this.data = data;

            amountSliderText.value = data.amount;
            amountSliderText.SetBounds(0f, data.capacity);
            amountSliderText.Init(showMaxValue, true);

            previewSlider.minValue = 0f;
            previewSlider.maxValue = data.capacity;
            
            
            resourceImage.sprite = _bitAttachableFactory.GetBitProfile(data.type).refinedSprite;

        }
        
        public override void Init(ResourceAmount data)
        {
            Init(data, false);

        }

        //====================================================================================================================//

        public void PreviewChange(float changeAmount)
        {
            if (changeAmount == 0)
            {
                previewSlider.value = 0;
                amountSliderText.value = data.amount;
                return;
            }
            
            var color = changeAmount > 0f ? Color.green : Color.red;
            previewSlider.fillRect.gameObject.GetComponent<Image>().color = color;


            if (changeAmount < 0f)
            {
                amountSliderText.value = data.amount + changeAmount;
                previewSlider.value = data.amount;
            }
            else
            {
                previewSlider.value = data.amount + changeAmount;
            }
            
        }

#if UNITY_EDITOR

        [Button, DisableInPrefabs, DisableInEditorMode, HorizontalGroup("Row1")]
        private void PreviewNegativeChange()
        {
            PreviewChange(data.amount * 0.33f * -1f);
        }
        
        [Button, DisableInPrefabs, DisableInEditorMode, HorizontalGroup("Row1")]
        private void PreviewNoChange()
        {
            PreviewChange(0f);
        }
        
        [Button, DisableInPrefabs, DisableInEditorMode, HorizontalGroup("Row1")]
        private void PreviewPositiveChange()
        {
            PreviewChange(data.amount * 0.33f);
        }
        
#endif

        //============================================================================================================//
    }
}