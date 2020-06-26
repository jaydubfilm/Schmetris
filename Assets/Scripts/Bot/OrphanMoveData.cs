using UnityEngine;

namespace StarSalvager
{
    public class OrphanMoveData
    {
        public IAttachable attachableBase;
        public DIRECTION moveDirection;
        public float distance;
        public Vector2Int intendedCoordinates;
    }
}

