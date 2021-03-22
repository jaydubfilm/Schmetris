using System;
using System.Collections;
using System.Collections.Generic;
using Recycling;
using StarSalvager.Cameras;
using StarSalvager.Factories;
using StarSalvager.Values;
using UnityEngine;
using Random = UnityEngine.Random;

namespace StarSalvager.AI
{
    public class PulseCannonEnemy : Enemy
    {

        public override bool IgnoreObstacleAvoidance => true;
        public override bool SpawnAboveScreen => true;

        //====================================================================================================================//

        [SerializeField, Range(0f, 1f), Tooltip("1.0 is 100% accurate and 0.0 is anywhere in front")]
        private float dotThreshold = 0.1f;


        [SerializeField]
        private float anticipcationTime;
        private float _anticipationTimer;

        [SerializeField]
        private float burstShotDelay;
        private float _burstShotDelayTimer;
        
        [SerializeField]
        private int burstCount;
        private int _burstCount;

        //====================================================================================================================//
        

        private Vector2 _playerPosition;

        //[SerializeField]
        private float aimRotation = -45;

        private bool _flipped;
        private Vector2 _checkDirection;

        //====================================================================================================================//

        public override void LateInit()
        {
            //--------------------------------------------------------------------------------------------------------//
            
            float GetNewXPosition(in Vector2 range)
            {
                //Used to ensure the CameraVisibleRect is updated
                CameraController.IsPointInCameraRect(Vector2.zero, Constants.VISIBLE_GAME_AREA);
            
                var cameraRect = CameraController.VisibleCameraRect;
                var xBounds = new Vector2(cameraRect.xMin, cameraRect.xMax);
                //var yBounds = new Vector2(cameraRect.yMin, cameraRect.yMax);

                return Mathf.Lerp(xBounds.x, xBounds.y, Random.Range(range.x, range.y));
            }

            //--------------------------------------------------------------------------------------------------------//
            
            base.LateInit();

            var currentPosition = Position;
            var leftSide = Random.value > 0.5;
            var xPos = GetNewXPosition(leftSide ? new Vector2(0f, 0.15f) : new Vector2(0.85f, 1f));
            currentPosition.x = xPos;
            transform.position = currentPosition;
            
            
            //TODO Need to choose a facing direction (Default is left Facing)
            var botPosition = (Vector2)LevelManager.Instance.BotInLevel.transform.position;
            var dir = botPosition - (Vector2) currentPosition;
            _flipped = dir.x > 0f;

            _checkDirection = (Quaternion.Euler(0, 0, -45 * (_flipped ? -1f : 1f)) * Vector3.down).normalized;
            renderer.flipX = _flipped;


            SetState(STATE.IDLE);
        }


        //====================================================================================================================//
        
        protected override void StateChanged(STATE newState)
        {
            switch (newState)
            {
                case STATE.NONE:
                    return;
                case STATE.IDLE:
                    break;
                case STATE.ANTICIPATION:
                    _anticipationTimer = anticipcationTime;
                    break;
                case STATE.ATTACK:
                    _burstCount = burstCount;
                    _burstShotDelayTimer = 0f;
                    break;
                case STATE.DEATH:
                    Recycler.Recycle<LaserTurretEnemy>(this);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
            }
        }

        protected override void StateUpdate()
        {
            ApplyFallMotion();

            
            switch (currentState)
            {
                case STATE.NONE:
                    return;
                case STATE.IDLE:
                    IdleState();
                    break;
                case STATE.ANTICIPATION:
                    AnticipationState();
                    break;
                case STATE.ATTACK:
                    AttackState();
                    break;
                case STATE.DEATH:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        //====================================================================================================================//

        private void IdleState()
        {
            var currentPosition = transform.position;
            var dir = (_playerPosition - (Vector2) currentPosition).normalized;

#if UNITY_EDITOR
            void DebugLines()
            {
                Debug.DrawRay(currentPosition, dir * 100f, Color.green);
                
                Debug.DrawRay(currentPosition, _checkDirection * 100f, Color.cyan);

                var angle = Mathf.Acos(dotThreshold) * Mathf.Rad2Deg;
                var up = Quaternion.Euler(0, 0, angle ) * _checkDirection;
                var down = Quaternion.Euler(0, 0, -angle) * _checkDirection;

                Debug.DrawRay(currentPosition, up * 100f, Color.blue);
                Debug.DrawRay(currentPosition, down * 100f, Color.blue);
            }

            DebugLines();
#endif


            var dot = Vector2.Dot(dir, _checkDirection);


            if (dot < dotThreshold)
                return;
            
            SetState(STATE.ANTICIPATION);
        }

        private void AnticipationState()
        {
            _anticipationTimer -= Time.deltaTime;
            
            //TODO Charge up the cannon
            if (_anticipationTimer > 0f)
                return;
            
            SetState(STATE.ATTACK);
            
            
        }

        private void AttackState()
        {
            if (_burstShotDelayTimer > 0f)
            {
                _burstShotDelayTimer -= Time.deltaTime;
                return;
            }

            _burstCount--;
            
            //TODO Spawn a projectile here
            FireAttack();

            if (_burstCount <= 0)
            {
                SetState(STATE.IDLE);
                return;
            }

            _burstShotDelayTimer = burstShotDelay;
        }

        //====================================================================================================================//

        protected override void FireAttack()
        {
            var currentPosition = transform.position;

            if (!CameraController.IsPointInCameraRect(currentPosition, Constants.VISIBLE_GAME_AREA))
                return;

            var angle = Mathf.Acos(dotThreshold) * Mathf.Rad2Deg;

            var shootDirection =
                Quaternion.Euler(0, 0, Random.Range(-angle, angle)) * _checkDirection;

            FactoryManager.Instance.GetFactory<ProjectileFactory>()
                .CreateObjects<Projectile>(
                    m_enemyData.ProjectileType,
                    currentPosition,
                    _playerPosition,
                    Vector3.up * ((Constants.gridCellSize * Time.deltaTime) / Globals.TimeForAsteroidToFallOneSquare),
                    shootDirection,
                    1f,
                    "Player",
                    null,
                    0f,
                    false,
                    true);
        }

        //====================================================================================================================//
        
        
        public override void UpdateEnemy(Vector2 playerLocation)
        {
            _playerPosition = playerLocation;
            StateUpdate();
        }

        //====================================================================================================================//
        
        public override Type GetOverrideType()
        {
            return typeof(PulseCannonEnemy);
        }

#if UNITY_EDITOR

        private void OnDrawGizmosSelected()
        {
            var currentPosition = gameObject.transform.position;
            var direction = (Quaternion.Euler(0, 0, -45 * (_flipped ? -1f : 1f)) * Vector3.down).normalized;
           // var dir = (_playerPosition - (Vector2) currentPosition).normalized;

           Gizmos.color = Color.cyan;
           Gizmos.DrawRay(currentPosition, direction * 100f);

           var angle = Mathf.Acos(dotThreshold) * Mathf.Rad2Deg;
           var up = Quaternion.Euler(0, 0, angle ) * direction;
           var down = Quaternion.Euler(0, 0, -angle) * direction;

           Gizmos.color = Color.blue;
           Gizmos.DrawRay(currentPosition, up * 100f);
           Gizmos.DrawRay(currentPosition, down * 100f);
        }
        
#endif

    }
    
}
