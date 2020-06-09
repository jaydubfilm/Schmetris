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

        public abstract GameObject CreateGameObject();
        public abstract T CreateObject<T>()where T: Object;
    }
}

