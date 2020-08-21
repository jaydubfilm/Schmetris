using System;
using Recycling;
using StarSalvager.Utilities.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace StarSalvager.Factories
{
    public class ParticleFactory : FactoryBase
    {
        private readonly GameObject explosionPrefab;
        
        //============================================================================================================//

        public ParticleFactory(GameObject explosionPrefab)
        {
            this.explosionPrefab = explosionPrefab;
        }
        
        //============================================================================================================//
        
        public override GameObject CreateGameObject()
        {
            throw new NotImplementedException();
        }

        //The intention would be to create multiple types of particles here
        public override T CreateObject<T>()
        {
            GameObject gameObject;
            
            var type = typeof(T).Name;
            switch (type)
            {
                case nameof(Explosion):
                    gameObject = CreateExplosion();
                    return gameObject.GetComponent<T>();
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
        
        //============================================================================================================//

        private GameObject CreateExplosion()
        {
            if (!Recycler.TryGrab<Explosion>(out GameObject gameObject))
            {
                gameObject = Object.Instantiate(explosionPrefab);
            }

            return gameObject;
        }
        
    }
}


