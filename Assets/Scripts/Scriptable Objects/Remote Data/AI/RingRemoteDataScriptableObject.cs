﻿using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using StarSalvager.ScriptableObjects;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.Saving;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace StarSalvager.AI
{
    public class NodeData
    {
        public NodeType NodeType;
        public int ringIndex = -1;
        public int waveIndex = -1;
    }
    
    [CreateAssetMenu(fileName = "Ring Remote", menuName = "Star Salvager/Scriptable Objects/Ring Remote Data")]
    public class RingRemoteDataScriptableObject : ScriptableObject
    {
        //Wrecks
        //====================================================================================================================//
        
        [Range(1,10), BoxGroup("Wrecks")]
        public int wreckFrequency = 3;

        [BoxGroup("Wrecks/Patches"), MinMaxSlider(1, 10, true)]
        public Vector2Int patchSpawnCount = new Vector2Int(2, 4);
        [BoxGroup("Wrecks/Patches"), MinMaxSlider(1, 2, true)]
        public Vector2Int patchLevelRange = new Vector2Int(1, 2);
        [FormerlySerializedAs("patches")] [BoxGroup("Wrecks/Patches")]
        public List<PATCH_TYPE> patchOptions;

        //====================================================================================================================//
        
        [Space(10f)]
        public List<WaveRemoteDataScriptableObject> WaveRemoteData;

        //====================================================================================================================//
        
        public WaveRemoteDataScriptableObject GetRemoteData(int waveNumber)
        {
            return WaveRemoteData[waveNumber];
        }

        public int GetNumberOfWaves()
        {
            return WaveRemoteData.Count;
        }

        public int GetNodeCount()
        {
            var waves = GetNumberOfWaves();
            var wrecks = Mathf.FloorToInt((float)waves / (wreckFrequency));
                
            //1 represents the base the player starts at
            return 1 + waves + wrecks;
        }

        public List<NodeData> GetAsNodes()
        {
            var outList = new List<NodeData>();
            
            var count = GetNodeCount();
            var waveIndex = 0;
            for (int i = 0; i < count; i++)
            {
                if(i == 0)
                {
                    outList.Add(new NodeData
                    {
                        NodeType = NodeType.Base,
                        waveIndex = -1
                    });
                    
                    continue;
                }

                if (i % (wreckFrequency + 1) == 0)
                {
                    outList.Add(new NodeData
                    {
                        NodeType = NodeType.Wreck,
                        waveIndex = -1
                    });
                    continue;
                }
                
                outList.Add(new NodeData
                {
                    NodeType = NodeType.Level,
                    waveIndex = waveIndex
                });
                
                waveIndex++;
            }

            return outList;
        }

        //====================================================================================================================//

        public IEnumerable<PatchData> GenerateRingPatches()
        {
            if (patchOptions.IsNullOrEmpty())
                return new PatchData[0];

            var timeoutCounter = 0;
            var assignIndex = 0;
            var count = Random.Range(patchSpawnCount.x, patchSpawnCount.y + 1);

            var outData = new PatchData[count];

            while (assignIndex < count)
            {
                if (timeoutCounter++ >= 1000)
                    throw new TimeoutException("Unable to find enough Patches to present");
                
                var patchData  = new PatchData
                {
                    Type = (int)patchOptions[Random.Range(0, patchOptions.Count)],
                    Level = Random.Range(patchLevelRange.x - 1, patchLevelRange.y)
                };

                
                if (!PlayerDataManager.IsPatchUnlocked(patchData))
                    continue;

                outData[assignIndex++] = patchData;
            }

            return outData;
        }
    }
}
