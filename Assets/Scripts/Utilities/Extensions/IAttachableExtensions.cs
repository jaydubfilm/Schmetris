using System;
using StarSalvager.Values;
using UnityEngine;

namespace StarSalvager.Utilities.Extensions
{
    public static class IAttachableExtensions
    {
        public static void RotateCoordinate(this IAttachable attachable, ROTATION rotation)
        {
            var coordinate = attachable.Coordinate;
            var temp = Vector2Int.zero;
            
            switch (rotation)
            {
                case ROTATION.CW:
                    temp.x = coordinate.y;
                    temp.y = coordinate.x * -1;
                    
                    break;
                case ROTATION.CCW:
                    temp.x = coordinate.y * -1;
                    temp.y = coordinate.x;
                    
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(rotation), rotation, null);
            }
            
            attachable.Coordinate = temp;

            
            /*//Custom rotate
            if (attachable is ICustomRotate customRotate)
            {
                Debug.Log("Rotate");
                customRotate.CustomRotate();
                return;
            }*/
            
            //Rotate opposite of the Core rotation 
            //attachable.transform.localRotation *= rotation.ToInverseQuaternion();
            
        }
        
        /*public static void RotateSprite(this IAttachable attachable, ROTATION rotation)
        {
            if (attachable is ICustomRotate customRotate)
            {
                Debug.Log("Rotate");
                customRotate.CustomRotate();
                return;
            }
            
            //Rotate opposite of the Core rotation 
            attachable.transform.localRotation *= rotation.ToInverseQuaternion();
            
        }*/

        public static void Bounce(this IAttachable attachable, Vector2 contactPoint)
        {
            if (!(attachable is IObstacle obstacle))
                return;
                
            Vector2 directionBounce = (Vector2)attachable.transform.position - contactPoint;
            directionBounce.Normalize();
            if (directionBounce != Vector2.up)
            {
                Vector2 downVelocity = Vector2.down * Constants.gridCellSize / Globals.AsteroidFallTimer;
                downVelocity.Normalize();
                downVelocity *= 0.5f;
                directionBounce += downVelocity;
                directionBounce.Normalize();
            }
            else
            {
                Vector2 sideVelocity = Vector2.left * (UnityEngine.Random.Range(0, 2) * 2 - 1);
                sideVelocity *= 0.5f;
                directionBounce += sideVelocity;
                directionBounce.Normalize();
            }

            float rotation = 180.0f;
            if (directionBounce.x >= 0)
            {
                rotation *= -1;
            }
            
            
            LevelManager.Instance.ObstacleManager.BounceObstacle(obstacle, directionBounce, rotation, true, true, true);
        }
    }
}

