﻿using System.Collections.Generic;
using System.Linq;
using StarSalvager.AI;
using StarSalvager.Factories.Data;
using UnityEngine;
using UnityEngine.Serialization;

namespace StarSalvager.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Asteroid Remote", menuName = "Star Salvager/Scriptable Objects/Asteroid Remote Data")]
    public class AsteroidRemoteDataScriptableObject : ScriptableObject
    {
        [FormerlySerializedAs("BitRemoteData")] 
        public List<AsteroidRemoteData> asteroidRemoteData = new List<AsteroidRemoteData>();

        private Dictionary<ASTEROID_SIZE, AsteroidRemoteData> data;

        public AsteroidRemoteData GetRemoteData(ASTEROID_SIZE size)
        {
            if (data == null)
            {
                data = new Dictionary<ASTEROID_SIZE, AsteroidRemoteData>();
            }

            if (!data.ContainsKey(size))
            {
                data.Add(size, asteroidRemoteData
                        .FirstOrDefault(p => p.asteroidSize == size));
            }

            return data[size];
        }
    }
}

