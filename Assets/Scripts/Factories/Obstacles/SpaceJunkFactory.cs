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
    public class SpaceJunkFactory : FactoryBase
    {
        private readonly GameObject _prefab;

        private readonly SpaceJunkRemoteDataScriptableObject _spaceJunkRemote;

        //============================================================================================================//
        
        public SpaceJunkFactory(GameObject prefab, SpaceJunkRemoteDataScriptableObject spaceJunkRemote) : base()
        {
            _prefab = prefab;
            _spaceJunkRemote = spaceJunkRemote;
        }

        //============================================================================================================//

        public SpaceJunk CreateSpaceJunk()
        {
            SpaceJunk spaceJunk = CreateObject<SpaceJunk>();

            spaceJunk.RDSTableOdds = new List<int>();
            spaceJunk.RDSTables = new List<RDSTable>();
            for (int i = 0; i < _spaceJunkRemote.RDSTableData.Count; i++)
            {
                int randomRoll = Random.Range(1, 101);
                if (randomRoll > _spaceJunkRemote.RDSTableData[i].DropChance)
                {
                    continue;
                }

                RDSTable rdsTable = new RDSTable();
                rdsTable.SetupRDSTable(_spaceJunkRemote.RDSTableData[i].NumDrops,
                    _spaceJunkRemote.RDSTableData[i].RDSLootDatas,
                    _spaceJunkRemote.RDSTableData[i].EvenWeighting);

                spaceJunk.RDSTableOdds.Add(_spaceJunkRemote.RDSTableData[i].DropChance);
                spaceJunk.RDSTables.Add(rdsTable);
            }

            return spaceJunk;
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

