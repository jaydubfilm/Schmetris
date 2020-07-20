using StarSalvager.AI;
using System.Collections.Generic;

namespace StarSalvager
{
    public class EnemyKilledMission : Mission
    {
        private ENEMY_TYPE m_enemyType;
        private int m_amountNeeded;
        private int m_currentAmount;

        public EnemyKilledMission()
        {
            MissionEventType = MISSION_EVENT_TYPE.ENEMY_KILLED;
        }

        public override bool MissionComplete()
        {
            return m_currentAmount >= m_amountNeeded;
        }

        public void ProcessMissionData(ENEMY_TYPE resourceType, int amount)
        {
            if (resourceType == m_enemyType)
            {
                m_currentAmount += amount;
            }
        }
    }
}