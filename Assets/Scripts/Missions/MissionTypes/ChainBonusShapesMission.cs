using StarSalvager.AI;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JsonDataTypes;
using System.Collections.Generic;

namespace StarSalvager.Missions
{
    [System.Serializable]
    public class ChainBonusShapesMission : Mission
    {
        public int m_shapeNumber;

        public ChainBonusShapesMission(int shapeNumber, string missionName, string missionDescription, List<IMissionUnlockCheck> missionUnlockData, float amountNeeded = 1.0f) : base(missionName, missionDescription, amountNeeded, missionUnlockData)
        {
            MissionEventType = MISSION_EVENT_TYPE.CHAIN_BONUS_SHAPES;
            m_shapeNumber = shapeNumber;
        }

        public override bool MissionComplete()
        {
            return m_currentAmount >= m_amountNeeded;
        }

        public void ProcessMissionData(int shapeNumber)
        {
            if (shapeNumber == m_shapeNumber)
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

                BonusShapeNumber = m_shapeNumber
            };
        }
    }
}