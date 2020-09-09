using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager
{
    public class OffGridMovementLerp : OffGridMovement
    {
        public OffGridMovementLerp(IObstacle bit, Vector3 startingPosition, Vector3 endPosition, float lerpSpeed, float spinSpeed, bool despawnOnEnd, bool spinning, bool parentToGrid) : base(bit, startingPosition, endPosition, lerpSpeed, spinSpeed, despawnOnEnd, spinning, parentToGrid)
        {

        }

        public override void Move(Vector3 shiftValue)
        {
            ShiftOnGrid(shiftValue);
            Bit.transform.localPosition = Vector3.Lerp(StartingPosition, EndPosition, LerpTimer);
        }
    }
}