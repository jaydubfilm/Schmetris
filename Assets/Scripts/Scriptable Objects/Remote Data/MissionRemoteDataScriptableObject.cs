﻿using System.Linq;
using StarSalvager.Factories.Data;
using UnityEngine;
using System.Collections.Generic;
using StarSalvager.AI;

namespace StarSalvager.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Mission_Remote", menuName = "Star Salvager/Scriptable Objects/Mission Remote Data")]
    public class MissionRemoteDataScriptableObject : ScriptableObject
    {
        public List<MissionRemoteData> m_missionRemoteData = new List<MissionRemoteData>();

        public List<Mission> GenerateMissionData()
        {
            List<Mission> missions = new List<Mission>();

            foreach (MissionRemoteData data in m_missionRemoteData)
            {
                if (data.MissionType == MISSION_EVENT_TYPE.RESOURCE_COLLECTED)
                {
                    ResourceCollectedMission mission = new ResourceCollectedMission(data.ResourceType, data.MissionName, data.MissionUnlockType, data.AmountNeeded);
                    missions.Add(mission);
                }
                else if (data.MissionType == MISSION_EVENT_TYPE.ENEMY_KILLED)
                {
                    EnemyKilledMission mission = new EnemyKilledMission(data.EnemyType, data.MissionName, data.MissionUnlockType, data.AmountNeeded);
                    missions.Add(mission);
                }
                else if (data.MissionType == MISSION_EVENT_TYPE.COMBO_BLOCKS)
                {
                    ComboBlocksMission mission = new ComboBlocksMission(data.ResourceType, data.MissionName, data.MissionUnlockType, data.AmountNeeded);
                    missions.Add(mission);
                }
                else if (data.MissionType == MISSION_EVENT_TYPE.LEVEL_PROGRESS)
                {
                    LevelProgressMission mission = new LevelProgressMission(data.SectorNumber, data.WaveNumber, data.MissionName, data.MissionUnlockType);
                    missions.Add(mission);
                }
            }

            return missions;
        }
    }

}