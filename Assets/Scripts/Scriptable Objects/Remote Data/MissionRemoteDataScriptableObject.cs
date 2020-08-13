using System.Linq;
using StarSalvager.Factories.Data;
using UnityEngine;
using System.Collections.Generic;
using StarSalvager.AI;
using StarSalvager.Missions;

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
                    ResourceCollectedMission mission = new ResourceCollectedMission(data.ResourceValue(), data.MissionName, data.GetMissionUnlockData(), data.AmountNeeded);
                    missions.Add(mission);
                }
                else if (data.MissionType == MISSION_EVENT_TYPE.ENEMY_KILLED)
                {
                    EnemyKilledMission mission = new EnemyKilledMission(data.EnemyValue(), data.MissionName, data.GetMissionUnlockData(), data.AmountNeeded);
                    missions.Add(mission);
                }
                else if (data.MissionType == MISSION_EVENT_TYPE.COMBO_BLOCKS)
                {
                    ComboBlocksMission mission = new ComboBlocksMission(data.ResourceValue(), data.ComboLevel, data.MissionName, data.GetMissionUnlockData(), data.AmountNeeded);
                    missions.Add(mission);
                }
                else if (data.MissionType == MISSION_EVENT_TYPE.LEVEL_PROGRESS)
                {
                    LevelProgressMission mission = new LevelProgressMission(data.SectorNumber, data.WaveNumber, data.MissionName, data.GetMissionUnlockData());
                    missions.Add(mission);
                }
                else if (data.MissionType == MISSION_EVENT_TYPE.CRAFT_PART)
                {
                    CraftPartMission mission = new CraftPartMission(data.PartType, data.PartLevel, data.MissionName, data.GetMissionUnlockData(), data.AmountNeeded);
                    missions.Add(mission);
                }
                else if (data.MissionType == MISSION_EVENT_TYPE.WHITE_BUMPER)
                {
                    WhiteBumperMission mission = new WhiteBumperMission(data.ThroughPart, data.PartType, data.MissionName, data.GetMissionUnlockData(), data.AmountNeeded);
                    missions.Add(mission);
                }
                else if (data.MissionType == MISSION_EVENT_TYPE.ASTEROID_COLLISION)
                {
                    AsteroidCollisionMission mission = new AsteroidCollisionMission(data.ResourceValue(), data.MissionName, data.GetMissionUnlockData(), data.AmountNeeded);
                    missions.Add(mission);
                }
                else if (data.MissionType == MISSION_EVENT_TYPE.LIQUID_RESOURCE)
                {
                    LiquidResourceConvertedMission mission = new LiquidResourceConvertedMission(data.ResourceValue(), data.MissionName, data.GetMissionUnlockData(), data.AmountNeeded);
                    missions.Add(mission);
                }
                else if (data.MissionType == MISSION_EVENT_TYPE.CHAIN_WAVES)
                {
                    ChainWavesMission mission = new ChainWavesMission(data.WaveNumber, data.MissionName, data.GetMissionUnlockData(), data.AmountNeeded);
                    missions.Add(mission);
                }
                else if (data.MissionType == MISSION_EVENT_TYPE.SECTORS_COMPLETED)
                {
                    SectorsCompletedMission mission = new SectorsCompletedMission(data.MissionName, data.GetMissionUnlockData(), data.AmountNeeded);
                    missions.Add(mission);
                }
                else if (data.MissionType == MISSION_EVENT_TYPE.FLIGHT_LENGTH)
                {
                    FlightLengthMission mission = new FlightLengthMission(data.FlightLength, data.MissionName, data.GetMissionUnlockData(), data.AmountNeeded);
                    missions.Add(mission);
                }
            }

            return missions;
        }
    }

}