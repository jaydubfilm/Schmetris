using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JsonDataTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.ScriptableObjects
{
    [Serializable]
    public class PlayerLevelRemoteData
    {
        public int GearsToLevelUp;

        [SerializeField]
        private int maxDrops;

        [SerializeField]
        private List<RDSLootData> RDSLevelUpLoot = new List<RDSLootData>();

        public RDSTable rdsTable;

        public void ConfigureLootTable()
        {
            rdsTable = new RDSTable();
            rdsTable.SetupRDSTable(maxDrops, RDSLevelUpLoot);
        }
    }
}