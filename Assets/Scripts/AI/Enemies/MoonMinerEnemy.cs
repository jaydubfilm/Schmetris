using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.AI
{
    public class MoonMinerEnemy : Enemy
    {
        public override bool IsAttachable => false;
        public override bool IgnoreObstacleAvoidance => true;
        public override bool SpawnHorizontal => true;


        //============================================================================================================//

        #region Movement

        public override void ProcessMovement()
        {
            throw new System.NotImplementedException();
        }

        public override Vector3 GetDestination()
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