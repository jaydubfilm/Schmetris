using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace StarSalvager.UI.Scrapyard
{
    public class TEST_FacilityItem: IEquatable<TEST_FacilityItem>
    {
        public string name;
        public string description;

        #region IEquatable

        public bool Equals(TEST_FacilityItem other)
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
            return Equals((TEST_FacilityItem) obj);
        }

        public override int GetHashCode()
        {
            return (name != null ? name.GetHashCode() : 0);
        }

        #endregion //IEquatable
    }
    public class FacilityItemUIElement : UIElement<TEST_FacilityItem>, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField]
        private TMP_Text itemNameText;

        private Action<TEST_FacilityItem, bool> _onHoverCallback;
        
        public void Init(TEST_FacilityItem data, Action<TEST_FacilityItem, bool> onHoverCallback)
        {
            Init(data);

            _onHoverCallback = onHoverCallback;
        }
        
        public override void Init(TEST_FacilityItem data)
        {
            this.data = data;

            itemNameText.text = this.data.name;
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
