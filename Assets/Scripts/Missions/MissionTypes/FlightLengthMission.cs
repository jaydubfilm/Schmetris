﻿using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JsonDataTypes;
using System.Collections.Generic;

namespace StarSalvager.Missions
{
    [System.Serializable]
    public class FlightLengthMission : Mission
    {
        float m_flightLength;
        
        public FlightLengthMission(float flightLength, string missionName, List<IMissionUnlockCheck> missionUnlockData, float amountNeeded = 1.0f) : base(missionName, amountNeeded, missionUnlockData)
        {
            MissionEventType = MISSION_EVENT_TYPE.FLIGHT_LENGTH;
            m_flightLength = flightLength;
        }

        public override bool MissionComplete()
        {
            return m_currentAmount >= m_amountNeeded;
        }

        public void ProcessMissionData(float flightLength)
        {
            if (flightLength >= m_flightLength)
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
                AmountNeeded = m_amountNeeded,
                CurrentAmount = m_currentAmount,
                MissionEventType = this.MissionEventType,
                MissionStatus = this.MissionStatus,
                MissionUnlockChecks = missionUnlockChecks.ExportMissionUnlockParametersDatas(),

                FlightLength = m_flightLength
            };
        }
    }
}