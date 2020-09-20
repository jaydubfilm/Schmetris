using System.Collections.Generic;
using System.Linq;
using StarSalvager.AI;
using StarSalvager.Factories.Data;
using StarSalvager.Prototype;
using UnityEngine;

namespace StarSalvager.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Asteroid_Profile", menuName = "Star Salvager/Scriptable Objects/Asteroid Profile")]
    public class AsteroidProfileScriptableObject : ScriptableObject
    {
        [SerializeField]
        private List<AsteroidProfile> asteroidProfiles = new List<AsteroidProfile>();

        public AsteroidProfile GetAsteroidProfile(ASTEROID_SIZE size)
        {
            var matches = asteroidProfiles
                .FindAll(p => p.Size == size);
            return matches[Random.Range(0, matches.Count)];
        }
    }
}