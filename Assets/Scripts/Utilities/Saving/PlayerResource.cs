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
        public float Ammo => _botAmmo;


        [JsonProperty]
        private float _botAmmo;

        [JsonIgnore]
        public int AmmoCapacity => _botAmmoCapacity;

        [JsonProperty]
        private int _botAmmoCapacity;

        public PlayerResource(BIT_TYPE type, int botAmmo, int botAmmoCapacity)
        {
            bitType = type;

            _botAmmo = botAmmo;
            _botAmmoCapacity = botAmmoCapacity;
        }

        public void SetAmmo(float amount, bool updateValuesChanged = true)
        {
            _botAmmo = Mathf.Clamp(amount, 0f, _botAmmoCapacity);

            if (updateValuesChanged)
            {
                PlayerDataManager.OnValuesChanged?.Invoke();
            }
        }

        public void SetAmmoCapacity(int amount, bool updateCapacitiesChanged = true)
        {
            _botAmmoCapacity = amount;
            _botAmmo = Mathf.Clamp(_botAmmo, 0f, _botAmmoCapacity);

            if (updateCapacitiesChanged)
            {
                PlayerDataManager.OnCapacitiesChanged?.Invoke();
            }
        }

        public void AddAmmo(float amount, bool updateValuesChanged = true)
        {
            _botAmmo = Mathf.Min(_botAmmo + amount, _botAmmoCapacity);

            if (updateValuesChanged)
            {
                PlayerDataManager.OnValuesChanged?.Invoke();
            }
        }

        public void SubtractAmmo(float amount, bool updateValuesChanged = true)
        {
            _botAmmo = Mathf.Max(_botAmmo - amount, 0);

            if (updateValuesChanged)
            {
                PlayerDataManager.OnValuesChanged?.Invoke();
            }
        }
    }
}