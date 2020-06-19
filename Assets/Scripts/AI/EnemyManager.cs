using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarSalvager.Constants;
using StarSalvager.AI;

public class EnemyManager : MonoBehaviour
{
    private Enemy[] m_enemies;

    // Start is called before the first frame update
    void Start()
    {
        m_enemies = new Enemy[Values.numberEnemiesSpawn];
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
