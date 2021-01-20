using System;
using System.Collections;
using System.Collections.Generic;
using Recycling;
using UnityEngine;
using Random = UnityEngine.Random;

namespace StarSalvager.AI
{
    public class VoltEnemy : Enemy
    {
        public override bool IsAttachable => false;
        public override bool IgnoreObstacleAvoidance => true;
        public override bool SpawnHorizontal => true;

        private Vector2 _playerLocation;
        private Vector2 _targetOffset;
        private int _jumpCount;

        private float _anticipationWaitTime = 1f;
        private float _flySpeed => 10f;
        private float _minDistance => 7;
        private float _maxDistance => 10;


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
                    _jumpCount = Random.Range(1, 4);
                    _targetOffset = ChooseOffset(_minDistance, _maxDistance);
                    break;
                case STATE.ANTICIPATION:
                    _anticipationWaitTime = 1f;
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
                transform.position = Vector2.MoveTowards(currentPosition, targetPosition, _flySpeed);
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
            if (_anticipationWaitTime > 0f)
            {
                _anticipationWaitTime -= Time.deltaTime;
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
            var pos = Random.insideUnitCircle * maxDist;

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
            throw new System.NotImplementedException();
        }

        #endregion

        //============================================================================================================//
    }
}