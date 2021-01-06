using Newtonsoft.Json;
using StarSalvager;
using StarSalvager.Values;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace StarSalvager.Utilities.Saving
{
    [Serializable]
    public class PlayerResource
    {
        [JsonIgnore] public BIT_TYPE BitType => bitType;
        [JsonProperty]
        private BIT_TYPE bitType;

        [JsonIgnore]
        public int resource => _resource;
        [JsonProperty]
        private int _resource;

        [JsonIgnore]
        public int resourceCapacity => _resourceCapacity;
        [JsonProperty]
        private int _resourceCapacity;

        [JsonIgnore]
        public float liquid
        {
            get => _mainBotLiquid;
        }


        [JsonProperty]
        private float _mainBotLiquid;
        [JsonProperty]
        private float _recoveryBotLiquid;

        [JsonIgnore]
        public int liquidCapacity
        {
            get => _mainBotLiquidCapacity;
        }

        [JsonProperty]
        private int _mainBotLiquidCapacity;

        public PlayerResource(BIT_TYPE type, int resource, int resourceCapacity, int mainBotLiquid, int mainBotLiquidCapacity, int recoveryBotLiquid, int recoveryBotLiquidCapacity)
        {
            bitType = type;

            _resource = resource;
            _resourceCapacity = resourceCapacity;
            _mainBotLiquid = mainBotLiquid;
            _mainBotLiquidCapacity = mainBotLiquidCapacity;
        }

        public void SetResource(int amount, bool updateValuesChanged = true)
        {
            _resource = Mathf.Clamp(amount, 0, _resourceCapacity);

            if (updateValuesChanged)
            {
                PlayerDataManager.OnValuesChanged?.Invoke();
            }
        }

        public void SetResourceCapacity(int amount, bool updateCapacitiesChanged = true)
        {
            _resourceCapacity = amount; ;
            _resource = Mathf.Clamp(_resource, 0, _resourceCapacity);

            if (updateCapacitiesChanged)
            {
                PlayerDataManager.OnCapacitiesChanged?.Invoke();
            }
        }

        public void SetLiquid(float amount, bool updateValuesChanged = true)
        {
            _mainBotLiquid = Mathf.Clamp(amount, 0f, _mainBotLiquidCapacity);

            if (updateValuesChanged)
            {
                PlayerDataManager.OnValuesChanged?.Invoke();
            }
        }

        public void SetLiquidCapacity(int amount, bool updateCapacitiesChanged = true)
        {
            _mainBotLiquidCapacity = amount;
            _mainBotLiquid = Mathf.Clamp(_mainBotLiquid, 0f, _mainBotLiquidCapacity);

            if (updateCapacitiesChanged)
            {
                PlayerDataManager.OnCapacitiesChanged?.Invoke();
            }
        }

        public void AddResource(int amount, bool updateValuesChanged = true)
        {
            _resource += amount;
            _resource = Mathf.Min(_resource, _resourceCapacity);

            if (updateValuesChanged)
            {
                PlayerDataManager.OnValuesChanged?.Invoke();
            }
        }

        public void AddResourceReturnWasted(int amount, out int totalWasted, bool updateValuesChanged = true)
        {
            totalWasted = default;
            
            _resource += amount;
            totalWasted = Mathf.Max(0, _resource - _resourceCapacity);
            _resource = Mathf.Min(_resource, _resourceCapacity);

            if (updateValuesChanged)
            {
                PlayerDataManager.OnValuesChanged?.Invoke();
            }
        }

        public void AddResourceCapacity(int amount, bool updateCapacitiesChanged = true)
        {
            _resourceCapacity += amount;

            if (updateCapacitiesChanged)
            {
                PlayerDataManager.OnValuesChanged?.Invoke();
            }
        }

        public void AddLiquid(float amount, bool updateValuesChanged = true)
        {
            _mainBotLiquid = Mathf.Min(_mainBotLiquid + amount, _mainBotLiquidCapacity);

            if (updateValuesChanged)
            {
                PlayerDataManager.OnValuesChanged?.Invoke();
            }
        }

        public void SubtractResource(int amount, bool updateValuesChanged = true)
        {
            _resource -= amount;
            _resource = Mathf.Max(_resource, 0);

            if (updateValuesChanged)
            {
                PlayerDataManager.OnValuesChanged?.Invoke();
            }
        }

        public void SubtractLiquid(float amount, bool updateValuesChanged = true)
        {
            _mainBotLiquid = Mathf.Min(_mainBotLiquid - amount, 0);

            if (updateValuesChanged)
            {
                PlayerDataManager.OnValuesChanged?.Invoke();
            }
        }
    }
}