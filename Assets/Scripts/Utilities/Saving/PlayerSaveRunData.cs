using System;
using StarSalvager.Utilities.JsonDataTypes;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using StarSalvager.Values;
using StarSalvager.Utilities.Extensions;

namespace StarSalvager.Utilities.Saving
{
    [Serializable]
    public class PlayerSaveRunData
    {
        //============================================================================================================//

        public bool runStarted = false;

        [JsonProperty]
        private List<PlayerResource> _playerResources = new List<PlayerResource>() {
            new PlayerResource(BIT_TYPE.BLUE, 75, 300, 0, 0, 0, 0),
            new PlayerResource(BIT_TYPE.GREEN, 0, 300, 0, 0, 0, 0),
            new PlayerResource(BIT_TYPE.GREY, 0, 300, 0, 0, 0, 0),
            new PlayerResource(BIT_TYPE.RED, 100, 300, 30, 0, 30, 0),
            new PlayerResource(BIT_TYPE.YELLOW, 0, 300, 0, 0, 0, 0)
        };


        //TODO: Add an add/subtract function for ResourceAmount, and make this IReadOnlyDictionary<>
        /*[JsonIgnore]
        public Dictionary<BIT_TYPE, int> Resources => _resources;

        [JsonProperty]
        private Dictionary<BIT_TYPE, int> _resources = new Dictionary<BIT_TYPE, int>
        {
            {BIT_TYPE.RED, 100},
            {BIT_TYPE.BLUE, 75},
            {BIT_TYPE.YELLOW, 0},
            {BIT_TYPE.GREEN, 0},
            {BIT_TYPE.GREY, 0},
        };

        [JsonIgnore]
        public Dictionary<BIT_TYPE, int> ResourceCapacities => _resourceCapacity;
        [JsonProperty]
        private Dictionary<BIT_TYPE, int> _resourceCapacity = new Dictionary<BIT_TYPE, int>
        {
            {BIT_TYPE.RED, 300},
            {BIT_TYPE.BLUE, 300},
            {BIT_TYPE.YELLOW, 300},
            {BIT_TYPE.GREEN, 300},
            {BIT_TYPE.GREY, 300},
        };*/

        public int RationCapacity = 500;

        [JsonIgnore]
        public Dictionary<COMPONENT_TYPE, int> Components => _components;
        [JsonProperty]
        private Dictionary<COMPONENT_TYPE, int> _components = new Dictionary<COMPONENT_TYPE, int>
        {
            {COMPONENT_TYPE.FUSOR, 0},
            {COMPONENT_TYPE.CHIP, 0},
            {COMPONENT_TYPE.NUT, 0},
            {COMPONENT_TYPE.BOLT, 0},
            {COMPONENT_TYPE.COIL, 0}
        };

        /*[JsonIgnore]
        public IReadOnlyDictionary<BIT_TYPE, float> MainDroneLiquidResources => _liquidResources;
        [JsonProperty]
        //FIXME This needs to use some sort of capacity value
        private Dictionary<BIT_TYPE, float> _liquidResources = new Dictionary<BIT_TYPE, float>
        {
            {BIT_TYPE.RED, 30},
            {BIT_TYPE.BLUE, 0},
            {BIT_TYPE.YELLOW, 0},
            {BIT_TYPE.GREEN, 0},
            {BIT_TYPE.GREY, 0},
        };

        [JsonIgnore]
        public IReadOnlyDictionary<BIT_TYPE, float> RecoveryDroneLiquidResources => _recoveryDroneLiquidResources;
        [JsonProperty]
        //FIXME This needs to use some sort of capacity value
        private Dictionary<BIT_TYPE, float> _recoveryDroneLiquidResources = new Dictionary<BIT_TYPE, float>
        {
            {BIT_TYPE.RED, 30},
            {BIT_TYPE.BLUE, 0},
            {BIT_TYPE.YELLOW, 0},
            {BIT_TYPE.GREEN, 0},
            {BIT_TYPE.GREY, 0},
        };

        //FIXME I think that this should not be so persistent (Shouldn't need to be saved data)
        [JsonIgnore]
        public IReadOnlyDictionary<BIT_TYPE, int> MainDroneLiquidCapacity => _liquidCapacity;
        [JsonProperty]
        private Dictionary<BIT_TYPE, int> _liquidCapacity = new Dictionary<BIT_TYPE, int>
        {
            {BIT_TYPE.RED, 0},
            {BIT_TYPE.BLUE, 0},
            {BIT_TYPE.YELLOW, 0},
            {BIT_TYPE.GREEN, 0},
            {BIT_TYPE.GREY, 0},
        };

        //FIXME I think that this should not be so persistent (Shouldn't need to be saved data)
        [JsonIgnore]
        public IReadOnlyDictionary<BIT_TYPE, int> RecoveryDroneLiquidCapacity => _recoveryDroneLiquidCapacity;
        [JsonProperty]
        private Dictionary<BIT_TYPE, int> _recoveryDroneLiquidCapacity = new Dictionary<BIT_TYPE, int>
        {
            {BIT_TYPE.RED, 0},
            {BIT_TYPE.BLUE, 0},
            {BIT_TYPE.YELLOW, 0},
            {BIT_TYPE.GREEN, 0},
            {BIT_TYPE.GREY, 0},
        };*/

