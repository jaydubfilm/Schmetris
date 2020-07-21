using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager
{
    public class MissionsCurrentData
    {
        //TODO: Switch this set of lists into a dictionary with key = type, value = list<mission>
        public List<ResourceCollectedMission> m_resourceCollectedMissions;
        public List<EnemyKilledMission> m_enemyKilledMissions;
        public List<ComboBlocksMission> m_comboBlocksMissions;
        public List<LevelProgressMission> m_levelProgressMissions;

        public List<Mission> m_notStartedMissions;
        public List<Mission> m_completedMissions;

        public MissionsCurrentData()
        {
            m_resourceCollectedMissions = new List<ResourceCollectedMission>();
            m_enemyKilledMissions = new List<EnemyKilledMission>();
            m_comboBlocksMissions = new List<ComboBlocksMission>();
            m_levelProgressMissions = new List<LevelProgressMission>();
            m_completedMissions = new List<Mission>();
            m_notStartedMissions = new List<Mission>();
        }

        public void AddMission(Mission mission)
        {
            if (m_notStartedMissions.Find(m => m.m_missionName == mission.m_missionName) != null)
            {
                m_notStartedMissions.RemoveAll(m => m.m_missionName == mission.m_missionName);
                switch (mission)
                {
                    case ResourceCollectedMission resourceCollectedMission:
                        m_resourceCollectedMissions.Add(resourceCollectedMission);
                        break;
                    case EnemyKilledMission enemyKilledMission:
                        m_enemyKilledMissions.Add(enemyKilledMission);
                        break;
                    case ComboBlocksMission comboBlocksMission:
                        m_comboBlocksMissions.Add(comboBlocksMission);
                        break;
                    case LevelProgressMission levelProgressMission:
                        m_levelProgressMissions.Add(levelProgressMission);
                        break;
                }
            }
        }

        public void AddMission(Mission mission, int atIndex)
        {
            if (m_notStartedMissions.Find(m => m.m_missionName == mission.m_missionName) != null)
            {
                m_notStartedMissions.RemoveAll(m => m.m_missionName == mission.m_missionName);
                switch (mission)
                {
                    case ResourceCollectedMission resourceCollectedMission:
                        m_resourceCollectedMissions.Insert(atIndex, resourceCollectedMission);
                        break;
                    case EnemyKilledMission enemyKilledMission:
                        m_enemyKilledMissions.Insert(atIndex, enemyKilledMission);
                        break;
                    case ComboBlocksMission comboBlocksMission:
                        m_comboBlocksMissions.Insert(atIndex, comboBlocksMission);
                        break;
                    case LevelProgressMission levelProgressMission:
                        m_levelProgressMissions.Insert(atIndex, levelProgressMission);
                        break;
                }
            }
        }
    }
}