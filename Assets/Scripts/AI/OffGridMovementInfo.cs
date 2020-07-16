using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager
{
    public class OffGridMovementInfo
    {
        public Bit Bit;
        public Vector2 StartingPosition;
        public Vector2 EndPosition;
        public float LerpSpeed;
        public float LerpTimer;

        public OffGridMovementInfo(Bit bit, Vector2 startingPosition, Vector2 endPosition)
        {
            Bit = bit;
            StartingPosition = startingPosition;
            EndPosition = endPosition;
            LerpSpeed = 0.5f;
            LerpTimer = 0.0f;
        }
    }
}