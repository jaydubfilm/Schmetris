using UnityEngine;

namespace StarSalvager.Utilities.JsonDataTypes
{
    public interface IBlockData
    {
        string ClassType { get; }
        Vector2Int Coordinate { get; set; }
        int Type { get; set; }
    }
}
