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
        
        public float GetUpgradeValue(in UPGRADE_TYPE upgradeType, in BIT_TYPE bitType, in int level)
        {
            return GetRemoteData(upgradeType, bitType).Levels[level].value;
        }
        
        public UpgradeRemoteData GetRemoteData(in UPGRADE_TYPE upgradeType, in BIT_TYPE bitType)
        {
            var temp = upgradeType;
            var tempBit = bitType;

            return upgrades.FirstOrDefault(x => x.upgradeType == temp && x.bitType == tempBit);
        }

        //====================================================================================================================//
        

    }
}
