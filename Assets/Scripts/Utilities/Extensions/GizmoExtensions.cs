using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.Utilities.Extensions
{
    public static class GizmoExtensions
    {
        /*public static void DrawRect(Vector3 origin, Rect rect)
        {
            DrawRect(origin, rect, Gizmos.color);
        }
        
        public static void DrawRect(Vector3 origin, Rect rect, Color color)
        {
            var TL = origin + new Vector3(rect.xMin, rect.yMin);
            var TR = origin + new Vector3(rect.xMax, rect.yMin);
            var BR = origin + new Vector3(rect.xMax, rect.yMax);
            var BL = origin + new Vector3(rect.xMin, rect.yMax);

            Gizmos.color = color;
            
            Gizmos.DrawLine(TL, TR);
            Gizmos.DrawLine(TR, BR);
            Gizmos.DrawLine(BR, BL);
            Gizmos.DrawLine(BL, TL);
        }*/

        public static void DrawDebugRect(Rect rect, Color color, float duration = 0.1f)
        {
            var TL = new Vector2(rect.xMin, rect.yMin);
            var TR = new Vector2(rect.xMax, rect.yMin);
            var BR = new Vector2(rect.xMax, rect.yMax);
            var BL = new Vector2(rect.xMin, rect.yMax);
            
            Debug.DrawLine(TL, TR, color, duration);
            Debug.DrawLine(TR, BR, color, duration);
            Debug.DrawLine(BR, BL, color, duration);
            Debug.DrawLine(BL, TL, color, duration);
        }
        
        public static void DrawRect(Rect rect)
        {
            DrawRect(rect, Gizmos.color);
        }
        
        public static void DrawRect(Rect rect, Color color)
        {
            var TL = new Vector2(rect.xMin, rect.yMin);
            var TR = new Vector2(rect.xMax, rect.yMin);
            var BR = new Vector2(rect.xMax, rect.yMax);
            var BL = new Vector2(rect.xMin, rect.yMax);

            Gizmos.color = color;
            
            Gizmos.DrawLine(TL, TR);
            Gizmos.DrawLine(TR, BR);
            Gizmos.DrawLine(BR, BL);
            Gizmos.DrawLine(BL, TL);
        }
    }
}


