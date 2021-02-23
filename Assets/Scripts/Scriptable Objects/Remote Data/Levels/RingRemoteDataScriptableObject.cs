using System.Collections;
using System.Collections.Generic;
using StarSalvager.ScriptableObjects;
using StarSalvager.Values;
using UnityEngine;

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
        [Range(1,10)]
        public int wreckFrequency = 3;
        
        public List<WaveRemoteDataScriptableObject> WaveRemoteData;

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
    }
}
