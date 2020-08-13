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
        public PART_TYPE PartType;
        public int PartLevel;
        public bool ThroughPart;
        public float FlightLength;
    }
}
