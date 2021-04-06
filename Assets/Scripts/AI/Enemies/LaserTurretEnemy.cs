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
        const float DISTANCE = 100f;
        
        public override Vector3 Position => ((BoxCollider2D)collider).bounds.center;

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
        private float _attackEffectTimer;

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
            
            MostRecentMovementDirection = Vector3.down;
            
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
                    _attackEffectTimer = 0;
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
                    SetBeamsLengthPosition(transform.position, DISTANCE);
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

            //--------------------------------------------------------------------------------------------------------//
            
            float TryRaycast(in Vector2 rayStartPosition, in Vector3 direction, Color color)
            {
                var raycastHit2D = Physics2D.Raycast(rayStartPosition, direction, DISTANCE, collisionMask.value);

                if (raycastHit2D.collider == null)
                {
                    Debug.DrawRay(rayStartPosition, direction * DISTANCE, color);
                    return DISTANCE;
                }

                if (!(raycastHit2D.transform.GetComponent<Bot>() is Bot bot))
                    throw new Exception();
                
                Debug.DrawRay(rayStartPosition, direction * DISTANCE, Color.green);

                var damageToApply = damage * Time.deltaTime;

                var attachable = bot.GetClosestAttachable(raycastHit2D.point);

                if (_attackEffectTimer <= 0f)
                {
                    _attackEffectTimer = 0.5f;
                    CreateExplosionEffect(raycastHit2D.point);
                }
                else
                {
                    _attackEffectTimer -= Time.deltaTime;
                }

                bot.TryHitAt(attachable, damageToApply);

                //bot.TryHitAt(damageToApply);

                return raycastHit2D.distance;
            }

            /*void SetArmLengthPosition(in int index, in Vector2 worldPosition, in Vector2 direction, in float length)
            {
                var targetTransform = beamSpriteRenderers[index].transform;
                var size = beamSpriteRenderers[index].size;
                size.y = length;
                
                beamSpriteRenderers[index].size = size;
                
                targetTransform.up = direction;
                //targetTransform.localScale = Vector3.up * length;

                targetTransform.position = worldPosition + (direction * (length / 2));
            }*/

            //--------------------------------------------------------------------------------------------------------//
            
            var currentPosition = transform.position;
            var currentRotation = transform.rotation;

            _attackTimer -= Time.deltaTime;

            if (_attackTimer <= 0f)
            {
                SetState(STATE.ANTICIPATION);
                return;
            }

            /*if (!CameraController.IsPointInCameraRect(new Vector2(0, transform.position.y), Constants.VISIBLE_GAME_AREA)
            )
            {
                for (var i = 0; i < _directions.Length; i++)
                {
                    var direction = _directions[i];
                    SetArmLength(i, currentPosition, direction, DISTANCE);
                }

                SetState(STATE.ANTICIPATION);
                return;
            }*/

            //--------------------------------------------------------------------------------------------------------//



            for (var i = 0; i < _directions.Length; i++)
            {
                var currentDirection = currentRotation * _directions[i];
                
                var length = TryRaycast(currentPosition, currentDirection, Color.red);

                SetBeamLengthPosition(i, currentPosition, currentDirection, length);
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
        
        protected override void ApplyFleeMotion()
        {
            
        }
        

        #endregion

        protected override Vector2 GetMovementDirection(Vector2 playerLocation)
        {
            return Vector2.down;
        }

        //LaserTurretEnemy Functions
        //====================================================================================================================//

        private void SetBeamLengthPosition(in int index, in Vector2 worldPosition, in Vector2 direction, in float length)
        {
            var targetTransform = beamSpriteRenderers[index].transform;
            var size = beamSpriteRenderers[index].size;
            size.y = length;
                
            beamSpriteRenderers[index].size = size;
                
            targetTransform.up = direction;

            targetTransform.position = worldPosition + (direction * (length / 2));
        }
        
        private void SetBeamsLengthPosition(in Vector2 worldPosition, in float length)
        {
            var currentRotation = transform.rotation;
            
            for (int i = 0; i < _directions.Length; i++)
            {
                var currentDirection = (Vector2)(currentRotation * _directions[i]);
                
                var targetTransform = beamSpriteRenderers[i].transform;
                var size = beamSpriteRenderers[i].size;
                size.y = length;
                
                beamSpriteRenderers[i].size = size;
                
                targetTransform.up = currentDirection;

                targetTransform.position = worldPosition + (currentDirection * (length / 2));
            }

        }

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
