using Recycling;
using StarSalvager.Utilities.Extensions;
using UnityEngine;

namespace StarSalvager.AI
{
    public class EnemyAttachable : Enemy, IAttachable, ICustomRecycle
    {
        public Vector2Int Coordinate { get; set; }
        public bool Attached { get; set; }
        
        public bool CanShift => true;

        //============================================================================================================//

        protected override void Start()
        {
            base.Start();
        }

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
            var bot = gameObject.GetComponent<Bot>();

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
            }*/

            //Here we flip the direction of the ray so that we can tell the Bot where this piece might be added to
            var inDirection = (-(Vector2)m_mostRecentMovementDirection).ToDirection();

            if (inDirection == DIRECTION.NULL)
                inDirection = DIRECTION.UP;
            
            bot.TryAddNewAttachable(this, inDirection, hitPoint);
        }

        public void CustomRecycle(params object[] args)
        {
            SetAttached(false);
        }
    }
}