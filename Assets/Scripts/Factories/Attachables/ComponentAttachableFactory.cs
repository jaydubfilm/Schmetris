﻿using Recycling;
using StarSalvager.Factories.Data;
using StarSalvager.ScriptableObjects;
using StarSalvager.Utilities.Extensions;
using UnityEngine;

namespace StarSalvager.Factories
{
    public class ComponentAttachableFactory : AttachableFactoryBase<ComponentProfile, COMPONENT_TYPE>
    {
        private readonly ComponentRemoteDataScriptableObject _remoteData;
        
        //============================================================================================================//
        
        public ComponentAttachableFactory(AttachableProfileScriptableObject factoryProfile, ComponentRemoteDataScriptableObject remoteData) : base(factoryProfile)
        {
            _remoteData = remoteData;
        }
        
        //============================================================================================================//

        
        public ComponentProfile GetBitProfile(COMPONENT_TYPE type)
        {
            return factoryProfile.GetProfile(type);
        }
        
        public ComponentRemoteData GetBitRemoteData(COMPONENT_TYPE type)
        {
            return _remoteData.GetRemoteData(type);
        }
        
        //============================================================================================================//

        
        /// <summary>
        /// Sets the Bit data based on the BlockData passed. This includes Type, Sprite & level. Returns the GameObject
        /// </summary>
        /// <param name="blockData"></param>
        /// <returns></returns>
        public GameObject CreateGameObject(COMPONENT_TYPE type)
        {
            var remote = _remoteData.GetRemoteData(type);
            var profile = factoryProfile.GetProfile(type);

            
            var sprite = profile.GetSprite(0);

            //--------------------------------------------------------------------------------------------------------//
            
            Component temp;
            //If there is an animation associated with this profile entry, create the animated version of the prefab
            if (profile.animation != null)
            {
                var anim = CreateAnimatedObject<AnimatedComponent>();
                
                anim.SimpleAnimator.SetAnimation(profile.animation);
                temp = anim;
            }
            else
            {
                temp = CreateObject<Component>();
                temp.SetSprite(sprite);
            }
            
            //--------------------------------------------------------------------------------------------------------//

            temp.SetColliderActive(true);
            

            //Have to check for null, as the Asteroid/Energy does not have health
            if (remote != null)
            {
                var health = remote.health;
                temp.SetupHealthValues(health,health);
            }

            return temp.gameObject;
        }
        /// <summary>
        /// Sets the Bit data based on the BlockData passed. This includes Type, Sprite & level. Returns the T
        /// </summary>
        /// <param name="blockData"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T CreateObject<T>(COMPONENT_TYPE type)
        {
            var temp = CreateGameObject(type);

            return temp.GetComponent<T>();

        }
        
        //============================================================================================================//
        
        public GameObject CreateAnimatedGameObject()
        {
            if (!Recycler.TryGrab<Component>(out GameObject gameObject))
            {
                gameObject = Object.Instantiate(factoryProfile.AnimatedPrefab);
            }

            return gameObject;
        }

        public T CreateAnimatedObject<T>()
        {
            return CreateGameObject().GetComponent<T>();
        }
        
        //============================================================================================================//

        public override GameObject CreateGameObject()
        {
            if (!Recycler.TryGrab<Component>(out GameObject gameObject))
            {
                gameObject = Object.Instantiate(factoryProfile.Prefab);
            }

            return gameObject;
        }

        public override T CreateObject<T>()
        {
            return CreateGameObject().GetComponent<T>();
        }
        
        //============================================================================================================//
    }
}


