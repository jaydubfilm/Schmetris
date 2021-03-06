﻿using System;
using UnityEngine;


namespace StarSalvager.Utilities
{
    //Design Curves here: https://www.desmos.com/calculator
    //From: https://wiki.unity3d.com/index.php/Mathfx
    public sealed class Mathfx
    {
        public static string ToRoman(in int number)
        {
            if ((number < 0) || (number > 3999)) throw new ArgumentException("insert value between 1 and 3999");
            if (number < 1) return string.Empty;            
            if (number >= 1000) return "M" + ToRoman(number - 1000);
            if (number >= 900) return "CM" + ToRoman(number - 900); 
            if (number >= 500) return "D" + ToRoman(number - 500);
            if (number >= 400) return "CD" + ToRoman(number - 400);
            if (number >= 100) return "C" + ToRoman(number - 100);            
            if (number >= 90) return "XC" + ToRoman(number - 90);
            if (number >= 50) return "L" + ToRoman(number - 50);
            if (number >= 40) return "XL" + ToRoman(number - 40);
            if (number >= 10) return "X" + ToRoman(number - 10);
            if (number >= 9) return "IX" + ToRoman(number - 9);
            if (number >= 5) return "V" + ToRoman(number - 5);
            if (number >= 4) return "IV" + ToRoman(number - 4);
            if (number >= 1) return "I" + ToRoman(number - 1);
            throw new ArgumentException("something bad happened");
        }
        
        public static Vector2 GetAsPointOnCircle(in float degrees, in float radius = 1f)
        {
            if(radius == 0)
                return Vector2.zero;
            
            var radians = degrees * Mathf.Deg2Rad;

            var x = Mathf.Cos(radians);
            var y = Mathf.Sin(radians);
            
            return new Vector2(x, y) * radius; //Vector2 is fine, if you're in 2D
        }

        public static Vector2 Abs(in Vector2 vector2)
        {
            return new Vector2
            {
                x = Mathf.Abs(vector2.x),
                y = Mathf.Abs(vector2.y)
            };
        }
        
        public static Vector3 Abs(in Vector3 vector3)
        {
            return new Vector3
            {
                x = Mathf.Abs(vector3.x),
                y = Mathf.Abs(vector3.y),
                z = Mathf.Abs(vector3.z),
            };
        }
        
        //Custom Additions
        //====================================================================================================================//

        //y=\left(mx\cdot mx\cdot\frac{4-4\cdot mx}{mx}\right)^{2}
        //https://www.desmos.com/calculator/jhb1uugymj
        public static float HermiteCubed(float start, float end, float value)
        {
            if (value == 0f) return start;
            
            var v = value * value * ((4.0f - 4.0f * value) / value);
            
            
            return Mathf.Lerp(start, end, v * v);
        }
        
        public static Vector2 HermiteCubed(Vector2 start, Vector2 end, float value)
        {
            return new Vector2(HermiteCubed(start.x, end.x, value), HermiteCubed(start.y, end.y, value));
        }

        public static Vector3 HermiteCubed(Vector3 start, Vector3 end, float value)
        {
            return new Vector3(HermiteCubed(start.x, end.x, value), HermiteCubed(start.y, end.y, value),
                HermiteCubed(start.z, end.z, value));
        }

        //Base Functions
        //====================================================================================================================//
        
        //Ease in out
        public static float Hermite(float start, float end, float value)
        {
            return Mathf.Lerp(start, end, value * value * (3.0f - 2.0f * value));
        }

        public static Vector2 Hermite(Vector2 start, Vector2 end, float value)
        {
            return new Vector2(Hermite(start.x, end.x, value), Hermite(start.y, end.y, value));
        }

        public static Vector3 Hermite(Vector3 start, Vector3 end, float value)
        {
            return new Vector3(Hermite(start.x, end.x, value), Hermite(start.y, end.y, value),
                Hermite(start.z, end.z, value));
        }

        //Ease out
        public static float Sinerp(float start, float end, float value)
        {
            return Mathf.Lerp(start, end, Mathf.Sin(value * Mathf.PI * 0.5f));
        }

        public static Vector2 Sinerp(Vector2 start, Vector2 end, float value)
        {
            return new Vector2(Mathf.Lerp(start.x, end.x, Mathf.Sin(value * Mathf.PI * 0.5f)),
                Mathf.Lerp(start.y, end.y, Mathf.Sin(value * Mathf.PI * 0.5f)));
        }

        public static Vector3 Sinerp(Vector3 start, Vector3 end, float value)
        {
            return new Vector3(Mathf.Lerp(start.x, end.x, Mathf.Sin(value * Mathf.PI * 0.5f)),
                Mathf.Lerp(start.y, end.y, Mathf.Sin(value * Mathf.PI * 0.5f)),
                Mathf.Lerp(start.z, end.z, Mathf.Sin(value * Mathf.PI * 0.5f)));
        }

        //Ease in
        public static float Coserp(float start, float end, float value)
        {
            return Mathf.Lerp(start, end, 1.0f - Mathf.Cos(value * Mathf.PI * 0.5f));
        }

