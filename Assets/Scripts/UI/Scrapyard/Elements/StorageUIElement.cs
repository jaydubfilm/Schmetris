using StarSalvager.Utilities.JsonDataTypes;
using System;
using TMPro;
using UnityEngine;

namespace StarSalvager.UI.Scrapyard
{
    public class StorageUIElement : UIElement<TEST_Storage>
    {
        [SerializeField]
        private TMP_Text title;
        
        
        public override void Init(TEST_Storage data)
        {
            this.data = data;
            
            title.text = data.name;

        }
    }
    
    public class TEST_Storage : IEquatable<TEST_Storage>
    {
        public string name;
        public BlockData blockData;

        public bool Equals(TEST_Storage other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return name == other.name && blockData.Equals(other.blockData);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TEST_Storage) obj);
        }

        public override int GetHashCode()
        {
            return (name != null ? name.GetHashCode() : 0);
        }
    }
}

