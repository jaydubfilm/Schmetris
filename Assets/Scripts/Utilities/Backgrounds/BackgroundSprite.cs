﻿using Sirenix.OdinInspector;
using StarSalvager.Cameras.Data;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Values;
using UnityEngine;

namespace StarSalvager.Utilities.Backgrounds
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class BackgroundSprite : MonoBehaviour, IBackground
    {
        public float zDepth => _zDepth;
        //[SerializeField]
        private float _zDepth;

        //[SerializeField]
        //private Vector2 startPosition;

        public bool parentIsCamera => _parentIsCamera;
        [SerializeField]
        private bool _parentIsCamera;

        //public bool ignoreOrientationChanges => _ignoreOrientationChanges;
        //[SerializeField]
        //private bool _ignoreOrientationChanges;

        [SerializeField]
        private Vector2 moveSpeed;

        [SerializeField]
        private float horizontalMoveSpeed;

        [SerializeField]
        private Sprite[] spriteOptions;
        
        
        private new Transform transform;
        private new SpriteRenderer renderer;
        private IBackground _backgroundImplementation;
        private Camera _camera;
        
        //private static Plane[] _planes;
        private float lowestPoint;
        private float highestPoint;
        private int lastColumns;

        //============================================================================================================//

        public void Init(Transform cameraTransform, float zDepth)
        {
            _camera = cameraTransform.GetComponent<Camera>();
            transform = gameObject.transform;
            renderer = GetComponent<SpriteRenderer>();

            _zDepth = zDepth;
            
            var pos = transform.position;
            pos.z = zDepth;
            transform.position = pos;
            
            if(parentIsCamera)
                transform.SetParent(cameraTransform, true);

            //TODO Maybe i can set the color random as well?
            renderer.sprite = spriteOptions[Random.Range(0, spriteOptions.Length)];

            if (moveSpeed == Vector2.zero)
            {
                enabled = false;
                return;
            }
            
            //_planes = GeometryUtility.CalculateFrustumPlanes(_camera);
            lastColumns = Globals.ColumnsOnScreen;

        }

        [SerializeField, ReadOnly]
        private Vector3 horizontalMove;

        public void UpdatePosition(float moveAmount, bool ignoreInput = false)
        {
            lowestPoint = _camera.ViewportToWorldPoint(Vector3.zero).y;
            highestPoint = _camera.ViewportToWorldPoint(Vector3.one).y;
            
            if (Globals.ColumnsOnScreen != lastColumns)
                SetOrientation(ORIENTATION.VERTICAL);
            
            horizontalMove = GameTimer.IsPaused || ignoreInput
                ? Vector3.zero
                : Vector3.right * (horizontalMoveSpeed * moveAmount);
            
            var pos = transform.position;
            pos += ((Vector3)moveSpeed + horizontalMove) * Time.deltaTime;

            transform.position = pos;


            if (/*!GeometryUtility.TestPlanesAABB(_planes, renderer.bounds) && */transform.position.y < lowestPoint - renderer.bounds.extents.y)
            {
                pos = transform.localPosition;
                pos.y = Mathf.Abs(highestPoint + renderer.bounds.extents.y * 1.2f);
                
                transform.localPosition = pos;
            }
            
            

            //TODO Need to check if this object is in the camera view
        }

        public void SetOrientation(ORIENTATION newOrientation)
        {
            //_planes = GeometryUtility.CalculateFrustumPlanes(_camera);
            lowestPoint = _camera.ViewportToWorldPoint(Vector3.zero).y;
            highestPoint = _camera.ViewportToWorldPoint(Vector3.one).y;
            
            if (!parentIsCamera)
                return;
            
        }
    }
}

