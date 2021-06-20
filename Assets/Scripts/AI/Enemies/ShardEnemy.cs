using StarSalvager.Cameras;
using StarSalvager.Values;
using System;
using System.Linq;
using Recycling;
using StarSalvager.Audio;
using StarSalvager.Audio.Enemies;
using StarSalvager.Audio.Interfaces;
using UnityEngine;
using Random = UnityEngine.Random;

namespace StarSalvager.AI
{
    public class ShardEnemy : SpineEnemy, IPlayEnemySounds<ShardSounds>
    {
        public ShardSounds EnemySound => (ShardSounds) EnemySoundBase;

        public override bool IgnoreObstacleAvoidance => true;
        public override bool SpawnAboveScreen => true;

        [SerializeField, Range(1,10)]
        private float fallMultiplier = 3f;

        [SerializeField]
        private float damage = 25;

        [SerializeField]
        private LayerMask mask;


        private bool _triggered;
        [SerializeField]
        private float anticipationTime;
        private float _anticipationTimer;

        private Vector2 _targetLocation;

        //ISpine Override Tests
        //====================================================================================================================//

        private static readonly SpineAnimation IDLE_ANIMATION = new SpineAnimation("Idle", true);
        private static readonly SpineAnimation ANTICIPATION_ANIMATION = new SpineAnimation("Anticipation", false);
        private static readonly SpineAnimation FLY_ANIMATION = new SpineAnimation("Fly", false);

        

        //============================================================================================================//

        public override void OnSpawned()
        {
            EnemySoundBase = AudioController.Instance.ShardSounds;
            
            base.OnSpawned();

            SetState(STATE.MOVE);
            SetSpineAnimation(IDLE_ANIMATION);
        }

        #region Movement

        public override void UpdateEnemy(Vector2 playerLocation)
        {
            StateUpdate();
        }

        protected override Vector2 GetMovementDirection(Vector2 playerLocation)
        {
            return Vector2.down;
        }

        #endregion

        //====================================================================================================================//

        #region States

        protected override void StateChanged(STATE newState)
        {
            var currentPosition = transform.position;

            switch (newState)
            {
                case STATE.MOVE:
                    CameraController.IsPointInCameraRect(Vector2.zero, Constants.VISIBLE_GAME_AREA);

                    var cameraRect = CameraController.VisibleCameraRect;
                    var xBounds = new Vector2(cameraRect.xMin, cameraRect.xMax);
                    var yBounds = new Vector2(cameraRect.yMin, cameraRect.yMax);

                    _targetLocation = new Vector2
                    {
                        x = Mathf.Lerp(xBounds.x, xBounds.y, Random.Range(0.3f, 0.7f)),
                        y = Mathf.Lerp(yBounds.x, yBounds.y, Random.Range(0.85f, 0.85f))
                    };


                    currentPosition.x = _targetLocation.x;

                    transform.position = currentPosition;
                    break;
                case STATE.ANTICIPATION:
                    EnemySound.lockPositionSound.Play();
                    _anticipationTimer = anticipationTime;
                    _triggered = false;
                    break;
                case STATE.ATTACK:
                    EnemySound.beginAttackFallSound.Play();
                    SetSpineAnimation(FLY_ANIMATION);
                    break;
                case STATE.DEATH:

                    CreateExplosionEffect(currentPosition);
                    Recycler.Recycle<ShardEnemy>(this);
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

        private void MoveState()
        {
            //TODO Move into position at top of screen

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

        private void AnticipationState()
        {
            const float CAST_DISTANCE = 100f;

            if (!_triggered)
            {
                //TODO Wait at that location until

                var hit = Physics2D.Raycast(transform.position, Vector2.down, CAST_DISTANCE, mask.value);
                if (hit.collider == null)
                    return;

                var iHealth = hit.transform.GetComponent<IHealth>();

                switch (iHealth)
                {
                    case ForceField _: break;
                    case BotBase _: break;
                    default: return;
                }

                SetSpineAnimation(ANTICIPATION_ANIMATION);
                _triggered = true;
            }

            if (_anticipationTimer > 0f)
            {
                _anticipationTimer -= Time.deltaTime;
                return;
            }
            

            SetState(STATE.ATTACK);
        }

        private void AttackState()
        {
            var currentPosition = transform.position;
            //TODO Fall at speed until hit or off screen
            currentPosition += Vector3.down * (Time.deltaTime * EnemyMovementSpeed * fallMultiplier);

            if (currentPosition.y < -5)
            {
                DestroyEnemy();
                return;
            }

            transform.position = currentPosition;

            var hit = Physics2D.BoxCast(currentPosition, m_enemyData.Dimensions, 0f, Vector2.down, 1f, mask.value);
            if (hit.collider == null)
                return;

            var iHealth = hit.transform.GetComponent<IHealth>();

            switch (iHealth)
            {
                //--------------------------------------------------------------------------------------------------------//
                case ForceField forceField:
                    forceField.TryHitAt(damage);
                    break;

                //--------------------------------------------------------------------------------------------------------//
                case Bot bot:
                    var closestAttachable = bot.GetClosestAttachable(hit.point);
                    var coordinateBelow = closestAttachable.Coordinate + Vector2Int.down;

                    bot.TryHitAt(closestAttachable, damage);

                    var belowAttachable = bot.AttachedBlocks.FirstOrDefault(x => x.Coordinate == coordinateBelow);
                    if(!(belowAttachable is null))
                        bot.TryHitAt(belowAttachable, damage);
                    break;
                //--------------------------------------------------------------------------------------------------------//
                case DecoyDrone decoyDrone:
                    decoyDrone.TryHitAt(damage, true);
                    break;
                //--------------------------------------------------------------------------------------------------------//
                default:
                    return;
            }

            DestroyEnemy();
        }

        #endregion //States

        //============================================================================================================//

        public override Type GetOverrideType()
        {
            return typeof(ShardEnemy);
        }


    }
}
