using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarSalvager.Values;
using StarSalvager.Factories;
using StarSalvager.AI;
using System.Linq;
using StarSalvager.Utilities;
using StarSalvager.Utilities.Inputs;
using Random = UnityEngine.Random;
using Recycling;
using StarSalvager.Missions;

namespace StarSalvager
{
    public class EnemyManager : MonoBehaviour, IReset, IPausable, IMoveOnInput
    {
        private List<Enemy> m_enemies;

        //Variables to spawn enemies throughout a stage
        private List<string> m_enemiesToSpawn;
        private List<float> m_timesToSpawn;
        private const float m_endOfStageSpawnBuffer = 0.25f;
        private float m_spawnTimer;
        private int m_nextStageToSpawn;

        //Input Manager variables - -1.0f for left, 0 for nothing, 1.0f for right
        private float m_currentInput;

        private float m_distanceHorizontal = 0.0f;

        public bool isPaused => GameTimer.IsPaused;

        private bool m_enemiesInert = false;

        //============================================================================================================//
        
        // Start is called before the first frame update
        private void Start()
        {
            m_enemies = new List<Enemy>();
            m_enemiesToSpawn = new List<string>();
            m_timesToSpawn = new List<float>();
            RegisterPausable();

            RegisterMoveOnInput();
        }

        // Update is called once per frame
        private void Update()
        {
            if (isPaused)
                return;

            if (!LevelManager.Instance.EndWaveState)
            {
                if (LevelManager.Instance.CurrentStage == m_nextStageToSpawn)
                {
                    SetupStage(m_nextStageToSpawn);
                }
                CheckSpawns();
            }

            HandleEnemyMovement();
        }
        
        //============================================================================================================//

        public void Activate()
        {
            //Spawn enemies from wave 0
            SetupStage(0);
        }

        public void Reset()
        {
            for (int i = m_enemies.Count - 1; i >= 0; i--)
            {
                //Need to ensure that the fall through order has the base class at the bottom, and inheritors at the top
                switch (m_enemies[i])
                {
                    case EnemyAttachable _:
                        Recycling.Recycler.Recycle<EnemyAttachable>(m_enemies[i].gameObject);
                        break;
                    
                    case Enemy _:
                        Recycling.Recycler.Recycle<Enemy>(m_enemies[i].gameObject);
                        break;
                    default:
                        throw new ArgumentException();
                }
                
                m_enemies.RemoveAt(i);
            }
        }
        
        //============================================================================================================//

        private void HandleEnemyMovement()
        {
            Vector3 gridMovement = Vector3.zero;
            if (m_distanceHorizontal != 0)
            {
                if (m_distanceHorizontal > 0)
                {
                    float toMove = Mathf.Min(m_distanceHorizontal, Constants.botHorizontalSpeed * Time.deltaTime);
                    gridMovement = Vector3.right * toMove;
                    m_distanceHorizontal -= toMove;
                }
                else if (m_distanceHorizontal < 0)
                {
                    float toMove = Mathf.Min(Mathf.Abs(m_distanceHorizontal), Constants.botHorizontalSpeed * Time.deltaTime);
                    gridMovement = Vector3.left * toMove;
                    m_distanceHorizontal += toMove;
                }
            }

            if (m_enemiesInert)
            {
                gridMovement += Vector3.up * ((Constants.gridCellSize * Time.deltaTime) / Constants.timeForAsteroidsToFall);
            }

            //Iterate through all agents, and for each one, add the forces from nearby obstacles to their current direction vector
            //After adding the forces, normalize and multiply by the velocity to ensure consistent speed
            for (int i = 0; i < m_enemies.Count; i++)
            {
                if (m_enemies[i] is EnemyAttachable enemyAttachable)
                {
                    if (enemyAttachable.Attached)
                    {
                        continue;
                    }
                }

                //TODO: This process shouldn't be straight summing and averaging the different forces on different parts. 
                //We should be selecting for the strongest forces and using those in any given direction, otherwise, the strong forces on one position can be dampened by the weaker on others.
                m_enemies[i].transform.position -= gridMovement;

                if (m_enemiesInert)
                {
                    continue;
                }

                Vector3 destination = m_enemies[i].GetDestination();
                Vector2 sumDirection = Vector2.zero;
                foreach (Vector3 position in m_enemies[i].GetPositions())
                {
                    Vector2 direction = new Vector2(destination.x - position.x, destination.y - position.y);
                    direction.Normalize();
                    Vector2 force = LevelManager.Instance.AIObstacleAvoidance.CalculateForceAtPoint(position);
                    direction += force;
                    sumDirection += direction;
                }
                sumDirection.Normalize();

                m_enemies[i].ProcessMovement(sumDirection);
            }

            if (m_currentInput != 0.0f && Mathf.Abs(m_distanceHorizontal) <= 0.2f)
            {
                Move(m_currentInput);
            }
        }

