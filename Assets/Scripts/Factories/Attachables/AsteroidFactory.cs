using System.Collections.Generic;
using System.Runtime.InteropServices;
using Recycling;
using StarSalvager.AI;
using StarSalvager.Factories.Data;
using StarSalvager.ScriptableObjects;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JsonDataTypes;
using UnityEngine;
using Object = UnityEngine.Object;

namespace StarSalvager.Factories
{
    //FIXME This needs to be cleaned up, feels messy
    public class AsteroidFactory : FactoryBase
    {
        private readonly GameObject _prefab;
        private readonly AsteroidProfileScriptableObject _profileData;
        private readonly AsteroidRemoteDataScriptableObject _remoteData;

        //============================================================================================================//
        
        public AsteroidFactory(GameObject prefab, AsteroidProfileScriptableObject profileData, AsteroidRemoteDataScriptableObject remoteData) : base()
        {
            _prefab = prefab;
            _profileData = profileData;
            _remoteData = remoteData;
        }

        //============================================================================================================//

        public T CreateLargeAsteroid<T>(ASTEROID_SIZE asteroidSize)
        {
            var profile = ((AsteroidProfileScriptableObject)_profileData).GetAsteroidProfile(asteroidSize);
            var remote = _remoteData.GetRemoteData(asteroidSize);

            var sprite = profile.Sprites[Random.Range(0, profile.Sprites.Length)];

            //--------------------------------------------------------------------------------------------------------//

            Asteroid temp;
            //If there is an animation associated with this profile entry, create the animated version of the prefab
            temp = CreateObject<Asteroid>();

            //--------------------------------------------------------------------------------------------------------//

            ((BoxCollider2D)temp.collider).size = sprite.bounds.size;
            temp.SetColliderActive(true);
            temp.SetSprite(sprite);
            temp.SetRotating(true);

            var health = remote.health;
            temp.SetupHealthValues(health, health);

            return temp.GetComponent<T>();
        }

        //============================================================================================================//

        public override GameObject CreateGameObject()
        {
            return Object.Instantiate(_prefab);
        }

        public override T CreateObject<T>()
        {
            var temp = CreateGameObject();

            return temp.GetComponent<T>();
        }
        
        //============================================================================================================//
    }
}

