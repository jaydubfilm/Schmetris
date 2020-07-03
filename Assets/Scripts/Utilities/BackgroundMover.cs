﻿using UnityEngine;

namespace StarSalvager.Utilities
{
    public class BackgroundMover : MonoBehaviour
    {
        [SerializeField]
        private float backgroundSpeed;
        
        private Material _material;
        
        //============================================================================================================//
        
        // Start is called before the first frame update
        private void Start()
        {
            _material = GetComponent<Renderer>().material;
        }

        // Update is called once per frame
        void Update()
        {
            SetOffset(Vector2.down * (backgroundSpeed * Time.deltaTime));
        }
        
        //============================================================================================================//

        public void SetOffset(Vector2 offsetDelta)
        {
            var offset = _material.mainTextureOffset;

            offset += offsetDelta;

            if (Mathf.Abs(offset.x) >= 1f)
                offset.x = 0f;
            
            if (Mathf.Abs(offset.y) >= 1f)
                offset.y = 0f;

            _material.mainTextureOffset = offset;
        }
        
        //============================================================================================================//
        
    }
}


