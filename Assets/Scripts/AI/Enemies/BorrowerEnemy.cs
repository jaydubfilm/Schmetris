using System;
using System.Linq;
using Recycling;
using StarSalvager.Audio;
using StarSalvager.Audio.Enemies;
using StarSalvager.Cameras;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Values;
using UnityEngine;

namespace StarSalvager.AI
{
    public class BorrowerEnemy  : EnemyAttachable, IPlayEnemySounds<BorrowerSounds>
    {
        public float anticipationTime = 1f;

        public BorrowerSounds EnemySound => (BorrowerSounds) EnemySoundBase;

        //====================================================================================================================//
        
        public override bool IgnoreObstacleAvoidance => true;
        public override bool SpawnAboveScreen => true;

        //====================================================================================================================//

        public Bit CarryingBit => _carryingBit;
        
        private float _anticipationTime;
        private Vector2 _playerLocation;
        private Bit _carryingBit;
        private Bit _attachTarget;

        private int _stolenBits;

        private float _carrySpeed;

        public override void LateInit()
        {
            base.LateInit();

            _stolenBits = 0;
            
            SetState(STATE.PURSUE);
            
        }

        //====================================================================================================================//

        #region EnemyAttachable Overrides

        protected override void OnCollide(GameObject gameObject, Vector2 worldHitPoint)
        {
            if (currentState != STATE.PURSUE)
                return;
            
            base.OnCollide(gameObject, worldHitPoint);
        }

        public override void SetAttached(bool isAttached)
        {
            base.SetAttached(isAttached);
            
            if (currentState != STATE.PURSUE)
                return;
            
            SetState(Attached ? STATE.ANTICIPATION : STATE.PURSUE);
            
            //This is important to change the target, as attaching may have changed the intended target
            if(Attached)
                _attachTarget = AttachedBot.GetClosestAttachable(Coordinate, 1f) as Bit;

        }

        public override void OnBumped()
        {
            base.OnBumped();
            
            SetState(STATE.IDLE);
        }

        #endregion //EnemyAttachable Overrides

        //============================================================================================================//

        #region Movement

        public override void UpdateEnemy(Vector2 playerlocation)
        {
            _playerLocation = playerlocation;
            
            StateUpdate();
        }

        

        public Bit FindClosestBitOnBot()
        {
            var bot = LevelManager.Instance.BotInLevel;
            var bits = bot.AttachedBlocks.OfType<Bit>().ToArray();

            if (bits.IsNullOrEmpty())
                return null;

            var currentPosition = transform.position;

            var minDist = 999f;
            Bit selectedBit = null;
            
            foreach (var bit in bits)
            {
                if (EnemyManager.IsBitTargeted(this, bit))
                    continue;
                
                var dist = Vector2.Distance(currentPosition, bit.transform.position);
                
                if(dist >= minDist)
                    continue;

                minDist = dist;
                selectedBit = bit;
            }

            return selectedBit;
        }

        #endregion

        //====================================================================================================================//

        #region States

        protected override void StateChanged(STATE newState)
        {
            switch (newState)
            {
                case STATE.NONE:
                    break;
                case STATE.IDLE:
                    ClearTarget();
                    break;
                case STATE.PURSUE:
                    //Try to Find a Bit on the bot
                    _attachTarget = FindClosestBitOnBot();

                    EnemyManager.SetBorrowerTarget(this, _attachTarget);
                    _enemyMovementSpeed = m_enemyData.MovementSpeed;
                    break;
                case STATE.ANTICIPATION:
                    _anticipationTime = anticipationTime;
                    _enemyMovementSpeed = 0f;
                    break;
                case STATE.ATTACK:
                    break;
                case STATE.FLEE:
                    
                    _carrySpeed = m_enemyData.MovementSpeed / 2f;
                    _enemyMovementSpeed = _carrySpeed;
                    
                    Target = null;
                    AttachedBot?.ForceDetach(this);
                    AttachedBot = null;
                    break;
                case STATE.DEATH:
                    //Drop the attached bit
                    DropCarryingBit();
                    
                    EnemyManager.RemoveBorrowerTarget(this);
                    
                    Recycler.Recycle<DataLeechEnemy>(this);
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
                    return;
                case STATE.IDLE:
                    IdleState();
                    break;
                case STATE.PURSUE:
                    PursueState();
                    break;
                case STATE.ANTICIPATION:
                    AnticipationState();
                    break;
                case STATE.ATTACK:
                    AttackState();
                    break;
                case STATE.FLEE:
                    FleeState();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(currentState), currentState, null);
            }
        }

        //====================================================================================================================//
        
