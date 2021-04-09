using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using StarSalvager.Factories;
using StarSalvager.AI;
using StarSalvager.Utilities;
using Random = UnityEngine.Random;
using Recycling;
using StarSalvager.Audio;
using StarSalvager.Cameras;
using StarSalvager.Utilities.Extensions;

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

        private Dictionary<BorrowerEnemy, Bit> _borrowerTargets;

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

            if (!_hasActiveEnemies && m_enemies.Count > 0 && GameManager.IsState(GameState.LEVEL_ACTIVE) &&
                !GameManager.IsState(GameState.LevelEndWave))
            {
                _hasActiveEnemies = true;
                AudioController.CrossFadeTrack(MUSIC.ENEMY);
            }
            else if (_hasActiveEnemies && (m_enemies.Count == 0 || GameManager.IsState(GameState.LevelEndWave)))
            {
                _hasActiveEnemies = false;

                //TODO Get the Level Track
                AudioController.CrossFadeTrack(GameManager.IsState(GameState.LevelEndWave) ? MUSIC.NONE : MUSIC.FRINGE);

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
                Recycler.Recycle<Enemy>(m_enemies[i].gameObject);
                /*//Need to ensure that the fall through order has the base class at the bottom, and inheritors at the top
                switch (m_enemies[i])
                {
                    case EnemyAttachable enemyAttachable:
                        Recycler.Recycle<EnemyAttachable>(m_enemies[i].gameObject);
                        break;

                    case Enemy enemy:
                        Recycler.Recycle<Enemy>(m_enemies[i].gameObject);
                        break;
                    default:
                        throw new ArgumentException();
                }*/

                m_enemies.RemoveAt(i);
            }
        }

        //============================================================================================================//

        private void HandleEnemyUpdate()
        {
            Vector3 playerBotPosition = LevelManager.Instance.BotInLevel.ShootAtPosition;
            
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

        public Enemy SpawnEnemy(string enemyType, Vector2? spawnLocationOverride = null)
        {
            Enemy newEnemy = FactoryManager.Instance.GetFactory<EnemyFactory>().CreateObject<Enemy>(enemyType);

            if (!m_enemies.Contains(newEnemy))
            {
                //print("TRYING TO ADD DUPLICATE ENEMY");
                m_enemies.Add(newEnemy);
            }

            newEnemy.transform.parent = LevelManager.Instance.ObstacleManager.WorldElementsRoot.transform;

            newEnemy.transform.localPosition = spawnLocationOverride ?? LevelManager.Instance.WorldGrid.GetLocalPositionOfSpawnPositionForEnemy(newEnemy);

            newEnemy.LateInit();

            LevelManager.Instance.WaveEndSummaryData.AddEnemySpawned(newEnemy.EnemyName);

            return newEnemy;
        }

        public void AddEnemy(Enemy newEnemy)
        {
            if (newEnemy == null)
                return;

            m_enemies.Add(newEnemy);
            ReParentEnemy(newEnemy);
            newEnemy.transform.localPosition =
                LevelManager.Instance.WorldGrid.GetLocalPositionOfSpawnPositionForEnemy(newEnemy);
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

        //====================================================================================================================//
        
        #region Console Spawn
        
        public void InsertEnemySpawn(string enemyName, int count, float timeDelay)
        {
            StartCoroutine(SpawnEnemyCollectionCoroutine(enemyName, count, timeDelay));
        }
        public void InsertAllEnemySpawns(int count, float timeDelay)
        {
            var implementedEnemyNames = FactoryManager.Instance.EnemyRemoteData.m_enemyRemoteData
                .Where(x => x.isImplemented).Select(x => x.Name);
            foreach (var enemyName in implementedEnemyNames)
            {
                StartCoroutine(SpawnEnemyCollectionCoroutine(enemyName, count, timeDelay));
            }
        }

        private IEnumerator SpawnEnemyCollectionCoroutine(string enemyName, int count, float timeDelay)
        {
            if (timeDelay > 0)
                yield return new WaitForSeconds(timeDelay);

            if (isPaused)
                yield return new WaitUntil(() => !isPaused);

            if (!LevelManager.Instance.gameObject.activeSelf)
                yield break;

            string enemyId = FactoryManager.Instance.EnemyRemoteData.GetEnemyId(enemyName);

            for (int i = 0; i < count; i++)
            {
                SpawnEnemy(enemyId);
            }
        }
        
        #endregion //Console Spawn

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

        public void DamageAllEnemiesInRange(float damage, Vector2 damagePosition, float range)
        {
            var existingEnemies = new List<Enemy>(m_enemies);
            var damageAbs = Mathf.Abs(damage);
            foreach (var enemy in existingEnemies)
            {
                if (enemy.IsRecycled)
                    continue;

                if (!CameraController.IsPointInCameraRect(enemy.transform.position))
                    continue;

                if (Vector2.Distance(damagePosition, (Vector2) enemy.transform.position) > range)
                {
                    continue;
                }

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
                if (dist > shortestDist)
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

                if (dist > range)
                    continue;
                if (dist > shortestDist)
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

                if (dist > range)
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

        public void SetBorrowerTarget(in BorrowerEnemy borrowerEnemy, in Bit targetBit)
        {
            if (_borrowerTargets == null)
            {
                _borrowerTargets = new Dictionary<BorrowerEnemy, Bit>();
            }

            if (!_borrowerTargets.ContainsKey(borrowerEnemy))
            {
                _borrowerTargets.Add(borrowerEnemy, targetBit);
                return;
            }

            _borrowerTargets[borrowerEnemy] = targetBit;
        }

        public void RemoveBorrowerTarget(in BorrowerEnemy borrowerEnemy)
        {
            if (_borrowerTargets.IsNullOrEmpty())
                return;

            if (!_borrowerTargets.ContainsKey(borrowerEnemy))
                return;

            _borrowerTargets.Remove(borrowerEnemy);
        }

        public bool IsBitTargeted(in BorrowerEnemy borrowerAsking, in Bit targetBit)
        {
            //return !_borrowerTargets.IsNullOrEmpty() && _borrowerTargets.ContainsValue(targetBit);
            
            if (_borrowerTargets.IsNullOrEmpty())
                return false;

            foreach (var borrowerTarget in _borrowerTargets)
            {
                if(borrowerTarget.Key.Equals(borrowerAsking))
                    continue;
                
                var carryTarget = borrowerTarget.Key.CarryingBit;

                if (carryTarget != null && carryTarget.Equals(targetBit))
                    return true;
            }

            return false;
        }

        public bool IsBitCarried(in Bit targetBit)
        {
            if (_borrowerTargets.IsNullOrEmpty())
                return false;

            foreach (var borrowerTarget in _borrowerTargets)
            {
                /*var value = borrowerTarget.Value;
                if (value == null || !value.Equals(targetBit))
                    continue;*/

                var carryTarget = borrowerTarget.Key.CarryingBit;

                if (carryTarget != null && carryTarget.Equals(targetBit))
                    return true;
            }

            return false;
        }

        public List<Bit> GetCarriedBits()
        {
            return _borrowerTargets.IsNullOrEmpty() ? null : _borrowerTargets.Keys
                .Select(x => x.CarryingBit)
                .Where(x => x != null)
                .ToList();
        }

    //====================================================================================================================//
        

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