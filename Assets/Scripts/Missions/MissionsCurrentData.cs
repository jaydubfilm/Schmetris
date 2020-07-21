using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager
{
    public class MissionsCurrentData
    {
        public List<ResourceCollectedMission> m_resourceCollectedMissions;
        public List<EnemyKilledMission> m_enemyKilledMissions;

        public List<Mission> m_completedMissions;

        public MissionsCurrentData()
        {
            m_resourceCollectedMissions = new List<ResourceCollectedMission>();
            m_enemyKilledMissions = new List<EnemyKilledMission>();
            m_completedMissions = new List<Mission>();
        }

        public void AddMission(Mission mission)
        {
            switch(mission)
            {
                case ResourceCollectedMission resourceCollectedMission:
                    m_resourceCollectedMissions.Add(resourceCollectedMission);
                    break;
                case EnemyKilledMission enemyKilledMission:
                    m_enemyKilledMissions.Add(enemyKilledMission);
                    break;
            }
        }

        public void AddMission(Mission mission, int atIndex)
        {
            switch (mission)
            {
                case ResourceCollectedMission resourceCollectedMission:
                    m_resourceCollectedMissions.Insert(atIndex, resourceCollectedMission);
                    break;
                case EnemyKilledMission enemyKilledMission:
                    m_enemyKilledMissions.Insert(atIndex, enemyKilledMission);
                    break;
            }
        }
    }
}