﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager
{
    public class OffGridMovementInfo
    {
        public IObstacle Bit;
        public Vector2 StartingPosition;
        public Vector2 EndPosition;
        public float SpinSpeed;
        public bool Spinning;
        public bool DespawnOnEnd;
        public float LerpSpeed;
        public float LerpTimer;

        public OffGridMovementInfo(IObstacle bit, Vector2 startingPosition, Vector2 endPosition, float lerpSpeed, float spinSpeed, bool despawnOnEnd, bool spinning)
        {
            Bit = bit;
            StartingPosition = startingPosition;
            EndPosition = endPosition;
            LerpSpeed = lerpSpeed;
            LerpTimer = 0.0f;
            SpinSpeed = spinSpeed;
            DespawnOnEnd = despawnOnEnd;
            Spinning = spinning;
        }

        public void ShiftOnGrid(Vector3 shiftValue)
        {
            Vector2 shiftValueVector2 = shiftValue;
            Bit.transform.position += shiftValue;
            StartingPosition += shiftValueVector2;
            EndPosition += shiftValueVector2;
        }
    }
}