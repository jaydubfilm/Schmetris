using StarSalvager.AI;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JsonDataTypes;
using System.Collections.Generic;

namespace StarSalvager.Missions
{
    [System.Serializable]
    public class ComboBlocksMission : Mission
    {
        public BIT_TYPE? m_comboType;
        public int m_comboLevel;

        public ComboBlocksMission(BIT_TYPE? comboType, int comboLevel, string missionName, List<IMissionUnlockCheck> missionUnlockData, float amountNeeded) : base(missionName, amountNeeded, missionUnlockData)
        {
            MissionEventType = MISSION_EVENT_TYPE.ENEMY_KILLED;
            m_comboType = comboType;
            m_comboLevel = comboLevel;
        }

        public override bool MissionComplete()
        {
            return m_currentAmount >= m_amountNeeded;
        }

        public void ProcessMissionData(BIT_TYPE comboType, int comboLevel, int amount)
        {
            if ((m_comboType == null || comboType == m_comboType) && m_comboLevel == comboLevel)
            {
                m_currentAmount += amount;
            }
        }

        public override MissionData ToMissionData()
        {
            return new MissionData
            {
                ClassType = GetType().Name,
                MissionName = m_missionName,
                AmountNeeded = m_amountNeeded,
                CurrentAmount = m_currentAmount,
                MissionEventType = this.MissionEventType,
                MissionStatus = this.MissionStatus,
                MissionUnlockChecks = missionUnlockChecks.ExportMissionUnlockParametersDatas(),

                ResourceType = m_comboType
            };
        }
    }
}