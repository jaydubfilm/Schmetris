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
        public float liquid
        {
            get => _botLiquid;
        }


        [JsonProperty]
        private float _botLiquid;

        [JsonIgnore]
        public int liquidCapacity
        {
            get => _botLiquidCapacity;
        }

        [JsonProperty]
        private int _botLiquidCapacity;

        public PlayerResource(BIT_TYPE type, int botLiquid, int botLiquidCapacity)
        {
            bitType = type;

            _botLiquid = botLiquid;
            _botLiquidCapacity = botLiquidCapacity;
        }

        public void SetLiquid(float amount, bool updateValuesChanged = true)
        {
            _botLiquid = Mathf.Clamp(amount, 0f, _botLiquidCapacity);

            if (updateValuesChanged)
            {
                PlayerDataManager.OnValuesChanged?.Invoke();
            }
        }

        public void SetLiquidCapacity(int amount, bool updateCapacitiesChanged = true)
        {
            _botLiquidCapacity = amount;
            _botLiquid = Mathf.Clamp(_botLiquid, 0f, _botLiquidCapacity);

            if (updateCapacitiesChanged)
            {
                PlayerDataManager.OnCapacitiesChanged?.Invoke();
            }
        }

        public void AddLiquid(float amount, bool updateValuesChanged = true)
        {
            _botLiquid = Mathf.Min(_botLiquid + amount, _botLiquidCapacity);

            if (updateValuesChanged)
            {
                PlayerDataManager.OnValuesChanged?.Invoke();
            }
        }

        public void SubtractLiquid(float amount, bool updateValuesChanged = true)
        {
            _botLiquid = Mathf.Min(_botLiquid - amount, 0);

            if (updateValuesChanged)
            {
                PlayerDataManager.OnValuesChanged?.Invoke();
            }
        }
    }
}