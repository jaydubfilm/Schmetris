using StarSalvager.Cameras;
using StarSalvager.Factories;
using StarSalvager.Values;
using System;
using System.Collections;
using System.Collections.Generic;
using Recycling;
using UnityEngine;

namespace StarSalvager.AI
{
    public class SquartEnemy : Enemy
    {
        public override bool IgnoreObstacleAvoidance => true;
        public override bool SpawnAboveScreen => false;

        //Temp variables
        private Vector2 m_currentHorizontalMovementDirection = Vector2.right;
        private float m_horizontalMovementYLevel;

        private float m_sinusoidalValue = 0.0f;

        private float m_sinusoidalSpeed = 5.0f;
        private float m_sinusoidalModifier = Constants.gridCellSize * 2;
        private int m_numDirectionSwaps = 0;
        private int m_numTotalDirectionSwaps = 6;

        private float horizontalFarLeftX;
        private float horizontalFarRightX;
        //Endtemp variables

        //============================================================================================================//

        public override void LateInit()
        {
            base.LateInit();

            m_horizontalMovementYLevel = transform.position.y;
            horizontalFarLeftX = -1 * Constants.gridCellSize * Globals.ColumnsOnScreen / 3.5f;
            horizontalFarRightX = Constants.gridCellSize * Globals.ColumnsOnScreen / 3.5f;
        }

        //============================================================================================================//

        #region Movement

        private float GetHorizontalMovementYLevel()
        {
            return m_horizontalMovementYLevel + m_sinusoidalModifier * Mathf.Sin(m_sinusoidalValue);
        }


        public override void UpdateEnemy(Vector2 playerLocation)
        {
            StateUpdate();
        }

        protected override Vector2 GetMovementDirection(Vector2 playerLocation)
        {
            if (m_numDirectionSwaps >= m_numTotalDirectionSwaps)
            {
                return Vector2.down;
            }

            m_sinusoidalValue += Time.deltaTime * m_sinusoidalSpeed;

            if (transform.position.x <= playerLocation.x + horizontalFarLeftX && m_currentHorizontalMovementDirection != Vector2.right)
            {
                m_currentHorizontalMovementDirection = Vector2.right;
                m_numDirectionSwaps++;
            }
            else if (transform.position.x >= playerLocation.x + horizontalFarRightX && m_currentHorizontalMovementDirection != Vector2.left)
            {
                m_currentHorizontalMovementDirection = Vector2.left;
                m_numDirectionSwaps++;
            }

            Vector2 addedVertical = Vector2.up * (GetHorizontalMovementYLevel() - transform.position.y);

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
                    return;
                case STATE.MOVE:
                    break;
                case STATE.FLEE:
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
            switch (currentState)
            {
                case STATE.NONE:
                    break;
                case STATE.MOVE:
                    MoveState();
                    break;
                case STATE.FLEE:
                    FleeState();
                    break;
                case STATE.ATTACK:
                    AttackState();
                    break;
                case STATE.DEATH:
                    Recycler.Recycle<SquartEnemy>(this);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(currentState), currentState, null);
            }
        }

        private void MoveState()
        {
            
        }

        private void FleeState()
        {
            
        }

        private void AttackState()
        {
            
        }

        #endregion //States

        //============================================================================================================//

        #region Firing

        #endregion

        //============================================================================================================//

        public override Type GetOverrideType()
        {
            return typeof(SquartEnemy);
        }
    }
}