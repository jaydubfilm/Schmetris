using Sirenix.OdinInspector;
using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using StarSalvager.Utilities.Extensions;
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

        [SerializeField, Required] private TMP_Text resourceAmountText;
        [SerializeField, Required] private Image resourceImage;

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

            switch (data.resourceType)
            {
                case CraftCost.TYPE.Bit:
                    resourceImage.sprite = _bitAttachableFactory.GetBitProfile((BIT_TYPE) data.type).refinedSprite;
                    break;
                case CraftCost.TYPE.Component:
                    resourceImage.sprite = _componentAttachableFactory.GetComponentProfile((COMPONENT_TYPE) data.type)
                        .GetSprite(0);
                    break;
                case CraftCost.TYPE.Part:
                    resourceImage.sprite = _partAttachableFactory.GetProfileData((PART_TYPE) data.type).GetSprite(data.partPrerequisiteLevel);
                    break;
            }
        }



        //============================================================================================================//
    }
}