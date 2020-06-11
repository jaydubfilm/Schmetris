using System;
using Sirenix.OdinInspector;
using StarSalvager.ScriptableObjects;
using StarSalvager.Utilities;
using UnityEngine;

namespace StarSalvager.Factories
{
    //Based on: https://www.dofactory.com/net/factory-method-design-pattern
    public class AttachableFactory : Singleton<AttachableFactory>
    {
        //============================================================================================================//
        
        [SerializeField, Required]
        private AttachableProfileScriptableObject bitProfile;

        [SerializeField, Required] 
        private  AttachableProfileScriptableObject partProfile;
        
        //============================================================================================================//
    
        private BitAttachableFactory _bitAttachableFactory;
        private PartAttachableFactory _partAttachableFactory;
        
        //============================================================================================================//
    
        /// <summary>
        /// Obtains a FactoryBase of Type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        //TODO Investigate whether or not I can combine both factories into single Factory
        public T GetFactory<T>() where T: FactoryBase
        {
            var typeName = typeof(T).Name;
            switch (typeName)
            {
                case nameof(BitAttachableFactory):
                    return (_bitAttachableFactory ?? (_bitAttachableFactory = new BitAttachableFactory(bitProfile))) as T;

                case nameof(PartAttachableFactory):
                    return (_partAttachableFactory ?? (_partAttachableFactory = new PartAttachableFactory(partProfile))) as T;
            
                default:
                    throw new ArgumentOutOfRangeException(nameof(typeName), typeName, null);
            }
        }
        
        //============================================================================================================//
    }
}


