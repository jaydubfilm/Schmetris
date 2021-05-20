using System;
using StarSalvager.Cameras;
using StarSalvager.Values;
using System.Linq;
using Recycling;

using UnityEngine;

using Random = UnityEngine.Random;

namespace StarSalvager.AI
{
    public class MoonMinerEnemy : Enemy
    {
        private static readonly Color SEMI_TRANSPARENT = new Color(0.8f, 0.25f, 0.25f, 0.3f);

        public float anticipationTime = 1f;
        
        //====================================================================================================================//
        
        public override bool IgnoreObstacleAvoidance => true;
        public override bool SpawnAboveScreen => false;

        private float horizontalFarLeftX;
        private float horizontalFarRightX;
        private float verticalLowestAllowed;

        private Vector2 currentDestination;

        private float m_pauseMovementTimer = 0.0f;
        private bool m_pauseMovement;

        //====================================================================================================================//
        
        private Vector2 _targetLocation;
        private float _anticipationTime;
        private float _attackTime;
        private int _attackCount = 4;
        
        private float _attackEffectTimer;

        [SerializeField]
        private float damage;
        [SerializeField]
        private LayerMask collisionMask;
        [SerializeField]
        private GameObject beamObject;

        private SpriteRenderer _beamSpriteRenderer;
        private Transform _beamTransform;

        //====================================================================================================================//
        
        public override void LateInit()
        {
            base.LateInit();
            
            beamObject.SetActive(false);
            _beamTransform = beamObject.transform;
            _beamSpriteRenderer = beamObject.GetComponent<SpriteRenderer>();
            
            SetState(STATE.MOVE);
            
            /*currentDestination = transform.position;

            verticalLowestAllowed = 0.5f;
            horizontalFarLeftX = -1 * Constants.gridCellSize * Globals.ColumnsOnScreen / 3.5f;
            horizontalFarRightX = Constants.gridCellSize * Globals.ColumnsOnScreen / 3.5f;*/
        }

        //====================================================================================================================//

        #region Movement

        

        public override void UpdateEnemy(Vector2 playerLocation)
        {
            StateUpdate();
        }
        
