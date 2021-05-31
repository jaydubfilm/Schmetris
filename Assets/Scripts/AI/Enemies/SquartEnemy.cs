using StarSalvager.Cameras;
using StarSalvager.Factories;
using StarSalvager.Values;
using System;
using System.Collections;
using System.Collections.Generic;
using Recycling;
using StarSalvager.Utilities.Helpers;
using UnityEngine;
using Random = UnityEngine.Random;

namespace StarSalvager.AI
{
    public class SquartEnemy : Enemy
    {
        public override bool IgnoreObstacleAvoidance => true;
        public override bool SpawnAboveScreen => false;

        private Vector2 _targetLocation;

        //Temp variables
        /*private Vector2 m_currentHorizontalMovementDirection = Vector2.right;
        private float m_horizontalMovementYLevel;

        private float m_sinusoidalValue = 0.0f;

        private float m_sinusoidalSpeed = 5.0f;
        private float m_sinusoidalModifier = Constants.gridCellSize * 2;
        private int m_numDirectionSwaps = 0;
        private int m_numTotalDirectionSwaps = 6;

        private float horizontalFarLeftX;
        private float horizontalFarRightX;*/

        [SerializeField]
        private int flightPasses = 3;

        private int _flightPasses;

        [SerializeField]
        private float attackTime;
        private float _attackTimer;

        [SerializeField]
        private float sinFrequency = 5f;
        [SerializeField]
        private float sinMagnitude = 5f;

        
        private bool _flipped;

        [SerializeField]
        private AnimationCurve tCurve;

        private float _reachTargetTime;
        private float _t;
        private Vector2 _startPosition;
        
        //Endtemp variables

        //============================================================================================================//

        public override void LateInit()
        {
            base.LateInit();

            /*m_horizontalMovementYLevel = transform.position.y;
            horizontalFarLeftX = -1 * Constants.gridCellSize * Globals.ColumnsOnScreen / 3.5f;
            horizontalFarRightX = Constants.gridCellSize * Globals.ColumnsOnScreen / 3.5f;*/

            _attackTimer = attackTime;
            _flightPasses = flightPasses;

            SetState(STATE.ANTICIPATION);
        }

        //============================================================================================================//

        #region Movement

        /*private float GetHorizontalMovementYLevel()
        {
            return m_horizontalMovementYLevel + m_sinusoidalModifier * Mathf.Sin(m_sinusoidalValue);
        }*/


        public override void UpdateEnemy(Vector2 playerLocation)
        {
            StateUpdate();
        }

