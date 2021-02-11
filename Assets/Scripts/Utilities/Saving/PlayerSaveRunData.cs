using Newtonsoft.Json;
using StarSalvager.Utilities.JsonDataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using StarSalvager.Values;
using UnityEngine;

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
        private List<PlayerResource> _playerResources = new List<PlayerResource>
        {
            new PlayerResource(BIT_TYPE.BLUE, Globals.StartingAmmo, 100),
            new PlayerResource(BIT_TYPE.GREEN, Globals.StartingAmmo, 100),
            new PlayerResource(BIT_TYPE.GREY, Globals.StartingAmmo, 100),
            new PlayerResource(BIT_TYPE.RED, Globals.StartingAmmo, 100),
            new PlayerResource(BIT_TYPE.YELLOW, Globals.StartingAmmo, 100)
        };

        public int RationCapacity = 500;

        [JsonIgnore]
        public int Gears => _gears;

        [JsonProperty] private int _gears;

        public float currentBotHealth;
        public List<IBlockData> mainDroneBlockData = new List<IBlockData>();
        public List<IBlockData> partsInStorageBlockData = new List<IBlockData>();

        public List<PatchData> patchesInStorage = new List<PatchData>();

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

        public PlayerResource GetResource(in BIT_TYPE bitType)
        {
            var type = (int) bitType - 1;
            return _playerResources[type];
        }

        //============================================================================================================//

        public void SetGears(int value)
        {
            _gears = value;
        }

        //============================================================================================================//

        public void AddGears(int amount)
        {
            _gears += Mathf.Abs(amount);
        }

        public void SubtractGears(int amount)
        {
            _gears -= Mathf.Abs(amount);
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
