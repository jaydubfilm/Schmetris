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

        //public List<PatchData> patchesInStorage = new List<PatchData>();

        public int currentModularSectorIndex = 0;

        public bool firstFlight = true;

        public string PlaythroughID = string.Empty;

        public bool CanChoosePart = false;

        [JsonIgnore]
        public IReadOnlyList<string> DontShowAgainKeys => _dontShowAgainKeys;
        [JsonProperty]
        private List<string> _dontShowAgainKeys = new List<string>();

        /*[JsonIgnore]
        public LevelNodeTree LevelRingNodeTree = new LevelNodeTree();
        [JsonProperty, JsonConverter(typeof(IEnumberableVector2IntConverter))]
        private List<Vector2Int> LevelRingConnectionsJson = new List<Vector2Int>();*/

        public List<int> WreckNodes = new List<int>();

        public List<int> PlayerPreviouslyCompletedNodes = new List<int>()
        {
            0
        };

        [JsonIgnore]
        public IReadOnlyList<PatchData> PatchDatas => _patchDatas;
        [JsonProperty]

        private List<PatchData> _patchDatas = new List<PatchData>();

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

        public bool CheckIfCompleted(in int waveIndex)
        {
            Debug.LogError("Checks not yet setup");
            return false;
        }

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

        public void SetPatches(in IEnumerable<PatchData> patches)
        {
            _patchDatas = new List<PatchData>(patches);
        }

        public void ClearAllPatches()
        {
            _patchDatas.Clear();
        }

        public void RemovePatchAtIndex(in int index)
        {
            _patchDatas.RemoveAt(index);
        }

        //====================================================================================================================//
        


        public void SaveData()
        {
            //LevelRingConnectionsJson = LevelRingNodeTree.ConvertNodeTreeIntoConnections();
        }
    }
}
