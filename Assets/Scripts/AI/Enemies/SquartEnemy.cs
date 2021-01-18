using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.AI
{
    public class SquartEnemy : Enemy
    {
        public override bool IsAttachable => false;
        public override bool IgnoreObstacleAvoidance => true;
        public override bool SpawnHorizontal => true;


        //============================================================================================================//

        #region Movement

        public override void ProcessMovement(Vector2 playerlocation)
        {
            throw new System.NotImplementedException();
        }

        public override Vector2 GetMovementDirection(Vector2 playerLocation)
        {
            throw new System.NotImplementedException();
        }

        #endregion

        //============================================================================================================//

        #region Firing

        protected override void ProcessFireLogic()
        {
            throw new System.NotImplementedException();
        }

        protected override void FireAttack()
        {
            throw new System.NotImplementedException();
        }

        #endregion

        //============================================================================================================//
    }
}