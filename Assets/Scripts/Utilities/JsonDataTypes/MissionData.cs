using Sirenix.OdinInspector;
using System.Collections.Generic;

namespace StarSalvager.Utilities.JsonDataTypes
{
    [System.Serializable]
    public struct MissionData
    {
        public string ClassType;

        public string MissionName;
        public int AmountNeeded;
        public int CurrentAmount;
        public MISSION_EVENT_TYPE MissionEventType;
        public MISSION_STATUS MissionStatus;
        public List<MissionUnlockCheckData> MissionUnlockChecks;

        public BIT_TYPE ResourceType;
        public string EnemyType;
        public int SectorNumber;
        public int WaveNumber;
    }
}
