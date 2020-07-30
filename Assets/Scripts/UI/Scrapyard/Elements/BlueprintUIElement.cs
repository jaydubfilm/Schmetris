using System;
using TMPro;
using UnityEngine;

namespace StarSalvager.UI.Scrapyard
{

    
    public class BlueprintUIElement : ButtonReturnUIElement<TEST_Blueprint, string>
    {
        [SerializeField]
        private TMP_Text titleText;
        
        public override void Init(TEST_Blueprint data, Action<string> OnPressed)
        {
            this.data = data;

            titleText.text = data.name;
            
            button.onClick.AddListener(() =>
            {
                OnPressed?.Invoke(data.name);
            });
        }
    }
    
    public class TEST_Blueprint : IEquatable<TEST_Blueprint>
    {
        public string name;

        public bool Equals(TEST_Blueprint other)
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
            return Equals((TEST_Blueprint) obj);
        }

        public override int GetHashCode()
        {
            return (name != null ? name.GetHashCode() : 0);
        }
    }
}

