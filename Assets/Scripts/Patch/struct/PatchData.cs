using System;
using System.Collections;
using System.Linq;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using StarSalvager.Factories;
using StarSalvager.ScriptableObjects;
using StarSalvager.Utilities;
using UnityEngine;

namespace StarSalvager
{
    [Serializable]
    public struct PatchData : IEquatable<PatchData>
    {
        [ValueDropdown("GetPatches"), OnValueChanged("UpdateLevelRange")]
        public int Type;
        [PropertyRange("_minLevel", "_maxLevel"), DisableIf("Type", 0)]
        public int Level;

        public override string ToString()
        {
            return $"{(PATCH_TYPE)Type} {Mathfx.ToRoman(Level + 1)}";
        }

        #region IEquatable

        public bool Equals(PatchData other)
        {
            return Type == other.Type && Level == other.Level;
        }

        public override bool Equals(object obj)
        {
            return obj is PatchData other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Type * 397) ^ Level;
            }
        }

        #endregion //IEquatable

        //Unity Editor
        //====================================================================================================================//
        
#if UNITY_EDITOR

        [SerializeField, JsonIgnore, HideInInspector]
        private int _minLevel;
        [SerializeField, JsonIgnore, HideInInspector]
        private int _maxLevel;
        private IEnumerable GetPatches()=> PatchRemoteDataScriptableObject.GetImplementedPatchesInt(false);

        [OnInspectorInit]
        private void UpdateLevelRange()
        {
            var patchType = (PATCH_TYPE) Type;

            if (patchType == PATCH_TYPE.EMPTY)
            {
                Level = _minLevel = _maxLevel = 0;
                return;
            }
            
            var remoteData = UnityEngine.Object.FindObjectOfType<FactoryManager>().PatchRemoteData.GetRemoteData(patchType);
            _minLevel = remoteData.Levels.Min(x => x.level);
            _maxLevel = remoteData.Levels.Max(x => x.level);

            Level = Mathf.Clamp(Level, _minLevel, _maxLevel);
        }
        
#endif
    }
}
