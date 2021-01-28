using System;
using StarSalvager.Utilities.JsonDataTypes;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using StarSalvager.Values;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Factories;
using StarSalvager.Factories.Data;

namespace StarSalvager.Utilities.Saving
{
    [Serializable]
    public class PlayerSaveRunData
    {
        //============================================================================================================//

        public bool runStarted;
        public int CurrentNode = 0;

        //TEMP
        public bool hasSetupConverter;

        [JsonProperty]
        private List<PlayerResource> _playerResources = new List<PlayerResource>() {
            new PlayerResource(BIT_TYPE.BLUE, 0, 0),
            new PlayerResource(BIT_TYPE.GREEN, 0, 0),
            new PlayerResource(BIT_TYPE.GREY, 0, 0),
            new PlayerResource(BIT_TYPE.RED, 30, 0),
            new PlayerResource(BIT_TYPE.YELLOW, 0, 0)
        };

        public int RationCapacity = 500;

        [JsonIgnore]
        public int Components => _components;

        [JsonProperty] private int _components;

/*[JsonIgnore]
public Dictionary<COMPONENT_TYPE, int> Components => _components;
[JsonProperty]
private Dictionary<COMPONENT_TYPE, int> _components = new Dictionary<COMPONENT_TYPE, int>
{
    {COMPONENT_TYPE.FUSOR, 0},
    {COMPONENT_TYPE.CHIP, 0},
    {COMPONENT_TYPE.NUT, 0},
    {COMPONENT_TYPE.BOLT, 0},
    {COMPONENT_TYPE.COIL, 0}
};*/

        public List<IBlockData> mainDroneBlockData = new List<IBlockData>();
        public List<IBlockData> partsInStorageBlockData = new List<IBlockData>();

        public List<PatchData> patchesInStorage = new List<PatchData>();

        public List<SectorWaveModifier> levelResourceModifier = new List<SectorWaveModifier>();

        public int currentModularSectorIndex = 0;

        public bool firstFlight = true;

        public string PlaythroughID = string.Empty;

        public bool CanChoosePart = false;

        [JsonIgnore]
        public IReadOnlyList<string> DontShowAgainKeys => _dontShowAgainKeys;
        [JsonProperty]
        private List<string> _dontShowAgainKeys = new List<string>();

        [JsonIgnore]
        public LevelNodeTree LevelRingNodeTree = new LevelNodeTree();
        [JsonProperty, JsonConverter(typeof(IEnumberableVector2IntConverter))]
        private List<Vector2Int> LevelRingConnectionsJson = new List<Vector2Int>
        {

        };

        public List<int> WreckNodes = new List<int>()
        {

        };

        public List<int> PlayerPreviouslyCompletedNodes = new List<int>()
        {
            0
        };

        //============================================================================================================//

        public void SetupMap(List<Vector2Int> levelRingConnectionsJson = null, List<int> wreckNodes = null)
        {
            if (levelRingConnectionsJson != null)
            {
                LevelRingConnectionsJson.Clear();
                LevelRingConnectionsJson.AddRange(levelRingConnectionsJson);
            }

            if (wreckNodes != null)
            {
                WreckNodes.Clear();
                WreckNodes.AddRange(wreckNodes);
            }

            LevelRingNodeTree.ReadInNodeConnectionData(LevelRingConnectionsJson, WreckNodes);
        }

        //============================================================================================================//

        public List<PlayerResource> GetResources()
        {
            return _playerResources;
        }

        public PlayerResource GetResource(BIT_TYPE bitType)
        {
            int index = (int)bitType - 1;

            return _playerResources[index];
        }

        //============================================================================================================//

        public void SetComponents(int value)
        {
            _components = value;
        }

        /*public void SetComponents(Dictionary<COMPONENT_TYPE, int> liquidValues)
        {
            foreach (var value in liquidValues)
            {
                _components[value.Key] = value.Value;
            }
        }*/

        //============================================================================================================//

        public void AddComponent(int amount)
        {
            _components += Mathf.Abs(amount);
        }

        public void SubtractComponent(int amount)
        {
            _components -= Mathf.Abs(amount);
        }

