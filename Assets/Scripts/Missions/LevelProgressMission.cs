using StarSalvager.AI;
using System.Collections.Generic;

namespace StarSalvager
{
    [System.Serializable]
    public class LevelProgressMission : Mission
    {
        public int m_sectorNumber;
        public int m_waveNumber;

        public LevelProgressMission(int sectorNumber, int waveNumber, string missionName, List<Dictionary<string, object>> missionUnlockData, int amountNeeded = 1) : base(missionName, amountNeeded, missionUnlockData)
        {
            MissionEventType = MISSION_EVENT_TYPE.ENEMY_KILLED;
            m_sectorNumber = sectorNumber;
            m_waveNumber = waveNumber;
        }

        public override bool MissionComplete()
        {
            return m_currentAmount >= m_amountNeeded;
        }

        public void ProcessMissionData(int sectorNumber, int waveNumber)
        {
            if (sectorNumber == m_sectorNumber && waveNumber == m_waveNumber)
            {
                m_currentAmount += 1;
            }
        }
    }
}