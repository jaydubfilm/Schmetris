using System.Collections.Generic;
using StarSalvager.AI;
using StarSalvager.Utilities.Saving;
using StarSalvager.Values;
using UnityEngine;

namespace StarSalvager.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Sector Remote", menuName = "Star Salvager/Scriptable Objects/Sector Remote Data")]
    public class SectorRemoteDataScriptableObject : ScriptableObject
    {
        public List<WaveRemoteDataScriptableObject> WaveRemoteData = new List<WaveRemoteDataScriptableObject>();

        public SectorRemoteDataLootTablesScriptableObject sectorRemoteDataLootTablesScriptable;

        public int GridSizeX => Globals.GridSizeX;

        public WaveRemoteDataScriptableObject GetRemoteData(int waveNumber)
        {
            return WaveRemoteData[waveNumber];
        }

        public WaveRemoteDataScriptableObject GetIndexConvertedRemoteData(int sectorNumber, int waveNumber)
        {
            if (!PlayerDataManager.HasPlayerRunData() || PlayerDataManager.SectorWaveIndexConverter == null)
            {
                return GetRemoteData(waveNumber);
            }

            if (PlayerDataManager.SectorWaveIndexConverter.Count <= sectorNumber || !PlayerDataManager.SectorWaveIndexConverter[sectorNumber].ContainsKey(waveNumber))
            {
                return GetRemoteData(waveNumber);
            }

            int indexConverted = PlayerDataManager.SectorWaveIndexConverter[sectorNumber][waveNumber];

            if (waveNumber >= GetNumberOfWaves())
            {
                return WaveRemoteData[waveNumber];
            }

            return WaveRemoteData[indexConverted];
        }

        public int GetNumberOfWaves()
        {
            return WaveRemoteData.Count;
        }
    }
}