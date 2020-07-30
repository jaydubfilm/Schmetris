using System.Collections.Generic;
using Recycling;
using StarSalvager.Factories.Data;
using StarSalvager.ScriptableObjects;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JsonDataTypes;
using UnityEngine;
using Object = UnityEngine.Object;

namespace StarSalvager.Factories
{
    //FIXME This needs to be cleaned up, feels messy
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

                resources[bit.Type] += GetTotalResource(bit);
            }

            return resources;
        }
        
        public int GetTotalResource(Bit bit)
        {
            return remoteData.GetRemoteData(bit.Type).resource[bit.level];
        }

        public Dictionary<BIT_TYPE, int> GetTotalResources(IEnumerable<ScrapyardBit> bits)
        {
            var resources = new Dictionary<BIT_TYPE, int>();

            foreach (var bit in bits)
            {
                if (!resources.ContainsKey(bit.Type))
                    resources.Add(bit.Type, 0);

                resources[bit.Type] += remoteData.GetRemoteData(bit.Type).resource[bit.level];
            }

            return resources;
        }

        //============================================================================================================//

        public BitProfile GetBitProfile(BIT_TYPE type)
        {
            return factoryProfile.GetProfile(type);
        }
        
        public BitRemoteData GetBitRemoteData(BIT_TYPE type)
        {
            return remoteData.GetRemoteData(type);
        }
        
        //============================================================================================================//

        
        /// <summary>
        /// Sets the Bit data based on the BlockData passed. This includes Type, Sprite & level. Returns the GameObject
        /// </summary>
        /// <param name="blockData"></param>
        /// <returns></returns>
        public GameObject CreateGameObject(BlockData blockData)
        {
            var type = (BIT_TYPE) blockData.Type;
            
            var remote = remoteData.GetRemoteData(type);
            var profile = factoryProfile.GetProfile(type);
            //FIXME I may want to put this somewhere else, and leave the level dependent sprite obtaining here
            var sprite = type == BIT_TYPE.BLACK ? profile.GetRandomSprite() : profile.GetSprite(blockData.Level);

            //--------------------------------------------------------------------------------------------------------//
            
            Bit temp;
            //If there is an animation associated with this profile entry, create the animated version of the prefab
            if (profile.animation != null)
            {
                if (!Recycler.TryGrab(out AnimatedBit anim))
                {
                    anim = CreateAnimatedObject<AnimatedBit>();
                }
                
                anim.SimpleAnimator.SetAnimation(profile.animation);
                temp = anim;
            }
            else
            {
                if (!Recycler.TryGrab(out temp))
                {
                    temp = CreateObject<Bit>();
                }
            }
            
            //--------------------------------------------------------------------------------------------------------//

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

        /// <summary>
        /// Sets the Bit data based on the BlockData passed. This includes Type, Sprite & level. Returns the GameObject
        /// </summary>
        /// <param name="blockData"></param>
        /// <returns></returns>
        public GameObject CreateScrapyardGameObject(BlockData blockData)
        {
            var remote = remoteData.GetRemoteData((BIT_TYPE)blockData.Type);
            var profile = factoryProfile.GetProfile((BIT_TYPE)blockData.Type);
            var sprite = profile.GetSprite(blockData.Level);


            if (!Recycler.TryGrab(out ScrapyardBit temp))
            {
                temp = Object.Instantiate(factoryProfile.ScrapyardPrefab).GetComponent<ScrapyardBit>();
            }
            temp.SetSprite(sprite);
            temp.LoadBlockData(blockData);

            return temp.gameObject;
        }


        /// <summary>
        /// Sets the Bit data based on the BlockData passed. This includes Type, Sprite & level. Returns the T
        /// </summary>
        /// <param name="blockData"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T CreateScrapyardObject<T>(BlockData blockData)
        {
            var temp = CreateScrapyardGameObject(blockData);

            return temp.GetComponent<T>();

        }

        //============================================================================================================//

        /// <summary>
        /// Sets the Bit data based on the BlockData passed. This includes Type, Sprite & level. Returns the GameObject
        /// </summary>
        /// <param name="blockData"></param>
        /// <returns></returns>
        public GameObject CreateScrapyardGameObject(BIT_TYPE bitType, int level = 0)
        {
            var blockData = new BlockData
            {
                Level = level,
                Type = (int)bitType
            };

            return CreateScrapyardGameObject(blockData);
        }
        /// <summary>
        /// Sets the Bit data based on the BlockData passed. This includes Type, Sprite & level. Returns the T
        /// </summary>
        /// <param name="blockData"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T CreateScrapyardObject<T>(BIT_TYPE bitType, int level = 0)
        {
            var temp = CreateScrapyardGameObject(bitType, level);

            return temp.GetComponent<T>();
        }

        //============================================================================================================//
        
        public GameObject CreateAnimatedGameObject()
        {
            return Object.Instantiate(factoryProfile.AnimatedPrefab);
        }
        
        public T CreateAnimatedObject<T>()
        {
            var temp = CreateAnimatedGameObject();

            return temp.GetComponent<T>();
        }

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

