using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using StarSalvager;
using StarSalvager.Constants;
using StarSalvager.Factories;
using StarSalvager.Utilities.Extensions;
using UnityEngine;

public class PROTO_ShapeSpawner : MonoBehaviour
{
    public bool generateRandomSeed;
    [DisableIf("$generateRandomSeed")]
    public int seed = 1234567890;

    public BIT_TYPE type;
    public int bitCountMin;
    public int bitCountMax;
    
    private readonly DIRECTION[] directions = {
        DIRECTION.LEFT,
        DIRECTION.RIGHT
    };

    private new Transform transform;
    
    private void Start()
    {
        if (generateRandomSeed)
        {
            seed = Random.Range(int.MinValue, int.MaxValue);
            Debug.Log($"Generated Seed {seed}");
        }
            
        Random.InitState(seed);
        transform = gameObject.transform;

        CreateShape();
    }

    private void CreateShape()
    {
        var direction = directions[Random.Range(0, directions.Length)].ToVector2();
        var count = Random.Range(bitCountMin, bitCountMax);
        var shape = FactoryManager.Instance.GetFactory<ShapeFactory>().CreateGameObject(type, count);

        shape.name = $"Shape_{type}_{count}";
        shape.transform.position = direction * 10 * Values.gridCellSize;
        shape.transform.SetParent(transform, true);
    }
}
