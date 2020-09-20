using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager
{
    public abstract class OffGridMovement
    {
        public IObstacle Obstacle;
        public Vector3 StartingPosition;
        public Vector3 EndPosition;
        public float SpinSpeed;
        public bool Spinning;
        public bool DespawnOnEnd;
        public float LerpSpeed;
        public float LerpTimer;
        public bool ParentToGrid;

        public bool isVisible;

        public OffGridMovement(IObstacle obstacle, Vector3 startingPosition, Vector3 endPosition, float lerpSpeed, float spinSpeed, bool despawnOnEnd, bool spinning, bool parentToGrid)
        {
            Obstacle = obstacle;
            StartingPosition = startingPosition;
            EndPosition = endPosition;
            LerpSpeed = lerpSpeed;
            LerpTimer = 0.0f;
            SpinSpeed = spinSpeed;
            DespawnOnEnd = despawnOnEnd;
            Spinning = spinning;
            ParentToGrid = parentToGrid;
        }

        protected void ShiftOnGrid(Vector3 shiftValue)
        {
            Obstacle.transform.position += shiftValue;
            StartingPosition += shiftValue;
            EndPosition += shiftValue;
        }

        public abstract void Move(Vector3 shiftValue);

        public void Spin()
        {
            if (Spinning)
            {
                Obstacle.transform.Rotate(new Vector3(0, 0, SpinSpeed * Time.deltaTime));
            }
        }
    }
}