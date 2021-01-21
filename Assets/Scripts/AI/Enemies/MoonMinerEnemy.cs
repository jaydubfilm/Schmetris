using System;
using StarSalvager.Cameras;
using StarSalvager.Values;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.AI
{
    public class MoonMinerEnemy : Enemy
    {
        public override bool IsAttachable => false;
        public override bool IgnoreObstacleAvoidance => true;
        public override bool SpawnHorizontal => true;

        private float horizontalFarLeftX;
        private float horizontalFarRightX;
        private float verticalLowestAllowed;

        private Vector2 currentDestination;

        private float m_pauseMovementTimer = 0.0f;
        private bool m_pauseMovement;

        public override void LateInit()
        {
            base.LateInit();

            currentDestination = transform.position;

            verticalLowestAllowed = 0.5f;
            horizontalFarLeftX = -1 * Constants.gridCellSize * Globals.ColumnsOnScreen / 3.5f;
            horizontalFarRightX = Constants.gridCellSize * Globals.ColumnsOnScreen / 3.5f;
        }

        //============================================================================================================//

        #region Movement

        public override void UpdateEnemy(Vector2 playerLocation)
        {
            StateUpdate();
            /*if (m_pauseMovement)
            {
                m_pauseMovementTimer -= Time.deltaTime;
                if (m_pauseMovementTimer <= 0.0f)
                {
                    m_pauseMovement = false;
                    m_pauseMovementTimer = 1.0f;
                    FireAttack();
                }
                return;
            }*/

            //base.ProcessState(playerLocation);
        }

        /*protected override Vector2 GetMovementDirection(Vector2 playerLocation)
        {
            if (Vector2.Distance(transform.position, currentDestination) <= 0.1f)
            {
                currentDestination = new Vector2(playerLocation.x + Random.Range(horizontalFarLeftX, horizontalFarRightX),
                    Random.Range(LevelManager.Instance.WorldGrid.m_screenGridCellRange.y * verticalLowestAllowed, LevelManager.Instance.WorldGrid.m_screenGridCellRange.y));

                if (CameraController.IsPointInCameraRect(transform.position, 0.6f))
                {
                    m_pauseMovement = true;
                }
            }

            return currentDestination - (Vector2)transform.position;
        }*/

        #endregion

        //====================================================================================================================//

        #region States

        protected override void StateChanged(STATE newState)
        {
            switch (newState)
            {
                case STATE.NONE:
                    return;
                case STATE.MOVE:
                    break;
                case STATE.FLEE:
                    break;
                case STATE.ANTICIPATION:
                    break;
                case STATE.ATTACK:
                    break;
                case STATE.DEATH:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
            }
        }

        protected override void StateUpdate()
        {
            switch (currrentState)
            {
                case STATE.NONE:
                case STATE.DEATH:
                    return;
                case STATE.MOVE:
                    MoveState();
                    break;
                case STATE.FLEE:
                    FleeState();
                    break;
                case STATE.ANTICIPATION:
                    AnticipationState();
                    break;
                case STATE.ATTACK:
                    AttackState();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(currrentState), currrentState, null);
            }
        }

        private Vector2 _targetLocation;
        private float _anticipationTime;
        private float _attackTime;

        private void MoveState()
        {
            //TODO Move towards target position
            //TODO If within threshold, move to anticipation state
        }

        private void FleeState()
        {
            //TODO Move off screen
            
            //TODO When no longer visible, recycle this
        }

        private void AnticipationState()
        {
            //TODO Wait x Seconds
            //TODO Switch to Attack State
        }

        private void AttackState()
        {
            //TODO Spawn the laser beam
            //TODO Wait x Seconds
            //TODO Set to MoveState
        }

        #endregion //States

        //============================================================================================================//

        #region Firing

        protected override void ProcessFireLogic()
        {
            return;
        }

        protected override void FireAttack()
        {
            base.FireAttack();
        }

        #endregion

        //============================================================================================================//
    }
}