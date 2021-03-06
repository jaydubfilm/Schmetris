﻿using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace StarSalvager.Utilities.UI
{
    [RequireComponent(typeof(Image))]
    public class FadeUIImage : MonoBehaviour
    {
        [SerializeField] private float cycleTime;

        private float _timer;

        private Color _color, _clearColor;

        private bool _active = true;

        //============================================================================================================//

        private Image Image
        {
            get
            {
                if (_image == null)
                    _image = GetComponent<Image>();

                return _image;
            }
        }

        private Image _image;

        //============================================================================================================//

        private void Start()
        {
            SetColor(Image.color);
            SetActive(false);
        }

        private void LateUpdate()
        {
            if (!_active)
                return;

            _timer = Mathf.PingPong(Time.time, cycleTime);

            Image.color = Color.Lerp(_color, _clearColor, _timer / cycleTime);
            //This doesn't need to happen anymore because the icon is no longer part of the flashing
            ////Force the rotation to remain as default
            //transform.rotation = Quaternion.identity;
        }

        //============================================================================================================//

        private void SetColor(Color color)
        {
            _clearColor = _color = color;
            _clearColor.a = 0f;

            Image.color = _color;
        }

        //============================================================================================================//


        public void SetActive(bool state)
        {
            if (state == _active)
                return;

            _active = state;

            Image.enabled = state;
        }

        //====================================================================================================================//

        public void FlashOnce()
        {
            if (!gameObject.activeInHierarchy)
                return;
            
            if (Image.enabled)
                return;
            
            StartCoroutine(FlashOnceCoroutine());
        }

        private IEnumerator FlashOnceCoroutine()
        {
            var t = 0f;
            Image.enabled = true;


            while (t / cycleTime <= 1f)
            {
                
                Image.color = Color.Lerp(_clearColor, _color, t / cycleTime);
                
                t += Time.deltaTime;
                
                yield return null;
            }
            
            while (t / cycleTime > 0f)
            {
                
                Image.color = Color.Lerp(_clearColor, _color, t / cycleTime);
                
                t -= Time.deltaTime;
                
                yield return null;
            }
            
            Image.enabled = false;

        }
    }
}