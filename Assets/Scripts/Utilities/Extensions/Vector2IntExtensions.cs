using System;
using UnityEngine;

namespace StarSalvager.Utilities.Extensions
{
    public static class Vector2IntExtensions
    {
        private static readonly Vector2Int[] DirectionVectors = {
            new Vector2Int(-1, 0),   //LEFT
            new Vector2Int(0, 1),    //UP
            new Vector2Int(1, 0),    //RIGHT
            new Vector2Int(0, -1)    //DOWN
        };
        
        public static Vector2Int ToVector2Int(this DIRECTION direction)
        {
            switch (direction)
            {
                case DIRECTION.LEFT:
                case DIRECTION.UP:
                case DIRECTION.RIGHT:
                case DIRECTION.DOWN:
                    return DirectionVectors[(int) direction];
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }
        public static DIRECTION Reflected(this DIRECTION direction)
        {
            switch (direction)
            {
                case DIRECTION.LEFT:
                    return DIRECTION.RIGHT;
                case DIRECTION.UP:
                    return DIRECTION.DOWN;
                case DIRECTION.RIGHT:
                    return DIRECTION.LEFT;
                case DIRECTION.DOWN:
                    return DIRECTION.UP;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }
        public static Vector2Int Reflected(this Vector2Int direction)
        {
            return direction * -1;
        }
        public static DIRECTION ToDirection(this Vector2Int vector2Int)
        {
            if(vector2Int == Vector2Int.zero)
                //throw new ArgumentException($"Cannot convert {vector2Int} into a legal direction");
                return DIRECTION.NULL;
            
            if (vector2Int.x != 0 && vector2Int.y != 0)
                return DIRECTION.NULL;

            if (vector2Int.x == 0)
            {
                return vector2Int.y > 0 ? DIRECTION.UP : DIRECTION.DOWN;
            }
            
            if (vector2Int.y == 0)
            {
                return vector2Int.x > 0 ? DIRECTION.RIGHT : DIRECTION.LEFT;
            }
            
            //throw new ArgumentException($"Cannot convert {vector2Int} into a legal direction");
            return DIRECTION.NULL;
        }

        public static bool TryParseVector2Int(string s, out Vector2Int value)
        {
            value = Vector2Int.zero;

            try
            {
                //Trim all the unnecessary fat
                s = s.Replace("(", "").Replace(")", "").Replace(" ", "");

                var split = s.Split(',');

                value.x = int.Parse(split[0]);
                value.y = int.Parse(split[1]);

            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}
