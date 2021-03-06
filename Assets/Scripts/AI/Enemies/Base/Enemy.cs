﻿using System;
using Recycling;
using UnityEngine;
using StarSalvager.Factories;
using StarSalvager.Values;
using Sirenix.OdinInspector;
using StarSalvager.Utilities.Animations;
using StarSalvager.Utilities;
using System.Linq;
using StarSalvager.Audio;
using StarSalvager.Audio.Enemies;
using StarSalvager.Audio.Interfaces;
using StarSalvager.Cameras;
using StarSalvager.Prototype;
using StarSalvager.Utilities.Analytics;
using StarSalvager.Utilities.Analytics.SessionTracking;
using Random = UnityEngine.Random;
using StarSalvager.Utilities.Particles;
using StarSalvager.Utilities.Saving;
using StarSalvager.UI.Hints;
using StarSalvager.Utilities.Interfaces;

namespace StarSalvager.AI
{
    //Explore this as solution to removing the requirement: http://answers.unity.com/answers/874150/view.html
    //[RequireComponent(typeof(StateAnimator))]
    public abstract class Enemy : CollidableBase, ICanBeHit, IHealth, ICanFreeze, IStateAnimation, ICanBeSeen, IOverrideRecycleType, IPlayEnemySounds, IHasBounds
    {
        protected static EnemyManager EnemyManager
        {
            get
            {
                if (_enemyManager == null)
                    _enemyManager = LevelManager.Instance.EnemyManager;

                return _enemyManager;
            }
        }

        private static EnemyManager _enemyManager;
        
        public abstract bool IgnoreObstacleAvoidance { get; }

        public abstract bool SpawnAboveScreen { get; }

        public float EnemyMovementSpeed => _enemyMovementSpeed;
        protected float _enemyMovementSpeed { get; set; }

        public string EnemyName => m_enemyData.Name;
        
        //ICanBeSeen Properties
        //====================================================================================================================//
        
        public bool IsSeen { get; set; }
        public float CameraCheckArea => 0.6f;
        
        //============================================================================================================//
        
        protected EnemyData m_enemyData;

        protected float m_fireTimer;

        /*protected float m_fireTimer;
        private Vector3 m_spiralAttackDirection = Vector3.down;
        private List<Vector3> m_positions = new List<Vector3>();

        private Vector3 m_currentHorizontalMovementDirection = Vector3.right;
        private float m_horizontalMovementYLevel;
        private float m_horizontalMovementYLevelOrigin;
        private float m_oscillationTimer;

        private float horizontalFarLeftX;
        private float horizontalFarRightX;
        private float verticalLowestAllowed;*/

        public Vector3 MostRecentMovementDirection { get; protected set; }

        public bool Disabled { get; protected set; }

        public bool Frozen => FreezeTime > 0f;
        public float FreezeTime { get; private set; }

        public EnemySoundBase EnemySoundBase { get; protected set; }

        //IStateAnimation Properties 
        //============================================================================================================//

        public StateAnimator StateAnimator
        {
            get
            {
                if (_simpleAnimator == null)
                    _simpleAnimator = GetComponent<StateAnimator>();

                return _simpleAnimator;
            }
        }
        private StateAnimator _simpleAnimator;
        
        //============================================================================================================//

        public float StartingHealth { get; private set; }

        [ShowInInspector, ReadOnly, ProgressBar(0,"StartingHealth")]
        public float CurrentHealth { get; protected set; }

        //============================================================================================================//

        public virtual void Init(EnemyData enemyData)
        {
            m_enemyData = enemyData;
            _enemyMovementSpeed = enemyData.MovementSpeed;

            ((BoxCollider2D) collider).size = m_enemyData.Dimensions;
            
            SetupHealthValues(m_enemyData.Health, m_enemyData.Health);
            
            SetSprite(m_enemyData?.Sprite);
            SetAnimationController(m_enemyData?.AnimationController);
            
            RegisterCanBeSeen();
        }

        public virtual void OnSpawned()
        { }

        public virtual void SetFrozen(in float time)
        {
            FreezeTime = time;
        }

        protected void ApplyFallMotion()
        {
            Vector3 fallAmount = Vector3.down * (Constants.gridCellSize * Time.deltaTime / Globals.TimeForAsteroidToFallOneSquare);
            transform.position += fallAmount;
            
            
            MostRecentMovementDirection = Vector3.down;
            _enemyMovementSpeed = Constants.gridCellSize / Globals.TimeForAsteroidToFallOneSquare;

            if (transform.position.y < -10)
                DestroyEnemy();
        }
        
