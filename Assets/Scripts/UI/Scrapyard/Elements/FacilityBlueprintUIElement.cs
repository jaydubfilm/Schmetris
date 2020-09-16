using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
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

        public List<CraftCost> cost;

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

    public class FacilityBlueprintUIElement : ButtonReturnUIElement<TEST_FacilityBlueprint>, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField, Required]
        private TMP_Text nameText;
        [SerializeField, Required]
        private Button craftButton;
        
        private Action<TEST_FacilityBlueprint, bool> _onHoverCallback;

        public void Init(TEST_FacilityBlueprint data, Action<TEST_FacilityBlueprint> OnCraftPressed, Action<TEST_FacilityBlueprint, bool> onHoverCallback)
        {
            Init(data,OnCraftPressed);

            _onHoverCallback = onHoverCallback;
        }
        
        public override void Init(TEST_FacilityBlueprint data, Action<TEST_FacilityBlueprint> OnCraftPressed)
        {
            this.data = data;

            nameText.text = data.name;
            
            craftButton.onClick.RemoveAllListeners();
            craftButton.onClick.AddListener(() =>
            {
                OnCraftPressed?.Invoke(this.data);
            });
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
