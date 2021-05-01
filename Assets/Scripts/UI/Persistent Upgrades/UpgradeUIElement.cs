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
    [RequireComponent(typeof(Button))]
    public class UpgradeUIElement : ButtonReturnUIElement<UpgradeData>, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField, Required]
        private Button button;
        [SerializeField, Required]
        private Image buttonImage;
        [SerializeField, Required]
        private TMP_Text buttonText;

        private Action<UpgradeData, RectTransform> _onHover;

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
            float GetCost()
            {
                return FactoryManager.Instance.PersistentUpgrades.GetRemoteData(data.Type, data.BitType).Levels[data.Level].cost;
            }

            var hasPurchased = HasPurchased();
            buttonText.text = hasPurchased ? string.Empty : $"{GetCost()}{TMP_SpriteHelper.STAR_ICON}";
            button.interactable = IsUnlocked() && !hasPurchased;
        }

        private bool IsUnlocked()
        {
            var currentLevel = PlayerDataManager.GetCurrentUpgradeLevel(data.Type, data.BitType);

            return data.Level <= currentLevel + 1;
        }
        
        private bool HasPurchased()
        {
            var currentLevel = PlayerDataManager.GetCurrentUpgradeLevel(data.Type, data.BitType);

            return data.Level <= currentLevel;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _onHover?.Invoke(data, transform);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _onHover?.Invoke(new UpgradeData(), null);
        }
    }
}
