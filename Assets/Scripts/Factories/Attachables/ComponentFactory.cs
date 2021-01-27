using System;
using Recycling;
using StarSalvager.Factories.Data;
using StarSalvager.ScriptableObjects;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JsonDataTypes;
using UnityEngine;
using Object = UnityEngine.Object;

namespace StarSalvager.Factories
{
    //TODO Need to convert this factory away from the Attachable setup
    public class ComponentFactory : FactoryBase
    {
        private readonly GameObject _prefab;
        private readonly ComponentRemoteDataScriptableObject _remoteData;
        
        //============================================================================================================//
        
        public ComponentFactory(GameObject prefab, ComponentRemoteDataScriptableObject remoteData)
        {
            _prefab = prefab;
            _remoteData = remoteData;
        }

        public int GetNumComponentsGained()
        {
            return _remoteData.NumComponentsGained;
        }
        
        //============================================================================================================//

        public override GameObject CreateGameObject()
        {
            if (!Recycler.TryGrab<Component>(out GameObject gameObject))
            {
                gameObject = Object.Instantiate(_prefab);
            }
            gameObject.GetComponent<Component>().GearNum = _remoteData.NumComponentsGained;

            return gameObject;
        }

        public GameObject CreateGameObject(int gearNum)
        {
            if (!Recycler.TryGrab<Component>(out GameObject gameObject))
            {
                gameObject = Object.Instantiate(_prefab);
            }
            gameObject.GetComponent<Component>().GearNum = gearNum;

            return gameObject;
        }

        public override T CreateObject<T>()
        {
            return CreateGameObject().GetComponent<T>();
        }

        public T CreateObject<T>(int gearNum)
        {
            return CreateGameObject(gearNum).GetComponent<T>();
        }
    }
}


