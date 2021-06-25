using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using StarSalvager.Utilities.JSON.Converters;
using UnityEngine;

namespace StarSalvager.Utilities.JsonDataTypes
{
    [Serializable]
    public struct PartData : IBlockData, IEquatable<PartData>
    {
        [ShowInInspector, HideInTables]
        public string ClassType => nameof(Part);

        [ShowInInspector, JsonConverter(typeof(Vector2IntConverter)), HideInTables]
        public Vector2Int Coordinate { get; set; }

        public int Type { get; set; }

        public List<PatchData> Patches { get; set; }

        public PartData(in PartData partData)
        {
            Coordinate = partData.Coordinate;
            Type = partData.Type;
            Patches = new List<PatchData>(partData.Patches);
        }
        
        public void AddPatch(in PatchData patchData)
        {
            for (int i = 0; i < Patches.Count; i++)
            {
                if(Patches[i].Type != (int)PATCH_TYPE.EMPTY)
                    continue;

                Patches[i] = patchData;
                return;
            }

            throw new Exception("No available space for new patch");
        }

        #region IEquatable

        public bool Equals(PartData other)
        {
            return Coordinate.Equals(other.Coordinate) && Type == other.Type && Equals(Patches, other.Patches);
        }

        public override bool Equals(object obj)
        {
            return obj is PartData other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Coordinate.GetHashCode();
                hashCode = (hashCode * 397) ^ Type;
                hashCode = (hashCode * 397) ^ (Patches != null ? Patches.GetHashCode() : 0);
                return hashCode;
            }
        }

        #endregion //IEquatable

#if UNITY_EDITOR
        [JsonIgnore, ShowInInspector, DisplayAsString] 
        public string PartTypeName => ((PART_TYPE) Type).ToString();
        [JsonIgnore, ShowInInspector, DisplayAsString] 
        public string PatchNames => string.Join(", ",
            Patches.Select(x => x.ToString()));
#endif
    }
}
