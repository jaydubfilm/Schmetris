﻿using StarSalvager.AI;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JsonDataTypes;
using System.Collections.Generic;

namespace StarSalvager.Missions
{
    [System.Serializable]
    public class EnemyKilledMission : Mission
    {
        public string m_enemyType;

        public EnemyKilledMission(MissionRemoteData missionRemoteData) : base(missionRemoteData)
        {
            MissionEventType = MISSION_EVENT_TYPE.ENEMY_KILLED;
            m_enemyType = missionRemoteData.EnemyValue();
        }

        public EnemyKilledMission(MissionData missionData) : base(missionData)
        {
            MissionEventType = MISSION_EVENT_TYPE.ENEMY_KILLED;
            m_enemyType = missionData.EnemyTypeString;
        }

        public override bool MissionComplete()
        {
            return currentAmount >= amountNeeded;
        }

        public override void ProcessMissionData(MissionProgressEventData missionProgressEventData)
        {
            string enemyType = missionProgressEventData.enemyTypeString;
            int amount = missionProgressEventData.intAmount;
            
            if (m_enemyType == null || m_enemyType == string.Empty || enemyType == m_enemyType)
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

                EnemyTypeString = m_enemyType
            };
        }
    }
}