using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager
{
    public interface IEnemy
    {
        void ProcessMovement(Vector3 direction);
        Vector3 GetDestination();
        Vector3 GetAngleInOscillation();
        Vector3 GetDestinationForRotatePositionAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles);
        Vector3 GetDestinationForRotatePositionAroundPivotAtDistance(Vector3 point, Vector3 pivot, Vector3 angles, float distance);
    }
}
