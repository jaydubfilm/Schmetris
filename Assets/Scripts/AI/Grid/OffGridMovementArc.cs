using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager
{
    public class OffGridMovementArc : OffGridMovement
    {
        public Vector3 ArcValue;

        public OffGridMovementArc(IObstacle bit, Vector3 startingPosition, Vector3 arcValue, Vector3 endPosition, float lerpSpeed, float spinSpeed, bool despawnOnEnd, bool spinning, bool parentToGrid) : base(bit, startingPosition, endPosition, lerpSpeed, spinSpeed, despawnOnEnd, spinning, parentToGrid)
        {
            ArcValue = arcValue;
        }

        public override void Move(Vector3 shiftValue)
        {
            ShiftOnGrid(shiftValue);
            Bit.transform.localPosition = Vector3.Lerp(StartingPosition, EndPosition, LerpTimer);
            EndPosition += ArcValue * Time.deltaTime;
        }
    }
}