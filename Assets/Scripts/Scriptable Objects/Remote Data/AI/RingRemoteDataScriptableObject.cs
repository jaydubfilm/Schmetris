using System.Collections;
using System.Collections.Generic;
using StarSalvager.ScriptableObjects;
using StarSalvager.Values;
using UnityEngine;

namespace StarSalvager.AI
{
    [CreateAssetMenu(fileName = "Ring Remote", menuName = "Star Salvager/Scriptable Objects/Ring Remote Data")]
    public class RingRemoteDataScriptableObject : ScriptableObject
    {
        public List<WaveRemoteDataScriptableObject> WaveRemoteData;

        public WaveRemoteDataScriptableObject GetRemoteData(int waveNumber)
        {
            return WaveRemoteData[waveNumber];
        }

        public int GetNumberOfWaves()
        {
            return WaveRemoteData.Count;
        }
    }
}
