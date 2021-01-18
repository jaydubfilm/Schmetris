using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.AI
{
    public class DataLeechEnemy : EnemyAttachable
    {
        public override bool IsAttachable => true;
        public override bool IgnoreObstacleAvoidance => false;
        public override bool SpawnHorizontal => false;


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
            if (!Attached)
            {
                return;
            }

            base.ProcessFireLogic();
        }

        protected override void FireAttack()
        {
            //throw new System.NotImplementedException();
        }

        #endregion

        //============================================================================================================//
    }
}