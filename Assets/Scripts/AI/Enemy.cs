using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarSalvager.ScriptableObjects;
using StarSalvager.Factories;

namespace StarSalvager
{
    public class Enemy : MonoBehaviour
    {
        public new Transform transform;
        public Vector2 m_destination = Vector2.zero;

        private SpriteRenderer m_spriteRenderer;

        public EnemyData m_enemyData;

        private float m_fireTimer = 0;
        private float m_zigZagTimer = 0;

        private void Awake()
        {
            transform = gameObject.transform;
            m_spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            m_spriteRenderer.sprite = m_enemyData.Sprite;
        }

        private void Update()
        {
            //Count down fire timer. If ready to fire, call fireAttack()
            if (m_enemyData.AttackType != ENEMY_ATTACKTYPE.None)
            {
                m_fireTimer += Time.deltaTime;
                if (m_fireTimer >= m_enemyData.AttackSpeed)
                {
                    m_fireTimer -= m_enemyData.AttackSpeed;
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
            Vector3 fireLocation = GetFireLocation();
            Projectile newProjectile = FactoryManager.Instance.GetFactory<ProjectileFactory>().CreateObject<Projectile>(PROJECTILE_TYPE.Projectile1, fireLocation);
            newProjectile.transform.position = transform.position;
        }

        //Check what attack style this enemy uses, and use the appropriate method to get the firing location
        private Vector2 GetFireLocation()
        {
            //Firing styles are based on the player location. For now, hardcode this
            Vector3 playerLocation = new Vector3(50, 50, 0);

            switch (m_enemyData.AttackType)
            {
                case ENEMY_ATTACKTYPE.Forward:
                    return GetDestination() - transform.position;
                case ENEMY_ATTACKTYPE.AtPlayer:
                    return playerLocation - transform.position;
                case ENEMY_ATTACKTYPE.Spray:
                    return GetDestinationForRotatePositionAroundPivot(playerLocation, transform.position, new Vector3(0, 0, Random.Range(-30, 30))) - transform.position;
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
                case ENEMY_MOVETYPE.Zigzag:
                    //Find destination by rotating the playerLocation around the enemy position, at the angle output by the zig zag function
                    return GetDestinationForRotatePositionAroundPivot(playerLocation, transform.position, GetAngleInZigZag()); ;
                case ENEMY_MOVETYPE.Orbit:
                    //If outside the orbit radius, move towards the player location. If inside it, get the destination along the edge of the circle to move clockwise around it
                    float distanceSqr = Vector2.SqrMagnitude(transform.position - playerLocation);
                    if (distanceSqr > m_enemyData.OrbitRadiusSqr)
                    {
                        return playerLocation;
                    }
                    else
                    {
                        return GetDestinationForRotatePositionAroundPivotAtDistance(transform.position, playerLocation, new Vector3(0, 0, -5), m_enemyData.OrbitRadius);
                    }
            }

            return playerLocation;
        }

        //TODO: This function is badly made and needs a rewrite when my brain isn't burned out of math
        //Calculate the angle to move at, varying from 60 degrees clockwise and counterclockwise, for the zig zag movement
        //Methodology - uses a timer and checks a modular value on the timer to see where we are in the zig zag cycle. if the modular is 1, 0 is at the far left end of the cycle, 0.5 is at the far right end, 1 goes back to left
        //Future change thought process - method will work a lot cleaner if time 0 in cycle begins at 0, 0.25 is far left, 0.75 is far right, and 1 is back to center
        public Vector3 GetAngleInZigZag()
        {
            m_zigZagTimer += Time.deltaTime;
            float modular = 1 / m_enemyData.ZigZagsPerSecond;
            float zigZagAngleRange = 120.0f;
            float timer = m_zigZagTimer % modular;
            if (timer <= modular / 2)
            {
                return new Vector3(0, 0, zigZagAngleRange * (-0.5f + (timer * 2 / modular)));
            }
            else
            {
                return new Vector3(0, 0, zigZagAngleRange * (0.5f - ((timer - modular / 2) * 2 / modular)));
            }
        }

        //Rotate point around pivot by angles amount
        public Vector3 GetDestinationForRotatePositionAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
        {
            print(angles);
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
 