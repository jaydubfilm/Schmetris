using System;
using System.Linq;
using Sirenix.OdinInspector;
using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Values;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StarSalvager.UI
{
    public class CostUIElement : UIElement<CraftCost>
    {
        private static BitAttachableFactory _bitAttachableFactory;
        private static PartAttachableFactory _partAttachableFactory;
        private static ComponentAttachableFactory _componentAttachableFactory;
        
        //============================================================================================================//

        [SerializeField, Required]
        private Image resourceImage;

        [SerializeField, Required]
        private TMP_Text costText;
        
        //============================================================================================================//

        private void OnEnable()
        {
            PlayerData.OnValuesChanged += UpdateData;
        }

        private void OnDisable()
        {
            PlayerData.OnValuesChanged -= UpdateData;
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


            UpdateData();

        }

        private void UpdateData()
        {
            switch (data.resourceType)
            {
                case CraftCost.TYPE.Bit:
                    resourceImage.sprite = _bitAttachableFactory.GetBitProfile((BIT_TYPE) data.type).refinedSprite;
                    
                    costText.text = $"{PlayerPersistentData.PlayerData.resources[(BIT_TYPE)data.type]}/{data.amount}";
                    break;
                case CraftCost.TYPE.Component:
                    resourceImage.sprite = _componentAttachableFactory.GetComponentProfile((COMPONENT_TYPE) data.type)
                        
                        .GetSprite(0);
                    costText.text = $"{PlayerPersistentData.PlayerData.components[(COMPONENT_TYPE) data.type]}/{data.amount}";
                    break;
                case CraftCost.TYPE.Part:
                    resourceImage.sprite = _partAttachableFactory.GetProfileData((PART_TYPE) data.type)
                        .GetSprite(data.partPrerequisiteLevel);

                    var partCount = PlayerPersistentData.PlayerData.partsInStorageBlockData.Count(x => x.Type == data.type && x.Level == data.partPrerequisiteLevel);
                    
                    costText.text = $"{partCount}/{data.amount}";
                    break;
            }
        }
        
        //============================================================================================================//
    } 
}


