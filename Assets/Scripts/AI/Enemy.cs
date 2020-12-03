using System;
using System.Collections.Generic;
using Recycling;
using UnityEngine;
using StarSalvager.Factories;
using StarSalvager.Values;
using Sirenix.OdinInspector;
using StarSalvager.Utilities.Animations;
using StarSalvager.Missions;
using StarSalvager.Utilities;
using System.Linq;
using StarSalvager.Audio;
using StarSalvager.Cameras;
using StarSalvager.Projectiles;
using StarSalvager.Utilities.Analytics;
using Random = UnityEngine.Random;

namespace StarSalvager.AI
{
    [RequireComponent(typeof(StateAnimator))]
    public class Enemy : CollidableBase, ICanBeHit, IHealth, IStateAnimation, ICustomRecycle, ICanBeSeen
    {
        public bool IsAttachable => m_enemyData.IsAttachable;
        public bool IgnoreObstacleAvoidance => m_enemyData.IgnoreObstacleAvoidance;
        public ENEMY_MOVETYPE MovementType
        {
            get
            {
                if (m_enemyMovetypeOverride != null)
                {
                    return m_enemyMovetypeOverride.Value;
                }

                return m_enemyData.MovementType;
            }
        }
        public string EnemyName => m_enemyData.Name;

        //ICanBeSeen Properties
        //====================================================================================================================//
        
        public bool IsSeen { get; set; }
        public float CameraCheckArea => 0.6f;
        
        //============================================================================================================//
        
        protected EnemyData m_enemyData;

        private ENEMY_MOVETYPE? m_enemyMovetypeOverride = null;

        protected float m_fireTimer;
        private Vector3 m_spiralAttackDirection = Vector3.down;
        private List<Vector3> m_positions = new List<Vector3>();
        
        
        private Vector3 m_currentHorizontalMovementDirection = Vector3.right;
        private float m_horizontalMovementYLevel;
        private float m_horizontalMovementYLevelOrigin;
        private float m_oscillationTimer;

        private float horizontalFarLeftX;
        private float horizontalFarRightX;
        private float verticalLowestAllowed;

        protected Vector3 m_mostRecentMovementDirection = Vector3.zero;

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

        protected virtual void Start()
        {
            SetupPositions();

            m_horizontalMovementYLevel = transform.position.y;
            horizontalFarLeftX = -1 * Constants.gridCellSize * Globals.ColumnsOnScreen / 3.5f;
            horizontalFarRightX = Constants.gridCellSize * Globals.ColumnsOnScreen / 3.5f;
        }

        public void SetHorizontalMovementYLevel()
        {
            m_horizontalMovementYLevel = transform.position.y;
            m_horizontalMovementYLevelOrigin = m_horizontalMovementYLevel;
            verticalLowestAllowed = m_horizontalMovementYLevel / 2;
            horizontalFarLeftX = -1 * Constants.gridCellSize * Globals.ColumnsOnScreen / 3.5f;
            horizontalFarRightX = Constants.gridCellSize * Globals.ColumnsOnScreen / 3.5f;
        }

        protected virtual void Update()
        {
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
        }

        //============================================================================================================//

        public void Init(EnemyData enemyData)
        {
            m_enemyData = enemyData;
            
            SetupHealthValues(m_enemyData.Health, m_enemyData.Health);
            
            renderer.sprite = m_enemyData?.Sprite;
            StateAnimator.SetController(m_enemyData?.AnimationController);
            
            RegisterCanBeSeen();
        }

        private void SetupPositions()
        {
            for (float i = 0; i < m_enemyData.Dimensions.x; i++)
            {
                for (float k = 0; k < m_enemyData.Dimensions.y; k++)
                {
                    m_positions.Add(new Vector3(i - ((float)m_enemyData.Dimensions.x - 1) / 2, k - ((float)m_enemyData.Dimensions.y - 1) / 2, 0));
                }
            }
        }

        public void SetFrozen(float time)
        {
            FreezeTime = time;
        }

        //============================================================================================================//
        
        #region Firing
        
