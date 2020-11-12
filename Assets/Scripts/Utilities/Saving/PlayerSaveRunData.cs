﻿using System;
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

        //TEMP
        public List<Dictionary<int, int>> sectorWaveIndexConverter = new List<Dictionary<int, int>>();
        public bool hasSetupConverter;

        [JsonProperty]
        private List<PlayerResource> _playerResources = new List<PlayerResource>() {
            new PlayerResource(BIT_TYPE.BLUE, 75, 300, 0, 0, 0, 0),
            new PlayerResource(BIT_TYPE.GREEN, 0, 300, 0, 0, 0, 0),
            new PlayerResource(BIT_TYPE.GREY, 0, 300, 0, 0, 0, 0),
            new PlayerResource(BIT_TYPE.RED, 100, 300, 30, 0, 30, 0),
            new PlayerResource(BIT_TYPE.YELLOW, 0, 300, 0, 0, 0, 0)
        };

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
        [JsonProperty]
        private List<Vector2Int> LevelRingConnectionsJson = new List<Vector2Int>
        {

        };

        public List<int> ShortcutNodes = new List<int>()
        {

        };

        public List<int> PlayerPreviouslyCompletedNodes = new List<int>()
        {
            0
        };

        //============================================================================================================//

        public void SetupMap(List<Vector2Int> levelRingConnectionsJson = null, List<int> shortcutNodes = null)
        {
            //TEMP
            if (!hasSetupConverter)
            {
                for (int i = 0; i < FactoryManager.Instance.SectorRemoteData.Count; i++)
                {
                    List<int> availableIndexes = new List<int>();
                    sectorWaveIndexConverter.Add(new Dictionary<int, int>());

                    int numOptions = FactoryManager.Instance.SectorRemoteData[i].GetNumberOfWaves();

                    for (int k = 0; k < numOptions; k++)
                    {
                        availableIndexes.Add(k);
                    }

                    for (int k = 0; k < numOptions; k++)
                    {
                        int randomIndex = availableIndexes[UnityEngine.Random.Range(0, availableIndexes.Count)];
                        availableIndexes.Remove(randomIndex);
                        sectorWaveIndexConverter[i].Add(k, randomIndex);
                    }
                }
                hasSetupConverter = true;
            }
            //ENDTEMP

            if (levelRingConnectionsJson != null)
            {
                LevelRingConnectionsJson.Clear();
                LevelRingConnectionsJson.AddRange(levelRingConnectionsJson);
            }
            if (shortcutNodes != null)
            {
                ShortcutNodes.AddRange(shortcutNodes);
            }
            
            LevelRingNodeTree.ReadInNodeConnectionData(LevelRingConnectionsJson);
        }

        public void FacilityEffectsOnNewAccount()
        {
            var facilityTypes = Enum.GetValues(typeof(FACILITY_TYPE)).Cast<FACILITY_TYPE>().ToList();

            for (int i = 0; i < facilityTypes.Count; i++)
            {
                if (PlayerDataManager.CheckHasFacility(facilityTypes[i]))
                {
                    int level = PlayerDataManager.GetFacilityRanks()[facilityTypes[i]];
                    FacilityRemoteData remoteData = FactoryManager.Instance.FacilityRemote.GetRemoteData(facilityTypes[i]);
                    int increaseAmount = remoteData.levels[level].increaseAmount;

                    switch (facilityTypes[i])
                    {
                        case FACILITY_TYPE.FREEZER:
                            RationCapacity += increaseAmount;
                            break;
                        case FACILITY_TYPE.STORAGEELECTRICITY:
                            GetResource(BIT_TYPE.YELLOW).AddResourceCapacity(increaseAmount);
                            break;
                        case FACILITY_TYPE.STORAGEFUEL:
                            GetResource(BIT_TYPE.RED).AddResourceCapacity(increaseAmount);
                            break;
                        case FACILITY_TYPE.STORAGEPLASMA:
                            GetResource(BIT_TYPE.GREEN).AddResourceCapacity(increaseAmount);
                            break;
                        case FACILITY_TYPE.STORAGESCRAP:
                            GetResource(BIT_TYPE.GREY).AddResourceCapacity(increaseAmount);
                            break;
                        case FACILITY_TYPE.STORAGEWATER:
                            GetResource(BIT_TYPE.BLUE).AddResourceCapacity(increaseAmount);
                            break;
                        case FACILITY_TYPE.STARTINGELECTRICITY:
                            GetResource(BIT_TYPE.YELLOW).AddResource(increaseAmount);
                            break;
                        case FACILITY_TYPE.STARTINGFUEL:
                            GetResource(BIT_TYPE.RED).AddResource(increaseAmount);
                            break;
                        case FACILITY_TYPE.STARTINGPLASMA:
                            GetResource(BIT_TYPE.GREEN).AddResource(increaseAmount);
                            break;
                        case FACILITY_TYPE.STARTINGSCRAP:
                            GetResource(BIT_TYPE.GREY).AddResource(increaseAmount);
                            break;
                        case FACILITY_TYPE.STARTINGWATER:
                            GetResource(BIT_TYPE.BLUE).AddResource(increaseAmount);
                            break;
                    }
                }
            }
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
