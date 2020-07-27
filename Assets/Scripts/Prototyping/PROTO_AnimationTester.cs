using System.Collections;
using System.Collections.Generic;
using StarSalvager;
using StarSalvager.AI;
using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using StarSalvager.Utilities.Animations;
using UnityEngine;

public class PROTO_AnimationTester : MonoBehaviour
{
    [SerializeField] private GameObject objectPrefab;
    //[SerializeField]
    //private string EnemyTypeID;
    
    //private EnemyFactory _enemyFactory;

    [SerializeField]
    private int spawnCount = 1;

    // Start is called before the first frame update
    //private void Start()
    //{
    //    _enemyFactory = FactoryManager.Instance.GetFactory<EnemyFactory>();
    //}

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
            CreateAnimation(spawnCount);
    }

    private void CreateAnimation(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            //var temp = _enemyFactory.CreateObject<Enemy>(EnemyTypeID).transform;
            var temp = Instantiate(objectPrefab).transform;

            temp.position = Random.insideUnitCircle * 30f;
        }
    }
}
