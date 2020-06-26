using StarSalvager.Factories.Data;
using StarSalvager.ScriptableObjects;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JsonDataTypes;
using UnityEngine;

namespace StarSalvager.Factories
{
    public class PartAttachableFactory : AttachableFactoryBase<PartProfile, PART_TYPE>
    {
        private RemotePartProfileScriptableObject remotePartData;

        public PartAttachableFactory(AttachableProfileScriptableObject factoryProfile, RemotePartProfileScriptableObject remotePartData) : base(factoryProfile)
        {
            this.remotePartData = remotePartData;
        }

        //============================================================================================================//

        public GameObject CreateGameObject(BlockData blockData)
        {
            var profile = factoryProfile.GetProfile((PART_TYPE)blockData.Type);
            var sprite = profile.GetSprite(blockData.Level);

            var temp = Object.Instantiate(factoryProfile.Prefab).GetComponent<Part>();
            temp.SetSprite(sprite);
            temp.LoadBlockData(blockData);

            //temp.StartingHealth =

            temp.gameObject.name = $"{temp.Type}_{temp.level}";
            return temp.gameObject;
        }
        public T CreateObject<T>(BlockData blockData)
        {
            var temp = CreateGameObject(blockData);

            return temp.GetComponent<T>();

        }

        //============================================================================================================//

        public GameObject CreateGameObject(PART_TYPE partType, int level = 0)
        {
            var blockData = new BlockData
            {
                Level = level,
                Type = (int) partType
            };

            return CreateGameObject(blockData);
        }

        public T CreateObject<T>(PART_TYPE partType, int level = 0)
        {
            var temp = CreateGameObject(partType, level);

            return temp.GetComponent<T>();
        }

        //============================================================================================================//

        public override GameObject CreateGameObject()
        {
            return Object.Instantiate(factoryProfile.Prefab);
        }

        public override T CreateObject<T>()
        {
            var temp = CreateGameObject();

            return temp.GetComponent<T>();
        }

        //============================================================================================================//
    }
}
