using System;

namespace StarSalvager.PersistentUpgrades.Data
{
    public struct UpgradeData : IEquatable<UpgradeData>
    {
        public readonly UPGRADE_TYPE Type;
        public readonly BIT_TYPE BitType;
        public int Level { get; set; }

        public UpgradeData(in UPGRADE_TYPE upgradeType, in int level)
        {
            Type = upgradeType;
            BitType = BIT_TYPE.NONE;
            Level = level;
        }
        public UpgradeData(in UPGRADE_TYPE upgradeType, in BIT_TYPE bitType, in int level)
        {
            Type = upgradeType;
            BitType = bitType;
            Level = level;
        }

        #region IEquatable

        public bool Equals(UpgradeData other)
        {
            return Type == other.Type && BitType == other.BitType && Level == other.Level;
        }

        public override bool Equals(object obj)
        {
            return obj is UpgradeData other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int) Type;
                hashCode = (hashCode * 397) ^ (int) BitType;
                hashCode = (hashCode * 397) ^ Level;
                return hashCode;
            }
        }

        #endregion //IEquatable
        
    }
}
