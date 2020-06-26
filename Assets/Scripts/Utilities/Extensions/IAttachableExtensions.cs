﻿using System;
using UnityEngine;

namespace StarSalvager.Utilities.Extensions
{
    public static class IAttachableExtensions
    {
        public static void RotateCoordinate(this IAttachable attachable, ROTATION rotation)
        {
            var coordinate = attachable.Coordinate;
            var temp = Vector2Int.zero;
            
            switch (rotation)
            {
                case ROTATION.CW:
                    temp.x = coordinate.y;
                    temp.y = coordinate.x * -1;
                    
                    break;
                case ROTATION.CCW:
                    temp.x = coordinate.y * -1;
                    temp.y = coordinate.x;
                    
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(rotation), rotation, null);
            }
            
            ////Rotate opposite of the Core rotation 
            attachable.transform.localRotation *= rotation.ToInverseQuaternion();
            //attachable.Rotated(rotation);

            attachable.Coordinate = temp;
        }
    }
}
