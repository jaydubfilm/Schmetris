using UnityEngine;

namespace StarSalvager
{
    public class Shield : Actor2DBase
    {
        //============================================================================================================//

        public void SetAlpha(float value)
        {
            var color = renderer.color;

            color.a = value;

            renderer.color = color;
            
        }
        public void SetAlpha(int a)
        {
            SetAlpha(a / 255f);
        }
        
        //============================================================================================================//

        public void SetSize(int radius)
        {
            transform.localScale = new Vector3(radius, radius, 1);
        }

        
    }
}

