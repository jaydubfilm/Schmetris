using StarSalvager.AI;
using System.Collections.Generic;

namespace StarSalvager
{
    [System.Serializable]
    public class EnemyKilledMission : Mission
    {
        public ENEMY_TYPE m_enemyType;

        public EnemyKilledMission(ENEMY_TYPE enemyType, string missionName, int amountNeeded) : base(missionName, amountNeeded)
        {
            MissionEventType = MISSION_EVENT_TYPE.ENEMY_KILLED;
            m_enemyType = enemyType;
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