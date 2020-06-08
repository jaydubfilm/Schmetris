using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.Utilities
{
    public static class Extensions
    {
        private static readonly Vector2Int[] _directionVectors = {
            new Vector2Int(-1, 0), 
            new Vector2Int(0, 1),
            new Vector2Int(1, 0),
            new Vector2Int(0, -1)
        };
        
        public static Vector2Int DirectionToVector2Int(DIRECTION direction)
        {
            switch (direction)
            {
                case DIRECTION.LEFT:
                case DIRECTION.UP:
                case DIRECTION.RIGHT:
                case DIRECTION.DOWN:
                    return _directionVectors[(int) direction];
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }
        public static DIRECTION Vector2IntToDirection(Vector2Int vector2Int)
        {
            if(vector2Int == Vector2Int.zero)
                throw new ArgumentException($"Cannot convert {vector2Int} into a legal direction");

            if (vector2Int.x == 0)
            {
                return vector2Int.y > 0 ? DIRECTION.UP : DIRECTION.DOWN;
            }
            
            if (vector2Int.y == 0)
            {
                return vector2Int.x > 0 ? DIRECTION.RIGHT : DIRECTION.LEFT;
            }
            
            throw new ArgumentException($"Cannot convert {vector2Int} into a legal direction");
        }
    }
}

