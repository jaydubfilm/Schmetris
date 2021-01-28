﻿using StarSalvager.Cameras;
using StarSalvager.Values;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.AI
{
    public class ShardEnemy : Enemy
    {
        public override bool IsAttachable => false;
        public override bool IgnoreObstacleAvoidance => true;
        public override bool SpawnAboveScreen => true;

        private bool m_isAccelerating;
        private float m_accelerationAmount = 0.0f;

        //============================================================================================================//

        #region Movement

        public override void UpdateEnemy(Vector2 playerLocation)
        {
            StateUpdate();
            /*if (CameraController.IsPointInCameraRect(transform.position, 0.6f))
            {
                if (!m_isAccelerating)
                {
                    if (playerLocation.x - transform.position.x <= 1.0f)
                    {
                        m_isAccelerating = true;
                    }
                }
            }

            if (m_isAccelerating)
            {
                m_accelerationAmount = Mathf.Max(2.0f, m_accelerationAmount + 3 * Time.deltaTime);
            }

            Vector3 fallAmount = Vector3.up * ((Constants.gridCellSize * Time.deltaTime) / Globals.TimeForAsteroidToFallOneSquare) * (1.0f + m_accelerationAmount);
            transform.position -= fallAmount;*/
        }

        protected override Vector2 GetMovementDirection(Vector2 playerLocation)
        {
            return Vector2.down;
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

        protected override void ProcessFireLogic()
        {
            base.ProcessFireLogic();
        }

        protected override void FireAttack()
        {
            
        }

        #endregion

        public override void CustomRecycle(params object[] args)
        {
            m_isAccelerating = false;
            m_accelerationAmount = 0;
            
            base.CustomRecycle(args);
        }

        //============================================================================================================//
    }
}