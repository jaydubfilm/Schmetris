using System.Collections.Generic;
using System.Linq;
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
    public class CrateFactory : FactoryBase
    {
        private readonly GameObject _prefab;

        private readonly CrateRemoteDataScriptableObject _crateRemoteDataScriptableObject;

        public List<RDSTable> RDSTables { get; set; }

        //============================================================================================================//

        public CrateFactory(GameObject prefab, CrateRemoteDataScriptableObject crateRemoteDataScriptableObject) : base()
        {
            _prefab = prefab;
            _crateRemoteDataScriptableObject = crateRemoteDataScriptableObject;
        }

        public void UpdateCrateData(int level, ref Crate crate)
        {
            crate.SetSprite(_crateRemoteDataScriptableObject.CrateLevelSprites[Mathf.Min(level, _crateRemoteDataScriptableObject.CrateLevelSprites.Count - 1)]);
        }

        public Sprite GetCrateSprite(int level)
        {
            return _crateRemoteDataScriptableObject.CrateLevelSprites[level];
        }

        /*public List<IRDSObject> GetCrateLoot()
        {
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

            return rdsTable.rdsResult.ToList();
        }*/

        //============================================================================================================//

        public Crate CreateCrateObject(CrateData crateData)
        {
            Crate crate = CreateObject<Crate>();
            crate.Type = CRATE_TYPE.STANDARD;
            //crate.IncreaseLevel(level);
            UpdateCrateData(crateData.Level, ref crate);
            crate.Coordinate = crateData.Coordinate;

            return crate;
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

