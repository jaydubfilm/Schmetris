using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace StarSalvager.Utilities.Extensions
{
    public static class AttachableListExtensions
    {
        public static bool CoordinateOccupied<T>(this List<T> attachedBlocks, DIRECTION direction, ref Vector2Int coordinate) where T: AttachableBase
        {
            var check = coordinate;
            var exists = attachedBlocks
                .Any(b => b.Coordinate == check);

            if (!exists)
                return false;

            coordinate += direction.ToVector2Int();

            return attachedBlocks.CoordinateOccupied(direction, ref coordinate);
        }
        
    }
}