using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using StarSalvager.Utilities.Extensions;
using UnityEngine;

namespace StarSalvager
{
    [Serializable]
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public abstract class BotBase : MonoBehaviour, IHealth, ICanBeHit
    {
        public List<IAttachable> AttachedBlocks => _attachedBlocks ?? (_attachedBlocks = new List<IAttachable>());

        [SerializeField, ReadOnly, Space(10f), ShowInInspector]
        private List<IAttachable> _attachedBlocks;

        public Collider2D Collider => CompositeCollider2D;
        protected CompositeCollider2D CompositeCollider2D
        {
            get
            {
                if (!_compositeCollider2D)
                    _compositeCollider2D = GetComponent<CompositeCollider2D>();

                return _compositeCollider2D;
            }
        }
        private CompositeCollider2D _compositeCollider2D;
        
        protected new Rigidbody2D rigidbody
        {
            get
            {
                if (!_rigidbody)
                    _rigidbody = GetComponent<Rigidbody2D>();

                return _rigidbody;
            }
        }
        private Rigidbody2D _rigidbody;
        

        public abstract bool Rotating { get; }

        //IHealth Properties
        //====================================================================================================================//

        public float StartingHealth { get; protected set; }
        public float CurrentHealth { get; protected set; }

        //IBot Functions
        //====================================================================================================================//

        public abstract bool TryAddNewAttachable(IAttachable attachable, DIRECTION connectionDirection,
            Vector2 collisionPoint);

        public abstract void ForceDetach(ICanDetach canDetach);

        public bool CoordinateHasPathToCore(Vector2Int coordinate)
        {
            return AttachedBlocks.HasPathToCore(coordinate);
        }

        public bool CoordinateOccupied(Vector2Int coordinate)
        {
            return _attachedBlocks.Any(x => x.Coordinate == coordinate /*&& !(x is Part part && part.Destroyed)*/);
        }

        public virtual bool TryAttachNewBlock(Vector2Int coordinate, IAttachable newAttachable,
            bool checkForCombo = true,
            bool updateColliderGeometry = true, bool updatePartList = true)
        {
            throw new System.NotImplementedException();
        }

        public abstract void AttachAttachableToExisting(IAttachable newAttachable, IAttachable existingAttachable,
            DIRECTION direction,
            bool checkForCombo = true, bool updateColliderGeometry = true, bool checkMagnet = true,
            bool playSound = true,
            bool updatePartList = true);

        public void AttachToClosestAvailableCoordinate(Vector2Int coordinate, IAttachable newAttachable,
            DIRECTION desiredDirection,
            bool checkForCombo, bool updateColliderGeometry)
        {
            
            var directions = new[]
            {
                //Cardinal Directions
                Vector2Int.left,
                Vector2Int.up,
                Vector2Int.right,
                Vector2Int.down,

                //Corners
                new Vector2Int(-1,-1),
                new Vector2Int(-1,1),
                new Vector2Int(1,-1),
                new Vector2Int(1,1),
            };

            var avoid = desiredDirection.Reflected().ToVector2Int();

            var dist = 1;
            while (true)
            {
                for (var i = 0; i < directions.Length; i++)
                {

                    var check = coordinate + (directions[i] * dist);
                    if (AttachedBlocks.Any(x => x.Coordinate == check))
                        continue;

                    //We need to make sure that the piece wont be floating
                    if (!AttachedBlocks.HasPathToCore(check))
                        continue;
                    //Debug.Log($"Found available location for {newAttachable.gameObject.name}\n{coordinate} + ({directions[i]} * {dist}) = {check}");
                    AttachNewBlock(check, newAttachable, checkForCombo, updateColliderGeometry);
                    return;
                }

                if (dist++ > 10)
                    break;

            }
        }

        public abstract void AttachNewBlock(Vector2Int coordinate, IAttachable newAttachable, bool checkForCombo = true,
            bool updateColliderGeometry = true, bool checkMagnet = true, bool playSound = true,
            bool updatePartList = true);

        public abstract IAttachable GetClosestAttachable(Vector2Int checkCoordinate, float maxDistance = 999);

        public abstract void TryHitAt(IAttachable closestAttachable, float damage, bool withSound = true);
        
        protected DIRECTION GetAvailableConnectionDirection(Vector2Int existingAttachableCoordinate, DIRECTION direction)
        {
            var coordinate = existingAttachableCoordinate + direction.ToVector2Int();
            //Checks for attempts to add attachable to occupied location
            if (!AttachedBlocks.Any(a => a.Coordinate == coordinate))
            {
                return direction;
            }

            coordinate = existingAttachableCoordinate + DIRECTION.UP.ToVector2Int();
            //Checks for attempts to add attachable to occupied location
            if (!AttachedBlocks.Any(a => a.Coordinate == coordinate))
            {
                return DIRECTION.UP;
            }

            coordinate = existingAttachableCoordinate + DIRECTION.RIGHT.ToVector2Int();
            //Checks for attempts to add attachable to occupied location
            if (!AttachedBlocks.Any(a => a.Coordinate == coordinate))
            {
                return DIRECTION.RIGHT;
            }

            coordinate = existingAttachableCoordinate + DIRECTION.LEFT.ToVector2Int();
            //Checks for attempts to add attachable to occupied location
            if (!AttachedBlocks.Any(a => a.Coordinate == coordinate))
            {
                return DIRECTION.LEFT;
            }

            coordinate = existingAttachableCoordinate + DIRECTION.DOWN.ToVector2Int();
            //Checks for attempts to add attachable to occupied location
            if (!AttachedBlocks.Any(a => a.Coordinate == coordinate))
            {
                return DIRECTION.DOWN;
            }

            return direction;
        }

        //IHealth Functions
        //====================================================================================================================//


        public virtual void SetupHealthValues(float startingHealth, float currentHealth)
        {
            CurrentHealth = currentHealth;
            StartingHealth = startingHealth;
        }

        public abstract void ChangeHealth(float amount);

        //ICanBeHit Functions
        //====================================================================================================================//

        public abstract bool TryHitAt(Vector2 worldPosition, float damage);
    }
}
