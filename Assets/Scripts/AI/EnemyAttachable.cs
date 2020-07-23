using System;
using Recycling;
using StarSalvager.Utilities.Debugging;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Values;
using UnityEngine;

namespace StarSalvager.AI
{
    public class EnemyAttachable : Enemy, IAttachable, ICustomRecycle
    {
        public Vector2Int Coordinate { get; set; }
        public bool Attached { get; set; }
        
        public bool CanShift => true;
        
        [SerializeField]
        private LayerMask collisionMask;

        //============================================================================================================//

        protected override void Update()
        {
            if (!Attached) 
                return;
            
            m_fireTimer += Time.deltaTime;
                
            if (m_fireTimer < 1 / m_enemyData.AttackSpeed)
                return;
                
            m_fireTimer -= 1 / m_enemyData.AttackSpeed;
            LevelManager.Instance.BotGameObject.TryHitAt(LevelManager.Instance.BotGameObject.GetClosestAttachable(Coordinate), m_enemyData.AttackDamage);
        }

        //============================================================================================================//
        
        public void SetAttached(bool isAttached)
        {
            Attached = isAttached;
            collider.usedByComposite = isAttached;
        }

        protected override void OnCollide(GameObject gameObject, Vector2 hitPoint)
        {
            if(Attached)
                return;
            
            /*var bot = gameObject.GetComponent<Bot>();

            if (bot.Rotating)
            {
                Recycling.Recycler.Recycle<EnemyAttachable>(this.gameObject);
                return;
            }

            //If nothing was hit, ray failed, thus no reason to continue
            /*if (hit.collider == null)
            {
                SSDebug.DrawArrowRay(rayStartPosition, rayDirection * rayLength, Color.yellow);
                return;
            }#1#

            //Here we flip the direction of the ray so that we can tell the Bot where this piece might be added to
            var inDirection = (-(Vector2)m_mostRecentMovementDirection).ToDirection();
            

            if (inDirection == DIRECTION.NULL)
                inDirection = DIRECTION.UP;
            
            bot.TryAddNewAttachable(this, inDirection, hitPoint);*/
            //Debug.Break();
            
            
            var bot = gameObject.GetComponent<Bot>();

            if (bot.Rotating)
            {
                Recycler.Recycle<EnemyAttachable>(this);
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

            //Here we flip the direction of the ray so that we can tell the Bot where this piece might be added to
            var inDirection = (-rayDirection).ToDirection();
            bot.TryAddNewAttachable(this, inDirection, hit.point);
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
        
        //IHealth functions
        //============================================================================================================//
        
        public override void ChangeHealth(float amount)
        {
            _currentHealth += amount;
            
            if(_currentHealth <= 0)
                Recycler.Recycle<EnemyAttachable>(this);
        }
        
        //ICustomRecycle functions
        //============================================================================================================//

        public void CustomRecycle(params object[] args)
        {
            SetAttached(false);
        }
    }
}