using System;
using Newtonsoft.Json;
using StarSalvager.Factories;

namespace StarSalvager 
{
    [Obsolete]
    public class Blueprint : IEquatable<Blueprint>
    {
        public string name;
        public PART_TYPE partType;

        [JsonIgnore]
        public string DisplayString => $"{GetDisplayName()}";

        private string GetDisplayName()
        {
            var factoryManager = FactoryManager.Instance;
            return factoryManager is null ? string.Empty : factoryManager.PartsRemoteData.GetRemoteData(partType).name;
        }

        #region IEquatable

        public bool Equals(Blueprint other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return name == other.name && partType.Equals(other.partType);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Blueprint)obj);
        }

        public override int GetHashCode()
        {
            return (name != null ? name.GetHashCode() : 0);
        }

        #endregion //IEquatable
    }
}
