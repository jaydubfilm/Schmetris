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

        public ChainBonusShapesMission(MissionRemoteData missionRemoteData) : base(missionRemoteData)
        {
            MissionEventType = MISSION_EVENT_TYPE.CHAIN_BONUS_SHAPES;
            m_shapeNumber = missionRemoteData.BonusShapeNumber;
        }

        public ChainBonusShapesMission(MissionData missionData) : base(missionData)
        {
            MissionEventType = MISSION_EVENT_TYPE.CHAIN_BONUS_SHAPES;
            m_shapeNumber = missionData.IntAmount;
        }

        public override bool MissionComplete()
        {
            return currentAmount >= amountNeeded;
        }

        public override void ProcessMissionData(MissionProgressEventData missionProgressEventData)
        {
            int shapeNumber = missionProgressEventData.intAmount;
            
            if (shapeNumber == m_shapeNumber)
            {
                currentAmount += 1;
            }
        }

        public override string GetMissionProgressString()
        {
            if (MissionComplete())
            {
                return "";
            }

            int curAmount = 0;
            if (LevelManager.Instance != null && LevelManager.Instance.WaveEndSummaryData != null)
            {
                curAmount = LevelManager.Instance.WaveEndSummaryData.numBonusShapesMatched;
            }

            if (curAmount == 0 && amountNeeded == 1)
            {
                return "";
            }

            return $" ({ +curAmount}/{ +m_shapeNumber})";
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

                IntAmount = m_shapeNumber
            };
        }
    }
}