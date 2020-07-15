using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using StarSalvager.Cameras.Data;
using UnityEngine;

namespace StarSalvager.Utilities.Backgrounds
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class BackgroundSprite : MonoBehaviour, IBackground
    {
        public float zDepth => _zDepth;
        [SerializeField]
        private float _zDepth;

        [SerializeField]
        private Vector2 startPosition;

        public bool parentIsCamera => _parentIsCamera;
        [SerializeField]
        private bool _parentIsCamera;

        public bool ignoreOrientationChanges => _ignoreOrientationChanges;
        [SerializeField]
        private bool _ignoreOrientationChanges;

        [SerializeField]
        private Vector2 moveSpeed;

        [SerializeField]
        private Sprite[] spriteOptions;
        
        
        private new Transform transform;
        private new SpriteRenderer renderer;
        private IBackground _backgroundImplementation;

        //============================================================================================================//

        public void Init(Transform cameraTransform)
        {
            transform = gameObject.transform;
            renderer = GetComponent<SpriteRenderer>();
            
            transform.position = new Vector3(startPosition.x, startPosition.y, zDepth);
            
            if(parentIsCamera)
                transform.SetParent(cameraTransform, true);

            //TODO Maybe i can set the color random as well?
            renderer.sprite = spriteOptions[Random.Range(0, spriteOptions.Length)];
            
        }

        public void UpdatePosition()
        {
            var pos = (Vector2)transform.position;
            pos += moveSpeed * Time.deltaTime;

            transform.position = pos;
            
            //TODO Need to check if this object is in the camera view
        }

        public void SetOrientation(ORIENTATION newOrientation)
        {
            if (ignoreOrientationChanges)
                return;
            
        }
    }
}

