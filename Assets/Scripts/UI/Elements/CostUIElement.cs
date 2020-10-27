using System;
using System.Linq;
using Sirenix.OdinInspector;
using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.Saving;
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

        private static DroneDesigner mDroneDesigner;

        //============================================================================================================//


        [SerializeField, Required]
        private Image resourceImage;

        [SerializeField, Required]
        private TMP_Text costText;
        
        //============================================================================================================//

        private void OnEnable()
        {
            PlayerDataManager.OnValuesChanged += UpdateData;
        }

        private void OnDisable()
        {
            PlayerDataManager.OnValuesChanged -= UpdateData;
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

            if (mDroneDesigner == null)
                mDroneDesigner = FindObjectOfType<DroneDesigner>();

            this.data = data;


            UpdateData();

        }

        private void UpdateData()
        {
            switch (data.resourceType)
            {
                case CraftCost.TYPE.Bit:
                    resourceImage.sprite = _bitAttachableFactory.GetBitProfile((BIT_TYPE) data.type).refinedSprite;
                    
                    costText.text = $"{PlayerDataManager.GetResource((BIT_TYPE)data.type).resource}/{data.amount}";
                    break;
                case CraftCost.TYPE.Component:
                    resourceImage.sprite = _componentAttachableFactory.GetComponentProfile((COMPONENT_TYPE) data.type)
                        
                        .GetSprite(0);
                    costText.text = $"{PlayerDataManager.GetComponents()[(COMPONENT_TYPE) data.type]}/{data.amount}";
                    break;
                case CraftCost.TYPE.Part:
                    resourceImage.sprite = _partAttachableFactory.GetProfileData((PART_TYPE) data.type)
                        .GetSprite(data.partPrerequisiteLevel);

                    int partCount;
                    if (data.type == (int)PART_TYPE.CORE)
                    {
                        partCount = mDroneDesigner._scrapyardBot.attachedBlocks.GetBlockDatas().Count(x => x.Type == (int)PART_TYPE.CORE && x.Level == data.partPrerequisiteLevel);
                    }
                    else
                    {
                        partCount = PlayerDataManager.GetCurrentPartsInStorage().Count(x => x.Type == data.type && x.Level == data.partPrerequisiteLevel);
                    }
                    
                    costText.text = $"{partCount}/{data.amount}";
                    break;
            }
        }
        
        //============================================================================================================//
    } 
}


