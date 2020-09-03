﻿using StarSalvager.AI;
using StarSalvager.Utilities.JsonDataTypes;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Wave Remote", menuName = "Star Salvager/Scriptable Objects/Wave Remote Data")]
    public class WaveRemoteDataScriptableObject : ScriptableObject
    {
        public List<StageRemoteData> StageRemoteData = new List<StageRemoteData>();

        [SerializeField]
        private int maxDrops;

        [SerializeField]
        private List<RDSLootData> RDSEndOfWaveLoot = new List<RDSLootData>();

        public RDSTable rdsTable;

        public void ConfigureLootTable()
        {
            rdsTable = new RDSTable();
            rdsTable.rdsCount = maxDrops;

            foreach (var rdsData in RDSEndOfWaveLoot)
            {
                if (rdsData.rdsData == RDSLootData.TYPE.Bit)
                {
                    BlockData bitBlockData = new BlockData
                    {
                        ClassType = "Bit",
                        Type = rdsData.type,
                        Level = rdsData.level
                    };
                    rdsTable.AddEntry(new RDSValue<BlockData>(bitBlockData, rdsData.probability, rdsData.isUniqueSpawn, rdsData.isAlwaysSpawn, true));
                }
                else if (rdsData.rdsData == RDSLootData.TYPE.Component)
                {
                    BlockData componentBlockData = new BlockData
                    {
                        ClassType = "Component",
                        Type = rdsData.type,
                    };
                    rdsTable.AddEntry(new RDSValue<BlockData>(componentBlockData, rdsData.probability, rdsData.isUniqueSpawn, rdsData.isAlwaysSpawn, true));
                }
                else if (rdsData.rdsData == RDSLootData.TYPE.Blueprint)
                {
                    Blueprint blueprintData = new Blueprint
                    {
                        name = (PART_TYPE)rdsData.type + " " + rdsData.level,
                        partType = (PART_TYPE)rdsData.type,
                        level = rdsData.level
                    };
                    rdsTable.AddEntry(new RDSValue<Blueprint>(blueprintData, rdsData.probability, rdsData.isUniqueSpawn, rdsData.isAlwaysSpawn, true));
                }
            }
        }

        public StageRemoteData GetRemoteData(int waveNumber)
        {
            return StageRemoteData[waveNumber];
        }

        public bool TrySetCurrentStage(float stageTimer, out int currentStage)
        {
            currentStage = 0;

            while (stageTimer >= StageRemoteData[currentStage].StageDuration)
            {
                if (StageRemoteData[currentStage].WaitUntilAllEnemiesDefeatedToBegin &&
                    LevelManager.Instance.EnemyManager.HasEnemiesRemaining())
                {
                    return true;
                }

                stageTimer -= StageRemoteData[currentStage].StageDuration;
                currentStage++;

                if (currentStage >= StageRemoteData.Count)
                {
                    return false;
                }
            }

            return true;
        }

        public float GetWaveDuration()
        {
            float waveDuration = 0;

            for (int i = 0; i < StageRemoteData.Count; i++)
            {
                waveDuration += StageRemoteData[i].StageDuration;   
            }

            return waveDuration;
        }
    }
}

