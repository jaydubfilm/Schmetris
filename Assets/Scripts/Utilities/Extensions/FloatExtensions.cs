using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.Utilities.Extensions
{
    public static class FloatExtensions
    {
        public static DIRECTION GetHorizontalDirection(this float value)
        {
            if (value < 0f)
                return DIRECTION.LEFT;
            if (value > 0f)
                return DIRECTION.RIGHT;

            return DIRECTION.NULL;
        }
        
        public static DIRECTION GetVerticalDirection(this float value)
        {
            if (value < 0f)
                return DIRECTION.DOWN;
            if (value > 0f)
                return DIRECTION.UP;

            return DIRECTION.NULL;
        }
    }
}


