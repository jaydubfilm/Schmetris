using Newtonsoft.Json;
using System;
using StarSalvager.Utilities.JSON.Converters;
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
        public float Ammo => _ammo;


        [JsonProperty, JsonConverter(typeof(DecimalConverter))]
        private float _ammo;

        [JsonIgnore]
        public int AmmoCapacity => _botAmmoCapacity;

        [JsonProperty]
        private int _botAmmoCapacity;

        public PlayerResource(BIT_TYPE type, int ammo, int botAmmoCapacity)
        {
            bitType = type;

            _ammo = ammo;
            _botAmmoCapacity = botAmmoCapacity;
        }

        public void SetAmmo(float amount, bool updateValuesChanged = true)
        {
            _ammo = Mathf.Clamp(amount, 0f, _botAmmoCapacity);

            if (updateValuesChanged)
            {
                PlayerDataManager.OnValuesChanged?.Invoke();
            }
        }

        public void SetAmmoCapacity(int amount, bool updateCapacitiesChanged = true)
        {
            _botAmmoCapacity = amount;
            _ammo = Mathf.Clamp(_ammo, 0f, _botAmmoCapacity);

            if (updateCapacitiesChanged)
            {
                PlayerDataManager.OnCapacitiesChanged?.Invoke();
            }
        }

        public void AddAmmo(float amount, bool updateValuesChanged = true)
        {
            _ammo = Mathf.Min(_ammo + amount, _botAmmoCapacity);

            if (updateValuesChanged)
            {
                PlayerDataManager.OnValuesChanged?.Invoke();
            }
        }

        public void SubtractAmmo(float amount, bool updateValuesChanged = true)
        {
            _ammo = Mathf.Max(_ammo - amount, 0);

            if (updateValuesChanged)
            {
                PlayerDataManager.OnValuesChanged?.Invoke();
            }
        }
    }
}