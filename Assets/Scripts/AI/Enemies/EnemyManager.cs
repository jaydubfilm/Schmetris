using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarSalvager.Values;
using StarSalvager.Factories;
using StarSalvager.AI;
using StarSalvager.Utilities;
using StarSalvager.Utilities.Inputs;
using Random = UnityEngine.Random;
using Recycling;
using StarSalvager.Audio;
using StarSalvager.Cameras;

namespace StarSalvager
{
    public class EnemyManager : MonoBehaviour, IReset, IPausable
    {
        private List<Enemy> m_enemies;

        //Variables to spawn enemies throughout a stage
        private List<string> m_enemiesToSpawn;
        private List<float> m_timesToSpawn;
        private const float m_endOfStageSpawnBuffer = 0.5f;
        private float m_spawnTimer;
        private int m_nextStageToSpawn;

        //Input Manager variables - -1.0f for left, 0 for nothing, 1.0f for right
        //private float m_currentInput;

        //private float m_distanceHorizontal = 0.0f;

        public bool isPaused => GameTimer.IsPaused;

        private bool m_enemiesInert = false;

        private bool _hasActiveEnemies;

        //============================================================================================================//
         
        // Start is called before the first frame update
        private void Start()
        {
            m_enemies = new List<Enemy>();
            m_enemiesToSpawn = new List<string>();
            m_timesToSpawn = new List<float>();
            RegisterPausable();

            //RegisterMoveOnInput();
        }

        // Update is called once per frame
        private void Update()
        {
            if (isPaused)
                return;

            if (!GameManager.IsState(GameState.LEVEL) || GameManager.IsState(GameState.LevelBotDead))
            {
                return;
            }

            if (LevelManager.Instance.CurrentStage == m_nextStageToSpawn)
            {
                SetupStage(m_nextStageToSpawn);
            }
            CheckSpawns();

            HandleEnemyUpdate();
        }

        private void LateUpdate()
        {
            if (!GameManager.IsState(GameState.LEVEL) || GameManager.IsState(GameState.LevelBotDead))
                return;
            
            if (!_hasActiveEnemies && m_enemies.Count > 0 && GameManager.IsState(GameState.LEVEL_ACTIVE) && !GameManager.IsState(GameState.LevelEndWave))
            {
                _hasActiveEnemies = true;
                AudioController.CrossFadeTrack(MUSIC.ENEMY);
            }
            else if (_hasActiveEnemies && (m_enemies.Count == 0 || GameManager.IsState(GameState.LevelEndWave)))
            {
                _hasActiveEnemies = false;

                //TODO Get the Level Track
                AudioController.CrossFadeTrack(MUSIC.FRINGE);

                /*if (m_enemies.Count == 0 && GameManager.IsState(GameState.LEVEL_ACTIVE))
                {
                    //AudioController.CrossFadePreviousTrack();
                    AudioController.fade
                }*/
            }
        }

        //============================================================================================================//

        public void Activate()
        {
            //Spawn enemies from stage 0
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
                        Recycler.Recycle<EnemyAttachable>(m_enemies[i].gameObject);
                        break;
                    
                    case Enemy _:
                        Recycler.Recycle<Enemy>(m_enemies[i].gameObject);
                        break;
                    default:
                        throw new ArgumentException();
                }
                
