using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JsonDataTypes;
using StarSalvager.Utilities.Saving;
using System.Collections.Generic;

namespace StarSalvager.Missions
{
    [System.Serializable]
    public class SectorsCompletedMission : Mission
    {
        int m_sectorIndex;

        public SectorsCompletedMission(MissionRemoteData missionRemoteData) : base(missionRemoteData)
        {
            MissionEventType = MISSION_EVENT_TYPE.SECTORS_COMPLETED;
            m_sectorIndex = missionRemoteData.SectorNumber;
        }

        public SectorsCompletedMission(MissionData missionData) : base(missionData)
        {
            MissionEventType = MISSION_EVENT_TYPE.SECTORS_COMPLETED;
        }

        public override bool MissionComplete()
        {
            return currentAmount >= amountNeeded;
        }

        public override void ProcessMissionData(MissionProgressEventData missionProgressEventData)
        {
            int numCompleted = 0;
            IReadOnlyList<int> completedNodes = PlayerDataManager.GetPlayerPreviouslyCompletedNodes();
            for (int i = 0; i < completedNodes.Count; i++)
            {
                if (PlayerDataManager.GetLevelRingNodeTree().ConvertNodeIndexIntoSectorWave(completedNodes[i]).Item1 == m_sectorIndex)
                {
                    numCompleted++;
                }
            }

            if (numCompleted >= 5)
            {
                currentAmount += 1;
            }
        }


        public override string GetMissionProgressString()
        {
            if (MissionComplete())
            {
                return "";
            }

            int numCompleted = 0;
            IReadOnlyList<int> completedNodes = PlayerDataManager.GetPlayerPreviouslyCompletedNodes();
            for (int i = 0; i < completedNodes.Count; i++)
            {
                if (PlayerDataManager.GetLevelRingNodeTree().ConvertNodeIndexIntoSectorWave(completedNodes[i]).Item1 == m_sectorIndex)
                {
                    numCompleted++;
                }
            }

            return $" ({ +numCompleted}/5)";
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
            };
        }
    }
}