        /// <summary>
        /// Move the enemy away from the player, at the specified move speed, then when IsOffScreen() returns true, destroy the enemy
        /// </summary>
        protected virtual void ApplyFleeMotion()
        {
            var currentPosition = transform.position;
            var dir = (LevelManager.Instance.BotInLevel.transform.position - currentPosition).normalized;

            if(m_enemyData.MovementSpeed > 0)
                _enemyMovementSpeed = m_enemyData.MovementSpeed;
                
                
            currentPosition -= dir * (EnemyMovementSpeed * Time.deltaTime);

            transform.position = currentPosition;
        }

        //States
        //====================================================================================================================//
        
        protected STATE currentState {get; private set;}= STATE.NONE;
        protected STATE previousState {get; private set;}= STATE.NONE;
        
        
        protected void SetState(STATE newState)
        {
            previousState = currentState;

            currentState = newState;

            StateChanged(currentState);
        }

        protected abstract void StateChanged(STATE newState);

        protected abstract void StateUpdate();

        protected virtual void CleanStateData()
        {
            currentState = previousState = STATE.NONE;
        }

        //============================================================================================================//

        #region Firing

        protected virtual void ProcessFireLogic() { }
        /*{
            //Count down fire timer. If ready to fire, call fireAttack()
            if (m_enemyData.FireType == FIRE_TYPE.NONE)
                return;

            if (FreezeTime > 0)
            {
                FreezeTime -= Time.deltaTime;
                return;
            }
            
            if(GameTimer.IsPaused || !GameManager.IsState(GameState.LevelActive) || GameManager.IsState(GameState.LevelActiveEndSequence) || Disabled)
                return;
            
            m_fireTimer += Time.deltaTime;

            if (m_fireTimer < 1 / m_enemyData.RateOfFire)
                return;

            m_fireTimer -= 1 / m_enemyData.RateOfFire;
            FireAttack();
        }*/

        protected virtual void FireAttack() { }
        /*{
            if (!CameraController.IsPointInCameraRect(transform.position, 0.6f))
                return;

            Vector2 playerLocation = LevelManager.Instance.BotObject != null
                ? LevelManager.Instance.BotObject.transform.position
                : Vector3.right * 50;

            Vector2 targetLocation = m_enemyData.FireAtTarget ? playerLocation : Vector2.down;

            Vector2 shootDirection = m_enemyData.FireAtTarget
                ? (targetLocation - (Vector2)transform.position).normalized
                : Vector2.down;


            FactoryManager.Instance.GetFactory<ProjectileFactory>()
                .CreateObjects<Projectile>(
                    m_enemyData.ProjectileType,
                    transform.position,
                    targetLocation,
                    shootDirection,
                    1f,
                    "Player",
                    null);
        }*/

        #endregion Firing

        //============================================================================================================//

        #region Movement

        public bool CanMove()
        {
            const GameState STATE = GameState.LevelActiveEndSequence | GameState.LevelBotDead | GameState.LevelEndWave | ~ GameState.LevelActive;

            if (GameTimer.IsPaused)
                return false;

            if (Disabled)
            {
                ApplyFallMotion();
                return false;
            }
           
            if (GameManager.ContainsState(STATE))
            {
                //FIXME Might be better to broadcast to every enemy that the level has concluded
                if (this is EnemyAttachable enemyAttachable && enemyAttachable.IsAttachable)
                    enemyAttachable.SetAttached(false);

                ApplyFleeMotion();
                
                //If an enemy moves offscreen during the wave outro, we can clean them up
                if(IsOffScreen(Position))
                    DestroyEnemy();

                return false;
            }

            if (!Frozen) return true;

            FreezeTime -= Time.deltaTime;
            return false;

        }

        public abstract void UpdateEnemy(Vector2 playerLocation);
        /*{
            if (Disabled || Frozen)
            {
                Vector3 fallAmount = Vector3.up * ((Constants.gridCellSize * Time.deltaTime) / Globals.TimeForAsteroidToFallOneSquare);
                transform.position -= fallAmount;
                return;
            }

            Vector3 movementDirection = GetMovementNormalized(playerLocation);
            movementDirection.Normalize();
            m_mostRecentMovementDirection = movementDirection;

            transform.position = gameObject.transform.position + (movementDirection * (m_enemyData.MovementSpeed * Time.deltaTime));
        }*/

        /*private Vector2 GetMovementNormalized(Vector2 playerLocation)
        {
            Vector2 movementDirection = GetMovementDirection(playerLocation).normalized;

            if (IgnoreObstacleAvoidance)
            {
                return movementDirection;
            }

            Vector2 force = LevelManager.Instance.AIObstacleAvoidance.CalculateForceAtPoint(transform.position, IsAttachable);
            movementDirection += force;

            movementDirection.Normalize();

            return movementDirection;
        }*/

        protected virtual Vector2 GetMovementDirection(Vector2 playerLocation)
        {
            return Vector2.zero;
        }

