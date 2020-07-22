﻿using StarSalvager.AI;
using System.Collections.Generic;

namespace StarSalvager
{
    [System.Serializable]
    public class EnemyKilledMission : Mission
    {
        public string m_enemyType;

        public EnemyKilledMission(string enemyType, string missionName, MISSION_UNLOCK_PARAMETERS missionUnlockType, int amountNeeded) : base(missionName, amountNeeded, missionUnlockType)
        {
            MissionEventType = MISSION_EVENT_TYPE.ENEMY_KILLED;
            m_enemyType = enemyType;
        }

        public override bool MissionComplete()
        {
            return m_currentAmount >= m_amountNeeded;
        }

        public void ProcessMissionData(string enemyType, int amount)
        {
            if (enemyType == m_enemyType)
            {
                m_currentAmount += amount;
            }
        }
    }
}