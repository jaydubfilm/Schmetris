using System;
using System.Collections.Generic;
using System.Linq;
using Recycling;
using StarSalvager;
using StarSalvager.Values;
using StarSalvager.Utilities.Debugging;
using StarSalvager.Utilities.Extensions;
using UnityEngine;
using Random = UnityEngine.Random;

namespace StarSalvager
{
    [RequireComponent(typeof(CompositeCollider2D))]
    public class Shape : CollidableBase, IObstacle, ICustomRecycle, ICanBeHit
    {
        //================================================================================================================//

        [SerializeField] private LayerMask collisionMask;

        //================================================================================================================//

        public List<Bit> AttachedBits => attachedBits;
        private List<Bit> attachedBits => _attachedBits ?? (_attachedBits = new List<Bit>());
        private List<Bit> _attachedBits;

        public bool CanMove => true;

        //================================================================================================================//

        protected new Rigidbody2D rigidbody
        {
            get
            {
                if (_rigidbody != null)
                    return _rigidbody;

                _rigidbody = gameObject.GetComponent<Rigidbody2D>();
                return _rigidbody;
            }
        }

        private Rigidbody2D _rigidbody;

        private CompositeCollider2D CompositeCollider => collider as CompositeCollider2D;

        //================================================================================================================//

        /// <summary>
        /// Creates the shape from a list of bits that will make up the body of the shape
        /// </summary>
        /// <param name="bits"></param>
        public void Setup(IEnumerable<Bit> bits)
        {
            foreach (var bit in bits)
            {
                bit.transform.parent = transform;
                bit.transform.localPosition = (Vector2) bit.Coordinate * Constants.gridCellSize;
                attachedBits.Add(bit);
            }

            CompositeCollider.GenerateGeometry();
        }

        //================================================================================================================//

        //This is used for generating a shape, instead of using pre existing Bits
        public void PushNewBit(Bit bit, DIRECTION direction, bool fromRandomExisting)
        {

            var newCoord = direction.ToVector2Int();

            if (attachedBits.Count == 0)
            {
                newCoord = Vector2Int.zero;
            }
            else
            {
                if (fromRandomExisting)
                    newCoord = attachedBits[Random.Range(0, attachedBits.Count)].Coordinate + direction.ToVector2Int();

                attachedBits.FindUnoccupiedCoordinate(direction, ref newCoord);

            }


            bit.Coordinate = newCoord;
            bit.SetAttached(true);
            bit.transform.position = transform.position + (Vector3) (Vector2.one * newCoord * Constants.gridCellSize);
            bit.transform.SetParent(transform);

            attachedBits.Add(bit);

            CompositeCollider.GenerateGeometry();
        }

        public void PushNewBit(Bit bit, Vector2Int coordinate)
        {
            if (attachedBits.Any(b => b.Coordinate == coordinate))
                return;

            bit.Coordinate = coordinate;
            bit.SetAttached(true);
            bit.transform.position = transform.position + (Vector3)(Vector2.one * coordinate * Constants.gridCellSize);
            bit.transform.SetParent(transform);

            attachedBits.Add(bit);

            CompositeCollider.GenerateGeometry();
        }

        //================================================================================================================//

        //TODO Determine if we need to ensure the validity of the shape after removing a piece
        public void DestroyBit(Bit bit)
        {
            attachedBits.Remove(bit);

            bit.SetAttached(false);
            Recycler.Recycle<Bit>(bit.gameObject);

            if (attachedBits.Count > 0)
            {
                CompositeCollider.GenerateGeometry();
                return;
            }

            Recycler.Recycle<Shape>(this);
        }
        public void DestroyBit(Vector2Int coordinate)
        {
            if (!attachedBits.Any(b => b.Coordinate == coordinate))
                return;

            Bit bit = attachedBits.FirstOrDefault(b => b.Coordinate == coordinate);
            
            attachedBits.Remove(bit);

            bit.SetAttached(false);
            Recycler.Recycle<Bit>(bit.gameObject);

            if (attachedBits.Count > 0)
            {
                CompositeCollider.GenerateGeometry();
                return;
            }

            Recycler.Recycle<Shape>(this);
        }

        public override void SetColor(Color color)
        {
            foreach (var bit in attachedBits)
            {
                bit.SetColor(color);
            }
        }