        //============================================================================================================//

        public bool CheckIfCompleted(int sector, int waveAt)
        {
            for (int i = 0; i < PlayerPreviouslyCompletedNodes.Count; i++)
            {
                (int, int) curSectorWaveTuple = LevelRingNodeTree.ConvertNodeIndexIntoSectorWave(PlayerPreviouslyCompletedNodes[i]);

                if (curSectorWaveTuple.Item1 == sector && curSectorWaveTuple.Item2 == waveAt)
                {
                    return true;
                }
            }

            return false;
        }

        //============================================================================================================//

        public float GetLevelResourceModifier(int sector, int wave)
        {
            int index = levelResourceModifier.FindIndex(s => s.Sector == sector && s.Wave == wave);

            if (index == -1)
            {
                levelResourceModifier.Add(new SectorWaveModifier
                {
                    Sector = sector,
                    Wave = wave,
                    Modifier = 1.0f
                });
                index = levelResourceModifier.FindIndex(s => s.Sector == sector && s.Wave == wave);
            }

            return levelResourceModifier[index].Modifier;
        }

        public void ReduceLevelResourceModifier(int sector, int wave)
        {
            int index = levelResourceModifier.FindIndex(s => s.Sector == sector && s.Wave == wave);
            float previousModifier;

            if (index >= 0)
            {
                previousModifier = levelResourceModifier[index].Modifier;
                levelResourceModifier.RemoveAt(index);
            }
            else
            {
                previousModifier = 1.0f;
            }

            levelResourceModifier.Add(new SectorWaveModifier
            {
                Sector = sector,
                Wave = wave,
                Modifier = previousModifier * Globals.LevelResourceDropReductionAmount
            });
        }

        //============================================================================================================//

        //DontShowAgain Tracking Functions
        //====================================================================================================================//

        public void AddDontShowAgainKey(string key)
        {
            _dontShowAgainKeys.Add(key);
        }

        //====================================================================================================================//

        public List<IBlockData> GetCurrentBlockData()
        {
            return mainDroneBlockData;
        }

        public void SetShipBlockData(List<IBlockData> blockData)
        {
            mainDroneBlockData.Clear();
            mainDroneBlockData.AddRange(blockData);
        }

        public List<IBlockData> GetCurrentPartsInStorage()
        {
            return partsInStorageBlockData;
        }

        public void SetCurrentPartsInStorage(List<IBlockData> blockData)
        {
            partsInStorageBlockData.Clear();
            partsInStorageBlockData.AddRange(blockData);
        }

        public void AddPartToStorage(IBlockData blockData)
        {
            partsInStorageBlockData.Add(blockData);
            PlayerDataManager.OnValuesChanged?.Invoke();
        }

        public void RemovePartFromStorage(IBlockData blockData)
        {
            partsInStorageBlockData.Remove(partsInStorageBlockData.FirstOrDefault(b => b.Type == blockData.Type));
        }

        public void RemovePartFromStorageAtIndex(int index)
        {
            if (partsInStorageBlockData.Count > index)
            {
                partsInStorageBlockData.RemoveAt(index);
            }
        }

        //Patches
        //====================================================================================================================//
        public List<PatchData> GetCurrentPatchesInStorage()
        {
            return patchesInStorage;
        }

        public void AddPatchToStorage(PatchData patchData)
        {
            patchesInStorage.Add(patchData);
            PlayerDataManager.OnValuesChanged?.Invoke();
        }

        public void RemovePatchFromStorage(PatchData patchData)
        {
            var index = patchesInStorage.FindIndex(b => b.Type == patchData.Type);

            if (index < 0)
                throw new ArgumentException();

            patchesInStorage.RemoveAt(index);
        }

        public void RemovePatchFromStorageAtIndex(int index)
        {
            if (index >= patchesInStorage.Count)
                return;

            patchesInStorage.RemoveAt(index);
        }

        //====================================================================================================================//


        public void SaveData()
        {
            //LevelRingConnectionsJson = LevelRingNodeTree.ConvertNodeTreeIntoConnections();
        }
    }
}
