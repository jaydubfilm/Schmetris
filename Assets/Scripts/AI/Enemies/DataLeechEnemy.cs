﻿using System.Collections;
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

        public override void ProcessMovement(Vector2 playerlocation)
        {
            if (Attached)
            {
                return;
            }

            base.ProcessMovement(playerlocation);
        }

        public override Vector2 GetMovementDirection(Vector2 playerLocation)
        {
            return playerLocation - (Vector2)transform.position;
        }

        #endregion

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