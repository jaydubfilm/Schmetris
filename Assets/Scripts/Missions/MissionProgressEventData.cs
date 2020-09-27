using StarSalvager.AI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.Missions
{
    [System.Serializable]
    public struct MissionProgressEventData
    {
        public BIT_TYPE? bitType;
        public PART_TYPE partType;
        public COMPONENT_TYPE componentType;
        public FACILITY_TYPE facilityType;
        public string enemyTypeString;

        public int level;
        public int intAmount;
        public float floatAmount;

        public int sectorNumber;
        public int waveNumber;

        public bool bitDroppedFromEnemyLoot;
        public bool comboIsAdvancedCombo;
        public bool bumperShiftedThroughPart;
        public bool bumperOrphanedBits;
        public bool bumperCausedCombos;
    }
}