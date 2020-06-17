using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace StarSalvager.Prototype
{
    public class PROTO_BitSpawner : MonoBehaviour
    {
        public bool generateRandomSeed;
        [DisableIf("$generateRandomSeed")]
        public int seed = 1234567890;
        
        public int bitCount;

        public float bitSize = 1.28f;

        public GameObject BitPrefab;

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

        private void CreateGrid()
        {
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
                
                var temp = Instantiate(BitPrefab).transform;
                
                

                var position = (Vector2)coordinate * bitSize;

                temp.gameObject.name = $"BitPrefab_{coordinate}";
                temp.position = position;
                temp.SetParent(transform, true);

                
                usedCoordinates.Add(coordinate);
            }
        }
        
    }
}

