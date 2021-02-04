using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Recycling;
using StarSalvager.Audio;
using StarSalvager.Cameras;
using StarSalvager.Factories;
using StarSalvager.Utilities.Analytics;
using StarSalvager.Utilities.Particles;
using StarSalvager.Values;
using UnityEngine;
using Random = UnityEngine.Random;

namespace StarSalvager.AI
{
    public class VoltEnemy : Enemy
    {
        private float anticipationTime => 0.0f;
        private float timeChooseNewPosition => 1.5f;
        private int chanceSwapDirections = 7;

        public float AverageOrbitDistance = 6.5f;
        public int LaserDamage = 1;

        //====================================================================================================================//

        public override bool IsAttachable => false;
        public override bool IgnoreObstacleAvoidance => true;
        public override bool SpawnAboveScreen => false;

        //====================================================================================================================//
        
        private Vector2 _playerLocation;
        private Vector2 _targetOffset;
        private int _jumpCount;
        private bool _clockwiseMovement;

        //====================================================================================================================//
        

        private float _anticipationTime;
        public float _minDistance => AverageOrbitDistance - 1.5f;
        public float _maxDistance => AverageOrbitDistance + 1.5f;

        private float _repositionMinDistance => 0.5f;
        private float _repositionMaxDistance => 1.75f;

        private bool _hasReachedPlayer = false;
        private float _timeChooseNewPosition = 0.0f;

        [SerializeField]
        private LayerMask collisionMask;

        //====================================================================================================================//



        public override void LateInit()
        {
            base.LateInit();

            _hasReachedPlayer = false;
            _timeChooseNewPosition = 0.0f;

            _targetOffset = ChooseOffset(_minDistance, _maxDistance);
            _clockwiseMovement = Random.Range(0, 2) == 0;

            _jumpCount = Random.Range(6, 9);

            SetState(STATE.MOVE);
        }

        //============================================================================================================//

        #region Movement

        public override void UpdateEnemy(Vector2 playerlocation)
        {
            _playerLocation = playerlocation;
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
                case STATE.ATTACK:

                    break;
                case STATE.MOVE:
                    break;
                case STATE.ANTICIPATION:
                    _anticipationTime = anticipationTime;
                    break;
                case STATE.DEATH:
                    Recycler.Recycle<VoltEnemy>(this);
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
                case STATE.ANTICIPATION:
                    AnticipationState();
                    break;
                case STATE.ATTACK:
                    AttackState();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override void CleanStateData()
        {
            base.CleanStateData();
            
            //_targetOffset = Vector2.zero;
            _jumpCount = 0;
        }

        private void MoveState()
        {
            if (_hasReachedPlayer)
            {
                _timeChooseNewPosition += Time.deltaTime;
            }

            if (_timeChooseNewPosition < timeChooseNewPosition)
            {
                var currentPosition = transform.position;
                var targetPosition = _playerLocation + _targetOffset;
                if (Vector2.Distance(currentPosition, targetPosition) > 0.1f)
                {
                    transform.position = Vector2.MoveTowards(currentPosition, targetPosition, EnemyMovementSpeed * Time.deltaTime);
                    m_mostRecentMovementDirection = (transform.position - currentPosition).normalized;
                    return;
                }
            }
            else
            {
                Debug.Log("Player is cheesing the volt, reposition");
            }

            _hasReachedPlayer = true;
            _timeChooseNewPosition = 0;
            _jumpCount--;

            _targetOffset += ChooseOffset(_repositionMinDistance, _repositionMaxDistance);

            if (_jumpCount <= 0)
            {
                SetState(STATE.ANTICIPATION);
                return;
            }
        }

        private void AnticipationState()
        {
            if (_anticipationTime > 0f)
            {
                _anticipationTime -= Time.deltaTime;
                return;
            }
            
            SetState(STATE.ATTACK);
        }

        private void AttackState()
        {
            FireAttack();
            
            SetState(STATE.MOVE);
        }

        private Vector2 ChooseOffset(in float minDist, in float maxDist)
        {
            if (Random.Range(0, 100) < chanceSwapDirections)
            {
                _clockwiseMovement = !_clockwiseMovement;
            }
            
            Vector2 angleBetweenBotAndEnemy = ((Vector2)transform.position - _playerLocation).normalized;
            Vector2 rotatedAngle;
            float angle;
            if (Vector2.Distance(transform.position, _playerLocation) >= _maxDistance)
            {
                angle = Random.Range(160.0f, 80.0f);
            }
            else
            {
                angle = Random.Range(110.0f, 40.0f);
            }

            if (_clockwiseMovement)
            {
                angle *= -1;
            }
            rotatedAngle = Quaternion.Euler(0, 0, angle) * angleBetweenBotAndEnemy;
            
            var pos = rotatedAngle * Random.Range(minDist, maxDist);
            
            return pos;
        }

        #endregion //States

        //============================================================================================================//

        #region Firing

        protected override void FireAttack()
        {
            if (!CameraController.IsPointInCameraRect(transform.position, Constants.VISIBLE_GAME_AREA))
                return;

            Vector2 targetLocation = _playerLocation;

            Vector2 shootDirection = (targetLocation - (Vector2)transform.position).normalized;

            var raycastHit = Physics2D.Raycast(transform.position, shootDirection, 100, collisionMask.value);
            //Debug.DrawRay(transform.position, shootDirection * 100, Color.blue, 1.0f);

            if (raycastHit.collider == null)
            {
                return;
            }

            if (!(raycastHit.transform.GetComponent<Bot>() is Bot bot))
                throw new Exception();

            if (LevelManager.Instance.BotInLevel.GetClosestAttachable(raycastHit.point - _playerLocation) is Bit)
            {
                return;
            }

            var lineShrink = FactoryManager.Instance
                .GetFactory<EffectFactory>()
                .CreateObject<LineShrink>();

            var didHitTarget = true;
            _jumpCount = Random.Range(6, 9);

            lineShrink.Init(transform.position, targetLocation);

            if (didHitTarget)
            {
                LevelManager.Instance.BotInLevel.TryHitAt(targetLocation, LaserDamage);
            }


            /*FactoryManager.Instance.GetFactory<ProjectileFactory>()
                .CreateObjects<Projectile>(
                    m_enemyData.ProjectileType,
                    transform.position,
                    targetLocation,
                    shootDirection,
                    1f,
                    "Player",
                    null);*/
        }

        #endregion

        //============================================================================================================//

        public override Type GetOverrideType()
        {
            return typeof(VoltEnemy);
        }
    }
}