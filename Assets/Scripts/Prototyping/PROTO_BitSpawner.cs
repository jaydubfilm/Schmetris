using System;
using System.Collections;
using System.Collections.Generic;
using Recycling;
using Sirenix.OdinInspector;
using StarSalvager.Audio;
using StarSalvager.Factories;
using StarSalvager.UI;
using StarSalvager.Utilities.SceneManagement;
using StarSalvager.Values;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace StarSalvager.Prototype
{
    [System.Obsolete]
    public class PROTO_BitSpawner : MonoBehaviour
    {
        void test()
        {


        }
        /*public bool generateRandomSeed;
        [DisableIf("$generateRandomSeed")]
        public int seed = 1234567890;
        
        public int bitCount;

        public Vector2Int spawnGridDimensions;

        private new Transform transform;

        private List<Vector2Int> usedCoordinates;
        
        
        
        // Start is called before the first frame update
        private void Start()
        {
            if (generateRandomSeed)
            {
                seed = Random.Range(int.MinValue, int.MaxValue);
                Debug.Log($"Generated Seed {seed}");
            }
            
            Random.InitState(seed);
            transform = gameObject.transform;

            CreateGrid();
        }

        private void LateUpdate()
        {
            if (Input.GetKeyDown(KeyCode.R))
                SceneManager.LoadScene("AlexB_Prototyping");
        }

        private void CreateGrid()
        {
            var bitFactory = FactoryManager.Instance.GetFactory<BitAttachableFactory>();
            usedCoordinates = new List<Vector2Int>();
            for (var i = 0; i < bitCount; i++)
            {
                var coordinate = new Vector2Int(
                    Random.Range(-spawnGridDimensions.x, spawnGridDimensions.x),
                    Random.Range(-spawnGridDimensions.y, spawnGridDimensions.y));
                
                
                while (coordinate == Vector2Int.zero || usedCoordinates.Contains(coordinate))
                {
                    coordinate = new Vector2Int(
                        Random.Range(-spawnGridDimensions.x, spawnGridDimensions.x),
                        Random.Range(-spawnGridDimensions.y, spawnGridDimensions.y));
                }

                var type = (BIT_TYPE) Random.Range(1, 7);
                //var type = BIT_TYPE.GREY;

                var temp = bitFactory.CreateGameObject(type).transform;

                var position = (Vector2)coordinate * Constants.gridCellSize;

                temp.gameObject.name = $"BitPrefab_{coordinate}";
                temp.position = position;
                temp.SetParent(transform, true);

                
                usedCoordinates.Add(coordinate);
            }
        }*/
        
    }
}

