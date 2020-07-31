using System;
using Recycling;
using StarSalvager.Factories;
using StarSalvager.Utilities.Debugging;
using StarSalvager.Utilities.Enemies;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Values;
using UnityEngine;

namespace StarSalvager.AI
{
    public class EnemyAttachable : Enemy, IAttachable, ICustomRecycle, ICustomRotate
    {
        private static readonly int DEFAULT = Animator.StringToHash("Default");
        private static readonly int ATTACK  = Animator.StringToHash("Attack");

        //============================================================================================================//
        public Vector2Int Coordinate { get; set; }
        public bool Attached { get; set; }

        public bool CountAsConnected => false;
        public bool CanDisconnect => true;
        public bool CanShift => true;

        [SerializeField]
        private LayerMask collisionMask;

        //============================================================================================================//

        private EnemyDecoy _enemyDecoy;

        private Bot attachedBot;
        private IAttachable target;
        private Vector2Int targetCoordinate;
        //private Vector2Int targetDirection;

        //FIXME This needs to be removed
        public GameObject TEST_TARGET;

        protected override void Update()
        {

            if (!Attached)
            {
                if(attachedBot != null )
                    Debug.Break();
                
                return;
            }

            if (LevelManager.Instance.EndWaveState)
            {
                target = null;
                attachedBot.ForceDetach(this);
                return;
            }

            EnsureTargetValidity();

            m_fireTimer += Time.deltaTime;

            if (m_fireTimer < 1 / m_enemyData.AttackSpeed)
                return;

            m_fireTimer -= 1 / m_enemyData.AttackSpeed;

            attachedBot.TryHitAt(target, m_enemyData.AttackDamage);
        }

        //============================================================================================================//

        public void SetAttached(bool isAttached)
        {
            //I can't assume that it will always be attached/Detached,as we need to ensure that the move is legal before setting all the values   
            
            //If the bot is telling us to detach, first we need to make sure we can't take the position of our old target
            //This is determined by whether or not it has a path to the core.
            if (isAttached)
            {
                Attached = true;
                collider.usedByComposite = true;
                StateAnimator.ChangeState(ATTACK);

                if (_enemyDecoy == null)
                    _enemyDecoy = FactoryManager.Instance.GetFactory<EnemyFactory>().CreateEnemyDecoy();
                
                _enemyDecoy.Setup(this, attachedBot.Collider);

                return;
            }
            
            if(TryMoveToTargetPosition())
                return;
            
            if (_enemyDecoy != null)
                _enemyDecoy.Disable();
            
            Attached = false;
            collider.usedByComposite = false;
            StateAnimator.ChangeState(DEFAULT);

            target = null;
            attachedBot = null;
            transform.rotation = Quaternion.identity;
            transform.parent = null;
        }

        protected override void OnCollide(GameObject gameObject, Vector2 hitPoint)
        {
            if (LevelManager.Instance.EndWaveState)
                return;
            
            if(Attached)
                return;

            var bot = gameObject.GetComponent<Bot>();

            if (bot.Rotating)
            {
                //Recycler.Recycle<Bit>(this);
                return;
            }

            var dir = (hitPoint - (Vector2)transform.position).ToVector2Int();

            //Checks to see if the player is moving in the correct direction to bother checking, and if so,
            //return the direction to shoot the ray
            if (!TryGetRayDirectionFromBot(Globals.MovingDirection, out var rayDirection))
                return;

            //Debug.Log($"Direction: {dir}, Ray Direction: {rayDirection}");

            if (dir != rayDirection && dir != Vector2Int.zero)
                return;

            //Long ray compensates for the players high speed
            var rayLength = Constants.gridCellSize * 3f;
            var rayStartPosition = (Vector2) transform.position + -rayDirection * (rayLength / 2f);


            //Checking ray against player layer mask
            var hit = Physics2D.Raycast(rayStartPosition, rayDirection, rayLength,  collisionMask.value);

            //If nothing was hit, ray failed, thus no reason to continue
            if (hit.collider == null)
            {
                Debug.DrawRay(rayStartPosition, rayDirection * rayLength, Color.yellow, 1f);
                SSDebug.DrawArrowRay(rayStartPosition, rayDirection * rayLength, Color.yellow);
                return;
            }

            Debug.DrawRay(hit.point, Vector2.up, Color.red);
            Debug.DrawRay(rayStartPosition, rayDirection * rayLength, Color.green);

            attachedBot = bot;
            
            //Here we flip the direction of the ray so that we can tell the Bot where this piece might be added to
            var inDirection = (-rayDirection).ToDirection();
            var attached = bot.TryAddNewAttachable(this, inDirection, hit.point);

            if (!attached)
            {
                attachedBot = null;
                return;
            }

            TryUpdateTarget();
        }

        //============================================================================================================//

