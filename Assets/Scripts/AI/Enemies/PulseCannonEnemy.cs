using System;
using System.Collections;
using System.Collections.Generic;
using Recycling;
using UnityEngine;

namespace StarSalvager.AI
{
    public class PulseCannonEnemy : Enemy
    {

        public override bool IgnoreObstacleAvoidance => true;
        public override bool SpawnAboveScreen => true;

        private Vector2 _playerPosition;

        //[SerializeField]
        private float aimRotation = -45;

        private bool _flipped;

        //====================================================================================================================//

        public override void LateInit()
        {
            base.LateInit();
            
            //TODO Need to choose a facing direction (Default is left Facing)
            
            SetState(STATE.IDLE);
        }


        //====================================================================================================================//
        
        protected override void StateChanged(STATE newState)
        {
            switch (newState)
            {
                case STATE.NONE:
                    return;
                case STATE.IDLE:
                    break;
                case STATE.ANTICIPATION:
                    break;
                case STATE.ATTACK:
                    break;
                case STATE.DEATH:
                    Recycler.Recycle<LaserTurretEnemy>(this);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
            }
        }

        protected override void StateUpdate()
        {
            ApplyFallMotion();

            Debug.DrawRay(transform.position, Quaternion.Euler(0,0,-45 * (_flipped ? -1f : 1f)) * Vector3.down, Color.cyan);
            
            switch (currentState)
            {
                case STATE.NONE:
                    return;
                case STATE.IDLE:
                    IdleState();
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

        //====================================================================================================================//

        private void IdleState()
        {
            
            //TODO Determine if the player is in sight
        }

        private void AnticipationState()
        {
            //TODO Charge up the cannon
        }

        private void AttackState()
        {
            //TODO Fire a burst 
        }

        //====================================================================================================================//
        
        public override void UpdateEnemy(Vector2 playerLocation)
        {
            _playerPosition = playerLocation;
            StateUpdate();
        }

        //====================================================================================================================//
        
        public override Type GetOverrideType()
        {
            return typeof(PulseCannonEnemy);
        }
    }
}
