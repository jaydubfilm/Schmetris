using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using StarSalvager.ScriptableObjects;
using StarSalvager.Utilities.Extensions;
using UnityEngine;
using Object = UnityEngine.Object;

namespace StarSalvager.Factories.Data
{
    [Serializable]
    public class PlayerLevelRemoteData
    {
        public enum UNLOCK_TYPE
        {
            PART,
            PATCH
        }

        [Serializable]
        public struct UnlockData
        {
            [HideInTables]
            public int levelIndex;
            
            public UNLOCK_TYPE Unlock;
            [VerticalGroup("Type"), ShowIf("$Unlock", UNLOCK_TYPE.PART), HideLabel, ValueDropdown("GetImplementedParts")]
            public PART_TYPE PartType;
            [VerticalGroup("Type"), ShowIf("$Unlock", UNLOCK_TYPE.PATCH), HideLabel, ValueDropdown("GetImplementedPatches")]
            public PATCH_TYPE PatchType;
            [ShowIf("$Unlock", UNLOCK_TYPE.PATCH)]
            public int Level;
#if UNITY_EDITOR

            private IEnumerable GetImplementedParts()
            {
                return Object.FindObjectOfType<FactoryManager>().PlayerLevelsRemoteData.GetRemainingPartOptions(levelIndex);
            }
            private IEnumerable GetImplementedPatches() => PatchRemoteDataScriptableObject.GetImplementedPatches();
            
#endif
        }

        //====================================================================================================================//
        
        [FoldoutGroup("$title"), HideInInspector]
        public int level;

        //TODO If I plan to allow overrides on Level XP, then I need to clamp the values
        [FoldoutGroup("$title"), HorizontalGroup("$title/Row1"), LabelWidth(150)]
        public bool overrideXPRequired;
        [FoldoutGroup("$title"), HorizontalGroup("$title/Row1"), EnableIf("$overrideXPRequired"), HideLabel, SuffixLabel("xp", true)]
        public int xpRequired;

        [FoldoutGroup("$title"), ReadOnly]
        public bool givesStarPoint = true;

        [FoldoutGroup("$title"), TableList, OnValueChanged("UpdateUnlockData")]
        public List<UnlockData> unlockData;

#if UNITY_EDITOR
        
        private string title => $"Level {level} | {(overrideXPRequired ? "[Override] " : "")}" +
                                $"{xpRequired}xp" +
                                $"{(givesStarPoint ? " | +1 Star" : "")}" +
                                $"{(unlockData.IsNullOrEmpty() ? "" : $" | +{unlockData.Count} Item(s)")}";

        [OnInspectorInit]
        //This is used to ensure that level value is set so when a list is presented to the designer it only shows remaining options
        private void UpdateUnlockData()
        {
            for (int i = 0; i < unlockData.Count; i++)
            {
                var data = unlockData[i];
                data.levelIndex = level;
                unlockData[i] = data;
            }
        }
        
        
#endif
    }
}
