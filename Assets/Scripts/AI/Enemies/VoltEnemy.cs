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
    public class VoltEnemy : Enemy
    {
        public float anticipationTime = 0.75f;
        
        //====================================================================================================================//
        
        public override bool IsAttachable => false;
        public override bool IgnoreObstacleAvoidance => true;
        public override bool SpawnHorizontal => true;

        //====================================================================================================================//
        
        private Vector2 _playerLocation;
        private Vector2 _targetOffset;
        private int _jumpCount;

        //====================================================================================================================//
        

        private float _anticipationTime;
        private float _minDistance => 4;
        private float _maxDistance => 5;

        private float _repositionMinDistance => 1;
        private float _repositionMaxDistance => 2;

        [SerializeField]
        private LayerMask collisionMask;

        //====================================================================================================================//



        public override void LateInit()
        {
            base.LateInit();

            _targetOffset = ChooseOffset(_minDistance, _maxDistance);
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
                    _jumpCount = Random.Range(4, 7);
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
            var currentPosition = transform.position;
            var targetPosition = _playerLocation + _targetOffset;
            if (Vector2.Distance(currentPosition, targetPosition) > 0.1f)
            {
                transform.position = Vector2.MoveTowards(currentPosition, targetPosition, EnemyMovementSpeed * Time.deltaTime);
                return;
            }

            _jumpCount--;

            if (_jumpCount <= 0)
            {
                SetState(STATE.ANTICIPATION);
                return;
            }

            _targetOffset += ChooseOffset(_repositionMinDistance, _repositionMaxDistance);
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
            Vector2 angleBetweenBotAndEnemy = ((Vector2)transform.position - _playerLocation).normalized;
            Vector2 rotatedAngle = Quaternion.Euler(0, 0, Random.Range(-110, -90)) * angleBetweenBotAndEnemy;
            
            var pos = rotatedAngle * Random.Range(minDist, maxDist);
            
            //var pos = Random.insideUnitCircle.normalized * Random.Range(minDist, maxDist);

            /*var checkX = Mathf.Clamp(Mathf.Abs(pos.x), minDist, maxDist);
            var checkY = Mathf.Clamp(Mathf.Abs(pos.y), minDist, maxDist);

            pos.x = pos.x < 0 ? checkX * -1f : checkX;
            pos.y = pos.y < 0 ? checkY * -1f : checkY;*/

            return pos;
        }

        #endregion //States

        //============================================================================================================//

        #region Firing

        protected override void FireAttack()
        {
            if (!CameraController.IsPointInCameraRect(transform.position, Constants.VISIBLE_GAME_AREA))
                return;

            Vector2 playerLocation = LevelManager.Instance.BotInLevel != null
                ? LevelManager.Instance.BotInLevel.transform.position
                : Vector3.right * 50;

            Vector2 targetLocation = m_enemyData.FireAtTarget ? playerLocation : Vector2.down;

            Vector2 shootDirection = m_enemyData.FireAtTarget
                ? (targetLocation - (Vector2) transform.position).normalized
                : Vector2.down;

            var raycastHit = Physics2D.Raycast(transform.position, shootDirection, 100, collisionMask.value);

            if (raycastHit.collider == null)
            {
                return;
            }

            if (!(raycastHit.transform.GetComponent<Bot>() is Bot bot))
                throw new Exception();

            if (LevelManager.Instance.BotInLevel.GetClosestAttachable(raycastHit.transform.position) is Bit)
            {
                return;
            }

            var lineShrink = FactoryManager.Instance
                .GetFactory<EffectFactory>()
                .CreateObject<LineShrink>();

            var didHitTarget = true;


            lineShrink.Init(transform.position, targetLocation);

            if (didHitTarget)
            {
                var damage = 10;
                LevelManager.Instance.BotInLevel.TryHitAt(targetLocation, damage);
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
    }
}