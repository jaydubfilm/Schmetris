using System;
using Recycling;
using Sirenix.OdinInspector;
using StarSalvager.Audio;
using StarSalvager.Audio.Enemies;
using StarSalvager.Cameras;
using StarSalvager.Utilities;
using StarSalvager.Values;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace StarSalvager.AI
{
    public class IceWingEnemy : Enemy, IPlayEnemySounds<IceWingSounds>
    {
        public IceWingSounds EnemySound => (IceWingSounds) EnemySoundBase;

        public override bool IgnoreObstacleAvoidance => true;
        public override bool SpawnAboveScreen => true;

        //Properties
        //====================================================================================================================//

        [SerializeField, Range(0f, 10f), SuffixLabel("sec", true)]
        private float anticipationWaitTime = 3f;

        [SerializeField, Range(1, 10), BoxGroup("Attack")]
        private int attackPasses = 2;

        [SerializeField, Range(1, 10), BoxGroup("Attack")]
        private int attackSwoops = 2;

        [SerializeField, BoxGroup("Attack"), SuffixLabel("units", true)]
        private float attackSwoopHeight;

        [SerializeField, BoxGroup("Attack"), SuffixLabel("sec", true)]
        private float apexWaitTime;

        [SerializeField, BoxGroup("Attack Beam"), SuffixLabel("sec", true)]
        private float freezeTime;

        [FormerlySerializedAs("checkDistance")] 
        [SerializeField, BoxGroup("Attack Beam"), SuffixLabel("units", true), DisableInPlayMode]
        private float beamLength;

        [SerializeField, Range(0f, 1f), Tooltip("1.0 is 100% accurate and 0.0 is anywhere in front"),
         BoxGroup("Attack Beam"), DisableInPlayMode]
        private float dotThreshold = 0.1f;


        //====================================================================================================================//

        private LineRenderer _lineRenderer;

        private float _travelTime;
        private bool _isLeftPos;

        private int _attackPassCount;
        private int _attackSwoopCount;
        private float _waitTimer;

        private float _t;
        private Vector2 _targetLocation;
        private Vector2 _startLocation;


        private Vector2 _currentPosition;
        private Vector2 _lastPosition;

        //====================================================================================================================//

        public override void LateInit()
        {
            //--------------------------------------------------------------------------------------------------------//

            //https://www.calculator.net/right-triangle-calculator.html
            float CalculateEndWidth(in float dotThreshold, in float castDistance)
            {
                var angleA = Mathf.Acos(dotThreshold) * Mathf.Rad2Deg;
                var lengthC = castDistance / Mathf.Cos(angleA);
                var lengthA = Mathf.Sqrt(Mathf.Pow(lengthC, 2) - Mathf.Pow(castDistance, 2));

                return lengthA;
            }

            //--------------------------------------------------------------------------------------------------------//

            base.LateInit();

            _attackPassCount = 0;
            _attackSwoopCount = 0;
            _waitTimer = 0f;

            if (!_lineRenderer) _lineRenderer = GetComponentInChildren<LineRenderer>();

            _lineRenderer.startWidth = 0;
            _lineRenderer.endWidth = 1f;
            _lineRenderer.widthMultiplier = CalculateEndWidth(dotThreshold, beamLength);
            _lineRenderer.enabled = false;

            SetState(STATE.MOVE);
        }

        //====================================================================================================================//

        protected override void StateChanged(STATE newState)
        {
            switch (newState)
            {
                case STATE.NONE:
                    break;
                case STATE.MOVE:
                    _isLeftPos = Random.value > 0.5f;
                    //TODO Determine if the enemy is on the left or right side of the screen
                    _targetLocation = GetNewPosition(GetPos(_isLeftPos));
                    _enemyMovementSpeed = m_enemyData.MovementSpeed;
                    break;
                case STATE.FLEE:
                    break;
                case STATE.ANTICIPATION:
                    _waitTimer = anticipationWaitTime;
                    _attackSwoopCount = 0;
                    break;
                case STATE.ATTACK:
                    _waitTimer = apexWaitTime;
                    _t = 0f;
                    _startLocation = GetNewPosition(GetPos(_isLeftPos));
                    _targetLocation = GetNewPosition(GetPos(!_isLeftPos));

                    var dist = Vector2.Distance(_startLocation, _targetLocation);
                    _travelTime = dist / m_enemyData.MovementSpeed;
                    
                    EnemySound.swoopSound.Play();
                    break;
                case STATE.DEATH:
                    Recycler.Recycle<IceWingEnemy>(this);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
            }
        }

        //State Updates
        //====================================================================================================================//

        #region State Updates

        protected override void StateUpdate()
        {
            _lastPosition = _currentPosition;
            _currentPosition = Position;

            
            /*Debug.DrawRay(_currentPosition,
                (_currentPosition - _lastPosition).normalized,
                Color.red);*/

            if (_waitTimer > 0f)
            {
                _waitTimer -= Time.deltaTime;
                
                _enemyMovementSpeed = 0;
                MostRecentMovementDirection = Vector3.zero;
                return;
            }
            
            MostRecentMovementDirection = (_currentPosition - _lastPosition).normalized;


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

        private void MoveState()
        {
            //TODO Move towards target position
            var currentPosition = transform.position;

            if (Vector2.Distance(currentPosition, _targetLocation) > 0.1f)
            {
                transform.position =
                    Vector2.MoveTowards(currentPosition, _targetLocation, EnemyMovementSpeed * Time.deltaTime);
                return;
            }

            SetState(STATE.ATTACK);

        }

        private void FleeState()
        {
            //TODO Fly away once passes are done
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
            SetState(STATE.ATTACK);
        }

        private void AttackState()
        {

            //--------------------------------------------------------------------------------------------------------//

            void TryFindBits(in Vector2 checkDirection)
            {
                var currentPosition = Position;
                var angle = Mathf.Acos(dotThreshold) * Mathf.Rad2Deg;
                var rayDirections = new[]
                {
                    Quaternion.Euler(0, 0, angle) * checkDirection,
                    Quaternion.Euler(0, 0, angle / 2) * checkDirection,
                    Quaternion.Euler(0, 0, -angle / 2) * checkDirection,
                    Quaternion.Euler(0, 0, -angle) * checkDirection
                };

                foreach (var dir in rayDirections)
                {
                    Debug.DrawRay(currentPosition, dir * beamLength, Color.yellow);

                    var hit = Physics2D.Raycast(currentPosition, dir);

                    if (hit.transform == null)
                        continue;

                    var hitCollidable = hit.transform.gameObject.GetComponent<ICanBeHit>();

                    switch (hitCollidable)
                    {
                        case Bot bot:
                        {
                            var closestAttachable = bot.GetClosestAttachable(hit.point);
                            if (closestAttachable is Bit bit)
                            {
                                bit.SetFrozen(freezeTime);
                                EnemySound.freezeSound.Play();
                            }
                        }
                            break;
                        case Bit bit when bit.Frozen == false:
                        {
                            bit.SetFrozen(freezeTime);
                            EnemySound.freezeSound.Play();
                        }
                            break;

                    }
                }

            }

            //--------------------------------------------------------------------------------------------------------//


            var t = _t / _travelTime;

            if (t >= 1f)
            {
                _isLeftPos = !_isLeftPos;

                if (++_attackSwoopCount >= attackSwoops)
                {
                    SetState(++_attackPassCount >= attackPasses ? STATE.FLEE : STATE.ANTICIPATION);
                }
                else
                    SetState(STATE.ATTACK);


                return;
            }
            
            var newPosition = Mathfx.Hermite(_startLocation, _targetLocation, t);
            newPosition.y = Mathfx.HermiteCubed(newPosition.y, attackSwoopHeight, t);

            var attacking = t >= 0.25f && t <= 0.55f;
            _lineRenderer.enabled = attacking;

            if (attacking)
            {
                //var currentPosition = newPosition;
                var checkDirection = Vector2.down/*(_currentPosition - _lastPosition).normalized*/;

                #region Unity Editor

#if UNITY_EDITOR
                void DebugLines()
                {

                    Debug.DrawRay(newPosition, checkDirection * beamLength, Color.cyan);

                    var angle = Mathf.Acos(dotThreshold) * Mathf.Rad2Deg;
                    var up = Quaternion.Euler(0, 0, angle) * checkDirection;
                    var down = Quaternion.Euler(0, 0, -angle) * checkDirection;

                    Debug.DrawRay(newPosition, up * beamLength, Color.blue);
                    Debug.DrawRay(newPosition, down * beamLength, Color.blue);
                }

                DebugLines();
#endif

                #endregion //Unity Editor

                TryFindBits(checkDirection);

                _lineRenderer.SetPositions(new[]
                {
                    (Vector3)newPosition,
                    (Vector3)newPosition + (Vector3)(checkDirection * beamLength)
                });

            }

            transform.position = newPosition;
            
            _enemyMovementSpeed = (_currentPosition - _lastPosition).magnitude / Time.deltaTime;

            _t += Time.deltaTime;
        }

        #endregion //State Updates

        //====================================================================================================================//
        
        private static Vector2 GetNewPosition(in float xPos, in float yPos = 0.85f)
        {
            //Used to ensure the CameraVisibleRect is updated
            CameraController.IsPointInCameraRect(Vector2.zero, Constants.VISIBLE_GAME_AREA);

            var cameraRect = CameraController.VisibleCameraRect;
            var xBounds = new Vector2(cameraRect.xMin, cameraRect.xMax);
            var yBounds = new Vector2(cameraRect.yMin, cameraRect.yMax);

            return new Vector2
            {
                x = Mathf.Lerp(xBounds.x, xBounds.y, xPos),
                y = Mathf.Lerp(yBounds.x, yBounds.y, yPos)
            };
        }

        private static float GetPos(in bool isLeft)
        {
            return isLeft ? 0.05f : 0.95f;
        }

        //Enemy Functions
        //====================================================================================================================//


        public override void UpdateEnemy(Vector2 playerLocation)
        {
            StateUpdate();
        }

        protected override void ApplyFleeMotion()
        {
            base.ApplyFleeMotion();

            _lineRenderer.enabled = false;
        }

        protected override Vector2 GetMovementDirection(Vector2 playerLocation)
        {
            return (_currentPosition - _lastPosition).normalized;
        }

        public override Type GetOverrideType()
        {
            return typeof(IceWingEnemy);
        }
    }
}
