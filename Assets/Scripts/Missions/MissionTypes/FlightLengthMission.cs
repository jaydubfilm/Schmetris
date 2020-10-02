using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JsonDataTypes;
using System.Collections.Generic;
using System.Diagnostics;

namespace StarSalvager.Missions
{
    [System.Serializable]
    public class FlightLengthMission : Mission
    {
        float m_flightLength;

        public FlightLengthMission(MissionRemoteData missionRemoteData) : base(missionRemoteData)
        {
            MissionEventType = MISSION_EVENT_TYPE.FLIGHT_LENGTH;
            m_flightLength = missionRemoteData.FlightLength;
        }

        public FlightLengthMission(MissionData missionData) : base(missionData)
        {
            MissionEventType = MISSION_EVENT_TYPE.FLIGHT_LENGTH;
            m_flightLength = missionData.FloatAmount;
        }

        public override bool MissionComplete()
        {
            return currentAmount >= amountNeeded;
        }

        public override void ProcessMissionData(MissionProgressEventData missionProgressEventData)
        {
            float amount = missionProgressEventData.floatAmount;
            
            if (amount >= m_flightLength)
            {
                Debug.WriteLine(amount + " --- " + m_flightLength);
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
                curAmount = (int)LevelManager.Instance.LevelTimer;
            }

            return $" ({ +curAmount}/{ +m_flightLength})";
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

                FloatAmount = m_flightLength
            };
        }
    }
}