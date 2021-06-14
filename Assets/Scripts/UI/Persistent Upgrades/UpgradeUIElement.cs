using System;
using Sirenix.OdinInspector;
using StarSalvager.Factories;
using StarSalvager.PersistentUpgrades.Data;
using StarSalvager.Utilities.Helpers;
using StarSalvager.Utilities.Saving;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace StarSalvager.UI.PersistentUpgrades
{
    //[RequireComponent(typeof(Button))]
    public class UpgradeUIElement : ButtonReturnUIElement<UpgradeData>, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField, Required]
        private Button button;
        [SerializeField, Required]
        private Image buttonImage;
        [SerializeField, Required]
        private TMP_Text buttonText;
        
        [SerializeField, Required]
        private Image glowImage;

        private Action<UpgradeData, RectTransform> _onHover;

        //Unity Functions
        //====================================================================================================================//

        private void OnEnable()
        {
            PlayerDataManager.OnValuesChanged += TryUpdate;
        }

        private void OnDisable()
        {
            PlayerDataManager.OnValuesChanged -= TryUpdate;
        }

        //====================================================================================================================//
        

        public void Init(UpgradeData data, Action<UpgradeData> OnPressed, Action<UpgradeData, RectTransform> OnHover)
        {
            Init(data, OnPressed);

            _onHover = OnHover;
        }
        public override void Init(UpgradeData data, Action<UpgradeData> OnPressed)
        {
            this.data = data;
            
            var remoteData = FactoryManager.Instance.PersistentUpgrades.GetRemoteData(data.Type, data.BitType);

            buttonImage.sprite = remoteData.sprite;
            
            
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnPressed?.Invoke(this.data));

            TryUpdate();
        }

        public void TryUpdate()
        {
            //--------------------------------------------------------------------------------------------------------//
            
            bool IsUnlocked()
            {
                var currentLevel = PlayerDataManager.GetCurrentUpgradeLevel(data.Type, data.BitType);

                return data.Level <= currentLevel + 1;
            }
            bool HasPurchased()
            {
                var currentLevel = PlayerDataManager.GetCurrentUpgradeLevel(data.Type, data.BitType);

                return data.Level <= currentLevel;
            }
            
            int GetCost() => FactoryManager.Instance.PersistentUpgrades.GetRemoteData(data.Type, data.BitType).Levels[data.Level].cost;
            
            bool CanAfford(in int stars) => PlayerDataManager.GetStars() >= stars;

            //--------------------------------------------------------------------------------------------------------//
            
            var cost = GetCost();
            var hasPurchased = HasPurchased();
            var isUnlocked = IsUnlocked();
            var canAfford = CanAfford(cost);

            var interactable = isUnlocked && !hasPurchased && canAfford;
            
            
            if (hasPurchased)
            {
                button.interactable = true;
                button.enabled = false;
                buttonText.text = string.Empty;
                glowImage.gameObject.SetActive(false);

                return;
            }
            
            button.interactable = interactable;
            button.enabled = true;
            buttonText.text = $"{cost}{TMP_SpriteHelper.STAR_ICON}";

            glowImage.gameObject.SetActive(interactable);
        }

        //Pointer Events
        //====================================================================================================================//
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            _onHover?.Invoke(data, transform);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _onHover?.Invoke(new UpgradeData(), null);
        }

        //====================================================================================================================//
        
    }
}