        protected virtual void FireAttack()
        {
            /*var distance = Vector3.Distance(transform.position, LevelManager.Instance.BotGameObject.transform.position);
            //TODO Determine if this fires at all times or just when bot is in range
            if (distance >= 100 * Constants.gridCellSize)
                return;*/

            //Vector3 screenPoint = Camera.main.WorldToViewportPoint();
            //bool onScreen = screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;
            if (!CameraController.IsPointInCameraRect(transform.position, 0.6f))
                return;
            
            Vector2 playerLocation = LevelManager.Instance.BotObject != null
                ? LevelManager.Instance.BotObject.transform.position
                : Vector3.right * 50;

            Vector2 targetLocation = m_enemyData.FireAtTarget ? playerLocation : Vector2.down;
            
            /*switch (m_enemyData.FireType)
            {
                case FIRE_TYPE.SPIRAL:
                    targetLocation = Vector2.down;
                    break;
                case FIRE_TYPE.FORWARD:
                    ;
                    break;
                case FIRE_TYPE.RANDOM_SPRAY:
                case FIRE_TYPE.FIXED_SPRAY:
                    targetLocation = playerLocation;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(m_enemyData.FireType), m_enemyData.FireType, null);
            }*/

            Vector2 shootDirection = m_enemyData.FireAtTarget
                ? (targetLocation - (Vector2) transform.position).normalized
                : Vector2.down;

            
            FactoryManager.Instance.GetFactory<ProjectileFactory>()
                .CreateObjects<Projectile>(
                    m_enemyData.ProjectileType, 
                    transform.position,
                    targetLocation,
                    shootDirection, /*m_mostRecentMovementDirection * m_enemyData.MovementSpeed,*/
                    m_enemyData.AttackDamage,
                    1f,
                    "Player");

            /*List<Vector2> fireLocations = GetFireDirection();
            foreach (Vector2 fireLocation in fireLocations)
            {
                Projectile newProjectile = FactoryManager.Instance.GetFactory<ProjectileFactory>()
                    .CreateObject<Projectile>(
                        m_enemyData.ProjectileType, 
                        fireLocation,
                        m_enemyData.AttackDamage,
                        "Player");

                newProjectile.transform.parent = LevelManager.Instance.gameObject.transform;
                newProjectile.transform.position = transform.position;
                if (m_enemyData.AddVelocityToProjectiles)
                {
                    newProjectile.m_enemyVelocityModifier = m_mostRecentMovementDirection * m_enemyData.MovementSpeed;
                }

                LevelManager.Instance.ProjectileManager.AddProjectile(newProjectile);
            }*/
            
            AudioController.PlayEnemyFireSound(m_enemyData.EnemyType, 1f);
        }

        //Check what attack style this enemy uses, and use the appropriate method to get the firing location
        /*private List<Vector2> GetFireDirection()
        {
            //Firing styles are based on the player location. For now, hardcode this
            Vector3 playerLocation = LevelManager.Instance.BotObject != null
                ? LevelManager.Instance.BotObject.transform.position
                : Vector3.right * 50;

            List<Vector2> fireDirections = new List<Vector2>();

            switch (m_enemyData.AttackType)
            {
                case ENEMY_ATTACKTYPE.Forward:
                    fireDirections.Add(GetDestination() - transform.position);
                    break;
                case ENEMY_ATTACKTYPE.AtPlayer:
                    fireDirections.Add(playerLocation - transform.position);
                    break;
                case ENEMY_ATTACKTYPE.AtPlayerCone:
                    //Rotate player position around enemy position slightly by a random angle to shoot somewhere in a cone around the player
                    fireDirections.Add(GetDestinationForRotatePositionAroundPivot(playerLocation, transform.position,
                        Vector3.forward * Random.Range(-m_enemyData.SpreadAngle,
                            m_enemyData.SpreadAngle)) - transform.position);
                    break;
                case ENEMY_ATTACKTYPE.Down:
                    fireDirections.Add(Vector3.down);
                    break;
                case ENEMY_ATTACKTYPE.Spray:
                    //For each shot in the spray, rotate player position around enemy position slightly by a random angle to shoot somewhere in a cone around the player
                    for (int i = 0; i < m_enemyData.SprayCount; i++)
                    {
                        fireDirections.Add(GetDestinationForRotatePositionAroundPivot(playerLocation,
                            transform.position,
                            Vector3.forward * Random.Range(-m_enemyData.SpreadAngle,
                                m_enemyData.SpreadAngle)) - transform.position);
                    }

                    break;
                case ENEMY_ATTACKTYPE.Spiral:
                    //Consult spiral formula to get the angle to shoot the next shot at
                    fireDirections.Add(GetSpiralAttackDirection());
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(m_enemyData.AttackType), m_enemyData.AttackType, null);
            }

            return fireDirections;
        }

        //Get the location that enemy is firing at, then create the firing projectile from the factory
        private Vector3 GetSpiralAttackDirection()
        {
            m_spiralAttackDirection =
                GetDestinationForRotatePositionAroundPivot(m_spiralAttackDirection + transform.position,
                    transform.position, Vector3.forward * 30) - transform.position;

            return m_spiralAttackDirection;
        }*/
        
