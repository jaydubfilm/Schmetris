using System;

namespace StarSalvager.AI
{
    /// <summary>
    /// If the asteroid sizes are edited, don't forget to update ObstacleManager, StageObstacleData, and EditorShapeGeneratorData
    /// </summary>
    [Serializable]
    public enum ASTEROID_SIZE : int
    {
        Bit, // one bit
        Small, // 2-3 bits
        Medium, // 4-5 bits
        Large // 6-8 bits
    }
}