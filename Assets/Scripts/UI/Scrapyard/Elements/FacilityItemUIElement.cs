using System;
using TMPro;
using UnityEngine;

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
    public class FacilityItemUIElement : UIElement<TEST_FacilityItem>
    {
        [SerializeField]
        private TMP_Text itemNameText;
        
        public override void Init(TEST_FacilityItem data)
        {
            this.data = data;

            itemNameText.text = this.data.name;
        }
    }
}
