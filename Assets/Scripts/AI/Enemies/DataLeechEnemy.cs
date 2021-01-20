using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.AI
{
    public class DataLeechEnemy : EnemyAttachable
    {
        public override bool IsAttachable => true;
        public override bool IgnoreObstacleAvoidance => true;
        public override bool SpawnHorizontal => false;


        private int m_dataLeechDamage = 1;

        //============================================================================================================//

        #region Movement

        public override void UpdateEnemy(Vector2 playerlocation)
        {
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
            throw new System.NotImplementedException();
        }

        protected override void StateUpdate()
        {
            throw new System.NotImplementedException();
        }

        #endregion //States

        //============================================================================================================//

        #region Firing



        protected override void ProcessFireLogic()
        {
            if (!_attachedBot || !Attached || Disabled)
            {
                return;
            }

            base.ProcessFireLogic();
        }

        protected override void FireAttack()
        {
            if (!_attachedBot || _target == null || !Attached || Disabled)
            {
                return;
            }

            _attachedBot.TryHitAt(_target, m_dataLeechDamage);
        }

        #endregion

        //============================================================================================================//
    }
}