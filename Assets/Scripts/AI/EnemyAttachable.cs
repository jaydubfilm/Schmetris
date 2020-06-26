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
    public class EnemyAttachable : Enemy
    {
        //============================================================================================================//

        public ENEMY_TYPE Type
        {
            get => _type;
            set => _type = value;
        }
        [SerializeField]
        private ENEMY_TYPE _type;
        public int level { get => _level; set => _level = value; }
        [SerializeField]
        private int _level;

        [SerializeField]
        private LayerMask collisionMask;

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

        private void Start()
        {
            renderer.sprite = m_enemyData.Sprite;
        }

        //============================================================================================================//

        protected void OnCollide(GameObject gameObject, Vector2 hitPoint)
        {
            var bot = gameObject.GetComponent<Bot>();

            if (bot.Rotating)
            {
                Recycling.Recycler.Recycle<Bit>(this.gameObject);
                return;
            }

            TryGetRayDirectionFromBot(bot.MoveDirection, out var rayDirection);

            //Long ray compensates for the players high speed
            var rayLength = Values.gridCellSize * 3f;
            var rayStartPosition = (Vector2)transform.position + -rayDirection * (rayLength / 2f);


            //Checking ray against player layer mask
            var hit = Physics2D.Raycast(rayStartPosition, rayDirection, rayLength, collisionMask.value);

            //If nothing was hit, ray failed, thus no reason to continue
            if (hit.collider == null)
            {
                SSDebug.DrawArrowRay(rayStartPosition, rayDirection * rayLength, Color.yellow);
                return;
            }

            //Here we flip the direction of the ray so that we can tell the Bot where this piece might be added to
            var inDirection = (-(Vector2)m_mostRecentMovementDirection).ToDirection();
            bot.TryAddNewAttachable(this, inDirection, hit.point);
        }

        private bool TryGetRayDirectionFromBot(DIRECTION direction, out Vector2 rayDirection)
        {
            rayDirection = Vector2.zero;
            //Returns the opposite direction based on the current players move direction.
            switch (direction)
            {
                case DIRECTION.NULL:
                    rayDirection = Vector2.down;
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
    }
}