﻿using System;
using Recycling;
using Sirenix.OdinInspector;
using StarSalvager.Audio;
using StarSalvager.Values;
using StarSalvager.Factories;
using StarSalvager.Utilities;
using StarSalvager.Utilities.Debugging;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.Inputs;
using StarSalvager.Utilities.JsonDataTypes;
using UnityEngine;
using Random = UnityEngine.Random;

namespace StarSalvager
{
    public class Mine : CollidableBase, IObstacle, ICustomRecycle
    {
        public MINE_TYPE Type;
        
        //IObstacle Properties
        //============================================================================================================//
        public bool CanMove => true;

        public bool IsRegistered { get; set; } = false;

        public bool IsMarkedOnGrid { get; set; } = false;

        //Bit Functions
        //============================================================================================================//

        protected override void OnCollide(GameObject gameObject, Vector2 worldHitPoint)
        {
            //Debug.Break();
            
            
            var bot = gameObject.GetComponent<Bot>();

            if (bot == null)
            {
                return;
            }

            if (bot.Rotating)
            {
                this.Bounce(worldHitPoint, transform.position, bot.MostRecentRotate);
                AudioController.PlaySound(SOUND.BIT_BOUNCE);
                return;
            }

            var dir = (worldHitPoint - (Vector2)transform.position).ToVector2Int();
            var direction = dir.ToDirection();

            //Checks to see if the player is moving in the correct direction to bother checking, and if so,
            //return the direction to shoot the ray
            if (!TryGetRayDirectionFromBot(Globals.MovingDirection, out var rayDirection))
                return;
            
            if (dir != rayDirection && dir != Vector2Int.zero)
                return;

            if (!TryFindClosestCollision(rayDirection.ToDirection(), out var point))
                return;

            //Here we flip the direction of the ray so that we can tell the Bot where this piece might be added to
            var inDirection = (-rayDirection).ToDirection();
            //bot.TryAddNewAttachable(this, inDirection, point);

            Debug.Log("MINE EXPLODE");
        }

        private bool TryFindClosestCollision(DIRECTION direction, out Vector2 point)
        {
            const float rayLength = Constants.gridCellSize * 3f;
            
            point = Vector2.zero;
            
            var currentPosition = (Vector2)transform.position;
            var vectorDirection = direction.ToVector2();
            var startOffset = -vectorDirection * (rayLength / 2f);
            Vector2 positionOffset;
            
            switch (direction)
            {
                case DIRECTION.RIGHT:
                case DIRECTION.LEFT:
                    positionOffset = Vector2.up * 0.33f;
                    break;
                case DIRECTION.UP:
                case DIRECTION.DOWN:
                    positionOffset = Vector2.right * 0.33f;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
            
            var startPositions = new[]
            {
                currentPosition + startOffset,
                (currentPosition - positionOffset) + startOffset,
                (currentPosition + positionOffset) + startOffset,
            };

            var shortestDis = 999f;
            RaycastHit2D? shortestHit = null;
            foreach (var rayStartPosition in startPositions)
            {
                var hit = Physics2D.Raycast(rayStartPosition, vectorDirection, rayLength);

                //If nothing was hit, ray failed, thus no reason to continue
                if (hit.collider == null)
                {
                    //Debug.DrawRay(rayStartPosition, vectorDirection * rayLength, Color.yellow, 1f);
                    SSDebug.DrawArrowRay(rayStartPosition, vectorDirection * rayLength, Color.yellow);
                    continue;
                }

                Debug.DrawRay(hit.point, Vector2.up, Color.red);
                Debug.DrawRay(rayStartPosition, vectorDirection * rayLength, Color.green);

                if (hit.distance >= shortestDis)
                    continue;
                
                shortestDis = hit.distance;
                shortestHit = hit;
            }

            if (!shortestHit.HasValue)
                return false;

            point = shortestHit.Value.point;
            
            return true;
        }

        //============================================================================================================//

        public virtual void CustomRecycle(params object[] args)
        {
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;

            SetSortingLayer(LayerHelper.ACTORS);
        }

        //IHasBounds Functions
        //====================================================================================================================//

        public Bounds GetBounds()
        {
            return new Bounds
            {
                center = transform.position,
                size = Vector2.one * Constants.gridCellSize
            };
        }

        //====================================================================================================================//
        
    }
}
