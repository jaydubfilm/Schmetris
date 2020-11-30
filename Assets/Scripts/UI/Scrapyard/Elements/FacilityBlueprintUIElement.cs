using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using StarSalvager.Utilities.Saving;
using StarSalvager.Values;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace StarSalvager.UI.Scrapyard
{
    public class TEST_FacilityBlueprint: IEquatable<TEST_FacilityBlueprint>
    {
        public string name;
        public string description;
        public FACILITY_TYPE facilityType;
        public int level;

        public int patchCost;

        #region IEquatable

        public bool Equals(TEST_FacilityBlueprint other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return name == other.name;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TEST_FacilityBlueprint) obj);
        }

        public override int GetHashCode()
        {
            return (name != null ? name.GetHashCode() : 0);
        }

        #endregion //IEquatable
    }

    public class FacilityBlueprintUIElement : UIElement<TEST_FacilityBlueprint>, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField, Required]
        private TMP_Text nameText;
        [SerializeField, Required]
        private Button craftButton;

        [SerializeField, Required]
        private Image stickerImage;

        private bool _canShowSticker;

        private bool _isHovered;
        private float _hoverTimer = 0;

        private Action<TEST_FacilityBlueprint, bool> _onHoverCallback;

        public void Init(TEST_FacilityBlueprint data,
            Action<TEST_FacilityBlueprint> onCraftPressed,
            Action<TEST_FacilityBlueprint, bool> onHoverCallback, bool craftButtonInteractable, bool canShowSticker = true)
        {
            Init(data);
            _canShowSticker = canShowSticker;

            craftButton.interactable = craftButtonInteractable && PlayerDataManager.CanAffordFacilityBlueprint(data);
            stickerImage.gameObject.SetActive(_canShowSticker && PlayerDataManager.CheckHasFacilityBlueprintAlert(data));

            _onHoverCallback = onHoverCallback;

            craftButton.onClick.RemoveAllListeners();
            craftButton.onClick.AddListener(() => { onCraftPressed?.Invoke(this.data); });
        }

        public override void Init(TEST_FacilityBlueprint data)
        {
            this.data = data;

            nameText.text = data.name;
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            _onHoverCallback?.Invoke(data, true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _onHoverCallback?.Invoke(null, false);
        }
    }
}
