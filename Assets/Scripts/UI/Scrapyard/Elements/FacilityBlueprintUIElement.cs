using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
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

    public class FacilityBlueprintUIElement : ButtonReturnUIElement<TEST_FacilityBlueprint>
    {
        [SerializeField, Required]
        private TMP_Text nameText;
        [SerializeField, Required]
        private Button craftButton;

        public override void Init(TEST_FacilityBlueprint data, Action<TEST_FacilityBlueprint> OnPressed)
        {
            this.data = data;
            
            craftButton.onClick.RemoveAllListeners();
            craftButton.onClick.AddListener(() =>
            {
                OnPressed?.Invoke(this.data);
            });
        }
    }
}
