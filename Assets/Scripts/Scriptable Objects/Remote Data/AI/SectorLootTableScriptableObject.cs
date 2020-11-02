using StarSalvager.Utilities.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.ScriptableObjects
{
    [Serializable]
    public class SectorLootTableScriptableObject
    {
        [SerializeField]
        private int maxDrops = 1;

        [SerializeField]
        private List<RDSLootData> RDSEndOfWaveLoot = new List<RDSLootData>();

        public RDSTable rdsTable;

        public void ConfigureLootTable()
        {
            rdsTable = new RDSTable();
            rdsTable.SetupRDSTable(maxDrops, RDSEndOfWaveLoot);
        }
    }
}