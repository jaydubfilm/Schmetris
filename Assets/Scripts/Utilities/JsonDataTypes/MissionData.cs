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

        public BIT_TYPE? ResourceType;
        public int ComboLevel;
        public string EnemyType;
        public int SectorNumber;
        public int WaveNumber;
        public int BonusShapeNumber;
        public PART_TYPE PartType;
        public FACILITY_TYPE FacilityType;
        public int PartLevel;
        public int FacilityLevel;
        public int PlayerLevel;
        public bool ThroughPart;
        public bool OrphanBit;
        public bool HasCombos;
        public float FlightLength;
        public bool IsFromEnemyLoot;
        public bool IsAdvancedCombo;
        public COMPONENT_TYPE? ComponentType;
    }
}
