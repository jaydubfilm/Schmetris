using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager
{
    public interface IBot
    {
        List<IAttachable> AttachedBlocks { get; }

        Collider2D Collider { get; }

        bool Rotating { get; }

        //====================================================================================================================//
        

        bool TryAddNewAttachable(IAttachable attachable, DIRECTION connectionDirection, Vector2 collisionPoint);
        void ForceDetach(ICanDetach attachable);
        bool CoordinateHasPathToCore(Vector2Int coordinate);
        bool CoordinateOccupied(Vector2Int coordinate);

        bool TryAttachNewBlock(Vector2Int coordinate, IAttachable newAttachable,
            bool checkForCombo = true,
            bool updateColliderGeometry = true,
            bool updatePartList = true);

        void AttachAttachableToExisting(IAttachable newAttachable, IAttachable existingAttachable,
            DIRECTION direction,
            bool checkForCombo = true,
            bool updateColliderGeometry = true,
            bool checkMagnet = true,
            bool playSound = true,
            bool updatePartList = true);

        void AttachToClosestAvailableCoordinate(Vector2Int coordinate, IAttachable newAttachable,
            DIRECTION desiredDirection, bool checkForCombo,
            bool updateColliderGeometry);

        void AttachNewBlock(Vector2Int coordinate, IAttachable newAttachable,
            bool checkForCombo = true,
            bool updateColliderGeometry = true,
            bool checkMagnet = true,
            bool playSound = true,
            bool updatePartList = true);

        IAttachable GetClosestAttachable(Vector2Int checkCoordinate, float maxDistance = 999f);

        //====================================================================================================================//

        void TryHitAt(IAttachable closestAttachable, float damage, bool withSound = true);

    }
}
