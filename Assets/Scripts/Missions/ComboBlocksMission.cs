using StarSalvager.AI;
using System.Collections.Generic;

namespace StarSalvager
{
    [System.Serializable]
    public class ComboBlocksMission : Mission
    {
        public BIT_TYPE m_comboType;

        public ComboBlocksMission(BIT_TYPE comboType, string missionName, MISSION_UNLOCK_PARAMETERS missionUnlockType, int amountNeeded) : base(missionName, amountNeeded, missionUnlockType)
        {
            MissionEventType = MISSION_EVENT_TYPE.ENEMY_KILLED;
            m_comboType = comboType;
        }

        public override bool MissionComplete()
        {
            return m_currentAmount >= m_amountNeeded;
        }

        public void ProcessMissionData(BIT_TYPE comboType, int amount)
        {
            if (comboType == m_comboType)
            {
                m_currentAmount += amount;
            }
        }
    }
}