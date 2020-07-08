using System.Collections.Generic;
using StarSalvager.AI;
using UnityEngine;

namespace StarSalvager.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Sector Remote", menuName = "Star Salvager/Scriptable Objects/Sector Remote Data")]
    public class SectorRemoteDataScriptableObject : ScriptableObject
    {
        public List<WaveRemoteDataScriptableObject> WaveRemoteData = new List<WaveRemoteDataScriptableObject>();

        public WaveRemoteDataScriptableObject GetRemoteData(int waveNumber)
        {
            return WaveRemoteData[waveNumber];
        }
    }
}