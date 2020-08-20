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

namespace StarSalvager.AI
{
    [RequireComponent(typeof(StateAnimator))]
    public class Enemy : CollidableBase, ICanBeHit, IHealth, IStateAnimation
    {
        public EnemyData m_enemyData;

        protected float m_fireTimer;
        private Vector3 m_spiralAttackDirection = Vector3.down;
        private List<Vector3> m_positions = new List<Vector3>();
        
        
        private Vector3 m_currentHorizontalMovementDirection = Vector3.right;
        private float m_horizontalMovementYLevel;
        private float m_oscillationTimer;

        private float horizontalFarLeftX;
        private float horizontalFarRightX;

        protected Vector3 m_mostRecentMovementDirection = Vector3.zero;

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
            horizontalFarLeftX = (-1 * Values.Constants.gridCellSize * Values.Globals.ColumnsOnScreen) / 3.5f;
            horizontalFarRightX = (Values.Constants.gridCellSize * Values.Globals.ColumnsOnScreen) / 3.5f;
        }

        protected virtual void Update()
        {
            //Count down fire timer. If ready to fire, call fireAttack()
            if (m_enemyData.AttackType == ENEMY_ATTACKTYPE.None)
                return;
            
            if(GameTimer.IsPaused || LevelManager.Instance.EndWaveState)
                return;
            

            m_fireTimer += Time.deltaTime;

            if (m_fireTimer < 1 / m_enemyData.AttackSpeed)
                return;

            m_fireTimer -= 1 / m_enemyData.AttackSpeed;
            FireAttack();
        }

        //============================================================================================================//

        public virtual void SetupSprite()
        {
            renderer.sprite = m_enemyData?.Sprite;
            StateAnimator.SetController(m_enemyData?.AnimationController);
        }

        private void SetupPositions()
        {
            for (float i = 0; i < m_enemyData.Dimensions.x; i++)
            {
                for (float k = 0; k < m_enemyData.Dimensions.y; k++)
                {
                    m_positions.Add(new Vector3(i - (((float)m_enemyData.Dimensions.x - 1) / 2), k - (((float)m_enemyData.Dimensions.y - 1) / 2), 0));
                }
            }
        }

        //============================================================================================================//
        
        #region Firing
        
        private void FireAttack()
        {
            /*var distance = Vector3.Distance(transform.position, LevelManager.Instance.BotGameObject.transform.position);
            //TODO Determine if this fires at all times or just when bot is in range
            if (distance >= 100 * Constants.gridCellSize)
                return;*/

            Vector3 screenPoint = Camera.main.WorldToViewportPoint(transform.position);
            bool onScreen = screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;
            if (!onScreen)
                return;

            List<Vector2> fireLocations = GetFireDirection();
            foreach (Vector2 fireLocation in fireLocations)
            {
                Projectile newProjectile = FactoryManager.Instance.GetFactory<ProjectileFactory>()
                    .CreateObject<Projectile>(m_enemyData.ProjectileType, fireLocation, "Player");
                newProjectile.DamageAmount = m_enemyData.AttackDamage;
                newProjectile.transform.parent = LevelManager.Instance.gameObject.transform;
                newProjectile.transform.position = transform.position;
                if (m_enemyData.AddVelocityToProjectiles)
                {
                    newProjectile.m_enemyVelocityModifier = m_mostRecentMovementDirection * m_enemyData.MovementSpeed;
                }

                LevelManager.Instance.ProjectileManager.AddProjectile(newProjectile);
            }
        }

