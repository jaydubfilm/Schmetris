using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager
{
    public class OffGridMovementArc : OffGridMovement
    {
        public Vector2 ArcValue;

        public OffGridMovementArc(IObstacle bit, Vector2 startingPosition, Vector2 arcValue, Vector2 endPosition, float lerpSpeed, float spinSpeed, bool despawnOnEnd, bool spinning) : base(bit, startingPosition, endPosition, lerpSpeed, spinSpeed, despawnOnEnd, spinning)
        {
            ArcValue = arcValue;
        }

        public override void Move(Vector3 shiftValue)
        {
            ShiftOnGrid(shiftValue);
            Bit.transform.position = Vector2.Lerp(StartingPosition, EndPosition, LerpTimer);
            EndPosition += ArcValue * Time.deltaTime;
        }
    }
}