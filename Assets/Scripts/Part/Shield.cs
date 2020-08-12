using Recycling;
using UnityEngine;

namespace StarSalvager
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class Shield : MonoBehaviour, IRecycled
    {
        public bool IsRecycled { get; set; }
        
        public new SpriteRenderer renderer
        {
            get
            {
                if (!_renderer)
                    _renderer = GetComponent<SpriteRenderer>();

                return _renderer;
            }
        }
        private SpriteRenderer _renderer;
        
        public new Transform transform
        {
            get
            {
                if (!_transform)
                    _transform = gameObject.transform;

                return _transform;
            }
        }
        private Transform _transform;

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

