﻿using System;
using System.Collections.Generic;
using Recycling;
using StarSalvager.Factories.Data;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Values;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace StarSalvager.Factories
{
    public class ShapeFactory : FactoryBase
    {
        private readonly GameObject prefab;

        private List<EditorShapeGeneratorData> customShapeData;
        private Dictionary<string, List<EditorShapeGeneratorData>> customShapeCategoryData;
        
        //============================================================================================================//

        public ShapeFactory(GameObject prefab, List<EditorShapeGeneratorData> customShapeData)
        {
            this.prefab = prefab;
            this.customShapeData = customShapeData;
            customShapeCategoryData = new Dictionary<string, List<EditorShapeGeneratorData>>();
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

            /*if (LevelManager.Instance != null)
                LevelManager.Instance.ObstacleManager.AddMovableToList(shape);*/

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

            /*if (LevelManager.Instance != null)
                LevelManager.Instance.ObstacleManager.AddMovableToList(shape);*/

            return shape.GetComponent<T>();
        }

        /*public T CreateObject<T>(SELECTION_TYPE selectionType, BIT_TYPE bitType, int totalBits)
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

            return shape.GetComponent<T>();
        }*/
    
        public T CreateObject<T>(SELECTION_TYPE selectionType, string identifier, int numRotations)
        {
            //FIXME
            if (selectionType == SELECTION_TYPE.CATEGORY)
            {
                EditorShapeGeneratorData shapeData = GetRandomInCategory(identifier);

                int totalBits = shapeData.BlockData.Count;

                var bitFactory = FactoryManager.Instance.GetFactory<BitAttachableFactory>();

                var shape = CreateObject<Shape>();
                for (var i = 0; i < totalBits; i++)
                {
                    var bit = bitFactory.CreateObject<Bit>((BIT_TYPE)shapeData.BlockData[i].Type, shapeData.BlockData[i].Level);
                    shape.PushNewBit(bit, shapeData.BlockData[i].Coordinate);
                }

                for (int i = 0; i < numRotations; i++)
                {
                    foreach (var attachable in shape.AttachedBits)
                    {
                        attachable.RotateCoordinate(ROTATION.CW);
                        attachable.transform.localPosition = (Vector2)attachable.Coordinate;
                    }
                }

                foreach (var attachable in shape.AttachedBits)
                {
                    attachable.transform.rotation = Quaternion.identity;
                }

                shape.GenerateGeometry();

                return shape.GetComponent<T>();
            }
            else if (selectionType == SELECTION_TYPE.SHAPE)
            {
                EditorShapeGeneratorData shapeData = GetByName(identifier);

                int totalBits = shapeData.BlockData.Count;

                var bitFactory = FactoryManager.Instance.GetFactory<BitAttachableFactory>();

                var shape = CreateObject<Shape>();
                for (var i = 0; i < totalBits; i++)
                {
                    var bit = bitFactory.CreateObject<Bit>((BIT_TYPE)shapeData.BlockData[i].Type, shapeData.BlockData[i].Level);
                    shape.PushNewBit(bit, shapeData.BlockData[i].Coordinate);
                }

                for (int i = 0; i < numRotations; i++)
                {
                    foreach (var attachable in shape.AttachedBits)
                    {
                        attachable.RotateCoordinate(ROTATION.CW);
                        attachable.transform.localPosition = (Vector2)attachable.Coordinate * Constants.gridCellSize;
                    }
                }

                foreach (var attachable in shape.AttachedBits)
                {
                    attachable.transform.rotation = Quaternion.identity;
                }

                shape.GenerateGeometry();

                return shape.GetComponent<T>();
            }
            else
            {
                return CreateObject<T>((BIT_TYPE)Random.Range(0, 6), 1);
            }
        }

        public T CreateObject<T>(List<Bit> bits)
        {
            if (bits == null || bits.Count == 0)
                return default;
            
            
            var shape = CreateObject<Shape>();
            var baseCoordinate = bits[0].Coordinate;
            shape.transform.position = bits[0].transform.position;
            
            foreach (var bit in bits)
            {
                bit.transform.rotation = Quaternion.identity;
                
                bit.Coordinate -= baseCoordinate;
            }
            
            shape.Setup(bits);

            /*if (LevelManager.Instance != null)
                LevelManager.Instance.ObstacleManager.AddMovableToList(shape);*/

            return shape.GetComponent<T>();
        }
        
        public GameObject CreateGameObject(List<Bit> bits)
        {
            if (bits is null || bits.Count == 0)
                return default;
            
            var shape = CreateObject<Shape>();
            var baseCoordinate = bits[0].Coordinate;
            shape.transform.position = bits[0].transform.position;
            
            foreach (var bit in bits)
            {
                bit.Coordinate -= baseCoordinate;
            }
            
            shape.Setup(bits);

            /*if (LevelManager.Instance != null)
                LevelManager.Instance.ObstacleManager.AddMovableToList(shape);*/

            return shape.gameObject;
        }

        //============================================================================================================//

        private List<EditorShapeGeneratorData> GetCategoryData(string category)
        {
            if (!customShapeCategoryData.ContainsKey(category))
                UpdateCatgeoryData(category);

            return customShapeCategoryData[category];
        }

        private EditorShapeGeneratorData GetRandomInCategory(string category)
        {
            List<EditorShapeGeneratorData> categoryData = GetCategoryData(category);
            return categoryData[Random.Range(0, categoryData.Count)];
        }

        private void UpdateCatgeoryData(string category)
        {
            customShapeCategoryData.Add(category, customShapeData.FindAll(s => s.Categories.Contains(category)));
        }

        private EditorShapeGeneratorData GetByName(string name)
        {
            return customShapeData.Find(s => s.Name == name);
        }
    }
}