        public List<BlockData> mainDroneBlockData = new List<BlockData>();
        public List<BlockData> recoveryDroneBlockData = new List<BlockData>();
        public List<BlockData> partsInStorageBlockData = new List<BlockData>();

        public List<SectorWaveModifier> levelResourceModifier = new List<SectorWaveModifier>();

        public int currentModularSectorIndex = 0;

        public bool firstFlight = true;

        public string PlaythroughID = string.Empty;

        [JsonIgnore]
        public IReadOnlyList<string> DontShowAgainKeys => _dontShowAgainKeys;
        [JsonProperty] 
        private List<string> _dontShowAgainKeys = new List<string>();

        [JsonIgnore]
        public LevelRingNodeTree LevelRingNodeTree = new LevelRingNodeTree();
        [JsonIgnore]
        private List<Vector2Int> LevelRingConnectionsJson = new List<Vector2Int>
        {

        };

        [JsonIgnore]
        public List<int> ShortcutNodes = new List<int>()
        {

        };

        public List<int> PlayerPreviouslyCompletedNodes = new List<int>()
        {
            0
        };

        //============================================================================================================//

        public PlayerSaveRunData(List<Vector2Int> levelRingConnectsionsJson, List<int> shortcutNodes)
        {
            LevelRingConnectionsJson.AddRange(levelRingConnectsionsJson);
            ShortcutNodes.AddRange(shortcutNodes);
            
            LevelRingNodeTree.ReadInNodeConnectionData(LevelRingConnectionsJson);
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

        public void SetComponents(COMPONENT_TYPE type, int value)
        {
            _components[type] = value;
        }

        public void SetComponents(Dictionary<COMPONENT_TYPE, int> liquidValues)
        {
            foreach (var value in liquidValues)
            {
                _components[value.Key] = value.Value;
            }
        }

        //============================================================================================================//

        public void AddComponent(COMPONENT_TYPE type, int amount)
        {
            _components[type] += Mathf.Abs(amount);
        }

        public void SubtractComponent(COMPONENT_TYPE type, int amount)
        {
            _components[type] -= Mathf.Abs(amount);
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
        
        public List<BlockData> GetCurrentBlockData()
        {
            return mainDroneBlockData;
        }

        public void SetShipBlockData(List<BlockData> blockData)
        {
            mainDroneBlockData.Clear();
            mainDroneBlockData.AddRange(blockData);
        }

        public List<BlockData> GetRecoveryDroneBlockData()
        {
            return recoveryDroneBlockData;
        }

        public void SetRecoveryDroneBlockData(List<BlockData> blockData)
        {
            recoveryDroneBlockData.Clear();
            recoveryDroneBlockData.AddRange(blockData);
        }

        public List<BlockData> GetCurrentPartsInStorage()
        {
            return partsInStorageBlockData;
        }

        public void SetCurrentPartsInStorage(List<BlockData> blockData)
        {
            partsInStorageBlockData.Clear();
            partsInStorageBlockData.AddRange(blockData);
        }

        public void AddPartToStorage(BlockData blockData)
        {
            partsInStorageBlockData.Add(blockData);
            PlayerDataManager.OnValuesChanged?.Invoke();
        }

        public void RemovePartFromStorage(BlockData blockData)
        {
            partsInStorageBlockData.Remove(partsInStorageBlockData.FirstOrDefault(b => b.Level == blockData.Level && b.Type == blockData.Type));
        }

        public void SaveData()
        {
            LevelRingConnectionsJson = LevelRingNodeTree.ConvertNodeTreeIntoConnections();
        }
    }
}
