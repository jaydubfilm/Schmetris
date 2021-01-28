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
    public class BlackHoleFactory : FactoryBase
    {
        private readonly GameObject _prefab;

        private readonly BlackHoleRemoteDataScriptableObject _blackHoleRemote;

        //============================================================================================================//
        
        public BlackHoleFactory(GameObject prefab, BlackHoleRemoteDataScriptableObject blackHoleRemote) : base()
        {
            _prefab = prefab;
            _blackHoleRemote = blackHoleRemote;
        }

        public float GetBlackHoleMaxPull()
        {
            return _blackHoleRemote.BlackHoleMaxPull;
        }
        public float GetBlackHoleMaxDistance()
        {
            return _blackHoleRemote.BlackHoleMaxDistance;
        }


        //============================================================================================================//

        public BlackHole CreateBlackHole()
        {
            BlackHole blackHole = CreateObject<BlackHole>();

            return blackHole;
        }

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

