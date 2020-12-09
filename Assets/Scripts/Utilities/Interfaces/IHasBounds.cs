using UnityEngine;

namespace StarSalvager.Utilities.Interfaces
{
    public interface IHasBounds
    {
        /// <summary>
        /// Returns the bounds of the object in world space coordinates
        /// </summary>
        /// <returns></returns>
        Bounds GetBounds();
    }
}
