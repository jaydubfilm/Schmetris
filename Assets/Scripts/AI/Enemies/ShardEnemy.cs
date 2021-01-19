using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.AI
{
    public class ShardEnemy : Enemy
    {
        public override bool IsAttachable => true;
        public override bool IgnoreObstacleAvoidance => true;
        public override bool SpawnHorizontal => false;

        //============================================================================================================//

        #region Movement

        public override Vector2 GetMovementDirection(Vector2 playerLocation)
        {
            return playerLocation - (Vector2)transform.position;
        }

        #endregion

        //============================================================================================================//

        #region Firing

        protected override void ProcessFireLogic()
        {
            base.ProcessFireLogic();
        }

        protected override void FireAttack()
        {
            
        }

        #endregion

        //============================================================================================================//
    }
}