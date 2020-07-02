using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using StarSalvager;
using StarSalvager.Constants;
using StarSalvager.Factories;
using StarSalvager.Utilities.Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;
using Input = StarSalvager.Utilities.Inputs.Input;

public class PROTO_ShapeSpawner : MonoBehaviour
{
    public bool generateRandomSeed;
    [DisableIf("$generateRandomSeed")] public int seed = 1234567890;

    //public BIT_TYPE type;
    public int bitCountMin;
    public int bitCountMax;

    private readonly BIT_TYPE[] legalShapes =
    {
        BIT_TYPE.RED,
        BIT_TYPE.BLUE,
        BIT_TYPE.GREY,
        BIT_TYPE.BLACK,
        BIT_TYPE.GREEN,
        BIT_TYPE.YELLOW
    };

    private new Transform transform;

    private List<Shape> activeShapes;

    [SerializeField]
    private float fallSpeed = 30f;

    private void Start()
    {
        if (generateRandomSeed)
        {
            seed = Random.Range(int.MinValue, int.MaxValue);
            Debug.Log($"Generated Seed {seed}");
        }

        Random.InitState(seed);
        transform = gameObject.transform;

        activeShapes = new List<Shape>();
        //CreateShape();
    }

    private void Update()
    {
        if (UnityEngine.Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene("AlexB_Prototyping", LoadSceneMode.Single);
        }

        activeShapes = FindObjectsOfType<Shape>().ToList();
        
        if(activeShapes.Count == 0)
            CreateShape();

        for (var i = activeShapes.Count - 1; i >= 0; i--)
        {
            var activeShape = activeShapes[i];
            
            if(!activeShape.gameObject.activeInHierarchy)
                CreateShape();

            activeShape.transform.position += Vector3.down * (fallSpeed * Time.deltaTime);

            if (activeShape.transform.position.y < -50f)
            {
                activeShapes.Remove(activeShape);
                //Debug.Log(activeShape.transform.position.y);
                activeShape.Destroy();
            }
        }
    }

private void CreateShape()
    {
        //var direction = directions[Random.Range(0, directions.Length)].ToVector2();
        var type = legalShapes[Random.Range(0, legalShapes.Length)];
        //var type = BIT_TYPE.BLACK;
        var count = Random.Range(bitCountMin, bitCountMax);
        var shape = FactoryManager.Instance.GetFactory<ShapeFactory>().CreateObject<Shape>(type, count);

        shape.name = $"Shape_{type}_{count}";
        shape.transform.position = (Vector2.left * Random.Range(-10, 11) * Values.gridCellSize) + (Vector2.up * 20 * Values.gridCellSize);
        shape.transform.SetParent(transform, true);
    }
}