                m_enemies.RemoveAt(i);
            }
        }
        
        //============================================================================================================//

        private void HandleEnemyUpdate()
        {
            Vector3 playerBotPosition = LevelManager.Instance.BotInLevel.GetPosition();
            for (int i = 0; i < m_enemies.Count; i++)
            {
                Enemy enemy = m_enemies[i];

                //Check to see if the enemy can Move
                if (!enemy.CanMove())
                    continue;
                
                enemy.UpdateEnemy(playerBotPosition);

            }
        }

        public void MoveToNewWave()
        {
            SetupStage(0);
        }

        //Get the enemies in the specified stage of the wave, and determine their future spawn times in that stage
        private void SetupStage(int stageNumber)
        {
            if (GameManager.IsState(GameState.LevelActiveEndSequence) || GameManager.IsState(GameState.LevelBotDead))
            {
                return;
            }
            
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

        public void InsertEnemySpawn(string enemyName, int count, float timeDelay)
        {
            StartCoroutine(SpawnEnemyCollectionCoroutine(enemyName, count, timeDelay));
        }

        private IEnumerator SpawnEnemyCollectionCoroutine(string enemyName, int count, float timeDelay)
        {
            if(timeDelay > 0)
                yield return new WaitForSeconds(timeDelay);
            
            if(isPaused)
                yield return new WaitUntil(() => !isPaused);
            
            if(!LevelManager.Instance.gameObject.activeSelf)
                yield break;

            string enemyId = FactoryManager.Instance.EnemyRemoteData.GetEnemyId(enemyName);

            for (int i = 0; i < count; i++)
            {
                SpawnEnemy(enemyId);
            }
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

        public void SpawnEnemy(string enemyType, Vector2? spawnLocationOverride = null)
        {
            Enemy newEnemy = FactoryManager.Instance.GetFactory<EnemyFactory>().CreateObject<Enemy>(enemyType);

            if (!m_enemies.Contains(newEnemy))
            {
                //print("TRYING TO ADD DUPLICATE ENEMY");
                m_enemies.Add(newEnemy);
            }
            newEnemy.transform.parent = LevelManager.Instance.ObstacleManager.WorldElementsRoot.transform;

            if (spawnLocationOverride.HasValue)
            {
                newEnemy.transform.localPosition = spawnLocationOverride.Value;
            }
            else
            {
                newEnemy.transform.localPosition = LevelManager.Instance.WorldGrid.GetLocalPositionOfSpawnPositionForEnemy(newEnemy);
            }

            newEnemy.LateInit();

            LevelManager.Instance.WaveEndSummaryData.AddEnemySpawned(newEnemy.EnemyName);
        }
        
        public void AddEnemy(Enemy newEnemy)
        {
            if (newEnemy == null)
                return;
            
            m_enemies.Add(newEnemy);
            ReParentEnemy(newEnemy);
            newEnemy.transform.localPosition = LevelManager.Instance.WorldGrid.GetLocalPositionOfSpawnPositionForEnemy(newEnemy);
        }

        public void RemoveEnemy(Enemy newEnemy)
        {
            if (newEnemy == null)
                return;

            m_enemies.Remove(newEnemy);
        }

        public bool HasEnemiesRemaining()
        {
            return m_enemies.Count != 0;
        }

        public void ReParentEnemy(Enemy enemy)
        {
            LevelManager.Instance.ObstacleManager.AddTransformToRoot(enemy.transform);
        }
        
        //============================================================================================================//

        public void DamageAllEnemies(float damage)
        {
            var existingEnemies = new List<Enemy>(m_enemies);
            var damageAbs = Mathf.Abs(damage);
            foreach (var enemy in existingEnemies)
            {
                if (enemy.IsRecycled)
                    continue;

                if (!CameraController.IsPointInCameraRect(enemy.transform.position))
                    continue;

                if (enemy is ICanBeHit canBeHit)
                {
                    //Position doesn't matter for enemies
                    canBeHit.TryHitAt(Vector2.zero, damageAbs);
                }
            }
        }

        public Enemy GetClosestEnemy(Vector2 position)
        {
            var shortestDist = 999f;
            Enemy closestEnemy = null;
            foreach (var enemy in m_enemies)
            {
                if (enemy.IsRecycled)
                    continue;
                
                if (!CameraController.IsPointInCameraRect(enemy.transform.position))
                    continue;
                
                var dist = Vector2.Distance(position, enemy.transform.position);
                if(dist > shortestDist)
                    continue;

                shortestDist = dist;
                closestEnemy = enemy;
            }

            return closestEnemy;
        }

        /// <summary>
        /// Returns the closest active enemy in the range radius
        /// </summary>
        /// <param name="position"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public Enemy GetClosestEnemy(Vector2 position, float range)
        {
            var shortestDist = 999f;
            Enemy closestEnemy = null;
            foreach (var enemy in m_enemies)
            {
                if (enemy.IsRecycled)
                    continue;
                
                if (!CameraController.IsPointInCameraRect(enemy.transform.position))
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
        
        public List<Enemy> GetEnemiesInRange(Vector2 position, float range)
        {
            var outList = new List<Enemy>();
            foreach (var enemy in m_enemies)
            {
                if (enemy.IsRecycled)
                    continue;
                
                if (!CameraController.IsPointInCameraRect(enemy.transform.position))
                    continue;
                
                var dist = Vector2.Distance(position, enemy.transform.position);
                
                if(dist > range)
                    continue;

                outList.Add(enemy);
            }

            return outList;
        }
        
        public void SetEnemiesInert(bool inert)
        {
            if (inert)
            {
                m_enemiesToSpawn.Clear();
                m_timesToSpawn.Clear();
            }

            m_enemiesInert = inert;
        }

        public void SetEnemiesFallEndLevel()
        {
            m_enemiesToSpawn.Clear();
            m_timesToSpawn.Clear();
        }

        public void RecycleAllEnemies()
        {
            foreach (var enemy in m_enemies)
            {
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