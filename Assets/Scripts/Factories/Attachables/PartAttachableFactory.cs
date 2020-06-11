using StarSalvager.Factories.Data;
using StarSalvager.ScriptableObjects;
using StarSalvager.Utilities.JsonDataTypes;
using UnityEngine;

namespace StarSalvager.Factories
{
    public class PartAttachableFactory : AttachableFactoryBase<PartProfile, PART_TYPE>
    {
        public PartAttachableFactory(AttachableProfileScriptableObject factoryProfile) : base(factoryProfile)
        {
        }
        
        public GameObject CreateGameObject(BlockData blockData)
        {
            var temp = Object.Instantiate(factoryProfile.Prefab).GetComponent<Part>();
            temp.LoadBlockData(blockData);

            return temp.gameObject;
        }
        public T CreateObject<T>(BlockData blockData)
        {
            var temp = CreateGameObject(blockData);

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

