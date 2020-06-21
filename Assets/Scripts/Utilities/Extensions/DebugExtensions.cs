using UnityEngine;

namespace StarSalvager.Utilities.Debugging
{
    public static class SSDebug
    {
        //public static void DrawArrowRay(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f,
        //    float arrowHeadAngle = 20.0f)
        //{
        //    Debug.DrawRay(pos, direction);
        //    arrowHeadLength = direction.magnitude / 5f;
        //    
//
        //    Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) *
        //                    Vector3.forward;
        //    Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) *
        //                   Vector3.forward;
        //    Debug.DrawRay(pos + direction, right * arrowHeadLength);
        //    Debug.DrawRay(pos + direction, left * arrowHeadLength);
        //}

        public static void DrawArrowRay(Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f,
            float arrowHeadAngle = 20.0f)
        {
            Debug.DrawRay(pos, direction, color);
            arrowHeadLength = direction.magnitude / 3f;

            Vector3 right = Quaternion.LookRotation(direction, Vector3.forward) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) *
                            Vector3.forward;
            Vector3 left = Quaternion.LookRotation(direction, Vector3.forward) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) *
                           Vector3.forward;
            Debug.DrawRay(pos + direction, right * arrowHeadLength, color);
            Debug.DrawRay(pos + direction, left * arrowHeadLength, color);
        }
        
        //public static void DrawArrow(Vector3 startPos, Vector3 endPos, float arrowHeadLength = 0.25f,
        //    float arrowHeadAngle = 20.0f)
        //{
        //    if(startPos == endPos)
        //        return;
        //    
        //    Debug.DrawLine(startPos, endPos);
        //   
        //    arrowHeadLength = (endPos - startPos).magnitude / 3f;
        //    Vector3 direction = (endPos - startPos).normalized;
        //    
        //    Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) *
        //                    Vector3.forward;
        //    Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) *
        //                   Vector3.forward;
        //    Debug.DrawRay(endPos, right * arrowHeadLength);
        //    Debug.DrawRay(endPos, left * arrowHeadLength);
        //}

        public static void DrawArrow(Vector3 startPos, Vector3 endPos, Color color, float arrowHeadLength = 1f,
            float arrowHeadAngle = 20.0f)
        {
            if(startPos == endPos)
                return;
            
            Debug.DrawLine(startPos, endPos, color);
           
            arrowHeadLength = (endPos - startPos).magnitude / 3f;
            Vector3 direction = (endPos - startPos).normalized;
            
            Vector3 right = Quaternion.LookRotation(direction, Vector3.forward) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) *
                            Vector3.forward;
            Vector3 left = Quaternion.LookRotation(direction, Vector3.forward) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) *
                           Vector3.forward;
            Debug.DrawRay(endPos, right * arrowHeadLength, color);
            Debug.DrawRay(endPos, left * arrowHeadLength, color);
        }
    }
}

