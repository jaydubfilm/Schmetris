using Sirenix.OdinInspector;
using System.Collections.Generic;

namespace StarSalvager.Utilities.JsonDataTypes
{
    [System.Serializable]
    public struct MissionData
    {
        public string ClassType;

        public string m_missionName;
        public int m_amountNeeded;
        public int m_currentAmount;
        public MISSION_EVENT_TYPE MissionEventType;
        public MISSION_STATUS MissionStatus;

        public List<MissionUnlockCheck> missionUnlockChecks;
    }
}