        #endregion Firing

        //============================================================================================================//

        #region Movement
        
        public Vector3 GetDestination()
        {
            Vector3 playerLocation = LevelManager.Instance.BotObject != null
                ? LevelManager.Instance.BotObject.transform.position
                 : Vector3.zero;

            switch (MovementType)
            {
                case ENEMY_MOVETYPE.Standard:
                    return playerLocation;
                case ENEMY_MOVETYPE.Oscillate:
                    //Find destination by rotating the playerLocation around the enemy position, at the angle output by the oscillate function
                    return GetDestinationForRotatePositionAroundPivot(playerLocation, transform.position,
                        GetAngleInOscillation());
                case ENEMY_MOVETYPE.OscillateHorizontal:
                    //Find destination by determining whether to move left or right and then oscillating at the angle output by the oscillate function
                    return GetDestinationForRotatePositionAroundPivot(transform.position + SetHorizontalDirection(playerLocation),
                        transform.position, GetAngleInOscillation());
                case ENEMY_MOVETYPE.Orbit:
                    //If outside the orbit radius, move towards the player location. If inside it, get the destination along the edge of the circle to move clockwise around it
                    float distanceSqr = Vector2.SqrMagnitude(transform.position - playerLocation);
                    if (distanceSqr > m_enemyData.OrbitRadiusSqr)
                    {
                        return playerLocation;
                    }
                    else
                    {
                        return GetDestinationForRotatePositionAroundPivotAtDistance(transform.position, playerLocation,
                            Vector3.forward * -5, m_enemyData.OrbitRadius);
                    }
                case ENEMY_MOVETYPE.Horizontal:
                    return transform.position + SetHorizontalDirection(playerLocation);
                case ENEMY_MOVETYPE.HorizontalDescend:
                    return transform.position + SetHorizontalDirection(playerLocation, true);
                case ENEMY_MOVETYPE.Down:
                    return transform.position + Vector3.down;

            }

            return playerLocation;
        }
        
        //Determine whether this horizontal mover is going left or right
        public Vector3 SetHorizontalDirection(Vector3 playerLocation, bool isDescending = false)
        {
            if (transform.position.x <= playerLocation.x + horizontalFarLeftX && m_currentHorizontalMovementDirection != Vector3.right)
            {
                m_currentHorizontalMovementDirection = Vector3.right;
                if (isDescending)
                {
                    m_horizontalMovementYLevel -= Constants.gridCellSize * m_enemyData.NumberCellsDescend;
                    if (m_horizontalMovementYLevel <= verticalLowestAllowed)
                    {
                        m_enemyMovetypeOverride = ENEMY_MOVETYPE.Down;
                        //m_horizontalMovementYLevel = m_horizontalMovementYLevelOrigin;
                    }
                }
            }
            else if (transform.position.x >= playerLocation.x + horizontalFarRightX && m_currentHorizontalMovementDirection != Vector3.left)
            {
                m_currentHorizontalMovementDirection = Vector3.left;
                if (isDescending)
                {
                    m_horizontalMovementYLevel -= Constants.gridCellSize * m_enemyData.NumberCellsDescend;
                    if (m_horizontalMovementYLevel <= verticalLowestAllowed)
                    {
                        m_enemyMovetypeOverride = ENEMY_MOVETYPE.Down;
                        //m_horizontalMovementYLevel = m_horizontalMovementYLevelOrigin;
                    }
                }
            }

            //Modify the vertical level back to the stored horizontalYlevel, so enemies will return to their previous y level after avoiding an obstacle
            //TODO - this logic should apply to oscillatehorizontal but currently causes a bug with it. Resolve bug and add this functionality back
            Vector3 addedVertical = Vector3.zero;
            if (m_enemyData.MovementType != ENEMY_MOVETYPE.OscillateHorizontal)
            {
                addedVertical += Vector3.up * (m_horizontalMovementYLevel - transform.position.y);
            }

            return m_currentHorizontalMovementDirection + addedVertical;
        }