        private void IdleState()
        {
            ApplyFallMotion();
        }

        private void PursueState()
        {
            //If we currently don't have a target, and are able to find a new one
            if (_attachTarget is null)
            {
                var test = FindClosestBitOnBot();
                if (test == null)
                {
                    ApplyFleeMotion();
                    return;
                }
                
                EnemyManager.SetBorrowerTarget(this, test);
                _attachTarget = test;
            }

            if (EnemyManager.IsBitCarried(_attachTarget))
            {
                _attachTarget = null;
                return;
            }
            
            //Fly towards a specific Bit on the bot
            var currentPosition = transform.position;
            var targetPosition = _attachTarget.transform.position;

            MostRecentMovementDirection = GetMovementDirection(targetPosition);

            currentPosition = Vector3.MoveTowards(currentPosition, targetPosition,
                m_enemyData.MovementSpeed * Time.deltaTime);


            transform.position = currentPosition;
        }

        private void AnticipationState()
        {

            if (_attachTarget == null)
            {
                SetState(STATE.PURSUE);
                return;
            }

            MostRecentMovementDirection = Vector3.zero;
            
            //After wait time, move to attack state
            //If the Bit fell off the Bot, then we can attempt to steal it
            if (_anticipationTime > 0 && _attachTarget.Attached)
            {
                _anticipationTime -= Time.deltaTime;
                return;
            }

            MostRecentMovementDirection = GetMovementDirection(_attachTarget.transform.position);
            
            SetState(STATE.ATTACK);
        }

        private void AttackState()
        {
            Bit bit;
            //If we've switched from anticipation but the Target is null (Which means we're no longer attached to the bot)
            if (previousState == STATE.ANTICIPATION && Target == null && _attachTarget != null)
            {
                bit = _attachTarget;
            }
            else if (Target is Bit target)
            {
                bit = target;
            }
            else
            {
                return;
            }
                
            //Detach Bit from Bot, in the situation where we've fallen off bot, we have to check for null
            AttachedBot?.ForceDetach(bit);
            
            //Set Bit Parent to this object & Disable the collider
            bit.transform.SetParent(transform, false);
            bit.transform.localPosition = Vector3.down;
            bit.transform.localRotation = Quaternion.identity;
            
            bit.SetColliderActive(false);
            bit.SetAttached(true);

            _carryingBit = bit;
            
            //Set the State to Flee
            SetState(STATE.FLEE);
        }

        private void FleeState()
        {
            //If off screen, destroy bit, then set to pursue state
            if (IsOffScreen(_carryingBit.transform.position))
            {
                _stolenBits++;
                Recycler.Recycle<Bit>(_carryingBit);
                
                ClearTarget();

                //If the Borrower has stolen the last bit off of the bot, then to not harass the player, despawn
                if (_stolenBits > 0 && !LevelManager.Instance.BotInLevel.AttachedBlocks.OfType<Bit>().Any())
                {
                    DestroyEnemy();
                    return;
                }
                
                SetState(STATE.PURSUE);
                return;
            }
            
            //Move away from the Bot at half speed, until off screen
            var currentPosition = (Vector2)transform.position;
            //Away from the player
            var direction = (currentPosition - _playerLocation).normalized;


            currentPosition += direction  * (_carrySpeed * Time.deltaTime);

            MostRecentMovementDirection = GetMovementDirection(currentPosition);

            transform.position = currentPosition;
            
            
        }

        #endregion //States

        protected override void ApplyFleeMotion()
        {
            if (IsOffScreen(transform.position))
                return;
            
            base.ApplyFleeMotion();
        }

        private void DropCarryingBit()
        {
            if (!_carryingBit) 
                return;
            
            _carryingBit.SetAttached(false);
            _carryingBit.collider.enabled = true;
            _carryingBit.transform.parent = null;
            _carryingBit = null;
        }

        private void ClearTarget()
        {
            EnemyManager.SetBorrowerTarget(this, null);
            _attachTarget = null;
            Target = null;
            _carryingBit = null;
        }
        
        private static bool IsOffScreen(in Vector2 pos)
        {
            const float dif = 3 * Constants.gridCellSize;
            
            var screenRect = CameraController.VisibleCameraRect;

            if (pos.y <= screenRect.yMin - dif || pos.y >= screenRect.yMax + dif)
                return true;
                
            if (pos.x <= screenRect.xMin - dif || pos.x >= screenRect.xMax + dif)
                return true;
                    
                
            return false;
        }

        //============================================================================================================//
        
        public override Type GetOverrideType()
        {
            return typeof(BorrowerEnemy);
        }

    }
}