        protected override void OnCollide(GameObject gameObject, Vector2 worldHitPoint) { }

        #endregion

        //====================================================================================================================//

        #region Animations

        protected virtual void SetAnimationController(in AnimationControllerScriptableObject animationController)
        {
            StateAnimator.SetController(animationController);
        }

        #endregion //Animations

        //ICanBeHit functions
        //============================================================================================================//

        public bool TryHitAt(Vector2 worldPosition, float damage)
        {
            ChangeHealth(-damage);
            
            var explosion = FactoryManager.Instance.GetFactory<EffectFactory>().CreateEffect(EffectFactory.EFFECT.EXPLOSION);
            explosion.transform.position = worldPosition;
            
            var particleScaling = explosion.GetComponent<ParticleSystemGroupScaling>();
            var time = particleScaling.AnimationTime;

            Destroy(explosion, time);
            
            if(CurrentHealth > 0)
                AudioController.PlaySound(SOUND.ENEMY_IMPACT);

            return true;
        }
        

        //IHealth Functions
        //============================================================================================================//

        public void SetupHealthValues(float startingHealth, float currentHealth)
        {
            StartingHealth = startingHealth;
            CurrentHealth = currentHealth;
        }

        public virtual void ChangeHealth(float amount)
        {
            CurrentHealth += amount;

            if (amount < 0)
            {
                FloatingText.Create($"{Mathf.Abs(amount)}", transform.position, Color.red);
            }

            if (CurrentHealth > 0) 
                return;

            KilledEnemy();
        }

        protected void DropLoot()
        {
            for (int i = 0; i < m_enemyData.RDSTables.Count; i++)
            {
                int randomRoll = Random.Range(1, 101);
                if (randomRoll > m_enemyData.RDSTableOdds[i])
                {
                    continue;
                }

                LevelManager.Instance.DropLoot(m_enemyData.RDSTables[i].rdsResult.ToList(), transform.localPosition, true);
            }
        }

        protected void KilledEnemy(in STATE targetState = STATE.DEATH)
        {
            DropLoot();

            Killed();
            
            PlayerDataManager.RecordEnemyKilled(m_enemyData.EnemyType);

            LevelManager.Instance.WaveEndSummaryData.AddEnemyKilled(name);
            LevelManager.Instance.EnemyManager.RemoveEnemy(this);
            
            SetState(targetState);
        }

        protected void Killed()
        {
            EnemySoundBase.deathSound.Play();
        }

        /// <summary>
        /// Cleans the enemy from the Enemy Manager, without  recording this as a kill by the player
        /// </summary>
        /// <param name="targetState"></param>
        protected void DestroyEnemy(in STATE targetState = STATE.DEATH)
        {
            LevelManager.Instance.EnemyManager.RemoveEnemy(this);

            SetState(targetState);
        }

        //ICanBeSeen Functions
        //============================================================================================================//

        public void RegisterCanBeSeen()
        {
            CameraController.RegisterCanBeSeen(this);
        }

        public void UnregisterCanBeSeen()
        {
            CameraController.UnRegisterCanBeSeen(this);
        }

        //====================================================================================================================//
        
        public virtual void OnEnterCamera()
        {
            if (HintManager.CanShowHint(HINT.ENEMY))
                HintManager.TryShowHint(HINT.ENEMY, 2f, () => { }, this as IHasBounds);
            //AudioController.PlayEnemyMoveSound(m_enemyData?.EnemyType);
        }

        public virtual void OnExitCamera()
        {
            //AudioController.StopEnemyMoveSound(m_enemyData.EnemyType);
        }
        
        protected static bool IsOffScreen(in Vector2 pos)
        {
            const float dif = 3 * Constants.gridCellSize;
            
            var screenRect = CameraController.VisibleCameraRect;

            if (pos.y <= screenRect.yMin - dif || pos.y >= screenRect.yMax + dif)
                return true;
                
            if (pos.x <= screenRect.xMin - dif || pos.x >= screenRect.xMax + dif)
                return true;
                    
                
            return false;
        }
        
        //============================================================================================================//

        public override void CustomRecycle(params object[] args)
        {
            base.CustomRecycle(args);
            
            CleanStateData();
            
            MostRecentMovementDirection = Vector3.zero;

            FreezeTime = 0f;
            Disabled = false;
            //AudioController.StopEnemyMoveSound(m_enemyData.EnemyType);
            UnregisterCanBeSeen();
        }

        public abstract Type GetOverrideType();

        //============================================================================================================//

        //IHasBounds Functions
        //====================================================================================================================//

        public Bounds GetBounds()
        {
            return new Bounds
            {
                center = transform.position,
                size = Vector2.one * Constants.gridCellSize
            };
        }


        //============================================================================================================//


    }
}
 