        //Calculate the angle to move at for the oscillation movement
        //Methodology - uses a timer, with the value of the timer modification by the oscillationspersecond value, to see where we are in the zig zag cycle. 
        //if the modular is 1, 0 is at the far left end of the cycle, 0.5 is at the far right end, 1 goes back to left
        public Vector3 GetAngleInOscillation()
        {
            m_oscillationTimer += Time.deltaTime * m_enemyData.OscillationsPerSecond;

            if (m_oscillationTimer > 1)
            {
                m_oscillationTimer -= 1;
            }

            if (m_oscillationTimer <= 0.5f)
            {
                float angleAdjust = m_oscillationTimer * 2;
                return Vector3.forward * (m_enemyData.OscillationAngleRange * (-0.5f + angleAdjust));
            }
            else
            {
                float angleAdjust = (m_oscillationTimer - 0.5f) * 2;
                return Vector3.forward * (m_enemyData.OscillationAngleRange * (0.5f - angleAdjust));
            }
        }

        //Rotate point around pivot by angles amount
        public Vector3 GetDestinationForRotatePositionAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
        {
            Vector3 direction = point - pivot;
            direction = Quaternion.Euler(angles) * direction;
            return direction + pivot;
        }

        //Rotate point around pivot by angles amount, while ensuring that the point is a certain distance away from the pivot. Used for the orbit calculations to keep them orbiting on the outside
        public Vector3 GetDestinationForRotatePositionAroundPivotAtDistance(Vector3 point, Vector3 pivot,
            Vector3 angles, float distance)
        {
            Vector3 direction = point - pivot;
            direction.Normalize();
            direction *= distance;
            direction = Quaternion.Euler(angles) * direction;
            return direction + pivot;
        }

        public void ProcessMovement(Vector3 direction)
        {
            m_mostRecentMovementDirection = direction;
            transform.position += direction * (m_enemyData.MovementSpeed * Time.deltaTime);
        }

        public List<Vector3> GetPositions()
        {
            List<Vector3> positions = new List<Vector3>();
            foreach (var position in m_positions)
            {
                positions.Add(transform.position + position);
            }
            return positions;
        }

        #endregion

        //============================================================================================================//

        protected override void OnCollide(GameObject gameObject, Vector2 worldHitPoint)
        {

        }

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

            if (CurrentHealth > 0) 
                return;
            
            LevelManager.Instance.DropLoot(m_enemyData.rdsTable.rdsResult.ToList(), transform.localPosition, true);

            MissionProgressEventData missionProgressEventData = new MissionProgressEventData
            {
                enemyTypeString = m_enemyData.EnemyType,
                intAmount = 1
            };
            MissionManager.ProcessMissionData(typeof(EnemyKilledMission), missionProgressEventData);
            
            SessionDataProcessor.Instance.EnemyKilled(m_enemyData.EnemyType);
            AudioController.PlaySound(SOUND.ENEMY_DEATH);

            LevelManager.Instance.WaveEndSummaryData.AddEnemyKilled(name);
            
            

            LevelManager.Instance.EnemyManager.RemoveEnemy(this);

            Recycler.Recycle<Enemy>(this);
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

        public void EnteredCamera()
        {
            AudioController.PlayEnemyMoveSound(m_enemyData?.EnemyType);
        }

        public void ExitedCamera()
        {
            AudioController.StopEnemyMoveSound(m_enemyData.EnemyType);
        }
        //============================================================================================================//

        public virtual void CustomRecycle(params object[] args)
        {
            //m_horizontalMovementYLevel = 0.0f;
            //m_oscillationTimer = 0.0f;

            //horizontalFarLeftX = 0.0f;
            //horizontalFarRightX = 0.0f;

            //m_mostRecentMovementDirection = Vector3.zero;

            m_enemyMovetypeOverride = null;
            FreezeTime = 0f;
            Disabled = false;
            AudioController.StopEnemyMoveSound(m_enemyData.EnemyType);
            UnregisterCanBeSeen();
        }


        
        //============================================================================================================//

    }
}
 