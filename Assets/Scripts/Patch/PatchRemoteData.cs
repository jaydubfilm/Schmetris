using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using StarSalvager.Parts.Data;
using UnityEngine;

namespace StarSalvager.Factories.Data
{
    [Serializable]
    public class PatchRemoteData
    {
        //Structs
        //====================================================================================================================//
        
        [Serializable]
        public struct Data
        {

            [TableColumnWidth(75, false)]
            public int cost;
            [TableColumnWidth(300, true), Range(0f, 1f)]
            public float multiplier;

#if UNITY_EDITOR
            [DisplayAsString, PropertyOrder(-100), TableColumnWidth(50, false)]
            public int level;
#endif
        }
        
        

        //Properties
        //====================================================================================================================//
        
        [FoldoutGroup("$title")] public bool isImplemented;
        
        [FoldoutGroup("$title")] public string name;

        [FoldoutGroup("$title")] public PATCH_TYPE type;

        [TextArea, FoldoutGroup("$title")] public string description;

        [FoldoutGroup("$title"), TableList(AlwaysExpanded = true),LabelText("Patch Level Data"), OnValueChanged("UpdateLevels")]
        public List<Data> Levels;



        //====================================================================================================================//

        public float GetMultiplier(in int level)
        {
            return Levels[level].multiplier;
        }
        
        /*public T GetDataValue<T>(in int level, PartProperties.KEYS key)
        {
            var keyString = PartProperties.Names[(int)key];
            var dataValue = Levels[level].Properties.FirstOrDefault(d => d.key.Equals(keyString));

            if (dataValue.Equals(null))
                return default;

            if (!(dataValue.GetValue() is T i))
                return default;

            return i;
        }

        public bool TryGetValue<T>(in int level, PartProperties.KEYS key, out T value)
        {
            value = default;

            var keyString = PartProperties.Names[(int)key];
            var dataValue = Levels[level].Properties.FirstOrDefault(d => d.key.Equals(keyString));

            if (dataValue.Equals(null))
                return false;

            if (!(dataValue.GetValue() is T i))
                return false;

            value = i;

            return true;
        }*/

        //Unity Editor
        //====================================================================================================================//

        #region Unity Editor

#if UNITY_EDITOR
        
        public string title => $"{name} {(isImplemented ? string.Empty : "[NOT IMPLEMENTED]")}";

        [OnInspectorInit]
        private void UpdateLevels()
        {
            for (var i = 0; i < Levels.Count; i++)
            {
                var level = Levels[i];
                level.level = i + 1;

                Levels[i] = level;
            }
        }
        
#endif

        #endregion //Unity Editor
        
        //====================================================================================================================//
        
    }
}
