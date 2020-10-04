namespace StarSalvager.Utilities.Math
{
    public static class MathS
    {
        public static float ClampAngle(float angle)
        {
            angle %= 360;

            if (angle < 0)
            {
                return 360 + angle;
            }

            return angle;
        }
    }

}