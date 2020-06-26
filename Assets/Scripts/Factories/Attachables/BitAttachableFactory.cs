using Recycling;
using StarSalvager.Factories.Data;
using StarSalvager.ScriptableObjects;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JsonDataTypes;
using UnityEngine;

namespace StarSalvager.Factories
{
    public class BitAttachableFactory : AttachableFactoryBase<BitProfile, BIT_TYPE>
    {
        private BitRemoteDataScriptableObject remoteData;
        
        //============================================================================================================//
        
        public BitAttachableFactory(AttachableProfileScriptableObject factoryProfile, BitRemoteDataScriptableObject remoteData) : base(factoryProfile)
        {
            this.remoteData = remoteData;
        }

        public void UpdateBitData(BIT_TYPE bitType, int level, ref Bit bit)
        {
            var profile = factoryProfile.GetProfile(bitType);
            var sprite = profile.GetSprite(level);
            
            bit.SetSprite(sprite);
        }
        
        //============================================================================================================//
        
        /// <summary>
        /// Sets the Bit data based on the BlockData passed. This includes Type, Sprite & level. Returns the GameObject
        /// </summary>
        /// <param name="blockData"></param>
        /// <returns></returns>
        public GameObject CreateGameObject(BlockData blockData)
        {
            var profile = factoryProfile.GetProfile((BIT_TYPE)blockData.Type);
            var sprite = profile.GetSprite(blockData.Level);

            if (!Recycler.TryGrab(out Bit temp))
            {
                temp = Object.Instantiate(factoryProfile.Prefab).GetComponent<Bit>();
            }
            temp.SetSprite(sprite);
            temp.LoadBlockData(blockData);
            temp.SetupHealthValues(25, 25);

            return temp.gameObject;
        }
        /// <summary>
        /// Sets the Bit data based on the BlockData passed. This includes Type, Sprite & level. Returns the T
        /// </summary>
        /// <param name="blockData"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T CreateObject<T>(BlockData blockData)
        {
            var temp = CreateGameObject(blockData);

            return temp.GetComponent<T>();

        }
        
        //============================================================================================================//
        
                
        /// <summary>
        /// Sets the Bit data based on the BlockData passed. This includes Type, Sprite & level. Returns the GameObject
        /// </summary>
        /// <param name="blockData"></param>
        /// <returns></returns>
        public GameObject CreateGameObject(BIT_TYPE bitType, int level = 0)
        {
            var blockData = new BlockData
            {
                Level = level,
                Type = (int) bitType
            };

            return CreateGameObject(blockData);
        }
        /// <summary>
        /// Sets the Bit data based on the BlockData passed. This includes Type, Sprite & level. Returns the T
        /// </summary>
        /// <param name="blockData"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T CreateObject<T>(BIT_TYPE bitType, int level = 0)
        {
            var temp = CreateGameObject(bitType, level);
                
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

