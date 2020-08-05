using Sirenix.OdinInspector;
using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StarSalvager.UI
{
    public class ResourceUIElement : UIElement<CraftCost>
    {
        private static BitAttachableFactory _bitAttachableFactory;
        private static PartAttachableFactory _partAttachableFactory;
        private static ComponentAttachableFactory _componentAttachableFactory;
        
        [SerializeField, Required]
        private TMP_Text resourceAmountText;
        [SerializeField, Required]
        private Image resourceImage;
        
        public int Amount
        {
            set => resourceAmountText.text = $"{value}";
        }

        //============================================================================================================//

        public override void Init(CraftCost data)
        {
            if (_bitAttachableFactory == null)
                _bitAttachableFactory = FactoryManager.Instance.GetFactory<BitAttachableFactory>();

            if (_partAttachableFactory == null)
                _partAttachableFactory = FactoryManager.Instance.GetFactory<PartAttachableFactory>();

            if (_componentAttachableFactory == null)
                _componentAttachableFactory = FactoryManager.Instance.GetFactory<ComponentAttachableFactory>();

            this.data = data;

            Amount = data.amount;

            if (data.resourceType == CraftCost.TYPE.Bit)
            {
                resourceImage.sprite = _bitAttachableFactory.GetBitProfile((BIT_TYPE)data.type).Sprites[1];
            }
            else if (data.resourceType == CraftCost.TYPE.Component)
            {
                resourceImage.sprite = _componentAttachableFactory.GetBitProfile((COMPONENT_TYPE)data.type).Sprites[0];
            }
            else if (data.resourceType == CraftCost.TYPE.Part)
            {
                resourceImage.sprite = _partAttachableFactory.GetProfileData((PART_TYPE)data.type).Sprites[data.partPrerequisiteLevel];
            }
        }


        
        //============================================================================================================//
    }
}