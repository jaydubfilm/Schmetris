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

        //Input Manager variables - -1.0f for left, 0 for nothing, 1.0f for right
        private float m_currentInput;

        public bool Moving => _moving;
        private bool _moving;

        private float m_distanceHorizontal = 0.0f;

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

            LevelManager.Instance.DemoText.text =
                "\nEnemyType: " + m_enemies[0].m_enemyData.EnemyType +
                "\nMovementType: " + m_enemies[0].m_enemyData.MovementType +
                "\nAttackType: " + m_enemies[0].m_enemyData.AttackType +
                "\nMovementSpeed: " + m_enemies[0].m_enemyData.MovementSpeed +
                "\nAttackSpeed: " + m_enemies[0].m_enemyData.AttackSpeed;
        }

        private int tempDemoingVariable = 0;

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown("r"))
            {
                tempDemoingVariable++;
                if (tempDemoingVariable == 15)
                {
                    tempDemoingVariable = 0;
                }
                Destroy(m_enemies[0].gameObject);
                Enemy newEnemy = FactoryManager.Instance.GetFactory<EnemyFactory>().CreateObject<Enemy>((ENEMY_TYPE)tempDemoingVariable);
                m_enemies[0] = newEnemy;
                m_enemies[0].transform.position = LevelManager.Instance.WorldGrid.GetCenterOfGridSquareInGridPosition(Values.gridSizeX / 2, Values.gridSizeY / 2);

                LevelManager.Instance.DemoText.text = 
                    "\nEnemyType: " + m_enemies[0].m_enemyData.EnemyType +
                    "\nMovementType: " + m_enemies[0].m_enemyData.MovementType +
                    "\nAttackType: " + m_enemies[0].m_enemyData.AttackType +
                    "\nMovementSpeed: " + m_enemies[0].m_enemyData.MovementSpeed +
                    "\nAttackSpeed: " + m_enemies[0].m_enemyData.AttackSpeed;
            }
            else if (Input.GetKeyDown("t"))
            {
                m_enemies[0].transform.position = LevelManager.Instance.WorldGrid.GetCenterOfGridSquareInGridPosition(Values.gridSizeX / 2, Values.gridSizeY / 2);
            }

            Vector3 gridMovement = Vector3.zero;
            if (m_distanceHorizontal != 0)
            {
                if (m_distanceHorizontal > 0)
                {
                    float toMove = Mathf.Min(m_distanceHorizontal, Values.botHorizontalSpeed * Time.deltaTime);
                    gridMovement = Vector3.right * toMove;
                    m_distanceHorizontal -= toMove;
                }
                else if (m_distanceHorizontal < 0)
                {
                    float toMove = Mathf.Min(Mathf.Abs(m_distanceHorizontal), Values.botHorizontalSpeed * Time.deltaTime);
                    gridMovement = Vector3.left * toMove;
                    m_distanceHorizontal += toMove;
                }
            }


            //Iterate through all agents, and for each one, add the forces from nearby obstacles to their current direction vector
            //After adding the forces, normalize and multiply by the velocity to ensure consistent speed
            for (int i = 0; i < m_enemies.Length; i++)
            {
                Vector3 position = m_enemies[i].transform.position;
                Vector3 destination = m_enemies[i].GetDestination();

                Vector2 direction = new Vector2(destination.x - position.x, destination.y - position.y);
                direction.Normalize();

                if (!position.Equals(destination))
                {
                    direction += LevelManager.Instance.AIObstacleAvoidance.CalculateForceAtPoint(position);
                    direction.Normalize();
                }

                m_enemies[i].transform.position -= gridMovement;

                m_enemies[i].ProcessMovement(direction);
            }

            if (m_currentInput != 0.0f && Mathf.Abs(m_distanceHorizontal) <= 0.2f)
            {
                Move(m_currentInput);
            }
        }

        public void Move(float direction)
        {
            if (UnityEngine.Input.GetKey(KeyCode.LeftAlt))
            {
                m_currentInput = 0f;
                return;
            }

            m_currentInput = direction;

            m_distanceHorizontal += direction * Values.gridCellSize;

            _moving = true;
        }
    }
}