        //Check what attack style this enemy uses, and use the appropriate method to get the firing location
        private List<Vector2> GetFireDirection()
        {
            //Firing styles are based on the player location. For now, hardcode this
            Vector3 playerLocation = LevelManager.Instance.BotGameObject != null
                ? LevelManager.Instance.BotGameObject.transform.position
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
                        Vector3.forward * UnityEngine.Random.Range(-m_enemyData.SpreadAngle,
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
                            Vector3.forward * UnityEngine.Random.Range(-m_enemyData.SpreadAngle,
                                m_enemyData.SpreadAngle)) - transform.position);
                    }

                    break;
                case ENEMY_ATTACKTYPE.Spiral:
                    //Consult spiral formula to get the angle to shoot the next shot at
                    fireDirections.Add(GetSpiralAttackDirection());
                    break;
            }

            return fireDirections;
        }

        //Get the location that enemy is firing at, then create the firing projectile from the factory
        public Vector3 GetSpiralAttackDirection()
        {
            m_spiralAttackDirection =
                GetDestinationForRotatePositionAroundPivot(m_spiralAttackDirection + transform.position,
                    transform.position, Vector3.forward * 30) - transform.position;

            return m_spiralAttackDirection;
        }
        
        #endregion Firing

        //============================================================================================================//

        #region Movement
        
        public Vector3 GetDestination()
        {
            //Movement styles are based on the player location. For now, hardcode this
            Vector3 playerLocation = LevelManager.Instance.BotGameObject != null
                ? LevelManager.Instance.BotGameObject.transform.position
                : Vector3.right * 50;

            switch (m_enemyData.MovementType)
            {
                case ENEMY_MOVETYPE.Standard:
                    return playerLocation;
                case ENEMY_MOVETYPE.Oscillate:
                    //Find destination by rotating the playerLocation around the enemy position, at the angle output by the oscillate function
                    return GetDestinationForRotatePositionAroundPivot(playerLocation, transform.position,
                        GetAngleInOscillation());
                case ENEMY_MOVETYPE.OscillateHorizontal:
                    //Find destination by determining whether to move left or right and then oscillating at the angle output by the oscillate function
                    return GetDestinationForRotatePositionAroundPivot(transform.position + SetHorizontalDirection(),
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
                    return transform.position + SetHorizontalDirection();
                case ENEMY_MOVETYPE.HorizontalDescend:
                    return transform.position + SetHorizontalDirection(true);
                case ENEMY_MOVETYPE.Down:
                    return transform.position + Vector3.down;

            }

            return playerLocation;
        }
        
        //Determine whether this horizontal mover is going left or right
        public Vector3 SetHorizontalDirection(bool isDescending = false)
        {
            if (transform.position.x <= horizontalFarLeftX && m_currentHorizontalMovementDirection != Vector3.right)
            {
                m_currentHorizontalMovementDirection = Vector3.right;
                if (isDescending)
                {
                    m_horizontalMovementYLevel -= Constants.gridCellSize * m_enemyData.NumberCellsDescend;
                }
            }
            else if (transform.position.x >= horizontalFarRightX &&
                     m_currentHorizontalMovementDirection != Vector3.left)
            {
                m_currentHorizontalMovementDirection = Vector3.left;
                if (isDescending)
                {
                    m_horizontalMovementYLevel -= Constants.gridCellSize * m_enemyData.NumberCellsDescend;
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
            return (direction + pivot);
        }

        //Rotate point around pivot by angles amount, while ensuring that the point is a certain distance away from the pivot. Used for the orbit calculations to keep them orbiting on the outside
        public Vector3 GetDestinationForRotatePositionAroundPivotAtDistance(Vector3 point, Vector3 pivot,
            Vector3 angles, float distance)
        {
            Vector3 direction = point - pivot;
            direction.Normalize();
            direction *= distance;
            direction = Quaternion.Euler(angles) * direction;
            return (direction + pivot);
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

        protected override void OnCollide(GameObject gameObject, Vector2 hitPoint)
        {

        }

        //ICanBeHit functions
        //============================================================================================================//

        public void TryHitAt(Vector2 position, float damage)
        {
            //FIXME This should use IHealth
            LevelManager.Instance.ObstacleManager.SpawnBitExplosion(transform.position, m_enemyData.rdsTable.rdsResult.ToList());
            MissionManager.ProcessEnemyKilledMissionData(m_enemyData.EnemyType, 1);
            Recycler.Recycle<Enemy>(this);
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
            
            if(CurrentHealth <= 0)
                Recycler.Recycle<Enemy>(this);
        }

        //============================================================================================================//

    }
}
 