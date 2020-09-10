using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager
{
    public enum DIRECTION: int
    {
        NULL = -1,
        LEFT,
        UP,
        RIGHT,
        DOWN
    }


    public static class DirectionExtensions
    {
        public static DIRECTION ClampIntToDirection(this int value)
        {
            if (value <= 3)
                return (DIRECTION) value;
            
            var diff = value % 3;
            return (DIRECTION) diff;
        }
    }
}

