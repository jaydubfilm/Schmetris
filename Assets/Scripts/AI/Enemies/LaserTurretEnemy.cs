using System;
using Recycling;
using StarSalvager.Cameras;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Values;
using UnityEngine;

using Random = UnityEngine.Random;

namespace StarSalvager.AI
{
    public class LaserTurretEnemy  : Enemy
    {
        private static readonly Color SEMI_TRANSPARENT = new Color(0.8f, 0.25f, 0.25f, 0.3f);
        //====================================================================================================================//
        
        public override bool IgnoreObstacleAvoidance => true;
        public override bool SpawnAboveScreen => true;

        //====================================================================================================================//

        [SerializeField]
        private float anticipationTime = 1f;

        private float _anticipationTimer;
        [SerializeField]
        private float attackTime;
        private float _attackTimer;

        private Vector2 _playerPosition;

        private static Vector3[] _directions;

        //====================================================================================================================//

        [SerializeField] private float rotationSpeed = 5f;
        private int _rotateDirection;

        [SerializeField]
        private float damage;
        [SerializeField]
        private LayerMask collisionMask;
        [SerializeField]
        private SpriteRenderer[] beamSpriteRenderers;

        //====================================================================================================================//

        public override void LateInit()
        {
            base.LateInit();

            _rotateDirection = Random.value > 0.5f ? -1 : 1;
            transform.eulerAngles = Vector3.forward * Random.Range(0, 360);
            
            if(_directions.IsNullOrEmpty())
                _directions = new []
                {
                    Vector3.down,
                    Quaternion.Euler(0, 0, 115) * Vector3.down,
                    Quaternion.Euler(0, 0, 245) * Vector3.down
                };

            SetState(STATE.ANTICIPATION );
            
        }

        //State Functions
        //====================================================================================================================//

        #region State Functions


        protected override void StateChanged(STATE newState)
        {
            switch (newState)
            {
                case STATE.NONE:
                case STATE.ANTICIPATION:
                    SetBeamsActive(false);
                    _anticipationTimer = anticipationTime;
                    break;
                case STATE.ATTACK:
                    SetBeamsActive(true);
                    _attackTimer = attackTime;
                    break;
                case STATE.DEATH:
                    //Recycle ya boy
                    Recycler.Recycle<LaserTurretEnemy>(this);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override void StateUpdate()
        {
            ApplyFallMotion();
            Rotate();

            switch (currentState)
            {
                case STATE.NONE:
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

        protected override void CleanStateData()
        {
            base.CleanStateData();
            _anticipationTimer = anticipationTime;
        }

        //====================================================================================================================//

        private void AnticipationState()
        {
            if (!CameraController.IsPointInCameraRect(transform.position, Constants.VISIBLE_GAME_AREA))
                return;

            _anticipationTimer -= Time.deltaTime;

            if (_anticipationTimer <= anticipationTime / 3f)
            {
                SetBeamsActive(true, SEMI_TRANSPARENT);
            }

            if (_anticipationTimer > 0)
            {
                return;
            }

            SetState(STATE.ATTACK);
        }

        private void AttackState()
        {
            const float DISTANCE = 100f;

            //--------------------------------------------------------------------------------------------------------//
            
            void TryRaycast(in Vector2 worldPosition, in Vector3 direction, Color color)
            {
                var raycastHit2D = Physics2D.Raycast(worldPosition, direction, DISTANCE, collisionMask.value);

                if (raycastHit2D.collider == null)
                {
                    Debug.DrawRay(worldPosition, direction * DISTANCE, color);
                    return;
                }

                if (!(raycastHit2D.transform.GetComponent<Bot>() is Bot bot))
                    throw new Exception();
                
                Debug.DrawRay(worldPosition, direction * DISTANCE, Color.green);

                var damageToApply = damage * Time.deltaTime;

                bot.TryHitAt(damageToApply);
            }

            //--------------------------------------------------------------------------------------------------------//

            _attackTimer -= Time.deltaTime;

            if (_attackTimer <= 0f)
            {
                SetState(STATE.ANTICIPATION);
                return;
            }
            
            if (!CameraController.IsPointInCameraRect(new Vector2(0, transform.position.y), Constants.VISIBLE_GAME_AREA))
            {
                SetState(STATE.ANTICIPATION);
                return;
            }
            
            //--------------------------------------------------------------------------------------------------------//

            var currentPosition = transform.position;
            var currentRotation = transform.rotation;

            foreach (var direction in _directions)
            {
                TryRaycast(currentPosition, currentRotation * direction, Color.red);
            }
        }

        #endregion //State Functions

        //====================================================================================================================//

        #region Movement

        public override void UpdateEnemy(Vector2 playerLocation)
        {
            _playerPosition = playerLocation;
            StateUpdate();
        }
        
        private void Rotate()
        {
            var eulerAngles = transform.eulerAngles;
            
            eulerAngles += Vector3.forward * (rotationSpeed * _rotateDirection * Time.deltaTime);

            transform.eulerAngles = eulerAngles;
        }
        

        #endregion

        //LaserTurretEnemy Functions
        //====================================================================================================================//

        private void SetBeamsActive(in bool state)
        {
            SetBeamsActive(state, Color.white);
        }
        
        private void SetBeamsActive(in bool state, in Color color)
        {
            foreach (var spriteRenderer in beamSpriteRenderers)
            {
                if(state)
                    spriteRenderer.color = color;
                
                spriteRenderer.gameObject.SetActive(state);
            }
        }

        //====================================================================================================================//

        public override void CustomRecycle(params object[] args)
        {
            CleanStateData();
            
            base.CustomRecycle(args);
        }

        public override Type GetOverrideType()
        {
            return typeof(LaserTurretEnemy);
        }

        //====================================================================================================================//
    }
}
