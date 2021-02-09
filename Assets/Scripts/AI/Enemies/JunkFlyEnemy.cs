using Recycling;
using StarSalvager.Audio;
using StarSalvager.Cameras;
using StarSalvager.Factories;
using StarSalvager.Utilities.Analytics;
using StarSalvager.Utilities.Particles;
using StarSalvager.Values;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace StarSalvager.AI
{
    public class JunkFlyEnemy : Enemy, IOverrideRecycleType
    {
        public override bool IsAttachable => false;
        public override bool IgnoreObstacleAvoidance => true;
        public override bool SpawnAboveScreen => false;


        //Temp variables
        private Vector2 m_currentHorizontalMovementDirection = Vector2.right;
        private float m_horizontalMovementYLevel;

        private float horizontalFarLeftX;
        private float horizontalFarRightX;
        private float verticalLowestAllowed;
        //Endtemp variables

        private int m_numberCellsDescend = 2;
        private int m_numberTimesDescend = 4;

        private Vector2 _playerLocation;

        public override void LateInit()
        {
            base.LateInit();

            m_horizontalMovementYLevel = transform.position.y;
            verticalLowestAllowed = m_horizontalMovementYLevel - (Constants.gridCellSize * m_numberCellsDescend * m_numberTimesDescend);
            horizontalFarLeftX = -1 * Constants.gridCellSize * Globals.ColumnsOnScreen / 3.5f;
            horizontalFarRightX = Constants.gridCellSize * Globals.ColumnsOnScreen / 3.5f;

            SetState(STATE.MOVE);
        }

        #region Movement

        public override void UpdateEnemy(Vector2 playerLocation)
        {
            _playerLocation = playerLocation;
            StateUpdate();
        }

        protected override Vector2 GetMovementDirection(Vector2 playerLocation)
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

        //====================================================================================================================//

        #region States

        protected override void StateChanged(STATE newState)
        {
            switch (newState)
            {
                case STATE.NONE:
                case STATE.ATTACK:
                case STATE.MOVE:

                    break;
                case STATE.DEATH:
                    Recycler.Recycle<JunkFlyEnemy>(this);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override void StateUpdate()
        {
            switch (currentState)
            {
                case STATE.NONE:
                case STATE.DEATH:
                    return;
                case STATE.MOVE:
                    MoveState();
                    break;
                case STATE.ATTACK:
                    AttackState();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void MoveState()
        {
            Vector3 movementDirection = GetMovementDirection(_playerLocation).normalized;
            m_mostRecentMovementDirection = movementDirection;

            transform.position += (movementDirection * (m_enemyData.MovementSpeed * Time.deltaTime));

            m_fireTimer += Time.deltaTime;

            if (m_fireTimer < 1 / m_enemyData.RateOfFire)
                return;

            m_fireTimer -= 1 / m_enemyData.RateOfFire;

            SetState(STATE.ATTACK);
        }

        private void AttackState()
        {
            FireAttack();

            SetState(STATE.MOVE);
        }

        #endregion //States

        //============================================================================================================//

        #region Firing

        protected override void FireAttack()
        {
            if (!CameraController.IsPointInCameraRect(transform.position, Constants.VISIBLE_GAME_AREA))
                return;

            Vector2 playerLocation = LevelManager.Instance.BotInLevel != null
                ? LevelManager.Instance.BotInLevel.transform.position
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
                    null,
                    0f,
                    false,
                    true);
        }

        #endregion

        //============================================================================================================//
        public override Type GetOverrideType()
        {
            return typeof(JunkFlyEnemy);
        }
    }
}