        protected override Vector2 GetMovementDirection(Vector2 playerLocation)
        {
            return Vector2.down;
            /*if (m_numDirectionSwaps >= m_numTotalDirectionSwaps)
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

            return m_currentHorizontalMovementDirection + addedVertical;*/
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
                case STATE.ANTICIPATION:
                    var currentPosition = transform.position;

                    //FIXME Need to get the closest position
                    _targetLocation = GetClosestPosition(currentPosition, out _flipped);
                    
                    currentPosition.y = _targetLocation.y;
                    transform.position = currentPosition;
                    break;
                case STATE.MOVE:
                    _t = 0f;
                    _flipped = !_flipped;

                    _startPosition = _targetLocation;

                    MostRecentMovementDirection = Vector3.zero;
                    break;
                case STATE.FLEE:
                    break;
                case STATE.DEATH:
                    Recycler.Recycle<SquartEnemy>(this);
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
                case STATE.ANTICIPATION:
                    AnticipationState();
                    break;
                case STATE.MOVE:
                    MoveState();
                    break;
                case STATE.FLEE:
                    FleeState();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(currentState), currentState, null);
            }
        }

        private void AnticipationState()
        {
            //TODO Get into position
            
            //TODO Move towards target position
            var currentPosition = transform.position;

            
            if (Vector2.Distance(currentPosition, _targetLocation) > 0.1f)
            {
                transform.position = Vector2.MoveTowards(currentPosition, _targetLocation, EnemyMovementSpeed * Time.deltaTime);
                MostRecentMovementDirection = (transform.position - currentPosition).normalized;
                return;
            }

            SetState(STATE.MOVE);
        }
        
        private void MoveState()
        {
            //TODO SIN move between screen x locations
            void AttackUpdate()
            {
                if (_attackTimer > 0f)
                {
                    _attackTimer -= Time.deltaTime;
                    return;
                }

                FireAttack();
                _attackTimer = attackTime;
            }

            AttackUpdate();
            
            _targetLocation = GetNewPosition(_flipped);
            _reachTargetTime = Vector2.Distance(_startPosition, _targetLocation) / EnemyMovementSpeed;


            if (_t / _reachTargetTime <= 1.0f)
            {

                var newPosition = Vector2.Lerp(_startPosition, _targetLocation, tCurve.Evaluate(_t / _reachTargetTime));

                _t += Time.deltaTime;

                newPosition.y += Mathf.Sin(Time.time * sinFrequency) * sinMagnitude;

                transform.position = newPosition;
                
                return;
            }

            _flightPasses--;
            SetState(_flightPasses == 0 ? STATE.FLEE : STATE.MOVE);
        }

        private void FleeState()
        {
            //TODO Move off screen
            var currentPosition = transform.position;

            currentPosition += Vector3.up * (EnemyMovementSpeed * Time.deltaTime);

            transform.position = currentPosition;

            if (CameraController.IsPointInCameraRect(currentPosition))
                return;
            
            //TODO When no longer visible, recycle this
            DestroyEnemy();
        }

        #endregion //States

        //GetPositions
        //====================================================================================================================//
        
        private static Vector2 GetNewPosition(in bool flipped)
        {
            //Used to ensure the CameraVisibleRect is updated
            CameraController.IsPointInCameraRect(Vector2.zero, Constants.VISIBLE_GAME_AREA);
            
            var cameraRect = CameraController.VisibleCameraRect;
            var xBounds = new Vector2(cameraRect.xMin, cameraRect.xMax);
            var yBounds = new Vector2(cameraRect.yMin, cameraRect.yMax);
            
            var target = new Vector2
            {
                x = Mathf.Lerp(xBounds.x, xBounds.y, flipped ? 0.15f : 0.85f),
                y = Mathf.Lerp(yBounds.x, yBounds.y, 0.85f)
            };

            return target;
        }
        
        private static Vector2 GetClosestPosition(in Vector2 currentPosition, out bool flipped)
        {
            //Used to ensure the CameraVisibleRect is updated
            CameraController.IsPointInCameraRect(Vector2.zero, Constants.VISIBLE_GAME_AREA);
            
            var cameraRect = CameraController.VisibleCameraRect;
            var xBounds = new Vector2(cameraRect.xMin, cameraRect.xMax);
            var yBounds = new Vector2(cameraRect.yMin, cameraRect.yMax);

            var positions = new[]
            {
                new Vector2
                {
                    x = Mathf.Lerp(xBounds.x, xBounds.y, 0.85f),
                    y = Mathf.Lerp(yBounds.x, yBounds.y, 0.85f)
                },
                new Vector2
                {
                    x = Mathf.Lerp(xBounds.x, xBounds.y, 0.15f),
                    y = Mathf.Lerp(yBounds.x, yBounds.y, 0.85f)
                }
            };

            var dist = new[]
            {
                Vector2.Distance(currentPosition, positions[0]),
                Vector2.Distance(currentPosition, positions[1]),
            };

            if (dist[0] < dist[1])
            {
                flipped = false;
                return positions[0];
            }

            flipped = true;
            return positions[1];
        }

        //============================================================================================================//

        #region Firing

        protected override void FireAttack()
        {
            var currentPosition = transform.position;

            if (!CameraController.IsPointInCameraRect(currentPosition, Constants.VISIBLE_GAME_AREA))
                return;

            Vector2 playerLocation = LevelManager.Instance.BotInLevel != null
                ? LevelManager.Instance.BotInLevel.transform.position
                : Vector3.right * 50;

            Vector2 targetLocation = m_enemyData.FireAtTarget ? playerLocation : Vector2.down;

            Vector2 shootDirection = m_enemyData.FireAtTarget
                ? (targetLocation - (Vector2) transform.position).normalized
                : Vector2.down;


            FactoryManager.Instance.GetFactory<ProjectileFactory>()
                .CreateObjects<Projectile>(
                    m_enemyData.ProjectileType,
                    currentPosition,
                    targetLocation,
                    shootDirection,
                    1f,
                    new[] {TagsHelper.PLAYER},
                    null,
                    0f,
                    false,
                    true);
        }

        #endregion

        //============================================================================================================//

        public override Type GetOverrideType()
        {
            return typeof(SquartEnemy);
        }
    }
}