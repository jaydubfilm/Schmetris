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
    public class EnemyAttachable : AttachableBase, IEnemy
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

        public EnemyData EnemyData { get { return m_enemyData; } set { m_enemyData = value; } }
        private EnemyData m_enemyData;
        public GameObject GameObject { get { return gameObject; } }

        private float m_oscillationTimer = 0;
        private Vector3 m_currentHorizontalMovementDirection = Vector3.right;
        private float m_horizontalMovementYLevel;
        private float horizontalFarLeftX = 0;
        private float horizontalFarRightX = Values.gridSizeX * Values.gridCellSize;
        private Vector3 m_mostRecentMovementDirection = Vector3.zero;

        //============================================================================================================//

        public void ProcessMovement(Vector3 direction)
        {
            m_mostRecentMovementDirection = direction;
            transform.position = transform.position + (direction * m_enemyData.MovementSpeed * Time.deltaTime);
        }

        //Check what movement type is being used, and use the appropriate method to calculate what my current destination is
        public Vector3 GetDestination()
        {
            //Movement styles are based on the player location. For now, hardcode this
            Vector3 playerLocation = LevelManager.Instance.BotGameObject != null ? LevelManager.Instance.BotGameObject.transform.position : Vector3.right * 50;

            switch (m_enemyData.MovementType)
            {
                case ENEMY_MOVETYPE.Standard:
                    return playerLocation;
                case ENEMY_MOVETYPE.Oscillate:
                    //Find destination by rotating the playerLocation around the enemy position, at the angle output by the oscillate function
                    return GetDestinationForRotatePositionAroundPivot(playerLocation, transform.position, GetAngleInOscillation());
                case ENEMY_MOVETYPE.OscillateHorizontal:
                    //Find destination by determining whether to move left or right and then oscillating at the angle output by the oscillate function
                    return GetDestinationForRotatePositionAroundPivot(transform.position + SetHorizontalDirection(), transform.position, GetAngleInOscillation());
                case ENEMY_MOVETYPE.Orbit:
                    //If outside the orbit radius, move towards the player location. If inside it, get the destination along the edge of the circle to move clockwise around it
                    float distanceSqr = Vector2.SqrMagnitude(transform.position - playerLocation);
                    if (distanceSqr > m_enemyData.OrbitRadiusSqr)
                    {
                        return playerLocation;
                    }
                    else
                    {
                        return GetDestinationForRotatePositionAroundPivotAtDistance(transform.position, playerLocation, Vector3.forward * -5, m_enemyData.OrbitRadius);
                    }
                case ENEMY_MOVETYPE.Horizontal:
                    return transform.position + SetHorizontalDirection();
                case ENEMY_MOVETYPE.HorizontalDescend:
                    return transform.position + SetHorizontalDirection(true);
                case ENEMY_MOVETYPE.Down:
                    return transform.position + Vector3.down;

            }

            return playerLocation;
        }

        //Determine whether this horizontal mover is going left or right
        public Vector3 SetHorizontalDirection(bool isDescending = false)
        {
            if (transform.position.x <= horizontalFarLeftX && m_currentHorizontalMovementDirection != Vector3.right)
            {
                m_currentHorizontalMovementDirection = Vector3.right;
                if (isDescending)
                {
                    m_horizontalMovementYLevel -= Values.gridCellSize * m_enemyData.NumberCellsDescend;
                }
            }
            else if (transform.position.x >= horizontalFarRightX && m_currentHorizontalMovementDirection != Vector3.left)
            {
                m_currentHorizontalMovementDirection = Vector3.left;
                if (isDescending)
                {
                    m_horizontalMovementYLevel -= Values.gridCellSize * m_enemyData.NumberCellsDescend;
                }
            }

            //Modify the vertical level back to the stored horizontalYlevel, so enemies will return to their previous y level after avoiding an obstacle
            //TODO - this logic should apply to oscillatehorizontal but currently causes a bug with it. Resolve bug and add this functionality back
            Vector3 addedVertical = Vector3.zero;
            if (m_enemyData.MovementType != ENEMY_MOVETYPE.OscillateHorizontal)
            {
                addedVertical += Vector3.up * (m_horizontalMovementYLevel - transform.position.y);
            }

            return m_currentHorizontalMovementDirection + addedVertical;
        }

        //Calculate the angle to move at for the oscillation movement
        //Methodology - uses a timer, with the value of the timer modification by the oscillationspersecond value, to see where we are in the zig zag cycle. 
        //if the modular is 1, 0 is at the far left end of the cycle, 0.5 is at the far right end, 1 goes back to left
        public Vector3 GetAngleInOscillation()
        {
            m_oscillationTimer += Time.deltaTime * m_enemyData.OscillationsPerSecond;

            if (m_oscillationTimer > 1)
            {
                m_oscillationTimer -= 1;
            }

            if (m_oscillationTimer <= 0.5f)
            {
                float angleAdjust = m_oscillationTimer * 2;
                return Vector3.forward * (m_enemyData.OscillationAngleRange * (-0.5f + angleAdjust));
            }
            else
            {
                float angleAdjust = (m_oscillationTimer - 0.5f) * 2;
                return Vector3.forward * (m_enemyData.OscillationAngleRange * (0.5f - angleAdjust));
            }
        }

        //Rotate point around pivot by angles amount
        public Vector3 GetDestinationForRotatePositionAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
        {
            Vector3 direction = point - pivot;
            direction = Quaternion.Euler(angles) * direction;
            return (direction + pivot);
        }

        //Rotate point around pivot by angles amount, while ensuring that the point is a certain distance away from the pivot. Used for the orbit calculations to keep them orbiting on the outside
        public Vector3 GetDestinationForRotatePositionAroundPivotAtDistance(Vector3 point, Vector3 pivot, Vector3 angles, float distance)
        {
            Vector3 direction = point - pivot;
            direction.Normalize();
            direction *= distance;
            direction = Quaternion.Euler(angles) * direction;
            return (direction + pivot);
        }

        //============================================================================================================//

        public override BlockData ToBlockData()
        {
            return new BlockData
            {
                ClassType = GetType().Name,
                Coordinate = Coordinate,
                Type = (int)Type,
                Level = level
            };
        }

        public override void LoadBlockData(BlockData blockData)
        {
            Coordinate = blockData.Coordinate;
            Type = (ENEMY_TYPE)blockData.Type;
            level = blockData.Level;
        }

        protected override void OnCollide(GameObject gameObject, Vector2 hitPoint)
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