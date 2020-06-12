using UnityEngine;

namespace StarSalvager.Utilities.Extensions
{
    public static class Vector2Extensions
    {
        //FIXME This needs to consider angled directions
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
    }
}
