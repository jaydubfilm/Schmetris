using System;
using Recycling;
using StarSalvager.Values;
using UnityEngine;

namespace StarSalvager.AI
{
    public class DataLeechEnemy : EnemyAttachable
    {
        public override bool IgnoreObstacleAvoidance => true;
        public override bool SpawnAboveScreen => true;


        private int m_dataLeechDamage = 1;

        private Vector2 _playerLocation;

        public override void LateInit()
        {
            base.LateInit();
            SetState(STATE.PURSUE);
        }

        //====================================================================================================================//

        #region EnemyAttachable Overrides

        public override void SetAttached(bool isAttached)
        {
            base.SetAttached(isAttached);

            if (currentState != STATE.IDLE)
            {
                SetState(Attached ? STATE.ATTACK : STATE.PURSUE);
            }
        }

        /*public override void ChangeHealth(float amount)
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
            DropLoot();

            SessionDataProcessor.Instance.EnemyKilled(m_enemyData.EnemyType);
            AudioController.PlaySound(SOUND.ENEMY_DEATH);

            LevelManager.Instance.WaveEndSummaryData.AddEnemyKilled(name);
            LevelManager.Instance.EnemyManager.RemoveEnemy(this);
            
            SetState(STATE.DEATH);
        }*/

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

            //m_mostRecentMovementDirection = GetMovementDirection(_playerLocation);
            
            StateUpdate();
        }

        protected override Vector2 GetMovementDirection(Vector2 playerLocation)
        {
            return playerLocation - (Vector2)transform.position;
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
                    break;
                case STATE.ATTACK:
                    m_mostRecentMovementDirection = Vector3.zero;
                    break;
                case STATE.DEATH:
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
                    break;
                case STATE.IDLE:
                    IdleState();
                    break;
                case STATE.PURSUE:
                    PursueState();
                    break;
                case STATE.ATTACK:
                    AttackState();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(currentState), currentState, null);
            }
        }

        private void IdleState()
        {
            Vector3 fallAmount = Vector3.up * ((Constants.gridCellSize * Time.deltaTime) / Globals.TimeForAsteroidToFallOneSquare);
            transform.position -= fallAmount;
        }

        private void PursueState()
        {
            var currentPosition = transform.position;
            currentPosition = Vector3.MoveTowards(currentPosition, _playerLocation,
                m_enemyData.MovementSpeed * Time.deltaTime);

            m_mostRecentMovementDirection = GetMovementDirection(currentPosition);

            transform.position = currentPosition;
        }

        private void AttackState()
        {
            //TODO Once attached, attack the player
            EnsureTargetValidity();

            m_fireTimer += Time.deltaTime;

            if (m_fireTimer < 1 / m_enemyData.RateOfFire)
                return;

            m_fireTimer -= 1 / m_enemyData.RateOfFire;
            
            FireAttack();
        }

        #endregion //States

        //============================================================================================================//

        #region Firing

        protected override void FireAttack()
        {
            if (!AttachedBot || Target == null || !Attached || Disabled)
            {
                return;
            }

            AttachedBot.TryHitAt(Target, m_dataLeechDamage);
        }

        #endregion

        //============================================================================================================//
        public override Type GetOverrideType()
        {
            return typeof(DataLeechEnemy);
        }
    }
}