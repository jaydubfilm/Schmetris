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
        public bool m_isAdvancedCombo;

        public ComboBlocksMission(BIT_TYPE? comboType, int comboLevel, bool isAdvancedCombo, string missionName, string missionDescription, List<IMissionUnlockCheck> missionUnlockData, float amountNeeded) : base(missionName, missionDescription, amountNeeded, missionUnlockData)
        {
            MissionEventType = MISSION_EVENT_TYPE.COMBO_BLOCKS;
            m_comboType = comboType;
            m_comboLevel = comboLevel;
            m_isAdvancedCombo = isAdvancedCombo;
        }

        public override bool MissionComplete()
        {
            return m_currentAmount >= m_amountNeeded;
        }

        public void ProcessMissionData(BIT_TYPE comboType, int comboLevel, int amount, bool isAdvancedCombo)
        {
            if (!isAdvancedCombo && m_isAdvancedCombo)
            {
                return;
            }
            
            if ((!m_comboType.HasValue || comboType == m_comboType) && m_comboLevel == comboLevel)
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
                MissionDescription = m_missionDescription,
                AmountNeeded = m_amountNeeded,
                CurrentAmount = m_currentAmount,
                MissionEventType = this.MissionEventType,
                MissionStatus = this.MissionStatus,
                MissionUnlockChecks = missionUnlockChecks.ExportMissionUnlockParametersDatas(),

                ResourceType = m_comboType,
                ComboLevel = m_comboLevel,
                IsAdvancedCombo = m_isAdvancedCombo
            };
        }
    }
}