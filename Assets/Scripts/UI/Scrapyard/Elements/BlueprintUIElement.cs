using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using StarSalvager.Utilities.JsonDataTypes;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StarSalvager.UI.Scrapyard
{

    
    public class BlueprintUIElement : ButtonReturnUIElement<TEST_Blueprint, TEST_Blueprint>
    {
        [SerializeField]
        private TMP_Text titleText;

        [SerializeField]
        private Image image;
        
        public override void Init(TEST_Blueprint data, Action<TEST_Blueprint> OnPressed)
        {
            this.data = data;

            titleText.text = data.name;
            image.sprite = FactoryManager.Instance.GetFactory<PartAttachableFactory>().GetProfileData(data.remoteData.partType).Sprites[data.level];
            
            button.onClick.AddListener(() =>
            {
                OnPressed?.Invoke(data);
            });
        }
    }
    
    public class TEST_Blueprint : IEquatable<TEST_Blueprint>
    {
        public string name;
        public PartRemoteData remoteData;
        public int level;

        public bool Equals(TEST_Blueprint other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return name == other.name && remoteData.Equals(other.remoteData) && level == other.level;
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

