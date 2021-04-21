using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using StarSalvager.Factories.Data;
using UnityEngine;

namespace StarSalvager.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Persistent Upgrade Remote", menuName = "Star Salvager/Scriptable Objects/Persistent Upgrade Remote Data")]
    public class PersistentUpgradesScriptableObject : ScriptableObject
    {
        public List<UpgradeRemoteData> upgrades;

        //Functions
        //====================================================================================================================//
        
        public UpgradeRemoteData GetRemoteData(in UPGRADE_TYPE upgradeType)
        {
            var temp = upgradeType;

            return upgrades.FirstOrDefault(x => x.upgradeType == temp);
        }
        
        public float GetUpgradeValue(in UPGRADE_TYPE upgradeType, in int level)
        {
            return GetRemoteData(upgradeType).Levels[level].value;
        }
        
        public UpgradeRemoteData GetRemoteData(in UPGRADE_TYPE upgradeType, in BIT_TYPE bitType)
        {
            var temp = upgradeType;
            var tempBit = bitType;

            return upgrades.FirstOrDefault(x => x.upgradeType == temp && x.bitType == tempBit);
        }
        
        public float GetUpgradeValue(in UPGRADE_TYPE upgradeType, in int level, in BIT_TYPE bitType)
        {
            return GetRemoteData(upgradeType, bitType).Levels[level].value;
        }

        //====================================================================================================================//
        

    }
}
