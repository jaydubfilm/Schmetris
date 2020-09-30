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

        public ComboBlocksMission(MissionRemoteData missionRemoteData) : base(missionRemoteData)
        {
            MissionEventType = MISSION_EVENT_TYPE.COMBO_BLOCKS;
            m_comboType = missionRemoteData.ResourceValue();
            m_comboLevel = missionRemoteData.ComboLevel;
            m_isAdvancedCombo = missionRemoteData.IsAdvancedCombo;
        }

        public ComboBlocksMission(MissionData missionData) : base(missionData)
        {
            MissionEventType = MISSION_EVENT_TYPE.COMBO_BLOCKS;
            m_comboType = missionData.BitType;
            m_comboLevel = missionData.Level;
            m_isAdvancedCombo = missionData.ComboIsAdvancedCombo;
        }

        public override bool MissionComplete()
        {
            return currentAmount >= amountNeeded;
        }

        public override void ProcessMissionData(MissionProgressEventData missionProgressEventData)
        {
            BIT_TYPE bitType = missionProgressEventData.bitType.Value;
            int amount = missionProgressEventData.intAmount;
            int level = missionProgressEventData.level;
            bool isAdvancedCombo = missionProgressEventData.comboIsAdvancedCombo;

            if (!isAdvancedCombo && m_isAdvancedCombo)
            {
                return;
            }

            if ((!m_comboType.HasValue || bitType == m_comboType) && m_comboLevel == level)
            {
                currentAmount += amount;
            }
        }

        public override MissionData ToMissionData()
        {
            return new MissionData
            {
                ClassType = GetType().Name,
                MissionName = missionName,
                MissionDescription = missionDescription,
                AmountNeeded = amountNeeded,
                CurrentAmount = currentAmount,
                MissionEventType = this.MissionEventType,
                MissionStatus = this.MissionStatus,
                MissionUnlockChecks = missionUnlockChecks.ExportMissionUnlockParametersDatas(),

                BitType = m_comboType,
                Level = m_comboLevel,
                ComboIsAdvancedCombo = m_isAdvancedCombo
            };
        }
    }
}