        protected override void ApplyFleeMotion()
        {
            SetBeamActive(false);
            
            base.ApplyFleeMotion();
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
                case STATE.MOVE:
                    _targetLocation = GetNewPosition();
                    break;
                case STATE.FLEE:
                    break;
                case STATE.ANTICIPATION:
                    SetBeamActive(true, SEMI_TRANSPARENT);
                    _anticipationTime = anticipationTime;
                    break;
                case STATE.ATTACK:
                    SetBeamActive(true);
                    //beamObject.SetActive(true);
                    _attackTime = 2f;
                    _attackEffectTimer = 0;
                    break;
                case STATE.DEATH:
                    Recycler.Recycle<MoonMinerEnemy>(this);
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
                case STATE.MOVE:
                    MoveState();
                    break;
                case STATE.FLEE:
                    FleeState();
                    break;
                case STATE.ANTICIPATION:
                    AnticipationState();
                    break;
                case STATE.ATTACK:
                    AttackState();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(currentState), currentState, null);
            }
        }

        protected override void CleanStateData()
        {
            base.CleanStateData();
            _attackCount = 4;
        }



        private void MoveState()
        {
            //TODO Move towards target position
            var currentPosition = transform.position;

            if (Vector2.Distance(currentPosition, _targetLocation) > 0.1f)
            {
                transform.position = Vector2.MoveTowards(currentPosition, _targetLocation, EnemyMovementSpeed * Time.deltaTime);
                MostRecentMovementDirection = (transform.position - currentPosition).normalized;
                return;
            }

            //TODO If within threshold, move to anticipation state
            MostRecentMovementDirection = Vector3.zero;
            SetState(STATE.ANTICIPATION);
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
            SetState(STATE.DEATH);

        }

        private void AnticipationState()
        {
            //beamObject.SetActive(true);
            //_beamSpriteRenderer.color
            
            //TODO Wait x Seconds
            if (_anticipationTime > 0f)
            {
                _anticipationTime -= Time.deltaTime;
                return;
            }
            
            SetState(STATE.ATTACK);
            //TODO Switch to Attack State
        }

        private void AttackState()
        {
            const float DISTANCE = 100f;
            //--------------------------------------------------------------------------------------------------------//

            var raycastHit2D = Physics2D.Raycast(transform.position, Vector2.down, DISTANCE, collisionMask.value);

            if (raycastHit2D.collider != null)
            {
                if (!(raycastHit2D.transform.GetComponent<ICanBeHit>() is ICanBeHit botBase))
                    throw new Exception();

                var damageToApply = damage * Time.deltaTime;

                var playSound = false;
                if (_attackEffectTimer <= 0f)
                {
                    _attackEffectTimer = 0.5f;
                    CreateExplosionEffect(raycastHit2D.point);
                    playSound = true;
                }
                else
                {
                    _attackEffectTimer -= Time.deltaTime;
                }

                switch (botBase)
                {
                    case ForceField forceField:
                        forceField.TryHitAt(damageToApply);
                        break;
                    case Bot bot:
                        var closestAttachable = bot.GetClosestAttachable(raycastHit2D.point);
                        bot.TryHitAt(closestAttachable, damageToApply, playSound);
                        break;
                    case DecoyDrone decoyDrone:
                        decoyDrone.TryHitAt(damageToApply, playSound);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(botBase), botBase, null);
                }
              

                SetBeamLengthPosition(Position, Vector2.down, raycastHit2D.distance);
            }
            else
            {
                SetBeamLengthPosition(Position, Vector2.down, DISTANCE);
            }

            //--------------------------------------------------------------------------------------------------------//

            //TODO Wait x Seconds
            if (_attackTime > 0f)
            {
                _attackTime -= Time.deltaTime;
                return;
            }

            beamObject.SetActive(false);

            _attackCount--;

            //TODO Set to MoveState
            SetState(_attackCount == 0 ? STATE.FLEE : STATE.MOVE);
        }

        #endregion //States

        //============================================================================================================//

        /*#region Firing

        protected override void ProcessFireLogic()
        {
            return;
        }

        protected override void FireAttack()
        {
            base.FireAttack();
        }

        #endregion*/

        //====================================================================================================================//
        
        private void SetBeamActive(in bool state)
        {
            SetBeamActive(state, Color.white);
        }
        
        private void SetBeamActive(in bool state, in Color color)
        {
            _beamSpriteRenderer.color = color;
            beamObject.SetActive(state);
            
            /*foreach (var spriteRenderer in beamSpriteRenderers)
            {
                if(state)
                    spriteRenderer.color = color;
                
                spriteRenderer.gameObject.SetActive(state);
            }*/
        }
        private void SetBeamLengthPosition(in Vector2 worldPosition, in Vector2 direction, in float length)
        {
            //var targetTransform = _beamSpriteRenderer.transform;
            var size = _beamSpriteRenderer.size;
            size.y = length;
                
            _beamSpriteRenderer.size = size;
                
            _beamTransform.up = direction;

            _beamTransform.position = worldPosition + direction * (length / 2);
        }

        private static Vector2 GetNewPosition()
        {
            //Used to ensure the CameraVisibleRect is updated
            CameraController.IsPointInCameraRect(Vector2.zero, Constants.VISIBLE_GAME_AREA);
            
            var cameraRect = CameraController.VisibleCameraRect;
            var xBounds = new Vector2(cameraRect.xMin, cameraRect.xMax);
            var yBounds = new Vector2(cameraRect.yMin, cameraRect.yMax);

            return new Vector2
            {
                x = Mathf.Lerp(xBounds.x, xBounds.y, Random.Range(0.3f, 0.7f)),
                y = Mathf.Lerp(yBounds.x, yBounds.y, Random.Range(0.4f, 0.85f))
            };
        }

        public override Type GetOverrideType()
        {
            return typeof(MoonMinerEnemy);
        }

        //============================================================================================================//
    }
}