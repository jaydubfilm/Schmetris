﻿using System;
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
        private readonly BitRemoteDataScriptableObject _remoteData;

        //============================================================================================================//
        
        public BitAttachableFactory(AttachableProfileScriptableObject factoryProfile, BitRemoteDataScriptableObject remoteData) : base(factoryProfile)
        {
            _remoteData = remoteData;
        }

        public void UpdateBitData(BIT_TYPE bitType, int level, ref Bit bit)
        {
            var profile = factoryProfile.GetProfile(bitType);
            var sprite = profile.GetSprite(level);
            
            bit.SetSprite(sprite);

            switch (bit)
            {
                case AnimatedBit _:
                    bit.gameObject.name = $"{nameof(AnimatedBit)}_{bitType}_Lvl{level}";
                    break;
                case Bit _:
                    bit.gameObject.name = $"{nameof(Bit)}_{bitType}_Lvl{level}";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(bit), bit, null);
            }
        }

        public Dictionary<BIT_TYPE, int> GetTotalResources(IEnumerable<Bit> bits)
        {
            var resources = new Dictionary<BIT_TYPE, int>();

            foreach (var bit in bits)
            {
                if(!resources.ContainsKey(bit.Type))
                    resources.Add(bit.Type, 0);

                resources[bit.Type] += GetTotalResource(bit.Type, bit.level);
            }

            return resources;
        }

        public int GetTotalResource(BIT_TYPE bitType, int level)
        {
            return _remoteData.GetRemoteData(bitType).levels[level].resources;
        }

        //============================================================================================================//

        public BitProfile GetBitProfile(BIT_TYPE type)
        {
            return factoryProfile.GetProfile(type);
        }

        public Sprite GetJunkBitSprite()
        {
            return factoryProfile.JunkSprite;
        }

        public BitRemoteData GetBitRemoteData(BIT_TYPE type)
        {
            return _remoteData.GetRemoteData(type);
        }

        //============================================================================================================//

        /// <summary>
        /// Sets the Bit data based on the BlockData passed. This includes Type, Sprite & level. Returns the GameObject
        /// </summary>
        /// <param name="bitData"></param>
        /// <returns></returns>
        public GameObject CreateGameObject(BitData bitData)
        {
            var type = (BIT_TYPE) bitData.Type;
            
            var remote = _remoteData.GetRemoteData(type);
            var profile = factoryProfile.GetProfile(type);
            //FIXME I may want to put this somewhere else, and leave the level dependent sprite obtaining here
            var sprite = profile.GetSprite(bitData.Level);

            //--------------------------------------------------------------------------------------------------------//
            
            Bit temp;
            //If there is an animation associated with this profile entry, create the animated version of the prefab
            if (profile.animation != null)
            {
                if (!Recycler.TryGrab<AnimatedBit>(out AnimatedBit anim))
                {
                    anim = CreateAnimatedObject<AnimatedBit>();
                }
                
                anim.SimpleAnimator.SetAnimation(profile.animation);
                temp = anim;
                
                temp.gameObject.name = $"{nameof(AnimatedBit)}_{type}_Lvl{bitData.Level}";
            }
            else
            {
                if (!Recycler.TryGrab<Bit>(out temp))
                {
                    temp = CreateObject<Bit>();
                }
                
                temp.gameObject.name = $"{nameof(Bit)}_{type}_Lvl{bitData.Level}";
            }

            //--------------------------------------------------------------------------------------------------------//

            if (profile.animation == null)
            {
                ((BoxCollider2D)temp.collider).size = sprite.bounds.size;
            }
            temp.SetSprite(sprite);
            temp.SetColliderActive(true);
            temp.LoadBlockData(bitData);

            temp.RegisterCanBeSeen();

            //Have to check for null, as the Asteroid/Energy does not have health
            if (remote != null)
            {
                var health = remote.levels[bitData.Level].health;
                temp.SetupHealthValues(health,health);
            }

            return temp.gameObject;
        }
        /// <summary>
        /// Sets the Bit data based on the BlockData passed. This includes Type, Sprite & level. Returns the T
        /// </summary>
        /// <param name="bitData"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T CreateObject<T>(BitData bitData)
        {
            var temp = CreateGameObject(bitData);

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
            var blockData = new BitData
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

        public GameObject CreateJunkGameObject()
        {
            return Object.Instantiate(factoryProfile.JunkPrefab);
        }

        
        public T CreateJunkObject<T>()
        {
            var temp = CreateJunkGameObject();

            return temp.GetComponent<T>();
        }

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

