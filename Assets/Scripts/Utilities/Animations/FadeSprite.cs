using System;
using Recycling;
using StarSalvager.Factories;
using UnityEngine;

namespace StarSalvager.Utilities.Animations
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class FadeSprite : MonoBehaviour, IRecycled, ICustomRecycle
    {
        [SerializeField]
        private float cycleTime;

        private float _timer;

        private Color _color, _clearColor;
        
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

            _timer = Mathf.PingPong(Time.time, cycleTime);

            renderer.color = Color.Lerp(_color, _clearColor, _timer / cycleTime);
            //This doesn't need to happen anymore because the icon is no longer part of the flashing
            ////Force the rotation to remain as default
            //transform.rotation = Quaternion.identity;
        }
        
        //============================================================================================================//

        public void SetColor(Color color)
        {
            _clearColor = _color = color;
            _clearColor.a = 0f;
            
            renderer.color = _color;
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
            _timer = 0f;
        }

        //====================================================================================================================//
        
        public static FadeSprite Create(Transform parent, Vector3 localPosition, Color color, bool startActive = true)
        {
            var fadeSprite = FactoryManager.Instance.GetFactory<EffectFactory>().CreateObject<FadeSprite>();
            fadeSprite.transform.SetParent(parent);
            fadeSprite.transform.localPosition = localPosition;

            fadeSprite.SetColor(color);
            fadeSprite.SetActive(startActive);

            return fadeSprite;

        }

        //====================================================================================================================//
    }

}