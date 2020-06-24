using System;
using Sirenix.OdinInspector;
using StarSalvager.ScriptableObjects;
using StarSalvager.Utilities;
using UnityEngine;

namespace StarSalvager.Factories
{
    //Based on: https://www.dofactory.com/net/factory-method-design-pattern
    public class FactoryManager : Singleton<FactoryManager>
    {
        //============================================================================================================//
        
        [SerializeField, Required, BoxGroup("Attachables/Bits")]
        private AttachableProfileScriptableObject bitProfile;
        
        [SerializeField, Required, BoxGroup("Attachables/Bits")]
        private BitRemoteDataScriptableObject bitRemoteData;

        [SerializeField, Required, BoxGroup("Attachables/Parts")] 
        private AttachableProfileScriptableObject partProfile;
        
        [SerializeField, Required, BoxGroup("Attachables/Parts")] 
        private RemotePartProfileScriptableObject partRemoteData;

        [SerializeField, Required, BoxGroup("Attachables")] 
        private GameObject shapePrefab;

        [SerializeField, Required, BoxGroup("Enemies")]
        private EnemyProfileScriptableObject enemyProfile;

        [SerializeField, Required, BoxGroup("Enemies")]
        private EnemyRemoteDataScriptableObject enemyRemoteData;

        [SerializeField, Required, BoxGroup("Projectiles")]
        private ProjectileProfileScriptableObject projectileProfile;

        //============================================================================================================//

        private FactoryBase _bitAttachableFactory;
        private FactoryBase _partAttachableFactory;
        private FactoryBase _shapeFactory;
        private FactoryBase _enemyFactory;
        private FactoryBase _projectileFactory;
        
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
                    return (_bitAttachableFactory ?? (_bitAttachableFactory = new BitAttachableFactory(bitProfile, bitRemoteData))) as T;

                case nameof(PartAttachableFactory):
                    return (_partAttachableFactory ?? (_partAttachableFactory = new PartAttachableFactory(partProfile, partRemoteData))) as T;
                
                case nameof(ShapeFactory):
                    return (_shapeFactory ?? (_shapeFactory = new ShapeFactory(shapePrefab))) as T;

                case nameof(EnemyFactory):
                    return (_enemyFactory ?? (_enemyFactory = new EnemyFactory(enemyProfile, enemyRemoteData))) as T;

                case nameof(ProjectileFactory):
                    return (_projectileFactory ?? (_projectileFactory = new ProjectileFactory(projectileProfile))) as T;

                default:
                    throw new ArgumentOutOfRangeException(nameof(typeName), typeName, null);
            }
        }
        
        //============================================================================================================//
    }
}


