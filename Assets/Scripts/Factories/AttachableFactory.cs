using System;
using StarSalvager;
using UnityEngine;

namespace StarSalvager.Factories
{
    public class AttachableFactory : Singleton<AttachableFactory>
    {
        [SerializeField]
        private GameObject BitPrefab;

        [SerializeField] 
        private GameObject PartPrefab;
    
        private BitFactory _bitFactory;
        private PartFactory _partFactory;
    
        public T GetFactory<T>() where T: FactoryBase
        {
            var typeName = typeof(T).Name;
            switch (typeName)
            {
                case nameof(BitFactory):
                    return (_bitFactory ?? (_bitFactory = new BitFactory(BitPrefab))) as T;

                case nameof(PartFactory):
                    return (_partFactory ?? (_partFactory = new PartFactory(PartPrefab))) as T;
            
                default:
                    throw new ArgumentOutOfRangeException(nameof(typeName), typeName, null);
            }
        }
    }
}


