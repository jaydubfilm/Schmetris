using System;
using StarSalvager.Cameras;
using StarSalvager.Values;
using System.Linq;
using Recycling;
using StarSalvager.Audio;
using StarSalvager.Utilities;
using StarSalvager.Utilities.Analytics;
using StarSalvager.Utilities.Particles;
using UnityEngine;

using Random = UnityEngine.Random;

namespace StarSalvager.AI
{
    public class MoonMinerEnemy : Enemy
    {
        public override bool IsAttachable => false;
        public override bool IgnoreObstacleAvoidance => true;
        public override bool SpawnHorizontal => true;

        private float horizontalFarLeftX;
        private float horizontalFarRightX;
        private float verticalLowestAllowed;

        private Vector2 currentDestination;

        private float m_pauseMovementTimer = 0.0f;
        private bool m_pauseMovement;

        //====================================================================================================================//
        
        private Vector2 _targetLocation;
        private float _anticipationTime;
        private float _attackTime;
        private int _attackCount = 4;

        [SerializeField]
        private float damage;
        [SerializeField]
        private LayerMask collisionMask;
        [SerializeField]
        private GameObject beamObject;

        //====================================================================================================================//
        
        public override void LateInit()
        {
            base.LateInit();
            
            beamObject.SetActive(false);
            
            SetState(STATE.MOVE);
            
            /*currentDestination = transform.position;

            verticalLowestAllowed = 0.5f;
            horizontalFarLeftX = -1 * Constants.gridCellSize * Globals.ColumnsOnScreen / 3.5f;
            horizontalFarRightX = Constants.gridCellSize * Globals.ColumnsOnScreen / 3.5f;*/
        }

        //============================================================================================================//

        public override void ChangeHealth(float amount)
        {
            CurrentHealth += amount;

            if (amount < 0)
            {
                FloatingText.Create($"{Mathf.Abs(amount)}", transform.position, Color.red);
            }

            if (CurrentHealth > 0) 
                return;
            
            LevelManager.Instance.DropLoot(m_enemyData.rdsTable.rdsResult.ToList(), transform.localPosition, true);
            
            SessionDataProcessor.Instance.EnemyKilled(m_enemyData.EnemyType);
            AudioController.PlaySound(SOUND.ENEMY_DEATH);

            LevelManager.Instance.WaveEndSummaryData.AddEnemyKilled(name);
            
            

            LevelManager.Instance.EnemyManager.RemoveEnemy(this);

            Recycler.Recycle<MoonMinerEnemy>(this);
        }

        //====================================================================================================================//
        

        #region Movement

        

        public override void UpdateEnemy(Vector2 playerLocation)
        {
            StateUpdate();
            
            /*if (m_pauseMovement)
            {
                m_pauseMovementTimer -= Time.deltaTime;
                if (m_pauseMovementTimer <= 0.0f)
                {
                    m_pauseMovement = false;
                    m_pauseMovementTimer = 1.0f;
                    FireAttack();
                }
                return;
            }*/

            //base.ProcessState(playerLocation);
        }

        /*protected override Vector2 GetMovementDirection(Vector2 playerLocation)
        {
            if (Vector2.Distance(transform.position, currentDestination) <= 0.1f)
            {
                currentDestination = new Vector2(playerLocation.x + Random.Range(horizontalFarLeftX, horizontalFarRightX),
                    Random.Range(LevelManager.Instance.WorldGrid.m_screenGridCellRange.y * verticalLowestAllowed, LevelManager.Instance.WorldGrid.m_screenGridCellRange.y));

                if (CameraController.IsPointInCameraRect(transform.position, 0.6f))
                {
                    m_pauseMovement = true;
                }
            }

            return currentDestination - (Vector2)transform.position;
        }*/

        #endregion

        //====================================================================================================================//

        #region States

