﻿using System;
using System.Collections.Generic;
using System.Linq;
using Recycling;
using StarSalvager;
using StarSalvager.Audio;
using StarSalvager.Utilities;
using StarSalvager.Utilities.Animations;
using StarSalvager.Values;
using StarSalvager.Utilities.Debugging;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.Interfaces;
using UnityEngine;
using Random = UnityEngine.Random;

namespace StarSalvager
{
    [RequireComponent(typeof(CompositeCollider2D))]
    public class Shape : CollidableBase, IObstacle, ICanBeHit, IHasBounds
    {
        //================================================================================================================//

        [SerializeField] private LayerMask collisionMask;

        //================================================================================================================//

        public List<Bit> AttachedBits => _attachedBits ?? (_attachedBits = new List<Bit>());
        private List<Bit> _attachedBits;

        public bool CanMove => true;

        public bool IsRegistered { get; set; } = false;

        public bool IsMarkedOnGrid { get; set; } = false;

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
                AttachedBits.Add(bit);
            }

            CompositeCollider.GenerateGeometry();
            gameObject.name = $"{nameof(Shape)}_[{AttachedBits.Count}]";
        }

        //================================================================================================================//

        //This is used for generating a shape, instead of using pre existing Bits
        public void PushNewBit(Bit bit, DIRECTION direction, bool fromRandomExisting)
        {

            var newCoord = direction.ToVector2Int();

            if (AttachedBits.Count == 0)
            {
                newCoord = Vector2Int.zero;
            }
            else
            {
                if (fromRandomExisting)
                    newCoord = AttachedBits[Random.Range(0, AttachedBits.Count)].Coordinate + direction.ToVector2Int();

                AttachedBits.FindUnoccupiedCoordinate(direction, ref newCoord);

            }


            bit.Coordinate = newCoord;
            bit.SetAttached(true);
            bit.transform.position = transform.position + (Vector3) (Vector2.one * newCoord * Constants.gridCellSize);
            bit.transform.SetParent(transform);

            AttachedBits.Add(bit);

            GenerateGeometry();
        }

        public void PushNewBit(Bit bit, Vector2Int coordinate)
        {
            if (AttachedBits.Any(b => b.Coordinate == coordinate))
                return;

            bit.Coordinate = coordinate;
            bit.SetAttached(true);
            bit.transform.position = transform.position + (Vector3)(Vector2.one * coordinate * Constants.gridCellSize);
            bit.transform.SetParent(transform);

            AttachedBits.Add(bit);

            GenerateGeometry();
        }

        public void GenerateGeometry()
        {
            CompositeCollider.GenerateGeometry();
        }

        //====================================================================================================================//
        private FadeSprite[] _fadeSprites;

        public void FlashBits()
        {
            if (_fadeSprites != null && _fadeSprites.Length > 0)
            {
                //TODO Recycle any existing elements
                foreach (var flashSprite in _fadeSprites)
                {
                    Recycler.Recycle<FadeSprite>(flashSprite);
                }
            }

            _fadeSprites = new FadeSprite[AttachedBits.Count];

            for (var i = 0; i < AttachedBits.Count; i++)
            {
                var flashSprite = FadeSprite.Create(
                    transform,
                    (Vector2)AttachedBits[i].Coordinate * Constants.gridCellSize,
                    Color.white);

                _fadeSprites[i] = flashSprite;
            }

        }

        //================================================================================================================//

        //TODO Determine if we need to ensure the validity of the shape after removing a piece
        public void DestroyBit(Bit bit)
        {
            AttachedBits.Remove(bit);

            bit.SetAttached(false);
            Recycler.Recycle<Bit>(bit.gameObject);

            if (AttachedBits.Count > 0)
            {
                CompositeCollider.GenerateGeometry();
                return;
            }

            Recycler.Recycle<Shape>(this);
        }
        public void DestroyBit(Vector2Int coordinate, bool shouldRecycleIfEmpty = true)
        {
            if (AttachedBits.All(b => b.Coordinate != coordinate))
                return;

            Bit bit = AttachedBits.FirstOrDefault(b => b.Coordinate == coordinate);

            AttachedBits.Remove(bit);

            bit.SetAttached(false);
            Recycler.Recycle<Bit>(bit.gameObject);

            if (AttachedBits.Count > 0)
            {
                CompositeCollider.GenerateGeometry();
                return;
            }

            if (!shouldRecycleIfEmpty)
                return;

            Recycler.Recycle<Shape>(this);
        }



        public override void SetColliderActive(bool state)
        {
            //Its important that I set the children colliders instead of the Composite Collider as it wont reEnable correctly
            //Setting the Bits colliders is the correct way of doing this
            foreach (var bit in AttachedBits)
            {
                bit.SetColliderActive(state);
            }

            CompositeCollider.GenerateGeometry();
        }

        //Sprite Renderer Functions
        //====================================================================================================================//

        public override void SetColor(Color color)
        {
            foreach (var bit in AttachedBits)
            {
                bit.SetColor(color);
            }
        }

        public override void SetSortingLayer(string sortingLayerName, int sortingOrder = 0)
        {
            foreach (var attachedBit in AttachedBits)
            {
                attachedBit.SetSortingLayer(sortingLayerName, sortingOrder);
            }
        }

        //================================================================================================================//


        public bool TryHitAt(Vector2 worldPosition, float damage)
        {
            var closestAttachable = AttachedBits.GetClosestAttachable(worldPosition);

            //FIXME Need to see how to fix this
            if (closestAttachable is IHealth closestHealth)
            {
                closestHealth.ChangeHealth(-damage);

                if (closestHealth.CurrentHealth > 0)
                    return true;
            }

            DestroyBit(closestAttachable);

            return true;
        }

        //================================================================================================================//

        protected override void OnCollide(GameObject gameObject, Vector2 worldHitPoint)
        {
            if (!(gameObject.GetComponent<Bot>() is Bot bot))
                return;

            if (bot.Rotating)
            {
                this.Bounce(worldHitPoint, transform.position, bot.MostRecentRotate);

                    AudioController.PlaySound(SOUND.BIT_BOUNCE);
                    return;
            }

            if (!TryGetRayDirectionFromBot(Globals.MovingDirection, out var rayDirection))
                return;


            //Long ray compensates for the players high speed
            var rayLength = Constants.gridCellSize * 3f;
            var closestAttachable = AttachedBits.GetClosestAttachable(worldHitPoint) as IAttachable;

            closestAttachable = AttachedBits.GetAttachableInDirection(closestAttachable, rayDirection);


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

        //================================================================================================================//

        private void RecycleBits()
        {
            foreach (var bit in AttachedBits)
            {
                Recycler.Recycle<Bit>(bit);
            }
        }

        public override void CustomRecycle(params object[] args)
        {
            base.CustomRecycle(args);
            
            if (_fadeSprites != null)
            {
                foreach (var fadeSprite in _fadeSprites)
                {
                    Recycler.Recycle<FadeSprite>(fadeSprite);
                }

                _fadeSprites = null;
            }



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

            AttachedBits.Clear();
        }


        //IHasBounds Functions
        //====================================================================================================================//

        public Bounds GetBounds()
        {
            var maxY = AttachedBits.Max(x => x.Coordinate.y);
            var maxX = AttachedBits.Max(x => x.Coordinate.x);

            var minY = AttachedBits.Min(x => x.Coordinate.y);
            var minX = AttachedBits.Min(x => x.Coordinate.x);

            var size = new Vector2(maxX - minX, maxY - minY) * Constants.gridCellSize;
            size += Vector2.one;

            var centerPosition = AttachedBits.GetCollectionCenterPosition();

            return new Bounds
            {
                center = centerPosition,
                size = size
            };
        }

        //====================================================================================================================//
    }
}
