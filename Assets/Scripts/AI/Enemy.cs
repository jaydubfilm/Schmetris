using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarSalvager.ScriptableObjects;
using StarSalvager.Factories;
using StarSalvager.Constants;

namespace StarSalvager.AI
{
    public class Enemy : CollidableBase
    {
        public EnemyData m_enemyData;

        protected float m_fireTimer = 0;
        private float m_oscillationTimer = 0;
        private Vector3 m_currentHorizontalMovementDirection = Vector3.right;
        private float m_horizontalMovementYLevel;
        private Vector3 m_spiralAttackDirection = Vector3.down;
        private float horizontalFarLeftX;
        private float horizontalFarRightX;
        protected Vector3 m_mostRecentMovementDirection = Vector3.zero;


        private void Start()
        {
            renderer.sprite = m_enemyData.Sprite;
            m_horizontalMovementYLevel = transform.position.y;
            horizontalFarLeftX = 0;
            horizontalFarRightX = LevelManager.Instance.GridSizeX * Values.gridCellSize;
        }

        private void Update()
        {
            //Count down fire timer. If ready to fire, call fireAttack()
            if (m_enemyData.AttackType != ENEMY_ATTACKTYPE.None)
            {
                m_fireTimer += Time.deltaTime;
                if (m_fireTimer >= 1 / m_enemyData.AttackSpeed)
                {
                    m_fireTimer -= 1 / m_enemyData.AttackSpeed;
                    FireAttack();
                }
            }
        }

        public void ProcessMovement(Vector3 direction)
        {
            m_mostRecentMovementDirection = direction;
            transform.position = transform.position + (direction * m_enemyData.MovementSpeed * Time.deltaTime);
        }

        //Get the location that enemy is firing at, then create the firing projectile from the factory
        private void FireAttack()
        {
            List<Vector2> fireLocations = GetFireDirection();
            foreach (Vector2 fireLocation in fireLocations)
            {
                Projectile newProjectile = FactoryManager.Instance.GetFactory<ProjectileFactory>().CreateObject<Projectile>(m_enemyData.ProjectileType, fireLocation, "Player");
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
            Vector3 playerLocation = LevelManager.Instance.BotGameObject != null ? LevelManager.Instance.BotGameObject.transform.position : Vector3.right * 50;

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
                    fireDirections.Add(GetDestinationForRotatePositionAroundPivot(playerLocation, transform.position, Vector3.forward * Random.Range(-m_enemyData.SpreadAngle, m_enemyData.SpreadAngle)) - transform.position);
                    break;
                case ENEMY_ATTACKTYPE.Down:
                    fireDirections.Add(Vector3.down);
                    break;
                case ENEMY_ATTACKTYPE.Spray:
                    //For each shot in the spray, rotate player position around enemy position slightly by a random angle to shoot somewhere in a cone around the player
                    for (int i = 0; i < m_enemyData.SprayCount; i++)
                    {
                        fireDirections.Add(GetDestinationForRotatePositionAroundPivot(playerLocation, transform.position, Vector3.forward * Random.Range(-m_enemyData.SpreadAngle, m_enemyData.SpreadAngle)) - transform.position);
                    }
                    break;
                case ENEMY_ATTACKTYPE.Spiral:
                    //Consult spiral formula to get the angle to shoot the next shot at
                    fireDirections.Add(GetSpiralAttackDirection());
                    break;
            }

            return fireDirections;
        }

        //Check what movement type is being used, and use the appropriate method to calculate what my current destination is
        public Vector3 GetDestination()
        {
            //Movement styles are based on the player location. For now, hardcode this
            Vector3 playerLocation = LevelManager.Instance.BotGameObject != null ? LevelManager.Instance.BotGameObject.transform.position : Vector3.right * 50;

            switch (m_enemyData.MovementType)
            {
                case ENEMY_MOVETYPE.Standard:
                    return playerLocation;
                case ENEMY_MOVETYPE.Oscillate:
                    //Find destination by rotating the playerLocation around the enemy position, at the angle output by the oscillate function
                    return GetDestinationForRotatePositionAroundPivot(playerLocation, transform.position, GetAngleInOscillation());
                case ENEMY_MOVETYPE.OscillateHorizontal:
                    //Find destination by determining whether to move left or right and then oscillating at the angle output by the oscillate function
                    return GetDestinationForRotatePositionAroundPivot(transform.position + SetHorizontalDirection(), transform.position, GetAngleInOscillation());
                case ENEMY_MOVETYPE.Orbit:
                    //If outside the orbit radius, move towards the player location. If inside it, get the destination along the edge of the circle to move clockwise around it
                    float distanceSqr = Vector2.SqrMagnitude(transform.position - playerLocation);
                    if (distanceSqr > m_enemyData.OrbitRadiusSqr)
                    {
                        return playerLocation;
                    }
                    else
                    {
                        return GetDestinationForRotatePositionAroundPivotAtDistance(transform.position, playerLocation, Vector3.forward * -5, m_enemyData.OrbitRadius);
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

        //Rotate spiral attack direction around the enemy position slightly, and then return the value
        public Vector3 GetSpiralAttackDirection()
        {
            m_spiralAttackDirection = GetDestinationForRotatePositionAroundPivot(m_spiralAttackDirection + transform.position, transform.position, Vector3.forward * 30) - transform.position;

            return m_spiralAttackDirection;
        }

        //Determine whether this horizontal mover is going left or right
        public Vector3 SetHorizontalDirection(bool isDescending = false)
        {
            if (transform.position.x <= horizontalFarLeftX && m_currentHorizontalMovementDirection != Vector3.right)
            {
                m_currentHorizontalMovementDirection = Vector3.right;
                if (isDescending)
                {
                    m_horizontalMovementYLevel -= Values.gridCellSize * m_enemyData.NumberCellsDescend;
                }
            }
            else if (transform.position.x >= horizontalFarRightX && m_currentHorizontalMovementDirection != Vector3.left)
            {
                m_currentHorizontalMovementDirection = Vector3.left;
                if (isDescending)
                {
                    m_horizontalMovementYLevel -= Values.gridCellSize * m_enemyData.NumberCellsDescend;
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
        public Vector3 GetDestinationForRotatePositionAroundPivotAtDistance(Vector3 point, Vector3 pivot, Vector3 angles, float distance)
        {
            Vector3 direction = point - pivot;
            direction.Normalize();
            direction *= distance;
            direction = Quaternion.Euler(angles) * direction;
            return (direction + pivot);
        }

        protected override void OnCollide(GameObject gameObject, Vector2 hitPoint)
        {

        }
    }
}
 