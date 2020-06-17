using System;
using UnityEngine;

namespace StarSalvager.Utilities.Extensions
{
    public static class ROTATIONSExtensions
    {
        private static readonly Quaternion[] rotations =
        {
            Quaternion.Euler(0,0,-90),
            Quaternion.Euler(0,0,90)
        };
        
        private static readonly Quaternion[] rotationsInverse =
        {
            Quaternion.Euler(0,0,90),
            Quaternion.Euler(0,0,-90)
        };
        
        private static readonly float[] angles =
        {
            -90,
            90
        };
        
        private static readonly float[] anglesInverse =
        {
            90,
            -90
        };
        
        public static Quaternion ToQuaternion(this ROTATION rotation)
        {
            switch (rotation)
            {
                case ROTATION.CW:
                case ROTATION.CCW:
                    return rotations[(int) rotation];
                default:
                    throw new ArgumentOutOfRangeException(nameof(rotation), rotation, null);
            }
        }
        
        public static Quaternion ToInverseQuaternion(this ROTATION rotation)
        {
            switch (rotation)
            {
                case ROTATION.CW:
                case ROTATION.CCW:
                    return rotationsInverse[(int) rotation];
                default:
                    throw new ArgumentOutOfRangeException(nameof(rotation), rotation, null);
            }
        }
        
        public static float ToAngle(this ROTATION rotation)
        {
            switch (rotation)
            {
                case ROTATION.CW:
                case ROTATION.CCW:
                    return angles[(int) rotation];
                default:
                    throw new ArgumentOutOfRangeException(nameof(rotation), rotation, null);
            }
        }
        
        public static float ToInverseAngle(this ROTATION rotation)
        {
            switch (rotation)
            {
                case ROTATION.CW:
                case ROTATION.CCW:
                    return anglesInverse[(int) rotation];
                default:
                    throw new ArgumentOutOfRangeException(nameof(rotation), rotation, null);
            }
        }
    }
}

