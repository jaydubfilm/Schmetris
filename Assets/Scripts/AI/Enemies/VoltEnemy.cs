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
        public float anticipationTime = 0.5f;
        
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

        //====================================================================================================================//
        


        public override void LateInit()
        {
            base.LateInit();
            
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
                    _jumpCount = Random.Range(2, 5);
                    _targetOffset = ChooseOffset(_minDistance, _maxDistance);
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
            switch (currrentState)
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
            
            _targetOffset = Vector2.zero;
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

            _targetOffset = ChooseOffset(_minDistance, _maxDistance);
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

        private static Vector2 ChooseOffset(in float minDist, in float maxDist)
        {
            var pos = Random.insideUnitCircle.normalized * Random.Range(minDist, maxDist);

            var checkX = Mathf.Clamp(Mathf.Abs(pos.x), minDist, maxDist);
            var checkY = Mathf.Clamp(Mathf.Abs(pos.y), minDist, maxDist);

            pos.x = pos.x < 0 ? checkX * -1f : checkX;
            pos.y = pos.y < 0 ? checkY * -1f : checkY;

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


            FactoryManager.Instance.GetFactory<ProjectileFactory>()
                .CreateObjects<Projectile>(
                    m_enemyData.ProjectileType,
                    transform.position,
                    targetLocation,
                    shootDirection,
                    1f,
                    "Player",
                    null);
        }

        #endregion

        //============================================================================================================//
    }
}