﻿using UnityEngine;

namespace StarSalvager.Utilities.Extensions
{
    public static class Vector2Extensions
    {
        public static DIRECTION ToDirection(this Vector2 vector2)
        {
            if(vector2 == Vector2.zero)
                //throw new ArgumentException($"Cannot convert {vector2} into a legal direction");
                return DIRECTION.NULL;

            var vector2Int = new Vector2Int(
                Mathf.RoundToInt(vector2.x), 
                Mathf.RoundToInt(vector2.y));

            return vector2Int.ToDirection();
        }
        
        public static Vector2 ToVector2(this DIRECTION direction)
        {
            return direction.ToVector2Int();
        }
        
        public static Vector2Int ToVector2Int(this Vector2 vector2)
        {
            return new Vector2Int(
                Mathf.RoundToInt(vector2.x),
                Mathf.RoundToInt(vector2.y));
        }
    }
}
