using Recycling;
using StarSalvager.Factories;
using UnityEngine;


namespace StarSalvager.Utilities
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class FlashSprite : MonoBehaviour, IRecycled, ICustomRecycle
    {
        [SerializeField]
        private float onTime;
        [SerializeField]
        private float offTime;

        private float _timer;
        private bool _isOn;
        
        
        public bool IsRecycled { get; set; }

        private bool _active;

        //============================================================================================================//
        
        public new SpriteRenderer renderer
        {
            get
            {
                if (_renderer == null)
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

        private void LateUpdate()
        {
            if (!_active)
                return;
            
            if (_timer >= (_isOn ? onTime : offTime))
            {
                _isOn = !_isOn;
                renderer.enabled = _isOn;
                
                _timer = 0f;
            }
            else
            {
                _timer += Time.deltaTime;
            }
            
            //This doesn't need to happen anymore because the icon is no longer part of the flashing
            ////Force the rotation to remain as default
            //transform.rotation = Quaternion.identity;
        }
        
        //============================================================================================================//

        public void SetColor(Color color)
        {
            renderer.color = color;
        }
        
        //============================================================================================================//


        public void SetActive(bool state)
        {
            if (state == _active)
                return;
            
            _active = state;

            renderer.enabled = state;
        }
        
        //============================================================================================================//

        public void CustomRecycle(params object[] args)
        {
            SetColor(Color.white);
        }

        //====================================================================================================================//
        
        public static FlashSprite Create(Transform parent, Vector3 localPosition, Color color, bool startActive = true)
        {
            var flashSprite = FactoryManager.Instance.GetFactory<EffectFactory>().CreateObject<FlashSprite>();
            flashSprite.transform.SetParent(parent);
            flashSprite.transform.localPosition = localPosition;
            flashSprite.transform.localScale = Vector3.one;

            flashSprite.SetColor(color);
            flashSprite.SetActive(startActive);

            return flashSprite;
        }

        //====================================================================================================================//
        
    }
}

