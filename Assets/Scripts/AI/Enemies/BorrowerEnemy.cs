using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Recycling;
using StarSalvager.Audio;
using StarSalvager.Cameras;
using StarSalvager.Utilities.Analytics;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.Particles;
using StarSalvager.Values;
using UnityEngine;
using UnityEngine.Serialization;

namespace StarSalvager.AI
{
    public class BorrowerEnemy  : EnemyAttachable
    {
        public float anticipationTime = 1f;

        //====================================================================================================================//
        
        public override bool IsAttachable => true;
        public override bool IgnoreObstacleAvoidance => true;
        public override bool SpawnHorizontal => false;


        //====================================================================================================================//
        
        private float _anticipationTime;
        private Vector2 _playerLocation;
        private Bit _carryingBit;
        private Bit _attachTarget;

        public override void LateInit()
        {
            base.LateInit();
            SetState(STATE.PURSUE);
        }

        //====================================================================================================================//

        #region EnemyAttachable Overrides

        protected override void OnCollide(GameObject gameObject, Vector2 worldHitPoint)
        {
            if (currrentState != STATE.PURSUE)
                return;
            
            base.OnCollide(gameObject, worldHitPoint);
        }

        public override void SetAttached(bool isAttached)
        {
            base.SetAttached(isAttached);
            
            if (currrentState != STATE.PURSUE)
                return;
            
            SetState(Attached ? STATE.ANTICIPATION : STATE.PURSUE);
        }

        public override void ChangeHealth(float amount)
        {
            CurrentHealth += amount;
            
            if (amount < 0)
            {
                FloatingText.Create($"{Mathf.Abs(amount)}", transform.position, Color.red);
            }

            if (CurrentHealth > 0)
                return;

            if (AttachedBot)
            {
                AttachedBot.ForceDetach(this);
                AttachedBot = null;
            }
            
            transform.parent = LevelManager.Instance.ObstacleManager.WorldElementsRoot;
            LevelManager.Instance.DropLoot(m_enemyData.rdsTable.rdsResult.ToList(), transform.localPosition, true);

            SessionDataProcessor.Instance.EnemyKilled(m_enemyData.EnemyType);
            AudioController.PlaySound(SOUND.ENEMY_DEATH);

            LevelManager.Instance.WaveEndSummaryData.AddEnemyKilled(name);
            LevelManager.Instance.EnemyManager.RemoveEnemy(this);
            
            SetState(STATE.DEATH);
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

            m_mostRecentMovementDirection = GetMovementDirection(_playerLocation);
            
            StateUpdate();
        }

        protected override Vector2 GetMovementDirection(Vector2 playerLocation)
        {
            return playerLocation - (Vector2)transform.position;
        }

        private Bit FindClosestBitOnBot()
        {
            var bot = LevelManager.Instance.BotInLevel;
            var bits = bot.attachedBlocks.OfType<Bit>().ToArray();

            if (bits.IsNullOrEmpty())
                return null;

            var currentPosition = transform.position;

            var minDist = 999f;
            Bit selectedBit = null;
            
            foreach (var bit in bits)
            {
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
                    break;
                case STATE.PURSUE:
                    //Try to Find a Bit on the bot
                    _attachTarget = FindClosestBitOnBot();
                    break;
                case STATE.ANTICIPATION:
                    _anticipationTime = anticipationTime;
                    break;
                case STATE.ATTACK:
                    break;
                case STATE.FLEE:
                    Target = null;
                    AttachedBot?.ForceDetach(this);
                    AttachedBot = null;
                    break;
                case STATE.DEATH:
                    //Drop the attached bit
                    if (_carryingBit)
                    {
                        _carryingBit.SetAttached(false);
                        _carryingBit.collider.enabled = true;
                        _carryingBit.transform.parent = null;
                        _carryingBit = null;
                    }
                    
                    Recycler.Recycle<DataLeechEnemy>(this);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
            }
        }

        protected override void StateUpdate()
        {
            switch (currrentState)
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
                    throw new ArgumentOutOfRangeException(nameof(currrentState), currrentState, null);
            }
        }

        //====================================================================================================================//
        
        private void IdleState()
        {
            Vector3 fallAmount = Vector3.up * ((Constants.gridCellSize * Time.deltaTime) / Globals.TimeForAsteroidToFallOneSquare);
            transform.position -= fallAmount;
        }

        private void PursueState()
        {
            //IF there is not a bit on the bot,
            if (_attachTarget is null)
            {
                _attachTarget = FindClosestBitOnBot();
                return;
            }
            
            //Fly towards a specific Bit on the bot
            var currentPosition = transform.position;
            var targetPosition = _attachTarget.transform.position;

            currentPosition = Vector3.MoveTowards(currentPosition, targetPosition,
                m_enemyData.MovementSpeed * Time.deltaTime);


            transform.position = currentPosition;
        }

        private void AnticipationState()
        {
            //After wait time, move to attack state
            if (_anticipationTime > 0)
            {
                _anticipationTime -= Time.deltaTime;
                return;
            }
            
            SetState(STATE.ATTACK);
        }

        private void AttackState()
        {
            if (!(Target is Bit bit))
                return;
                
            //Detach Bit from Bot
            AttachedBot.ForceDetach(bit);
            
            //Set Bit Parent to this object & Disable the collider
            bit.transform.SetParent(transform, false);
            bit.transform.localPosition = Vector3.down;
            
            bit.SetColliderActive(false);
            bit.SetAttached(true);

            _carryingBit = bit;
            
            //Set the State to Flee
            SetState(STATE.FLEE);
        }

        private void FleeState()
        {
            //If off screen, destroy bit, then set to pursue state
            if (!CameraController.IsPointInCameraRect(_carryingBit.transform.position))
            {
                Recycler.Recycle<Bit>(_carryingBit);
                _carryingBit = null;
                
                SetState(STATE.PURSUE);
                return;
            }
            
            //Move away from the Bot at half speed, until off screen
            var currentPosition = (Vector2)transform.position;
            //Away from the player
            var direction = (currentPosition - _playerLocation).normalized;
            var carrySpeed = EnemyMovementSpeed / 2f;
            
            currentPosition += direction  * (carrySpeed * Time.deltaTime);
            
            transform.position = currentPosition;
            
            
        }

        #endregion //States
        

        //============================================================================================================//
        
        public override Type GetOverrideType()
        {
            return typeof(BorrowerEnemy);
        }
    }
}
