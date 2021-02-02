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
        public float anticipationTime = 0.75f;
        
        //====================================================================================================================//
        
        public override bool IsAttachable => false;
        public override bool IgnoreObstacleAvoidance => true;
        public override bool SpawnAboveScreen => false;

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

        public override void ChangeHealth(float amount)
        {
            CurrentHealth += amount;

            if (amount < 0)
            {
                FloatingText.Create($"{Mathf.Abs(amount)}", transform.position, Color.red);
            }

            if (CurrentHealth > 0)
                return;

            DropLoot();

            SessionDataProcessor.Instance.EnemyKilled(m_enemyData.EnemyType);
            AudioController.PlaySound(SOUND.ENEMY_DEATH);

            LevelManager.Instance.WaveEndSummaryData.AddEnemyKilled(name);



            LevelManager.Instance.EnemyManager.RemoveEnemy(this);

            Recycler.Recycle<VoltEnemy>(this);
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
            Vector2 rotatedAngle;
            if (Vector2.Distance(transform.position, _playerLocation) >= _maxDistance)
            {
                rotatedAngle = Quaternion.Euler(0, 0, Random.Range(-140, -100)) * angleBetweenBotAndEnemy;
            }
            else
            {
                rotatedAngle = Quaternion.Euler(0, 0, Random.Range(-90, -60)) * angleBetweenBotAndEnemy;
            }
            
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