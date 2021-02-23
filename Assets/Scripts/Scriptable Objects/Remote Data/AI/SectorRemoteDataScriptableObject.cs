using System;
using System.Collections.Generic;
using StarSalvager.AI;
using StarSalvager.Utilities.Saving;
using StarSalvager.Values;
using UnityEngine;

namespace StarSalvager.ScriptableObjects
{
    [Obsolete]
    [CreateAssetMenu(fileName = "Sector Remote", menuName = "Star Salvager/Scriptable Objects/Sector Remote Data")]
    public class SectorRemoteDataScriptableObject : ScriptableObject
    {
        public List<WaveRemoteDataScriptableObject> WaveRemoteData = new List<WaveRemoteDataScriptableObject>();

        public int GridSizeX => Globals.GridSizeX;

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