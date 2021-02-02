using System;
using System.Collections.Generic;
using Recycling;
using UnityEngine;
using StarSalvager.Factories;
using StarSalvager.Values;
using Sirenix.OdinInspector;
using StarSalvager.Utilities.Animations;
using StarSalvager.Utilities;
using System.Linq;
using StarSalvager.Audio;
using StarSalvager.Cameras;
using StarSalvager.Projectiles;
using StarSalvager.Utilities.Analytics;
using Random = UnityEngine.Random;
using StarSalvager.Utilities.Particles;

namespace StarSalvager.AI
{
    [RequireComponent(typeof(StateAnimator))]
    public abstract class Enemy : CollidableBase, ICanBeHit, IHealth, IStateAnimation, ICustomRecycle, ICanBeSeen
    {
        public abstract bool IsAttachable { get; }
        public abstract bool IgnoreObstacleAvoidance { get; }
        public abstract bool SpawnAboveScreen { get; }

        public float EnemyMovementSpeed => m_enemyData.MovementSpeed;
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

        [NonSerialized]
        public Vector3 m_mostRecentMovementDirection = Vector3.zero;

        public bool Disabled { get; protected set; }

        public bool Frozen => FreezeTime > 0f;
        protected float FreezeTime { get; set; }

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
            
            SetupHealthValues(m_enemyData.Health, m_enemyData.Health);
            
            renderer.sprite = m_enemyData?.Sprite;
            StateAnimator.SetController(m_enemyData?.AnimationController);
            
            RegisterCanBeSeen();
        }

        public virtual void LateInit()
        { }

        public void SetFrozen(float time)
        {
            FreezeTime = time;
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
            if(GameTimer.IsPaused )
                return false;

            if (!GameManager.IsState(GameState.LevelActive) || GameManager.IsState(GameState.LevelActiveEndSequence))
            {
                var currentPosition = transform.position;
                var dir = (LevelManager.Instance.BotInLevel.transform.position - currentPosition).normalized;
                
                
                currentPosition -= dir * EnemyMovementSpeed;
                transform.position = currentPosition;
                
                return false;
            }
            
            if (Disabled)
            {
                Vector3 fallAmount = Vector3.up * ((Constants.gridCellSize * Time.deltaTime) / Globals.TimeForAsteroidToFallOneSquare);
                transform.position -= fallAmount;
                
                return false;
            }
            
            if (FreezeTime > 0)
            {
                FreezeTime -= Time.deltaTime;
                return false;
            }

            return true;
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

        //ICanBeHit functions
        //============================================================================================================//

        public bool TryHitAt(Vector2 worldPosition, float damage)
        {
            ChangeHealth(-damage);
            
            var explosion = FactoryManager.Instance.GetFactory<EffectFactory>().CreateEffect(EffectFactory.EFFECT.EXPLOSION);
            explosion.transform.position = worldPosition;
            
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

            DropLoot();
            
            SessionDataProcessor.Instance.EnemyKilled(m_enemyData.EnemyType);
            AudioController.PlaySound(SOUND.ENEMY_DEATH);

            LevelManager.Instance.WaveEndSummaryData.AddEnemyKilled(name);
            
            

            LevelManager.Instance.EnemyManager.RemoveEnemy(this);

            Recycler.Recycle<Enemy>(this);
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
        
        public void OnEnterCamera()
        {
            AudioController.PlayEnemyMoveSound(m_enemyData?.EnemyType);
        }

        public void OnExitCamera()
        {
            AudioController.StopEnemyMoveSound(m_enemyData.EnemyType);
        }
        //============================================================================================================//

        public virtual void CustomRecycle(params object[] args)
        {
            CleanStateData();
            
            m_mostRecentMovementDirection = Vector3.zero;

            FreezeTime = 0f;
            Disabled = false;
            AudioController.StopEnemyMoveSound(m_enemyData.EnemyType);
            UnregisterCanBeSeen();
        }


        
        //============================================================================================================//

    }
}
 