using StarSalvager.Utilities.JsonDataTypes;
using UnityEngine;

namespace StarSalvager.Factories
{
    public class BitFactory : FactoryBase
    {
        //TODO This needs to consider the bit type more
        public BitFactory(GameObject prefab) : base(prefab)
        {
        }
        
        public GameObject CreateGameObject(BlockData blockData)
        {
            var temp = Object.Instantiate(prefab).GetComponent<Bit>();
            temp.LoadBlockData(blockData);

            return temp.gameObject;
        }
        public T CreateObject<T>(BlockData blockData)
        {
            var temp = Object.Instantiate(prefab).GetComponent<Bit>();
            temp.LoadBlockData(blockData);

            return temp.GetComponent<T>();

        }

        public override GameObject CreateGameObject()
        {
            return Object.Instantiate(prefab);
        }

        public override T CreateObject<T>()
        {
            var temp = Object.Instantiate(prefab);

            return temp.GetComponent<T>();
        }

        
    }
}

