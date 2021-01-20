using System;
using Recycling;
using StarSalvager.Factories;
using StarSalvager.Values;
using UnityEngine;

namespace StarSalvager.AI
{
    public class SensorMineEnemy  : Enemy
    {
        // Start is called before the first frame update
        public override bool IsAttachable => false;
        public override bool IgnoreObstacleAvoidance => true;
        public override bool SpawnHorizontal => true;

        public float Damage;
        public float Radius;

        private Vector2 _playerPosition;

        //State Functions
        //====================================================================================================================//

        #region State Functions

        private float AnticipationWaitTime = 1f;

        protected override void StateChanged(STATE newState)
        {
            switch (newState)
            {
                case STATE.NONE:
                case STATE.SEARCH:
                case STATE.ANTICIPATION:
                    //TODO Change animation to Anticipation Animation
                    break;
                case STATE.ATTACK:
                    var damage = FactoryManager.Instance.GetFactory<MineFactory>().GetMineMaxDamage();
                    var radius = FactoryManager.Instance.GetFactory<MineFactory>().GetMineMaxDistance();
                    //TODO Spawn explosion effect
                    
                    //Do damage to relevant blocks
                    LevelManager.Instance.BotObject.TryAOEDamageFrom(transform.position, radius, damage);
                    SetState(STATE.DEATH);
                    break;
                case STATE.DEATH:
                    //Recycle ya boy
                    Recycler.Recycle<SleeperMineEnemy>(this);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override void StateUpdate()
        {
            Vector3 fallAmount = Vector3.up * ((Constants.gridCellSize * Time.deltaTime) / Globals.TimeForAsteroidToFallOneSquare);
            transform.position -= fallAmount;
            
            switch (currrentState)
            {
                case STATE.NONE:
                case STATE.ATTACK:
                case STATE.DEATH:
                    return;
                case STATE.SEARCH:
                    SearchState();
                    break;
                case STATE.ANTICIPATION:
                    AnticipationState();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override void CleanStateData()
        {
            base.CleanStateData();
            AnticipationWaitTime = 1f;
        }

        //====================================================================================================================//

        private void SearchState()
        {
            var distance = Vector2.Distance(transform.position, _playerPosition);

            if (distance > 5)
                return;
            
            SetState(STATE.ANTICIPATION);
        }
        
        private void AnticipationState()
        {
            if (AnticipationWaitTime > 0f)
            {
                AnticipationWaitTime -= Time.deltaTime;
                return;
            }
            
            SetState(STATE.ATTACK);
        }

        #endregion //State Functions

        //====================================================================================================================//

        #region Movement

        public override void UpdateEnemy(Vector2 playerLocation)
        {
            _playerPosition = playerLocation;
            StateUpdate();
        }
        

        #endregion

        //====================================================================================================================//
        

        public override void CustomRecycle(params object[] args)
        {
            CleanStateData();
            
            base.CustomRecycle(args);
        }

        //============================================================================================================//
    }
}
