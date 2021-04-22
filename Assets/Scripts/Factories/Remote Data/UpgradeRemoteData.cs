using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using StarSalvager.Utilities.Extensions;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace StarSalvager.Factories.Data
{
    [Serializable]
    public class UpgradeRemoteData : RemoteDataBase
    {
        //Structs
        //====================================================================================================================//

        [Serializable]
        public struct Data
        {

            [TableColumnWidth(75, false), HideIf("@level == 0")]
            public int cost;

            [TableColumnWidth(300, true)]
            public float value;

#if UNITY_EDITOR
            [DisplayAsString, PropertyOrder(-100), TableColumnWidth(50, false)]
            public string Level;
            [HideInTables]
            public int level;
#endif
        }

        //====================================================================================================================//
        [FoldoutGroup("$title"), PreviewField]
        public Sprite sprite;
        
        [FoldoutGroup("$title"), OnValueChanged("ShouldResetBitType")]
        public UPGRADE_TYPE upgradeType;

        [FoldoutGroup("$title"), ShowIf("UsesBitType")]
        public BIT_TYPE bitType;
        
        [FoldoutGroup("$title")]
        public string name;
        
        [FoldoutGroup("$title"), TextArea]
        public string description;
        
        [FoldoutGroup("$title"), TableList(AlwaysExpanded = true), LabelText("Upgrade Level Data"), OnValueChanged("UpdateLevels")]
        public List<Data> Levels = new List<Data>()
        {
            new Data()
        };

        //Unity Editor
        //====================================================================================================================//

        #region Unity Editor

        public string title => upgradeType == UPGRADE_TYPE.CATEGORY_EFFICIENCY ? $"{bitType} - {name}" : name;

#if UNITY_EDITOR
        
        [OnInspectorInit]
        private void UpdateLevels()
        {
            if (Levels.IsNullOrEmpty())
                return;
            
            for (var i = 0; i < Levels.Count; i++)
            {
                var level = Levels[i];
                level.level = i;
                level.Level = i == 0 ? "Default" : $"{i}";

                Levels[i] = level;
            }
        }

        [FoldoutGroup("$title"), Button("See in Game Setting"), ShowIf(nameof(ShowGoToGameSettings))]
        private void GoToGameSettings()
        {
            var path = AssetDatabase.GetAssetPath(Object.FindObjectOfType<GameManager>().GameSettings);
            Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(path);
        }
        
        private bool ShowGoToGameSettings()
        {
            switch (upgradeType)
            {
                case UPGRADE_TYPE.AMMO_CAPACITY:
                case UPGRADE_TYPE.STARTING_CURRENCY:
                    return true;
                default:
                    return false;
            }
        }

        private bool UsesBitType()
        {
            switch (upgradeType)
            {
                case UPGRADE_TYPE.CATEGORY_EFFICIENCY:
                    return true;
                default:
                    return false;
            }
        }

        private void ShouldResetBitType()
        {
            if (UsesBitType())
                return;

            bitType = BIT_TYPE.NONE;
        }
        
#endif

        #endregion //Unity Editor
        
        //====================================================================================================================//

        #region IEquatable

        public override bool Equals(RemoteDataBase other)
        {
            if (other is UpgradeRemoteData upgradeRemote)
                return upgradeType == upgradeRemote.upgradeType;

            return false;
        }

        /// <summary>
        /// This only compares Type and not all individual properties
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((UpgradeRemoteData) obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion //IEquatable
    }

}