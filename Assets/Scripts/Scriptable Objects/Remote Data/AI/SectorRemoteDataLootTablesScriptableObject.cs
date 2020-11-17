using StarSalvager.Values;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Sector Remote Loot Tables", menuName = "Star Salvager/Scriptable Objects/Sector_Remote_Data_Loot_Tables")]
    public class SectorRemoteDataLootTablesScriptableObject : ScriptableObject
    {
        [SerializeField]
        public List<SectorLootTableScriptableObject> SectorRemoteDataLootTables = new List<SectorLootTableScriptableObject>();

        [SerializeField]
        private SectorLootTableScriptableObject SectorRemoteDataBackupLootTable;

        public SectorLootTableScriptableObject GetLootTableAtIndex(int i)
        {
            if (SectorRemoteDataLootTables.Count <= i)
            {
                return SectorRemoteDataBackupLootTable;
            }
            
            return SectorRemoteDataLootTables[i];
        }
    }
}