        public override void SetColliderActive(bool state)
        {
            //Its important that I set the children colliders instead of the Composite Collider as it wont reEnable correctly
            //Setting the Bits colliders is the correct way of doing this
            foreach (var bit in attachedBits)
            {
                bit.SetColliderActive(state);
            }

            CompositeCollider.GenerateGeometry();
        }
        
        //================================================================================================================//

        
        public void TryHitAt(Vector2 position, float damage)
        {
            var closestAttachable = attachedBits.GetClosestAttachable(position) as Bit;

            //FIXME Need to see how to fix this
            if (closestAttachable is IHealth closestHealth)
            {
                closestHealth.ChangeHealth(-damage);

                if (closestHealth.CurrentHealth > 0) 
                    return;
            }
            
            DestroyBit(closestAttachable);
        }

        //================================================================================================================//

        protected override void OnCollide(GameObject gameObject, Vector2 hitPoint)
        {
            if (!(gameObject.GetComponent<Bot>() is Bot bot))
                return;

            if (bot.Rotating)
            {
                if (attachedBits[0].Type == BIT_TYPE.BLACK)
                {
                    Recycler.Recycle<Shape>(this);
                    return;
                }

                float rotation = 180.0f;
                if (bot.MostRecentRotate == ROTATION.CW)
                {
                    rotation *= -1;
                }

                bool breakIntoBits = false;
                if (!breakIntoBits)
                {
                    Vector2 direction = (Vector2)transform.position - hitPoint;
                    direction.Normalize();
                    if (direction != Vector2.up)
                    {
                        Vector2 downVelocity = Vector2.down * Constants.gridCellSize / Globals.AsteroidFallTimer;
                        downVelocity.Normalize();
                        direction += downVelocity;
                        direction.Normalize();
                    }
                    LevelManager.Instance.ObstacleManager.BounceObstacle(this, direction, rotation, true, true);
                }
                else
                {
                    foreach (Bit bit in attachedBits)
                    {
                        Vector2 direction = (Vector2)bit.transform.position - hitPoint;
                        direction.Normalize();
                        if (direction != Vector2.up)
                        {
                            Vector2 downVelocity = Vector2.down * Constants.gridCellSize / Globals.AsteroidFallTimer;
                            downVelocity.Normalize();
                            direction += downVelocity;
                            direction.Normalize();
                        }
                        LevelManager.Instance.ObstacleManager.BounceObstacle(bit, direction, rotation, true, true);
                    }
                    Recycler.Recycle<Shape>(this, new
                    {
                        recycleBits = false
                    });
                }
                return;
            }

            //Debug.Break();


            if (!TryGetRayDirectionFromBot(Globals.MovingDirection, out var rayDirection))
                return;


            //Long ray compensates for the players high speed
            var rayLength = Constants.gridCellSize * 3f;
            var closestAttachable = attachedBits.GetClosestAttachable(hitPoint) as IAttachable;

            closestAttachable = attachedBits.GetAttachableInDirection(closestAttachable, rayDirection);


            var rayStartPosition = (Vector2) closestAttachable.transform.position + -rayDirection * (rayLength / 2f);

            //Debug.Log($"Closest {closestAttachable.gameObject.name}", closestAttachable);


            //Checking ray against player layer mask
            var hit = Physics2D.Raycast(rayStartPosition, rayDirection, rayLength, collisionMask.value);

            //If nothing was hit, ray failed, thus no reason to continue
            if (hit.collider == null)
            {
                SSDebug.DrawArrowRay(rayStartPosition, rayDirection * rayLength, Color.yellow);
                return;
            }



            //Here we flip the direction of the ray so that we can tell the Bot where this piece might be added to
            var inDirection = (-rayDirection).ToDirection();
            bot.TryAddNewShape(this, closestAttachable, inDirection, hit.point);
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

        //================================================================================================================//

        private void RecycleBits()
        {
            foreach (var bit in attachedBits)
            {
                Recycler.Recycle<Bit>(bit);
            }
        }

        public void CustomRecycle(params object[] args)
        {
            //by Default, I want to assume I'll be recycling the bits.
            var recycleBits = true;

            foreach (var o in args)
            {
                //Found: https://stackoverflow.com/a/1203538
                var v = o?.GetType().GetProperty(nameof(recycleBits))?.GetValue(o, null);

                if (v == null)
                    continue;

                recycleBits = (bool) v;
            }

            if (recycleBits)
                RecycleBits();

            attachedBits.Clear();
        }

        
    }
}