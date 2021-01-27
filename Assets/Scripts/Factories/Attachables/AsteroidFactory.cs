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

        public T CreateAsteroidRandom<T>()
        {
            ASTEROID_SIZE asteroidSize = (ASTEROID_SIZE)Random.Range(0, 4);
            return CreateAsteroid<T>(asteroidSize);
        }

        public T CreateAsteroid<T>(ASTEROID_SIZE asteroidSize)
        {
            var profile = _profileData.GetAsteroidProfile(asteroidSize);
            var remote = _remoteData.GetRemoteData(asteroidSize);

            var sprite = profile.Sprites[Random.Range(0, profile.Sprites.Length)];

            //--------------------------------------------------------------------------------------------------------//

            Asteroid temp;
            //If there is an animation associated with this profile entry, create the animated version of the prefab
            temp = CreateObject<Asteroid>();

            //--------------------------------------------------------------------------------------------------------//

            //((BoxCollider2D)temp.collider).size = sprite.bounds.size;
            temp.SetColliderActive(true);
            temp.SetSprite(sprite);
            temp.SetRotating(true);

            var health = remote.health;
            temp.SetupHealthValues(health, health);

            temp.SetRadius(Mathf.Max(sprite.bounds.size.x / 2, sprite.bounds.size.y / 2));

            temp.RDSTables = new List<RDSTable>();
            for (int i = 0; i < remote.RDSTableData.Count; i++)
            {
                int randomRoll = Random.Range(1, 101);
                if (randomRoll > remote.RDSTableData[i].DropChance)
                {
                    continue;
                }

                RDSTable rdsTable = new RDSTable();
                rdsTable.SetupRDSTable(remote.RDSTableData[i].NumDrops, remote.RDSTableData[i].RDSLootDatas, remote.RDSTableData[i].EvenWeighting);
                temp.RDSTables.Add(rdsTable);
            }

            temp.gameObject.name = $"{nameof(Asteroid)}_{asteroidSize}_[{Mathf.RoundToInt(sprite.bounds.size.x)},{Mathf.RoundToInt(sprite.bounds.size.y)}]";

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

