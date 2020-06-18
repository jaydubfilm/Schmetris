using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarSalvager.ScriptableObjects;
using StarSalvager.Factories;

namespace StarSalvager
{
    public class Enemy : MonoBehaviour, IEnemy
    {
        public Vector2 m_destination = Vector2.zero;

        public EnemyData m_enemyData;

        private float m_fireTimer = 0;
        private float m_oscillationTimer = 0;
        private Vector3 m_currentHorizontalMovementDirection = Vector3.right;
        private Vector3 m_spiralAttackDirection = Vector3.down;

        protected new Transform transform
        {
            get
            {
                if (m_transform == null)
                    m_transform = gameObject.GetComponent<Transform>();

                return m_transform;
            }
        }
        private Transform m_transform;

        protected new SpriteRenderer renderer
        {
            get
            {
                if (m_spriteRenderer == null)
                    m_spriteRenderer = gameObject.GetComponent<SpriteRenderer>();

                return m_spriteRenderer;
            }
        }
        private SpriteRenderer m_spriteRenderer;

        private void Start()
        {
            renderer.sprite = m_enemyData.Sprite;
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
            transform.position = transform.position + (direction * m_enemyData.MovementSpeed * Time.deltaTime);
        }

        //Get the location that enemy is firing at, then create the firing projectile from the factory
        private void FireAttack()
        {
            Vector3 fireLocation = GetFireDirection();
            Projectile newProjectile = FactoryManager.Instance.GetFactory<ProjectileFactory>().CreateObject<Projectile>(PROJECTILE_TYPE.Projectile1, fireLocation, "Player");
            newProjectile.transform.position = transform.position;
        }

        //Check what attack style this enemy uses, and use the appropriate method to get the firing location
        private Vector2 GetFireDirection()
        {
            //Firing styles are based on the player location. For now, hardcode this
            Vector3 playerLocation = new Vector3(50, 50, 0);

            switch (m_enemyData.AttackType)
            {
                case ENEMY_ATTACKTYPE.Forward:
                    return GetDestination() - transform.position;
                case ENEMY_ATTACKTYPE.AtPlayer:
                    return playerLocation - transform.position;
                case ENEMY_ATTACKTYPE.AtPlayerCone:
                    return GetDestinationForRotatePositionAroundPivot(playerLocation, transform.position, Vector3.forward *  Random.Range(-m_enemyData.AtPlayerConeAngle, m_enemyData.AtPlayerConeAngle)) - transform.position;
                case ENEMY_ATTACKTYPE.Down:
                    return Vector3.down;
                case ENEMY_ATTACKTYPE.Spray:
                    return playerLocation - transform.position;
                case ENEMY_ATTACKTYPE.Spiral:
                    return GetSpiralAttackDirection();
            }

            return playerLocation;
        }

        //Check what movement type is being used, and use the appropriate method to calculate what my current destination is
        public Vector3 GetDestination()
        {
            //Movement styles are based on the player location. For now, hardcode this
            Vector3 playerLocation = new Vector3(50, 50, 0);

            switch(m_enemyData.MovementType)
            {
                case ENEMY_MOVETYPE.Standard:
                    return playerLocation;
                case ENEMY_MOVETYPE.Oscillate:
                    //Find destination by rotating the playerLocation around the enemy position, at the angle output by the oscillate function
                    return GetDestinationForRotatePositionAroundPivot(playerLocation, transform.position, GetAngleInOscillation());
                case ENEMY_MOVETYPE.OscillateHorizontal:
                    //Find destination by determining whether to move left or right and then oscillating at the angle output by the oscillate function
                    return GetDestinationForRotatePositionAroundPivot(transform.position + Vector3.right, transform.position, GetAngleInOscillation());
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
                    return playerLocation;
                case ENEMY_MOVETYPE.Down:
                    return transform.position + Vector3.down;

            }

            return playerLocation;
        }

        public Vector3 GetSpiralAttackDirection()
        {
            m_spiralAttackDirection = GetDestinationForRotatePositionAroundPivot(m_spiralAttackDirection + transform.position, transform.position, Vector3.forward * 30) - transform.position;

            return m_spiralAttackDirection;
        }

        public Vector3 SetHorizontalDirection()
        {
            //Have far left and right borders on the x that they'll alternate between. Hardcode those borders for now.
            float farLeftX = 0;
            float farRightX = 100;

            if (transform.position.x <= farLeftX)
            {
                m_currentHorizontalMovementDirection = Vector3.right;
            }
            else if (transform.position.x >= farRightX)
            {
                m_currentHorizontalMovementDirection = Vector3.left;
            }
            
            return m_currentHorizontalMovementDirection;
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
    }
}
 