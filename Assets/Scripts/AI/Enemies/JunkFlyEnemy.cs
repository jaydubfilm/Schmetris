using StarSalvager.Cameras;
using StarSalvager.Factories;
using StarSalvager.Values;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.AI
{
    public class JunkFlyEnemy : Enemy
    {
        public override bool IsAttachable => false;
        public override bool IgnoreObstacleAvoidance => true;
        public override bool SpawnHorizontal => true;


        //Temp variables
        private Vector2 m_currentHorizontalMovementDirection = Vector2.right;
        private float m_horizontalMovementYLevel;
        private float m_horizontalMovementYLevelOrigin;

        private float horizontalFarLeftX;
        private float horizontalFarRightX;
        private float verticalLowestAllowed;
        //Endtemp variables

        private int m_numberCellsDescend = 2;
        private int m_numberTimesDescend = 4;

        public override void LateInit()
        {
            base.LateInit();

            m_horizontalMovementYLevel = transform.position.y;
            m_horizontalMovementYLevelOrigin = m_horizontalMovementYLevel;
            verticalLowestAllowed = m_horizontalMovementYLevel - (Constants.gridCellSize * m_numberCellsDescend * m_numberTimesDescend);
            horizontalFarLeftX = -1 * Constants.gridCellSize * Globals.ColumnsOnScreen / 3.5f;
            horizontalFarRightX = Constants.gridCellSize * Globals.ColumnsOnScreen / 3.5f;
        }

        //============================================================================================================//

        #region Movement

        public override void ProcessMovement(Vector2 playerlocation)
        {
            Vector3 movementDirection = GetMovementNormalized(playerlocation);
            movementDirection.Normalize();

            gameObject.transform.position = gameObject.transform.position + (movementDirection * m_enemyData.MovementSpeed * Time.deltaTime);
        }

        public override Vector2 GetMovementDirection(Vector2 playerLocation)
        {
            if (m_horizontalMovementYLevel <= verticalLowestAllowed)
            {
                return Vector2.down;
            }
            
            if (transform.position.x <= playerLocation.x + horizontalFarLeftX && m_currentHorizontalMovementDirection != Vector2.right)
            {
                m_currentHorizontalMovementDirection = Vector2.right;
                m_horizontalMovementYLevel -= Constants.gridCellSize * m_numberCellsDescend;
            }
            else if (transform.position.x >= playerLocation.x + horizontalFarRightX && m_currentHorizontalMovementDirection != Vector2.left)
            {
                m_currentHorizontalMovementDirection = Vector2.left;
                m_horizontalMovementYLevel -= Constants.gridCellSize * m_numberCellsDescend;
            }

            Vector2 addedVertical = Vector2.up * (m_horizontalMovementYLevel - transform.position.y);

            return m_currentHorizontalMovementDirection + addedVertical;
        }

        #endregion

        //============================================================================================================//

        #region Firing

        protected override void FireAttack()
        {
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
                    m_enemyData.AttackDamage,
                    1f,
                    "Player",
                    null);

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
            }
            
            AudioController.PlayEnemyFireSound(m_enemyData.EnemyType, 1f);*/
        }

        #endregion

        //============================================================================================================//
    }
}