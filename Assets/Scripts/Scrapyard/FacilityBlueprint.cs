using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager 
{
    public class FacilityBlueprint : IEquatable<FacilityBlueprint>
    {
        public string name;
        public FACILITY_TYPE facilityType;
        public int level;


        #region IEquatable

        public bool Equals(FacilityBlueprint other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return name == other.name && facilityType.Equals(other.facilityType) && level == other.level;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((FacilityBlueprint)obj);
        }

        public override int GetHashCode()
        {
            return (name != null ? name.GetHashCode() : 0);
        }

        #endregion //IEquatable
    }
}