        public static Vector2 Coserp(Vector2 start, Vector2 end, float value)
        {
            return new Vector2(Coserp(start.x, end.x, value), Coserp(start.y, end.y, value));
        }

        public static Vector3 Coserp(Vector3 start, Vector3 end, float value)
        {
            return new Vector3(Coserp(start.x, end.x, value), Coserp(start.y, end.y, value),
                Coserp(start.z, end.z, value));
        }

        //Boing
        public static float Berp(float start, float end, float value)
        {
            value = Mathf.Clamp01(value);
            value = (Mathf.Sin(value * Mathf.PI * (0.2f + 2.5f * value * value * value)) * Mathf.Pow(1f - value, 2.2f) +
                     value) * (1f + (1.2f * (1f - value)));
            return start + (end - start) * value;
        }

        public static Vector2 Berp(Vector2 start, Vector2 end, float value)
        {
            return new Vector2(Berp(start.x, end.x, value), Berp(start.y, end.y, value));
        }

        public static Vector3 Berp(Vector3 start, Vector3 end, float value)
        {
            return new Vector3(Berp(start.x, end.x, value), Berp(start.y, end.y, value), Berp(start.z, end.z, value));
        }

        //Like lerp with ease in ease out
        public static float SmoothStep(float x, float min, float max)
        {
            x = Mathf.Clamp(x, min, max);
            float v1 = (x - min) / (max - min);
            float v2 = (x - min) / (max - min);
            return -2 * v1 * v1 * v1 + 3 * v2 * v2;
        }

        public static Vector2 SmoothStep(Vector2 vec, float min, float max)
        {
            return new Vector2(SmoothStep(vec.x, min, max), SmoothStep(vec.y, min, max));
        }

        public static Vector3 SmoothStep(Vector3 vec, float min, float max)
        {
            return new Vector3(SmoothStep(vec.x, min, max), SmoothStep(vec.y, min, max), SmoothStep(vec.z, min, max));
        }

        public static float Lerp(float start, float end, float value)
        {
            return ((1.0f - value) * start) + (value * end);
        }

        public static Vector3 NearestPoint(Vector3 lineStart, Vector3 lineEnd, Vector3 point)
        {
            Vector3 lineDirection = Vector3.Normalize(lineEnd - lineStart);
            float closestPoint = Vector3.Dot((point - lineStart), lineDirection);
            return lineStart + (closestPoint * lineDirection);
        }

        public static Vector3 NearestPointStrict(Vector3 lineStart, Vector3 lineEnd, Vector3 point)
        {
            Vector3 fullDirection = lineEnd - lineStart;
            Vector3 lineDirection = Vector3.Normalize(fullDirection);
            float closestPoint = Vector3.Dot((point - lineStart), lineDirection);
            return lineStart + (Mathf.Clamp(closestPoint, 0.0f, Vector3.Magnitude(fullDirection)) * lineDirection);
        }

        //Bounce
        public static float Bounce(float x)
        {
            return Mathf.Abs(Mathf.Sin(6.28f * (x + 1f) * (x + 1f)) * (1f - x));
        }

        public static Vector2 Bounce(Vector2 vec)
        {
            return new Vector2(Bounce(vec.x), Bounce(vec.y));
        }

        public static Vector3 Bounce(Vector3 vec)
        {
            return new Vector3(Bounce(vec.x), Bounce(vec.y), Bounce(vec.z));
        }

        // test for value that is near specified float (due to floating point inprecision)
        // all thanks to Opless for this!
        public static bool Approx(float val, float about, float range)
        {
            return ((Mathf.Abs(val - about) < range));
        }

        // test if a Vector3 is close to another Vector3 (due to floating point inprecision)
        // compares the square of the distance to the square of the range as this 
        // avoids calculating a square root which is much slower than squaring the range
        public static bool Approx(Vector3 val, Vector3 about, float range)
        {
            return ((val - about).sqrMagnitude < range * range);
        }

        /*
          * CLerp - Circular Lerp - is like lerp but handles the wraparound from 0 to 360.
          * This is useful when interpolating eulerAngles and the object
          * crosses the 0/360 boundary.  The standard Lerp function causes the object
          * to rotate in the wrong direction and looks stupid. Clerp fixes that.
          */
        public static float Clerp(float start, float end, float value)
        {
            float min = 0.0f;
            float max = 360.0f;
            float half = Mathf.Abs((max - min) / 2.0f); //half the distance between min and max
            float retval = 0.0f;
            float diff = 0.0f;

            if ((end - start) < -half)
            {
                diff = ((max - start) + end) * value;
                retval = start + diff;
            }
            else if ((end - start) > half)
            {
                diff = -((max - end) + start) * value;
                retval = start + diff;
            }
            else retval = start + (end - start) * value;

            // Debug.Log("Start: "  + start + "   End: " + end + "  Value: " + value + "  Half: " + half + "  Diff: " + diff + "  Retval: " + retval);
            return retval;
        }

    }
}
