using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using StarSalvager.Parts.Data;
using UnityEngine;
using UnityEngine.Serialization;

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

            [FormerlySerializedAs("cost")] [TableColumnWidth(75, false)]
            public int gears;
            [TableColumnWidth(75, false)]
            public int silver;
            [TableColumnWidth(300, true), Range(0f, 1f)]
            public float multiplier;
            
            [DisplayAsString, PropertyOrder(-100), TableColumnWidth(50, false)]
            public int level;
        }

        //Properties
        //====================================================================================================================//
        
        [FoldoutGroup("$title")] public bool isImplemented;
        
        [FoldoutGroup("$title")] public string name;

        [FoldoutGroup("$title")] public PATCH_TYPE type;

        [TextArea, FoldoutGroup("$title")] public string description;

        [InfoBox("$GetInfoBox", InfoMessageType.Info)]
        [FoldoutGroup("$title"), TableList(AlwaysExpanded = true),LabelText("Patch Level Data"), OnValueChanged("UpdateLevels")]
        public List<Data> Levels;

        //====================================================================================================================//

        public float GetMultiplier(in int level)
        {
            return Levels[level].multiplier;
        }

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

        private string GetInfoBox()
        {
            switch (type)
            {
                case PATCH_TYPE.POWER:
                case PATCH_TYPE.RANGE:
                    return "Multiplier represents the additional percentage on top of default value";
                case PATCH_TYPE.REINFORCED:
                case PATCH_TYPE.EFFICIENCY:
                case PATCH_TYPE.FIRE_RATE:
                    return  "Multiplier represents the discounted percentage from the default value";
                
                case PATCH_TYPE.AOE:
                case PATCH_TYPE.BURN:
                    throw new NotImplementedException($"{type} not yet implemented");
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
        
#endif

        #endregion //Unity Editor
        
        //====================================================================================================================//
        
    }
}
