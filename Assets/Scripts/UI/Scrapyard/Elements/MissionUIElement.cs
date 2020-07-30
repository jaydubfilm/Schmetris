using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StarSalvager.UI.Scrapyard
{
    public class MissionUIElement : UIElement<TEST_Mission>
    {
        [SerializeField]
        private TMP_Text title;

        [SerializeField]
        private Button favouriteButton;

        public void Init(TEST_Mission data, Action<TEST_Mission> OnFavPressed)
        {
            Init(data);
            
            favouriteButton.onClick.AddListener(() =>
            {
                OnFavPressed?.Invoke(data);
            });
        }
        
        public override void Init(TEST_Mission data)
        {
            this.data = data;

            title.text = data.name;
        }
    }

    public class TEST_Mission : IEquatable<TEST_Mission>
    {
        public string name;


        public bool Equals(TEST_Mission other)
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
            return Equals((TEST_Mission) obj);
        }

        public override int GetHashCode()
        {
            return (name != null ? name.GetHashCode() : 0);
        }
    }
}


