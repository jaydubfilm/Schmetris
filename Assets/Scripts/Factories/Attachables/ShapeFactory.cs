﻿using System.Collections;
using System.Collections.Generic;
using Recycling;
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
            return !Recycler.TryGrab<Shape>(out GameObject gObject) ? Object.Instantiate(prefab) : gObject;
        }

        public override T CreateObject<T>()
        {
            return CreateGameObject().GetComponent<T>();
        }
        
        //============================================================================================================//
        
        public GameObject CreateGameObject(BIT_TYPE bitType, int totalBits)
        {
            var bitFactory = FactoryManager.Instance.GetFactory<BitAttachableFactory>();
            
            var shape = CreateObject<Shape>();
            for (var i = 0; i < totalBits; i++)
            {
                var bit = bitFactory.CreateObject<Bit>(bitType);
                shape.PushNewBit(bit, (DIRECTION)Random.Range(0, 4), true);
            }

            if (LevelManager.Instance != null)
                LevelManager.Instance.ObstacleManager.AddMovableToList(shape);

            return shape.gameObject;
        }

        public T CreateObject<T>(BIT_TYPE bitType, int totalBits)
        {
            var bitFactory = FactoryManager.Instance.GetFactory<BitAttachableFactory>();
            
            var shape = CreateObject<Shape>();
            for (var i = 0; i < totalBits; i++)
            {
                var bit = bitFactory.CreateObject<Bit>(bitType);
                //shape.PushNewBit(bit, (DIRECTION)Random.Range(0, 4));
                
                shape.PushNewBit(bit, (DIRECTION)Random.Range(0, 4), true);
            }

            if (LevelManager.Instance != null)
                LevelManager.Instance.ObstacleManager.AddMovableToList(shape);

            return shape.GetComponent<T>();
        }

        public T CreateObject<T>(SELECTION_TYPE selectionType, BIT_TYPE bitType, int totalBits)
        {
            BIT_TYPE type;
            if (selectionType == SELECTION_TYPE.RANDOMSINGLE || selectionType == SELECTION_TYPE.RANDOMVARIED)
            {
                type = (BIT_TYPE)Random.Range(0, 6);
            }
            else
            {
                type = bitType;
            }

            var bitFactory = FactoryManager.Instance.GetFactory<BitAttachableFactory>();

            var shape = CreateObject<Shape>();
            for (var i = 0; i < totalBits; i++)
            {
                var bit = bitFactory.CreateObject<Bit>(type);
                shape.PushNewBit(bit, (DIRECTION)Random.Range(0, 4), true);

                if (selectionType == SELECTION_TYPE.RANDOMVARIED && type != BIT_TYPE.BLACK)
                {
                    type = (BIT_TYPE)Random.Range(1, 6);
                }
            }

            if (LevelManager.Instance != null)
                LevelManager.Instance.ObstacleManager.AddMovableToList(shape);

            return shape.GetComponent<T>();
        }

        public T CreateObject<T>(List<Bit> bits)
        {
            var shape = CreateObject<Shape>();
            var baseCoordinate = bits[0].Coordinate;
            shape.transform.position = bits[0].transform.position;
            
            foreach (var bit in bits)
            {
                bit.transform.rotation = Quaternion.identity;
                
                bit.Coordinate -= baseCoordinate;
            }
            
            shape.Setup(bits);

            return shape.GetComponent<T>();
        }
        
        public GameObject CreateGameObject(List<Bit> bits)
        {
            if (bits is null || bits.Count == 0)
                return null;
            
            var shape = CreateObject<Shape>();
            var baseCoordinate = bits[0].Coordinate;
            shape.transform.position = bits[0].transform.position;
            
            foreach (var bit in bits)
            {
                bit.Coordinate -= baseCoordinate;
            }
            
            shape.Setup(bits);

            return shape.gameObject;
        }
        
        //============================================================================================================//
    }
}

