﻿using Recycling;
using UnityEngine;


namespace StarSalvager.Utilities
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class FlashSprite : MonoBehaviour, IRecycled
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
            
            //Force the rotation to remain as default
            transform.rotation = Quaternion.identity;
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
        
    }
}
