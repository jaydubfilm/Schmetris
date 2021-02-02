using StarSalvager.Cameras;
using StarSalvager.Factories;
using StarSalvager.Values;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.AI
{
    public class SquartEnemy : Enemy
    {
        public override bool IsAttachable => false;
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
            throw new System.NotImplementedException();
        }

        protected override void StateUpdate()
        {
            throw new System.NotImplementedException();
        }

        #endregion //States

        //============================================================================================================//

        #region Firing

        #endregion

        //============================================================================================================//
    }
}