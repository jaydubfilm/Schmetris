using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager
{
    public class OffGridMovementArc : OffGridMovement
    {
        public Vector3 ArcValue;

        public OffGridMovementArc(IObstacle obstacle, Vector3 startingPosition, Vector3 arcValue, Vector3 endPosition, float lerpSpeed, float spinSpeed, bool despawnOnEnd, bool spinning, bool parentToGrid) : base(obstacle, startingPosition, endPosition, lerpSpeed, spinSpeed, despawnOnEnd, spinning, parentToGrid)
        {
            ArcValue = arcValue;
        }

        public override void Move(Vector3 shiftValue)
        {
            ShiftOnGrid(shiftValue);
            Obstacle.transform.localPosition = Vector3.Lerp(StartingPosition, EndPosition, LerpTimer);
            EndPosition += ArcValue * Time.deltaTime;
        }
    }
}