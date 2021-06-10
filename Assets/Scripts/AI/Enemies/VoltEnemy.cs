using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Recycling;
using StarSalvager.Audio;
using StarSalvager.Audio.Enemies;
using StarSalvager.Audio.Interfaces;
using StarSalvager.Cameras;
using StarSalvager.Factories;
using StarSalvager.Utilities;
using StarSalvager.Utilities.Analytics;
using StarSalvager.Utilities.Particles;
using StarSalvager.Values;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace StarSalvager.AI
{
    public class VoltEnemy : Enemy, IPlayEnemySounds<VoltSounds>
    {
        public VoltSounds EnemySound => (VoltSounds) EnemySoundBase;
        
        [SerializeField]
        private float anticipationTime;
        [SerializeField]
        private float timeChooseNewPosition = 1.5f;
        [SerializeField]
        private int chanceSwapDirections = 7;

        [SerializeField]
        private float averageOrbitDistance = 6.5f;
        [SerializeField]
        private int laserDamage = 5;

        //====================================================================================================================//

        public override bool IgnoreObstacleAvoidance => true;
        public override bool SpawnAboveScreen => false;

        //====================================================================================================================//
        
        private Vector2 _playerLocation;
        private Vector2 _targetOffset;
        private int _jumpCount;
        private bool _clockwiseMovement;

        //====================================================================================================================//
        

        private float _anticipationTime;
        public float _minDistance => averageOrbitDistance - 1.5f;
        public float _maxDistance => averageOrbitDistance + 1.5f;

        private float _repositionMinDistance => 0.5f;
        private float _repositionMaxDistance => 1.75f;

        private bool _hasReachedPlayer;
        private float _timeChooseNewPosition;

        [SerializeField]
        private LayerMask collisionMask;

        //====================================================================================================================//



        public override void OnSpawned()
        {
            EnemySoundBase = AudioController.Instance.VoltSounds;
            
            base.OnSpawned();

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
                    EnemySound.anticipationSound.Play();
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
                    MostRecentMovementDirection = (transform.position - currentPosition).normalized;
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

            if (_jumpCount > 0)
                return;
            
            SetState(CanHitTarget(out _) ? STATE.ANTICIPATION : STATE.MOVE);
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

        //What this is doing is alternating between two different angle offsets from the boss to determine next location. If it is far away, it chooses an angle that guides it inwards, and vice versa if it is close, to make it vary around in movement a bunch.
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

        /// <summary>
        /// Ensure that the volt can attack the player at the intended location, avoid wasting shots on Bits, which are ineffective
        /// </summary>
        /// <param name="iCanBeHit"></param>
        /// <returns></returns>
        private bool CanHitTarget(out ICanBeHit iCanBeHit)
        {
            const float DISTANCE = 100f;

            iCanBeHit = null;
            
            var currentPosition = transform.position;
            Vector2 targetLocation = _playerLocation;
            Vector2 shootDirection = (targetLocation - (Vector2)currentPosition).normalized;

            var raycastHit = Physics2D.Raycast(currentPosition, shootDirection, DISTANCE, collisionMask.value);

            if (raycastHit.collider == null)
            {
                Debug.DrawRay(currentPosition, shootDirection * DISTANCE, Color.red, 1f);
                return false;
            }
            
            var canBeHit = raycastHit.transform.GetComponent<ICanBeHit>();
            
            if (canBeHit is Bot bot && bot.GetClosestAttachable(raycastHit.point) is Bit)
                return false;

            iCanBeHit = canBeHit;
            Debug.DrawLine(currentPosition, raycastHit.point, Color.green, 1f);
            return true;
        }

        protected override void FireAttack()
        {
            if (!CanHitTarget(out var iCanBeHit))
                return;

            Debug.DrawLine(Position, _playerLocation, Color.green, 1f);

            
            var lineShrink = FactoryManager.Instance
                .GetFactory<EffectFactory>()
                .CreateObject<LineShrink>();
            
            lineShrink.Init(Position, _playerLocation);

            
            _jumpCount = Random.Range(6, 9);


            iCanBeHit.TryHitAt(_playerLocation, laserDamage);
            EnemySound.attackSound.Play();
        }

        #endregion

        //============================================================================================================//

        public override Type GetOverrideType()
        {
            return typeof(VoltEnemy);
        }


    }
}