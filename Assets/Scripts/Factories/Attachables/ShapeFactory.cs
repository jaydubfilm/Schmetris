﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.Factories
{
    public class ShapeFactory : FactoryBase
    {
        private readonly GameObject prefab;
        
        //============================================================================================================//

        public ShapeFactory(GameObject prefab)
        {
            this.prefab = prefab;
        }
        
        //============================================================================================================//
        
        public override GameObject CreateGameObject()
        {
            return Object.Instantiate(prefab);
        }

        public override T CreateObject<T>()
        {
            return CreateGameObject().GetComponent<T>();
        }
        
        //============================================================================================================//

        public T CreateObject<T>(BIT_TYPE bitType, int totalBits)
        {
            var bitFactory = AttachableFactory.Instance.GetFactory<BitAttachableFactory>();
            
            var shape = CreateObject<Shape>();
            for (var i = 0; i < totalBits; i++)
            {
                var bit = bitFactory.CreateObject<Bit>(bitType);
                shape.PushNewBit(bit, (DIRECTION)Random.Range(0, 4));
            }

            return shape.GetComponent<T>();
        }
        
        //============================================================================================================//
    }
}