        public void MoveToNewWave()
        {
            SetupStage(0);
        }

        private void SetupStage(int stageNumber)
        {
            StageRemoteData waveRemoteData = LevelManager.Instance.CurrentWaveData.GetRemoteData(stageNumber);
            m_enemiesToSpawn.Clear();
            m_timesToSpawn.Clear();

            //Populate enemies to spawn list
            foreach (StageEnemyData stageEnemyData in waveRemoteData.StageEnemyData)
            {
                for (int i = 0; i < stageEnemyData.EnemyCount; i++)
                {
                    string enemyType = stageEnemyData.EnemyType;
                    m_enemiesToSpawn.Add(enemyType);
                }
            }

            //Randomize list order
            for (int i = 0; i < m_enemiesToSpawn.Count; i++)
            {
                var temp = m_enemiesToSpawn[i];
                int randomIndex = Random.Range(i, m_enemiesToSpawn.Count);
                m_enemiesToSpawn[i] = m_enemiesToSpawn[randomIndex];
                m_enemiesToSpawn[randomIndex] = temp;
            }

            //Populate times to spawn list
            for (int i = 0; i < m_enemiesToSpawn.Count; i++)
            {
                float timeToSpawn = Random.Range(0, waveRemoteData.StageDuration * (1.0f - m_endOfStageSpawnBuffer));
                m_timesToSpawn.Add(timeToSpawn);
            }
            m_timesToSpawn.Sort();

            m_spawnTimer = 0;
            m_nextStageToSpawn = stageNumber + 1;
        }

        private void CheckSpawns()
        {
            if (m_timesToSpawn.Count == 0)
                return;

            m_spawnTimer += Time.deltaTime;
            if (m_spawnTimer >= m_timesToSpawn[0])
            {
                SpawnEnemy(m_enemiesToSpawn[0]);
                m_enemiesToSpawn.RemoveAt(0);
                m_timesToSpawn.RemoveAt(0);
            }
        }

        private void SpawnEnemy(string enemyType)
        {
            Enemy newEnemy = FactoryManager.Instance.GetFactory<EnemyFactory>().CreateObject<Enemy>(enemyType);
            m_enemies.Add(newEnemy);
            newEnemy.transform.position = LevelManager.Instance.WorldGrid.GetSpawnPositionForEnemy(newEnemy.m_enemyData.MovementType);
        }

        public bool HasEnemiesRemaining()
        {
            return m_enemies.Count != 0;
        }

        //IMoveOnInput
        //============================================================================================================//
        
        public void RegisterMoveOnInput()
        {
            InputManager.RegisterMoveOnInput(this);
        }

        public void Move(float direction)
        {
            if (UnityEngine.Input.GetKey(KeyCode.LeftAlt))
            {
                m_currentInput = 0f;
                return;
            }

            m_currentInput = direction;

            m_distanceHorizontal += direction * Constants.gridCellSize;
        }
        
        //============================================================================================================//

        public Enemy GetClosestEnemy(Vector2 position)
        {
            var shortestDist = 999f;
            Enemy closestEnemy = null;
            foreach (var enemy in m_enemies)
            {
                if (enemy.IsRecycled)
                    continue;
                
                var dist = Vector2.Distance(position, enemy.transform.position);
                if(dist > shortestDist)
                    continue;

                shortestDist = dist;
                closestEnemy = enemy;
            }

            return closestEnemy;
        }

        public Enemy GetClosestEnemy(Vector2 position, float range)
        {
            var shortestDist = 999f;
            Enemy closestEnemy = null;
            foreach (var enemy in m_enemies)
            {
                if (enemy.IsRecycled)
                    continue;
                
                var dist = Vector2.Distance(position, enemy.transform.position);
                
                if(dist > range)
                    continue;
                if(dist > shortestDist)
                    continue;

                shortestDist = dist;
                closestEnemy = enemy;
            }

            return closestEnemy;
        }

        public void SetEnemiesInert(bool inert)
        {
            if (inert)
                m_enemiesToSpawn.Clear();

            m_enemiesInert = inert;
        }

        public void RecycleAllEnemies()
        {
            foreach (var enemy in m_enemies)
            {
                MissionManager.ProcessEnemyKilledMissionData(enemy.m_enemyData.EnemyType, 1);
                Recycler.Recycle<Enemy>(enemy);
            }
            m_enemies.Clear();
        }

        //============================================================================================================//

        public void RegisterPausable()
        {
            GameTimer.AddPausable(this);
        }

        public void OnResume()
        {

        }

        public void OnPause()
        {

        }

        //============================================================================================================//
    }
}