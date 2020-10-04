using Sirenix.OdinInspector;
using StarSalvager.Missions;
using System.Collections.Generic;

namespace StarSalvager.Utilities.JsonDataTypes
{
    [System.Serializable]
    public struct MissionData
    {
        public string ClassType;

        public string MissionName;
        public string MissionDescription;
        public float AmountNeeded;
        public float CurrentAmount;
        public MISSION_EVENT_TYPE MissionEventType;
        public MISSION_STATUS MissionStatus;
        public List<MissionUnlockCheckData> MissionUnlockChecks;

        public BIT_TYPE? BitType;
        public PART_TYPE PartType;
        public COMPONENT_TYPE? ComponentType;
        public FACILITY_TYPE FacilityType;
        public string EnemyTypeString;

        public int Level;
        public int IntAmount;
        public float FloatAmount;

        public int SectorNumber;
        public int WaveNumber;

        public bool BitDroppedFromEnemyLoot;
        public bool ComboIsAdvancedCombo;
        public bool BumperShiftedThroughPart;
        public bool BumperOrphanedBits;
        public bool BumperCausedCombos;
    }
}
