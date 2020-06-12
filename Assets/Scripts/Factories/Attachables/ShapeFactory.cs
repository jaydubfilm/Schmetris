using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.Factories
{
    public class ShapeFactory : FactoryBase
    {
        private readonly GameObject prefab;

        public ShapeFactory(GameObject prefab)
        {
            this.prefab = prefab;
        }
        
        public override GameObject CreateGameObject()
        {
            throw new System.NotImplementedException();
        }

        public override T CreateObject<T>()
        {
            throw new System.NotImplementedException();
        }
    }
}

