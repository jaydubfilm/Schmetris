using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using StarSalvager.Utilities.JsonDataTypes;
using System;
using Sirenix.OdinInspector;
using StarSalvager.Values;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace StarSalvager.UI.Scrapyard
{


    public class BlueprintUIElement : UIElement<TEST_Blueprint>, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField]
        private TMP_Text titleText;


        [SerializeField]
        private Image image;

        [SerializeField, Required]
        private Button craftButton;

        private Action<TEST_Blueprint, bool, RectTransform> hoverCallback;

        public void Init(TEST_Blueprint data, Action<TEST_Blueprint> OnCraftPressed, Action<TEST_Blueprint, bool, RectTransform> OnHover)
        {
            Init(data);

            hoverCallback = OnHover;

            craftButton.interactable =
                PlayerPersistentData.PlayerData.CanAffordPart(data.remoteData.partType, data.level, false);

            craftButton.onClick.RemoveAllListeners();
            craftButton.onClick.AddListener(() =>
            {
                OnCraftPressed?.Invoke(data);
            });
        }

        public override void Init(TEST_Blueprint data)
        {
            this.data = data;

            titleText.text = data.name;
            image.sprite = FactoryManager.Instance.GetFactory<PartAttachableFactory>().GetProfileData(data.remoteData.partType).Sprites[data.level];
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            hoverCallback?.Invoke(data, true, transform);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            hoverCallback?.Invoke(null, false, transform);
        }
    }

    public class TEST_Blueprint : IEquatable<TEST_Blueprint>
    {
        public string name;
        public PART_TYPE partType;
        public int level;

        public bool Equals(TEST_Blueprint other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return name == other.name && partType.Equals(other.partType) && level == other.level;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TEST_Blueprint) obj);
        }

        public override int GetHashCode()
        {
            return (name != null ? name.GetHashCode() : 0);
        }
    }
}
