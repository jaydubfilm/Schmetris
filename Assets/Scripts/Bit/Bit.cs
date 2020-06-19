using System;
using StarSalvager.Factories;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JsonDataTypes;
using UnityEngine;

namespace StarSalvager
{
    public class Bit : AttachableBase, IBit
    {
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
        
        //============================================================================================================//

        public void IncreaseLevel()
        {
            level++;
            
            //TODO Get the new sprite for the level
            //Debug.Log($"Upgrade {gameObject.name} to level {level}", this);

            var bit = this;
            FactoryManager.Instance.GetFactory<BitAttachableFactory>().UpdateBitData(_type, level, ref bit);
        }
        
        //============================================================================================================//

        protected override void OnCollide(GameObject _gameObject)
        {
            var bot = _gameObject.GetComponent<Bot>();
            
            if (bot.Rotating)
            {
                Destroy(gameObject);
                return;
            }
            
            //Checks to see if the player is moving in the correct direction to bother checking, and if so,
            //return the direction to shoot the ray
            if (!TryGetRayDirectionFromBot(bot.MoveDirection, out var rayDirection))
                return;

            //Long ray compensates for the players high speed
            var rayLength = 1.28f * 3f;
            var rayStartPosition = (Vector2)transform.position + -rayDirection * rayLength;

            
            //Checking ray against player layer mask
            var hit = Physics2D.Raycast(rayStartPosition, rayDirection, rayLength,  1 << 8);

            //If nothing was hit, ray failed, thus no reason to continue
            if (hit.collider == null)
            {
                //Debug.DrawRay(rayStartPosition, rayDirection * size, Color.yellow, 1f);
                return;
            }
            
            //Debug.DrawRay(hit.point, Vector2.up, Color.red);
            //Debug.DrawRay(rayStartPosition, rayDirection * size, Color.green);

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
                    return false;
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
            //FIXME Might want to consider BlockData that has Coordinate of (0, 0) or null
            Coordinate = blockData.Coordinate;
            Type = (BIT_TYPE) blockData.Type;
            level = blockData.Level;
        }
        
        //============================================================================================================//

    }
}