using System;
using System.Linq;
using Recycling;
using StarSalvager.Audio;
using StarSalvager.Cameras;
using StarSalvager.Factories;
using StarSalvager.Prototype;
using StarSalvager.Utilities.Analytics;
using StarSalvager.Utilities.Particles;
using StarSalvager.Values;
using UnityEngine;

namespace StarSalvager.AI
{
    public class LaserTurretEnemy  : Enemy, IOverrideRecycleType
    {
        public float anticipationTime = 1f;
        public float triggerDistance = 5f;
        
        //====================================================================================================================//
        
        public override bool IgnoreObstacleAvoidance => true;
        public override bool SpawnAboveScreen => true;

        //====================================================================================================================//

        private float _anticipationTime;
        private Vector2 _playerPosition;

        //====================================================================================================================//

        [SerializeField]
        private float damage;
        [SerializeField]
        private LayerMask collisionMask;
        [SerializeField]
        private GameObject beamObject1;
        [SerializeField]
        private GameObject beamObject2;
        [SerializeField]
        private GameObject beamObject3;

        //====================================================================================================================//

        public override void LateInit()
        {
            base.LateInit();

            beamObject1.SetActive(false);
            beamObject2.SetActive(false);
            beamObject3.SetActive(false);

            SetState(STATE.ANTICIPATION );
        }

        //State Functions
        //====================================================================================================================//

        #region State Functions


        protected override void StateChanged(STATE newState)
        {
            switch (newState)
            {
                case STATE.NONE:
                case STATE.ANTICIPATION:
                    break;
                case STATE.ATTACK:
                    beamObject1.SetActive(true);
                    beamObject2.SetActive(true);
                    beamObject3.SetActive(true);
                    break;
                case STATE.DEATH:
                    //Recycle ya boy
                    Recycler.Recycle<LaserTurretEnemy>(this);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override void StateUpdate()
        {
            Vector3 fallAmount = Vector3.up * ((Constants.gridCellSize * Time.deltaTime) / Globals.TimeForAsteroidToFallOneSquare);
            transform.position -= fallAmount;
            m_mostRecentMovementDirection = Vector3.down;

            switch (currentState)
            {
                case STATE.NONE:
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

        protected override void CleanStateData()
        {
            base.CleanStateData();
            _anticipationTime = anticipationTime;
        }

        //====================================================================================================================//

        private void AnticipationState()
        {
            if (!CameraController.IsPointInCameraRect(transform.position, Constants.VISIBLE_GAME_AREA))
                return;

            _anticipationTime -= Time.deltaTime;

            if (_anticipationTime > 0)
            {
                return;
            }

            SetState(STATE.ATTACK);
        }

        private void AttackState()
        {
            if (CameraController.IsPointInCameraRect(new Vector2(0, transform.position.y), Constants.VISIBLE_GAME_AREA))
            {
                SetState(STATE.ANTICIPATION);
                return;
            }
            //--------------------------------------------------------------------------------------------------------//

            Vector2 angle1 = Vector2.down;
            Vector2 angle2 = Quaternion.Euler(0, 0, 115) * Vector2.down;
            Vector2 angle3 = Quaternion.Euler(0, 0, -115) * Vector2.down;
            
            var raycastHit1 = Physics2D.Raycast(transform.position, angle1, 100, collisionMask.value);
            var raycastHit2 = Physics2D.Raycast(transform.position, angle2, 100, collisionMask.value);
            var raycastHit3 = Physics2D.Raycast(transform.position, angle3, 100, collisionMask.value);

            if (raycastHit1.collider != null)
            {
                if (!(raycastHit1.transform.GetComponent<Bot>() is Bot bot))
                    throw new Exception();

                var damageToApply = damage * Time.deltaTime;

                var toHit = bot.GetAttachablesInColumn(raycastHit1.point);
                foreach (var attachable in toHit)
                {
                    bot.TryHitAt(attachable, damageToApply, false);
                }
            }

            if (raycastHit2.collider != null)
            {
                if (!(raycastHit2.transform.GetComponent<Bot>() is Bot bot))
                    throw new Exception();

                var damageToApply = damage * Time.deltaTime;

                var toHit = bot.GetAttachablesInColumn(raycastHit2.point);
                foreach (var attachable in toHit)
                {
                    bot.TryHitAt(attachable, damageToApply, false);
                }
            }

            if (raycastHit3.collider != null)
            {
                if (!(raycastHit3.transform.GetComponent<Bot>() is Bot bot))
                    throw new Exception();

                var damageToApply = damage * Time.deltaTime;

                var toHit = bot.GetAttachablesInColumn(raycastHit3.point);
                foreach (var attachable in toHit)
                {
                    bot.TryHitAt(attachable, damageToApply, false);
                }
            }
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

        public override Type GetOverrideType()
        {
            return typeof(LaserTurretEnemy);
        }

        //====================================================================================================================//
        
    }
}
