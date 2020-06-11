using System;
using StarSalvager.Factories.Data;
using StarSalvager.ScriptableObjects;
using UnityEngine;
using Object = UnityEngine.Object;

namespace StarSalvager.Factories
{
    /// <summary>
    /// Base Factory class that can be inherited to create Objects easily
    /// </summary>
    public abstract class FactoryBase
    {
        /// <summary>
        /// Create GameObject version of preset Prefab
        /// </summary>
        /// <returns></returns>
        public abstract GameObject CreateGameObject();
        /// <summary>
        /// Create GameObject of preset prefab but return UnityEngine.Object of type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public abstract T CreateObject<T>() where T: Object;
    }
    
    /// <summary>
    /// Detailed Factory Base script which allows for creation of objects which use a specific IProfile & Enum
    /// </summary>
    /// <typeparam name="U">IProfile type to use for the Attachable Profile</typeparam>
    /// <typeparam name="K">Enum used within the Profile</typeparam>
    public abstract class AttachableFactoryBase<U, K> : FactoryBase 
        where U: IProfile
        where K: Enum
    {
        /// <summary>
        /// Profile which contains detailed information about the objects to be created
        /// 
        /// </summary>
        protected readonly AttachableProfileScriptableObject<U, K> factoryProfile;

        public AttachableFactoryBase(AttachableProfileScriptableObject factoryProfile)
        {
            this.factoryProfile = factoryProfile as AttachableProfileScriptableObject<U, K>;
        }
    }
}

