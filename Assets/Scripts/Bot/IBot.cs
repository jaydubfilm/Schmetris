using UnityEngine;

namespace StarSalvager
{
    public interface IBot
    {
        public Collider2D Collider { get; }

        public bool Rotating { get; }

        //====================================================================================================================//
        

        bool TryAddNewAttachable(IAttachable attachable, DIRECTION connectionDirection, Vector2 collisionPoint);
        public void ForceDetach(ICanDetach attachable);
        public bool CoordinateHasPathToCore(Vector2Int coordinate);
        public bool CoordinateOccupied(Vector2Int coordinate);

        public bool TryAttachNewBlock(Vector2Int coordinate, IAttachable newAttachable,
            bool checkForCombo = true,
            bool updateColliderGeometry = true,
            bool updatePartList = true);

        public IAttachable GetClosestAttachable(Vector2Int checkCoordinate, float maxDistance = 999f);

        //====================================================================================================================//

        public void TryHitAt(IAttachable closestAttachable, float damage, bool withSound = true);

    }
}
