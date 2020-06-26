using System;
using Recycling;
using StarSalvager.Constants;
using StarSalvager.Factories;
using StarSalvager.Utilities.Debugging;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JsonDataTypes;
using UnityEngine;

namespace StarSalvager
{
    public class Bit : CollidableBase, IAttachable, IBit, ISaveable, IHealth
    {
        
        public Vector2Int Coordinate { get; set; }
        public bool Attached { get; set; }
        
        public float StartingHealth { get { return _startingHealth; } }
        private float _startingHealth;
        public float CurrentHealth { get { return _currentHealth; } }
        private float _currentHealth;
        
        //============================================================================================================//
        
        public BIT_TYPE Type
        {
            get => _type;
            set => _type = value;
        }
        [SerializeField]
        private BIT_TYPE _type;
        public int level { get => _level; set => _level = value; }
        [SerializeField]
        private int _level;

        [SerializeField]
        private LayerMask collisionMask;
        
        //============================================================================================================//
        
        public void SetAttached(bool isAttached)
        {
            Attached = isAttached;
            collider.usedByComposite = isAttached;
        }

        public void SetupHealthValues(float startingHealth, float currentHealth)
        {
            _startingHealth = startingHealth;
            _currentHealth = currentHealth;
        }

        public void ChangeHealth(float amount)
        {
            _currentHealth += amount;

            if (_currentHealth <= 0)
            {
                Recycler.Recycle(typeof(Bit), this.gameObject);
            }
        }

        public void IncreaseLevel(int amount = 1)
        {
            level += amount;
            renderer.sortingOrder = level;
            
            //Sets the gameObject info (Sprite)
            var bit = this;
            FactoryManager.Instance.GetFactory<BitAttachableFactory>().UpdateBitData(_type, level, ref bit);
        }
        //============================================================================================================//

        protected override void OnCollide(GameObject gameObject, Vector2 hitPoint)
        {
            var bot = gameObject.GetComponent<Bot>();
            
            if (bot.Rotating)
            {
                Recycling.Recycler.Recycle<Bit>(this.gameObject);
                return;
            }
            
            //Checks to see if the player is moving in the correct direction to bother checking, and if so,
            //return the direction to shoot the ray
            if (!TryGetRayDirectionFromBot(bot.MoveDirection, out var rayDirection))
                return;

            //Long ray compensates for the players high speed
            var rayLength = Values.gridCellSize * 3f;
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
        
        //============================================================================================================//

        public BlockData ToBlockData()
        {
            return new BlockData
            {
                ClassType = GetType().Name,
                Coordinate = Coordinate,
                Type = (int)Type,
                Level = level
            };
        }

        public void LoadBlockData(BlockData blockData)
        {
            Coordinate = blockData.Coordinate;
            Type = (BIT_TYPE) blockData.Type;
            level = blockData.Level;
        }
        
        //============================================================================================================//




    }
}