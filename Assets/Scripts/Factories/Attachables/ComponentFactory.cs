using Recycling;
using StarSalvager.ScriptableObjects;
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

        /*public int GetNumComponentsGained()
        {
            return _remoteData.NumComponentsGained;
        }*/
        
        //============================================================================================================//

        public override GameObject CreateGameObject()
        {
            if (!Recycler.TryGrab<GearCollectable>(out GameObject gameObject))
            {
                gameObject = Object.Instantiate(_prefab);
            }

            return gameObject;
        }

        public GameObject CreateGameObject(int gearNum)
        {
            var component = CreateObject<GearCollectable>();
            
            component.GearNum = gearNum;

            return component.gameObject;
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


