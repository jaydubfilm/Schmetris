using StarSalvager.Factories;
using System;
using Sirenix.OdinInspector;
using StarSalvager.Values;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using StarSalvager.Utilities.Extensions;

namespace StarSalvager.UI.Scrapyard
{
    public class BlueprintUIElement : UIElement<Blueprint>, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField]
        private TMP_Text titleText;

        [SerializeField]
        private Image image;

        [SerializeField, Required]
        private Button craftButton;

        private Action<Blueprint, bool, RectTransform> hoverCallback;
        
        //============================================================================================================//

        private void OnEnable()
        {
            PlayerData.OnValuesChanged += UpdateUI;
        }

        private void OnDisable()
        {
            PlayerData.OnValuesChanged -= UpdateUI;
        }

        //============================================================================================================//

        public void Init(Blueprint data, Action<Blueprint> OnCraftPressed, Action<Blueprint, bool, RectTransform> OnHover)
        {
            Init(data);

            hoverCallback = OnHover;
            //craftButtonImage = craftButton.GetComponent<Image>();

            craftButton.interactable = data.CanAfford;
            
            /*if (PlayerPersistentData.PlayerData.CanAffordPart(data.partType, data.level, false))
                craftButtonImage.color = craftButton.colors.normalColor;
            else
                craftButton.GetComponent<Image>().color = craftButton.colors.disabledColor;*/

            craftButton.onClick.RemoveAllListeners();
            craftButton.onClick.AddListener(() =>
            {
                OnCraftPressed?.Invoke(data);
            });
        }

        public override void Init(Blueprint data)
        {
            this.data = data;

            titleText.text = data.name;
            
            //Only try and fill the image in the event its enabled
            if(image.isActiveAndEnabled)
                image.sprite = FactoryManager.Instance.GetFactory<PartAttachableFactory>().GetProfileData(data.partType).GetSprite(data.level);
        }
        
        //============================================================================================================//

        private void UpdateUI()
        {
            craftButton.interactable = data.CanAfford;
            /*if (PlayerPersistentData.PlayerData.CanAffordPart(data.partType, data.level, false))
                craftButtonImage.color = craftButton.colors.normalColor;
            else
                craftButton.GetComponent<Image>().color = craftButton.colors.disabledColor;*/
        }
        
        //============================================================================================================//
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            hoverCallback?.Invoke(data, true, transform);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            hoverCallback?.Invoke(null, false, transform);
        }
        
        //============================================================================================================//
    }
}
