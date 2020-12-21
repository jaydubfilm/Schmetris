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
    public class MineFactory : FactoryBase
    {
        private readonly GameObject _prefab;

        private readonly MineRemoteDataScriptableObject _mineRemote;

        //============================================================================================================//
        
        public MineFactory(GameObject prefab, MineRemoteDataScriptableObject mineRemote) : base()
        {
            _prefab = prefab;
            _mineRemote = mineRemote;
        }

        //============================================================================================================//

        public Mine CreateMine(MINE_TYPE type)
        {
            Mine mine = CreateObject<Mine>();
            mine.Type = type;

            return mine;
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

