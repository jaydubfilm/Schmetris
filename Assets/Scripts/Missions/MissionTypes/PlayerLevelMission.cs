using StarSalvager.AI;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JsonDataTypes;
using System.Collections.Generic;

namespace StarSalvager.Missions
{
    [System.Serializable]
    public class PlayerLevelMission : Mission
    {
        public int m_playerLevel;

        public PlayerLevelMission(MissionRemoteData missionRemoteData) : base(missionRemoteData)
        {
            MissionEventType = MISSION_EVENT_TYPE.PLAYER_LEVEL;
            m_playerLevel = missionRemoteData.PlayerLevel;
        }

        public PlayerLevelMission(MissionData missionData) : base(missionData)
        {
            MissionEventType = MISSION_EVENT_TYPE.PLAYER_LEVEL;
            m_playerLevel = missionData.Level;
        }

        public override bool MissionComplete()
        {
            return m_currentAmount >= m_amountNeeded;
        }

        public override void ProcessMissionData(MissionProgressEventData missionProgressEventData)
        {
            int level = missionProgressEventData.level;
            
            if (level >= m_playerLevel)
            {
                m_currentAmount += 1;
            }
        }

        public override MissionData ToMissionData()
        {
            return new MissionData
            {
                ClassType = GetType().Name,
                MissionName = m_missionName,
                MissionDescription = m_missionDescription,
                AmountNeeded = m_amountNeeded,
                CurrentAmount = m_currentAmount,
                MissionEventType = this.MissionEventType,
                MissionStatus = this.MissionStatus,
                MissionUnlockChecks = missionUnlockChecks.ExportMissionUnlockParametersDatas(),

                Level = m_playerLevel
            };
        }
    }
}