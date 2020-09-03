using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager
{
    public class OffGridMovementLerp : OffGridMovement
    {
        public OffGridMovementLerp(IObstacle bit, Vector2 startingPosition, Vector2 endPosition, float lerpSpeed, float spinSpeed, bool despawnOnEnd, bool spinning) : base(bit, startingPosition, endPosition, lerpSpeed, spinSpeed, despawnOnEnd, spinning)
        {

        }

        public override void Move(Vector3 shiftValue)
        {
            ShiftOnGrid(shiftValue);
            Bit.transform.localPosition = Vector2.Lerp(StartingPosition, EndPosition, LerpTimer);
        }
    }
}