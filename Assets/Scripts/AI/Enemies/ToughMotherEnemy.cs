using StarSalvager.Cameras;
using StarSalvager.Factories;
using StarSalvager.Values;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.AI
{
    public class ToughMotherEnemy : Enemy
    {
        public override bool IsAttachable => false;
        public override bool IgnoreObstacleAvoidance => true;
        public override bool SpawnAboveScreen => false;

        private float horizontalFarLeftX;
        private float horizontalFarRightX;
        private float verticalLowestAllowed;

        private Vector2 currentDestination;

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
        }

        protected override Vector2 GetMovementDirection(Vector2 playerLocation)
        {
            if (Vector2.Distance(transform.position, currentDestination) <= 0.1f)
            {
                currentDestination = new Vector2(playerLocation.x + UnityEngine.Random.Range(horizontalFarLeftX, horizontalFarRightX),
                    UnityEngine.Random.Range(LevelManager.Instance.WorldGrid.m_screenGridCellRange.y * verticalLowestAllowed, LevelManager.Instance.WorldGrid.m_screenGridCellRange.y));
            }

            return currentDestination - (Vector2)transform.position;
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
            if (!CameraController.IsPointInCameraRect(transform.position, Constants.VISIBLE_GAME_AREA))
                return;

            string enemyId = FactoryManager.Instance.EnemyRemoteData.GetEnemyId("DataLeech");
            LevelManager.Instance.EnemyManager.SpawnEnemy(enemyId, transform.position);
        }

        #endregion

        //============================================================================================================//

        public override Type GetOverrideType()
        {
            return typeof(ToughMotherEnemy);
        }
    }
}