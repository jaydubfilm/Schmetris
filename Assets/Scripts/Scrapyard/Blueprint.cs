﻿using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using StarSalvager.Values;
using UnityEngine;

namespace StarSalvager 
{
    public class Blueprint : IEquatable<Blueprint>
    {
        public string name;
        public PART_TYPE partType;
        public int level;

        [JsonIgnore]
        public bool CanAfford => PlayerPersistentData.PlayerData.CanAffordPart(partType, level, false);

        #region IEquatable

        public bool Equals(Blueprint other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return name == other.name && partType.Equals(other.partType) && level == other.level;
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