        protected override void StateChanged(STATE newState)
        {
            switch (newState)
            {
                case STATE.NONE:
                    return;
                case STATE.MOVE:
                   
                    _targetLocation = GetNewPosition();
                    break;
                case STATE.FLEE:
                    break;
                case STATE.ANTICIPATION:
                    _anticipationTime = 1f;
                    break;
                case STATE.ATTACK:
                    beamObject.SetActive(true);
                    _attackTime = 2f;
                    break;
                case STATE.DEATH:
                    Recycler.Recycle<MoonMinerEnemy>(this);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
            }
        }

        protected override void StateUpdate()
        {
            switch (currrentState)
            {
                case STATE.NONE:
                case STATE.DEATH:
                    return;
                case STATE.MOVE:
                    MoveState();
                    break;
                case STATE.FLEE:
                    FleeState();
                    break;
                case STATE.ANTICIPATION:
                    AnticipationState();
                    break;
                case STATE.ATTACK:
                    AttackState();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(currrentState), currrentState, null);
            }
        }

        protected override void CleanStateData()
        {
            base.CleanStateData();
            _attackCount = 4;
        }



        private void MoveState()
        {
            //TODO Move towards target position
            var currentPosition = transform.position;
            
            Debug.DrawLine(currentPosition, _targetLocation, Color.cyan);
            
            if (Vector2.Distance(currentPosition, _targetLocation) > 0.1f)
            {
                transform.position = Vector2.MoveTowards(currentPosition, _targetLocation, EnemyMovementSpeed * Time.deltaTime);
                return;
            }
            
            //TODO If within threshold, move to anticipation state
            SetState(STATE.ANTICIPATION);
        }

        private void FleeState()
        {
            //TODO Move off screen
            var currentPosition = transform.position;

            currentPosition += Vector3.up * (EnemyMovementSpeed * Time.deltaTime);

            transform.position = currentPosition;

            if (CameraController.IsPointInCameraRect(currentPosition))
                return;
            
            //TODO When no longer visible, recycle this
            SetState(STATE.DEATH);

        }

        private void AnticipationState()
        {
            //TODO Wait x Seconds
            if (_anticipationTime > 0f)
            {
                _anticipationTime -= Time.deltaTime;
                return;
            }
            
            SetState(STATE.ATTACK);
            //TODO Switch to Attack State
        }

        private void AttackState()
        {
            //--------------------------------------------------------------------------------------------------------//

            var raycastHit = Physics2D.Raycast(transform.position, Vector2.down, 100, collisionMask.value);

            if (raycastHit.collider != null)
            {
                if (!(raycastHit.transform.GetComponent<Bot>() is Bot bot))
                    throw new Exception();

                var damageToApply = damage * Time.deltaTime;

                var toHit = bot.GetAttachablesInColumn(raycastHit.point);
                foreach (var attachable in toHit)
                {
                    bot.TryHitAt(attachable, damageToApply, false);
                }
            }

            //--------------------------------------------------------------------------------------------------------//

            //TODO Wait x Seconds
            if (_attackTime > 0f)
            {
                _attackTime -= Time.deltaTime;
                return;
            }

            beamObject.SetActive(false);

            _attackCount--;

            //TODO Set to MoveState
            SetState(_attackCount == 0 ? STATE.FLEE : STATE.MOVE);
        }

        #endregion //States

        //============================================================================================================//

        /*#region Firing

        protected override void ProcessFireLogic()
        {
            return;
        }

        protected override void FireAttack()
        {
            base.FireAttack();
        }

        #endregion*/

        //====================================================================================================================//

        private static Vector2 GetNewPosition()
        {
            //Used to ensure the CameraVisibleRect is updated
            CameraController.IsPointInCameraRect(Vector2.zero, Constants.VISIBLE_GAME_AREA);
            
            var cameraRect = CameraController.VisibleCameraRect;
            var xBounds = new Vector2(cameraRect.xMin, cameraRect.xMax);
            var yBounds = new Vector2(cameraRect.yMin, cameraRect.yMax);

            return new Vector2
            {
                x = Mathf.Lerp(xBounds.x, xBounds.y, Random.Range(0.3f, 0.7f)),
                y = Mathf.Lerp(yBounds.x, yBounds.y, 0.9f)
            };
        }

        //============================================================================================================//
    }
}