        protected override bool TryGetRayDirectionFromBot(DIRECTION direction, out Vector2 rayDirection)
        {
            rayDirection = Vector2.zero;
            //Returns the opposite direction based on the current players move direction.
            switch (direction)
            {
                case DIRECTION.NULL:
                    rayDirection = new Vector2(
                        Mathf.RoundToInt(m_mostRecentMovementDirection.x),
                        Mathf.RoundToInt(m_mostRecentMovementDirection.y));//-(Vector2)m_mostRecentMovementDirection;

                    if(Mathf.Abs(rayDirection.x) > Mathf.Abs(rayDirection.y))
                        rayDirection *= Vector2.right;
                    else
                        rayDirection *= Vector2.up;

                    return true;
                case DIRECTION.LEFT:
                    rayDirection = Vector2.right;
                    return true;
                case DIRECTION.RIGHT:
                    rayDirection = Vector2.left;
                    return true;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

        //Attachable Enemy Movement when Attacking
        //============================================================================================================//

        private void EnsureTargetValidity()
        {
            //If our target has been destroyed (Killed/Recycled) we want to move to its position
            //This would occur if this wasn't attempted to be detached,
            //meaning it was sitting in a legal position that didn't require it to be detached
            //FIXME This may be an issue with those attached to shapes that get detached?
            if (target is IRecycled recyclable && recyclable.IsRecycled)
            {
                var health = target as IHealth;
                
                //Here I can assume that a Bit with no health was destroyed, and thus I can move into its position
                if (health?.CurrentHealth <= 0)
                {
                    if (TryMoveToTargetPosition())
                        return;
                }
                //If the Bit was recycled with a health above 0, I can assume that it was done because of a combo, 
                //and the enemy should try and find a new target relative to its current position
                else if (health?.CurrentHealth > 0)
                {
                    if(TryUpdateTarget())
                        return;
                }

                target = null;
                attachedBot.ForceDetach(this);
                return;
            }

            //Here we're making sure that the target is still part of what we're attacking
            if (target.transform.parent != transform.parent)
            {
                target = null;
                attachedBot.ForceDetach(this);
                return;
            }
            //TODO Need to account for bits that move due to combo solve
            

            //if (targetDirection != (this.Coordinate - target.Coordinate))
            //{
            //    RotateTowardsTarget(target);
            //    return;
            //}

            /*if (attackFromCoordinate != this.Coordinate)
            {
                UpdateTarget();
                return;
            }*/

            /*//We also want to make sure that we aren't currently targeting something that we shouldn't be
            if (target.Attached == false)
            {
                target = null;
                attachedBot.ForceDetach(this);
                return;
            }*/
            
            if (target is EnemyAttachable /*|| target.Attached == false*/)
            {
                TryUpdateTarget();
                return;
            }
        }



        private bool TryMoveToTargetPosition()
        {
            if (target == null)
                return false;

            //If the enemy didn't kill the bit, we shouldn't more to its position
            if (!DidIDestroyBit())
                return false;

            if (!attachedBot.CoordinateHasPathToCore(target.Coordinate))
                return false;

            if (attachedBot.CoordinateOccupied(target.Coordinate))
                return false;

            if (!attachedBot.TryAttachNewBit(target.Coordinate, this, false, true, false))
                return false;

            if (!TryUpdateTarget())
                return false;
            
            return true;
        }

        private bool TryUpdateTarget()
        {
            //We set the max distance here because we want to ensure we're attacking something right next to us
            target = attachedBot.GetClosestAttachable(Coordinate, 1f);

            if (target == null)
            {
                SetAttached(false);
                return false;
            }

            TEST_TARGET = target.gameObject;

            Debug.Log($"{gameObject.name} has new target. TARGET : {TEST_TARGET.gameObject.name}", TEST_TARGET);

            RotateTowardsTarget(target);

            return true;
        }
        
        private bool DidIDestroyBit()
        {
            var health = target as IHealth;
            var recyclable = target as IRecycled; 
            
            if (health?.CurrentHealth > 0)
                return false;
            
            return target.Attached  || !recyclable.IsRecycled;
        }

        //IHealth functions
        //============================================================================================================//

        public override void ChangeHealth(float amount)
        {
            _currentHealth += amount;

            if(_currentHealth <= 0)
                Recycler.Recycle<EnemyAttachable>(this);
        }

        //ICustomRotate functions
        //============================================================================================================//

        public void CustomRotate()
        {
            //We don't want to rotate the Attachable enemy because they need to face specific directions to
            //indicate their attack direction
        }

        private void RotateTowardsTarget(IAttachable Target)
        {
            if (target == null)
                return;
            
            var dir = (Target.Coordinate - Coordinate).ToDirection();
            var AddRotation = Vector3.zero;

            switch (dir)
            {
                case DIRECTION.LEFT:
                    AddRotation = Vector3.forward * 270f;
                    break;
                case DIRECTION.UP:
                    AddRotation = Vector3.forward * 180f;
                    break;
                case DIRECTION.RIGHT:
                    AddRotation = Vector3.forward * 90f;
                    break;
                case DIRECTION.DOWN:
                    AddRotation = Vector3.zero;
                    break;
                default:
                    dir = (-Coordinate).ToDirection();
                    break;
            }

            Debug.Log($"{gameObject.name} Rotate to Direction: {dir}", gameObject);

            transform.rotation = Quaternion.Euler(AddRotation);
        }

        //ICustomRecycle functions
        //============================================================================================================//

        public void CustomRecycle(params object[] args)
        {
            _enemyDecoy = null;
            attachedBot = null;
            target = null;
            SetAttached(false);
        }

        //============================================================================================================//

    }
}
