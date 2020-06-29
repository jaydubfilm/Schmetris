using System.Collections.Generic;
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

        public Dictionary<BIT_TYPE, int> GetTotalResources(IEnumerable<Bit> bits)
        {
            var resources = new Dictionary<BIT_TYPE, int>();

            foreach (var bit in bits)
            {
                if(!resources.ContainsKey(bit.Type))
                    resources.Add(bit.Type, 0);

                resources[bit.Type] += remoteData.GetRemoteData(bit.Type).resource[bit.level];
            }

            return resources;
        }
        
        //============================================================================================================//
        
        /// <summary>
        /// Sets the Bit data based on the BlockData passed. This includes Type, Sprite & level. Returns the GameObject
        /// </summary>
        /// <param name="blockData"></param>
        /// <returns></returns>
        public GameObject CreateGameObject(BlockData blockData)
        {
            var remote = remoteData.GetRemoteData((BIT_TYPE) blockData.Type);
            var profile = factoryProfile.GetProfile((BIT_TYPE)blockData.Type);
            var sprite = profile.GetSprite(blockData.Level);
            

            if (!Recycler.TryGrab(out Bit temp))
            {
                temp = Object.Instantiate(factoryProfile.Prefab).GetComponent<Bit>();
            }
            temp.SetColliderActive(true);
            temp.SetSprite(sprite);
            temp.LoadBlockData(blockData);
            
            //Have to check for null, as the Asteroid/Energy does not have health
            if(remote != null)
                temp.SetupHealthValues(remote.health[blockData.Level], remote.health[blockData.Level]);

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

