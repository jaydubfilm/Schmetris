using System.Collections;
using System.Collections.Generic;
using StarSalvager.Prototype;
using UnityEngine;

namespace StarSalvager.Factories
{
    //TODO This should be a container for all of the factories
    public abstract class FactoryBase
    {
        protected readonly GameObject prefab;

        public FactoryBase(GameObject prefab)
        {
            this.prefab = prefab;
        }

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
        public abstract T CreateObject<T>()where T: Object;
    }
}

