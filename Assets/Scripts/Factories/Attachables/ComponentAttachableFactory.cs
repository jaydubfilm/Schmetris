using System;
using Recycling;
using StarSalvager.Factories.Data;
using StarSalvager.ScriptableObjects;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JsonDataTypes;
using UnityEngine;
using Object = UnityEngine.Object;

namespace StarSalvager.Factories
{
    //TODO Need to convert this factory away from the Attachable setup
    [Obsolete]
    public class ComponentAttachableFactory : AttachableFactoryBase<ComponentProfile, COMPONENT_TYPE>
    {
        private readonly ComponentRemoteDataScriptableObject _remoteData;
        
        //============================================================================================================//
        
        public ComponentAttachableFactory(AttachableProfileScriptableObject factoryProfile, ComponentRemoteDataScriptableObject remoteData) : base(factoryProfile)
        {
            _remoteData = remoteData;
        }
        
        //============================================================================================================//

        
        public ComponentProfile GetComponentProfile(COMPONENT_TYPE type)
        {
            return factoryProfile.GetProfile(type);
        }
        
        public ComponentRemoteData GetComponentRemoteData(COMPONENT_TYPE type)
        {
            return _remoteData.GetRemoteData(type);
        }
        
        public void UpdateComponentData(COMPONENT_TYPE componentType, int level, ref Component component)
        {
            var profile = factoryProfile.GetProfile(componentType);
            var sprite = profile.GetSprite(level);
            
            component.SetSprite(sprite);
            
            switch (component)
            {
                case AnimatedComponent _:
                    component.gameObject.name = $"{nameof(AnimatedComponent)}_{componentType}_Lvl{level}";
                    break;
                case Component _:
                    component.gameObject.name = $"{nameof(Component)}_{componentType}_Lvl{level}";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(component), component, null);
            }
        }
        
        //============================================================================================================//


        /// <summary>
        /// Sets the Bit data based on the BlockData passed. This includes Type, Sprite & level. Returns the GameObject
        /// </summary>
        /// <param name="type"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public GameObject CreateGameObject(COMPONENT_TYPE type, int level = 0)
        {
            var remote = _remoteData.GetRemoteData(type);
            var profile = factoryProfile.GetProfile(type);

            
            var sprite = profile.GetSprite(level);

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

            //temp.Type = type;
            temp.SetColliderActive(true);
            

            //Have to check for null, as the Asteroid/Energy does not have health
            /*if (remote != null)
            {
                var health = remote.health;
                temp.SetupHealthValues(health,health);
            }*/

            return temp.gameObject;
        }

        /// <summary>
        /// Sets the Bit data based on the BlockData passed. This includes Type, Sprite & level. Returns the T
        /// </summary>
        /// <param name="type"></param>
        /// <param name="level"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T CreateObject<T>(COMPONENT_TYPE type, int level = 0)
        {
            var temp = CreateGameObject(type, level);

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
        public T CreateObject<T>(BlockData blockData)
        {
            var temp = CreateGameObject(blockData);

            return temp.GetComponent<T>();

        }
        
        public GameObject CreateGameObject(BlockData blockData)
        {
            var type = (COMPONENT_TYPE) blockData.Type;
            
            var remote = _remoteData.GetRemoteData(type);
            var profile = factoryProfile.GetProfile(type);

            var sprite = profile.GetSprite(blockData.Level);

            //--------------------------------------------------------------------------------------------------------//
            
            Component temp;
            //If there is an animation associated with this profile entry, create the animated version of the prefab
            if (profile.animation != null)
            {
                if (!Recycler.TryGrab(out AnimatedComponent anim))
                {
                    anim = CreateAnimatedObject<AnimatedComponent>();
                }
                
                anim.SimpleAnimator.SetAnimation(profile.animation);
                temp = anim;
                
                temp.gameObject.name = $"{nameof(AnimatedComponent)}_{type}_Lvl{blockData.Level}";
            }
            else
            {
                if (!Recycler.TryGrab(out temp))
                {
                    temp = CreateObject<Component>();
                }

                temp.gameObject.name = $"{nameof(Component)}_{type}_Lvl{blockData.Level}";
            }

            //--------------------------------------------------------------------------------------------------------//

            if (profile.animation == null)
            {
                ((BoxCollider2D)temp.collider).size = sprite.bounds.size;
            }
            temp.SetSprite(sprite);
            temp.SetColliderActive(true);
            //temp.LoadBlockData(blockData);

            //Have to check for null, as the Asteroid/Energy does not have health
            /*if (remote != null)
            {
                var health = remote.health;
                temp.SetupHealthValues(health,health);
            }*/

            return temp.gameObject;
        }
    }
}


