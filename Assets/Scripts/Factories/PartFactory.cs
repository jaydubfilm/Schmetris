using StarSalvager.Utilities;
using StarSalvager.Utilities.JsonDataTypes;
using UnityEngine;

namespace StarSalvager.Factories
{
    public class PartFactory : FactoryBase
    {
        public PartFactory(GameObject prefab) : base(prefab)
        {
        }
        
        public GameObject CreateGameObject(BlockData blockData)
        {
            var temp = Object.Instantiate(prefab).GetComponent<Part>();
            temp.LoadBlockData(blockData);

            return temp.gameObject;
        }
        public T CreateObject<T>(BlockData blockData)
        {
            var temp = Object.Instantiate(prefab).GetComponent<Part>();
            temp.LoadBlockData(blockData);

            return temp.GetComponent<T>();

        }

        public override GameObject CreateGameObject()
        {
            throw new System.NotImplementedException();
        }

        public override T CreateObject<T>()
        {
            throw new System.NotImplementedException();
        }
    }
}

