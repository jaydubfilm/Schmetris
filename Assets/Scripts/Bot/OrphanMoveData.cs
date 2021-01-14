using StarSalvager.Utilities.JsonDataTypes;
using UnityEngine;

namespace StarSalvager
{
    public class OrphanMoveData
    {
        public IAttachable attachableBase;
        public DIRECTION moveDirection;
        public float distance;
        
        public Vector2Int startingCoordinates;
        public Vector2Int intendedCoordinates;
    }
    
    public class OrphanMoveBlockData
    {
        public IBlockData blockData;
        public DIRECTION moveDirection;
        public float distance;
        
        public Vector2Int startingCoordinates;
        public Vector2Int intendedCoordinates;
    }
}

