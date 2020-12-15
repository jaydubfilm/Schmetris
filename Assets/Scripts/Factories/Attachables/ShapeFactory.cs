using System;
using System.Collections.Generic;
using System.Linq;
using Recycling;
using StarSalvager.Factories.Data;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JsonDataTypes;
using StarSalvager.Values;
using UnityEngine;
using UnityEngine.InputSystem.Interactions;
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

            shape.gameObject.name = $"{nameof(Shape)}_[{totalBits}]";

            return shape.GetComponent<T>();
        }
    
        public T CreateObject<T>(SELECTION_TYPE selectionType, string identifier, int numRotations, List<List<BlockData>> exclusionList = null, List<BIT_TYPE> allowedBitTypes = null, bool forceShape = false)
        {
            Shape shape;
            //FIXME
            if (selectionType == SELECTION_TYPE.CATEGORY)
            {
                EditorShapeGeneratorData shapeData = GetRandomInCategory(identifier, exclusionList);

                if (allowedBitTypes != null)
                {
                    for (int i = 0; i < 50; i++)
                    {
                        bool isAllowed = true;
                        for (int k = 0; k < shapeData.BlockData.Count; k++)
                        {
                            if (!allowedBitTypes.Contains((BIT_TYPE)shapeData.BlockData[k].Type))
                            {
                                //This is excluded from being allowed
                                isAllowed = false;
                                break;
                            }
                        }

                        if (isAllowed)
                        {
                            break;
                        }
                        shapeData = GetRandomInCategory(identifier, exclusionList);
                    }
                }

                int totalBits = shapeData.BlockData.Count;

                var bitFactory = FactoryManager.Instance.GetFactory<BitAttachableFactory>();

                if (!forceShape && totalBits == 1 && typeof(T) == typeof(IObstacle))
                {
                    return bitFactory.CreateObject<T>((BIT_TYPE)shapeData.BlockData[0].Type, shapeData.BlockData[0].Level);
                }

                shape = CreateObject<Shape>();
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

                
            }
            else if (selectionType == SELECTION_TYPE.SHAPE)
            {
                EditorShapeGeneratorData shapeData = GetByName(identifier);

                int totalBits = shapeData.BlockData.Count;

                var bitFactory = FactoryManager.Instance.GetFactory<BitAttachableFactory>();
                
                if (totalBits == 1 && typeof(T) == typeof(IObstacle))
                {
                    return bitFactory.CreateObject<T>((BIT_TYPE)shapeData.BlockData[0].Type, shapeData.BlockData[0].Level);
                }

                shape = CreateObject<Shape>();
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
            }
            else
            {
                return CreateObject<T>((BIT_TYPE)Random.Range(1, 6), 1);
            }

            shape.gameObject.name = $"{nameof(Shape)}_{selectionType}_[{shape.AttachedBits.Count}]";
            
            return shape.GetComponent<T>();
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

        public List<EditorShapeGeneratorData> GetCategoryData(string category)
        {
            if (!customShapeCategoryData.ContainsKey(category))
                UpdateCatgeoryData(category);

            return customShapeCategoryData[category];
        }

        public EditorShapeGeneratorData GetRandomInCategory(string category, List<List<BlockData>> exclusionList)
        {
            List<EditorShapeGeneratorData> categoryData = GetCategoryData(category).Where(s => !ShouldExclude(s, exclusionList)).ToList();
            if (categoryData.Count == 0)
            {
                categoryData = GetCategoryData(category);
            }

            return categoryData[Random.Range(0, categoryData.Count)];
        }

        private bool ShouldExclude(EditorShapeGeneratorData shapeData, List<List<BlockData>> exclusionList)
        {
            if (exclusionList == null)
            {
                return false;
            }
            
            List<BlockData> shapeBlockData = shapeData.BlockData;

            for (int i = 0; i < exclusionList.Count; i++)
            {
                List<BlockData> previousShapeBlockData = exclusionList[i];
                if (previousShapeBlockData.Count != shapeBlockData.Count)
                {
                    continue;
                }

                bool isEqual = true;
                for (int k = 0; k < previousShapeBlockData.Count; k++)
                {
                    if (!previousShapeBlockData[k].Equals(shapeBlockData[i]))
                    {
                        //They are not equal, we can break
                        isEqual = false;
                        break;
                    }
                }

                if (isEqual)
                {
                    //Shape is equal to previous shape
                    return true;
                }
            }

            return false;
        }

        private void UpdateCatgeoryData(string category)
        {
            customShapeCategoryData.Add(category, customShapeData.FindAll(s => s.Categories.Contains(category)));
        }

        public EditorShapeGeneratorData GetByName(string name)
        {
            return customShapeData.Find(s => s.Name == name);
        }
    }
}

