using StarSalvager.Constants;
using StarSalvager.Utilities.Debugging;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JsonDataTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.AI
{
    public class EnemyAttachable : Enemy, IAttachable
    {
        public Vector2Int Coordinate { get; set; }
        public bool Attached { get; set; }

        public bool CanShift => true;

        //============================================================================================================//

        protected new Transform transform
        {
            get
            {
                if (m_transform == null)
                    m_transform = gameObject.GetComponent<Transform>();

                return m_transform;
            }
        }
        private Transform m_transform;

        protected new SpriteRenderer renderer
        {
            get
            {
                if (m_spriteRenderer == null)
                    m_spriteRenderer = gameObject.GetComponent<SpriteRenderer>();

                return m_spriteRenderer;
            }
        }
        private SpriteRenderer m_spriteRenderer;

        

        public void SetAttached(bool isAttached)
        {
            Attached = isAttached;
            collider.usedByComposite = isAttached;
        }

        [SerializeField]
        private LayerMask collisionMask;

        //============================================================================================================//

        private void Start()
        {
            renderer.sprite = m_enemyData.Sprite;
        }

        private void Update()
        {
            if (Attached)
            {
                m_fireTimer += Time.deltaTime;
                if (m_fireTimer >= 1 / m_enemyData.AttackSpeed)
                {
                    m_fireTimer -= 1 / m_enemyData.AttackSpeed;
                    LevelManager.Instance.BotGameObject.TryHitAt(LevelManager.Instance.BotGameObject.GetClosestAttachable(Coordinate), m_enemyData.AttackDamage);
                }
            }
        }

        //============================================================================================================//

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
    }
}