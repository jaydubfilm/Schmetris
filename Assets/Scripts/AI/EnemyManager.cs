using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarSalvager.Constants;
using StarSalvager.Factories;
using StarSalvager.AI;

namespace StarSalvager
{
    public class EnemyManager : MonoBehaviour
    {
        private Enemy[] m_enemies;

        // Start is called before the first frame update
        void Start()
        {
            m_enemies = new Enemy[Values.numberEnemiesSpawn];

            for (int i = 0; i < m_enemies.Length; i++)
            {
                Enemy newEnemy = FactoryManager.Instance.GetFactory<EnemyFactory>().CreateObject<Enemy>(ENEMY_TYPE.Enemy1);
                m_enemies[i] = newEnemy;
                m_enemies[i].transform.position = LevelManager.Instance.WorldGrid.GetCenterOfGridSquareInGridPosition(Values.gridSizeX / 2, Values.gridSizeY / 2);
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}