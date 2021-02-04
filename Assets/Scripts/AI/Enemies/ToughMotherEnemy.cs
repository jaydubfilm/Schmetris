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
using UnityEngine;

namespace StarSalvager.AI
{
    public class ToughMotherEnemy : Enemy
    {
        public float anticipationTime = 1f;

        public override bool IsAttachable => false;
        public override bool IgnoreObstacleAvoidance => true;
        public override bool SpawnAboveScreen => false;

        private float horizontalFarLeftX;
        private float horizontalFarRightX;
        private float verticalLowestAllowed;

        private int _jumpCount;

        private Vector2 _targetLocation;
        private float _anticipationTime;

        private Vector2 currentDestination;

        public override void LateInit()
        {
            base.LateInit();

            SetState(STATE.MOVE);
        }

        //============================================================================================================//

        #region Movement

        public override void UpdateEnemy(Vector2 playerLocation)
        {
            StateUpdate();
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
                    _jumpCount = UnityEngine.Random.Range(3, 5);
                    _targetLocation = GetNewPosition();
                    break;
                case STATE.ANTICIPATION:
                    _anticipationTime = anticipationTime;
                    break;
                case STATE.ATTACK:
                    break;
                case STATE.DEATH:
                    TrySpawnDataLeech(Vector3.left);
                    TrySpawnDataLeech(Vector3.right);
                    TrySpawnDataLeech(Vector3.up);
                    
                    Recycler.Recycle<ToughMotherEnemy>(this);
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
                case STATE.DEATH:
                    return;
                case STATE.MOVE:
                    MoveState();
                    break;
                case STATE.ANTICIPATION:
                    AnticipationState();
                    break;
                case STATE.ATTACK:
                    AttackState();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(currentState), currentState, null);
            }
        }

        protected override void CleanStateData()
        {
            base.CleanStateData();

            _jumpCount = 0;
        }

        private void MoveState()
        {
            //TODO Move towards target position
            var currentPosition = transform.position;

            if (Vector2.Distance(currentPosition, _targetLocation) > 0.1f)
            {
                transform.position = Vector2.MoveTowards(currentPosition, _targetLocation, EnemyMovementSpeed * Time.deltaTime);
                m_mostRecentMovementDirection = (transform.position - currentPosition).normalized;
                return;
            }

            _jumpCount--;

            if (_jumpCount <= 0)
            {
                m_mostRecentMovementDirection = Vector3.zero;
                SetState(STATE.ANTICIPATION);
                return;
            }

            _targetLocation = GetNewPosition();
        }

        private void AnticipationState()
        {
            //TODO Wait x Seconds
            if (_anticipationTime > 0f)
            {
                _anticipationTime -= Time.deltaTime;
                return;
            }

            SetState(STATE.ATTACK);
            //TODO Switch to Attack State
        }

        private void AttackState()
        {
            TrySpawnDataLeech(Vector3.zero);

            SetState(STATE.MOVE);
        }

        private void TrySpawnDataLeech(Vector3 offsetPosition)
        {
            if (!CameraController.IsPointInCameraRect(transform.position, Constants.VISIBLE_GAME_AREA))
                return;

            string enemyId = FactoryManager.Instance.EnemyRemoteData.GetEnemyId("DataLeech");
            LevelManager.Instance.EnemyManager.SpawnEnemy(enemyId, transform.position + offsetPosition);
        }

        #endregion //States

        //============================================================================================================//

        private static Vector2 GetNewPosition()
        {
            //Used to ensure the CameraVisibleRect is updated
            CameraController.IsPointInCameraRect(Vector2.zero, Constants.VISIBLE_GAME_AREA);

            var cameraRect = CameraController.VisibleCameraRect;
            var xBounds = new Vector2(cameraRect.xMin, cameraRect.xMax);
            var yBounds = new Vector2(cameraRect.yMin, cameraRect.yMax);

            return new Vector2
            {
                x = Mathf.Lerp(xBounds.x, xBounds.y, UnityEngine.Random.Range(0.3f, 0.7f)),
                y = Mathf.Lerp(yBounds.x, yBounds.y, UnityEngine.Random.Range(0.4f, 0.85f))
            };
        }

        public override Type GetOverrideType()
        {
            return typeof(ToughMotherEnemy);
        }
    }
}