using System;
using StarSalvager.Factories.Data;
using StarSalvager.ScriptableObjects;

namespace StarSalvager.Factories
{
    /// <summary>
    /// Detailed Factory Base script which allows for creation of objects which use a specific IProfile & Enum
    /// </summary>
    /// <typeparam name="P">IProfile type to use for the Attachable Profile</typeparam>
    /// <typeparam name="E">Enum used within the Profile</typeparam>
    public abstract class AttachableFactoryBase<P, E> : FactoryBase 
        where P: IProfile
        where E: Enum
    {
        /// <summary>
        /// Profile which contains detailed information about the objects to be created
        /// 
        /// </summary>
        protected readonly AttachableProfileScriptableObject<P, E> factoryProfile;

        public AttachableFactoryBase(AttachableProfileScriptableObject factoryProfile)
        {
            this.factoryProfile = factoryProfile as AttachableProfileScriptableObject<P, E>;
        }
    }
}

