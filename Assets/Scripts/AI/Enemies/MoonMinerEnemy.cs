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

        public override void ProcessMovement(Vector2 playerLocation)
        {
            if (m_pauseMovement)
            {
                m_pauseMovementTimer -= Time.deltaTime;
                if (m_pauseMovementTimer <= 0.0f)
                {
                    m_pauseMovement = false;
                    m_pauseMovementTimer = 1.0f;
                    FireAttack();
                }
                return;
            }
            
            base.ProcessMovement(playerLocation);
        }

        public override Vector2 GetMovementDirection(Vector2 playerLocation)
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
        }

        #endregion

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