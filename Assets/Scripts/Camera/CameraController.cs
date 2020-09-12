using System;
using System.Collections.Generic;
using StarSalvager.Values;
using Sirenix.OdinInspector;
using StarSalvager.Cameras.Data;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.Inputs;
using UnityEngine;
using StarSalvager.Utilities.SceneManagement;

namespace StarSalvager.Cameras
{
    [DefaultExecutionOrder(-1000)]
    public class CameraController : MonoBehaviour, IMoveOnInput
    {
        //============================================================================================================//
        
        private Vector3 startPos;
        private Vector3 beginningLerpPos;
        private float horzExtent;
        private float lerpValue = 0.0f;

        //============================================================================================================//

        protected new Transform transform
        {
            get
            {
                if (m_transform == null)
                    m_transform = gameObject.transform;

                return m_transform;
            }
        }

        private Transform m_transform;

        protected new Camera camera
        {
            get
            {
                if (_camera == null)
                    _camera = GetComponent<Camera>();

                return _camera;
            }
        }

        private Camera _camera;

        //============================================================================================================//

        //Init
        private void Start()
        {
            Globals.OrientationChange += SetOrientation;
            
            if(Globals.CameraUseInputMotion)
                RegisterMoveOnInput();
        }

        private void OnEnable()
        {
            SetOrthographicSize(Values.Constants.gridCellSize * Values.Globals.ColumnsOnScreen, Vector3.zero);
            SetOrientation(Values.Globals.Orientation);
        }

        private Vector3 tempPosition;

        //Smooth camera to center over bot
        private void Update()
        {
            if (!Globals.CameraUseInputMotion || gameObject.scene.name != SceneLoader.LEVEL)
                return;

            if (InputManager.Instance.MostRecentSideMovement == 0 && tempPosition == transform.position && lerpValue == 0.0f)
            {
                beginningLerpPos = transform.position;
            }
            else if (lerpValue == 0.0f)
            {
                beginningLerpPos = startPos;
            }

            if (beginningLerpPos != startPos &&
                (InputManager.Instance.MostRecentSideMovement == 0 ||
                transform.position.x > Globals.CameraOffsetBounds ||
                transform.position.x < -Globals.CameraOffsetBounds))
            {
                lerpValue = Mathf.Min(1.0f, lerpValue + Globals.CameraSmoothing * Time.deltaTime);
                //print(lerpValue + " --- " + Mathf.SmoothStep(0.0f, 1.0f, lerpValue));
                transform.position = Vector3.Lerp(beginningLerpPos, startPos, Mathf.SmoothStep(0.0f, 1.0f, lerpValue));
                if (lerpValue == 1.0f)
                {
                    transform.position = startPos;
                    lerpValue = 0.0f;
                }
            }

            tempPosition = transform.position;
        }

        private void OnDestroy()
        {
            Globals.OrientationChange -= SetOrientation;
        }

        //============================================================================================================//

        private static Rect _cameraRect;
        private static Vector2 center;
        private static Vector2 pos;

        private static Dictionary<float, Rect> checkRects;

        public static bool IsPointInCameraRect(Vector2 position)
        {
            return _cameraRect.Contains(position);
        }
        
        public static bool IsPointInCameraRect(Vector2 position, float xTotal)
        {
            if (checkRects == null)
                checkRects = new Dictionary<float, Rect>();


            //Don't want to be calculating the dimensions of the rectangle all the time, so store it for future use
            if (!checkRects.TryGetValue(xTotal, out var rect))
            {
                rect = _cameraRect;
                rect.width *= xTotal;
                //rect.height *= yTotal;
                rect.x = center.x + rect.width / 2f;
                
                
                
                checkRects.Add(xTotal, rect);
            }
            
            
            GizmoExtensions.DrawDebugRect(rect, Color.red);
            
            return rect.Contains(position);
        }

        
        
        private void UpdateRect()
        {
            float orthographicSize;
            var width = camera.aspect * 2f * (orthographicSize = camera.orthographicSize);
            var height = 2f * orthographicSize;

            
            pos = transform.position;
            center = -new Vector2(width / 2f, height / 2f) + pos;

            
            _cameraRect = new Rect
            {
                center = center,
                height = height,
                width = width,
            }; 
            
        }


        //================================================================================================================//

        public void MoveCameraWithObstacles(Vector3 toMoveCamera)
        {
            if (!Globals.CameraUseInputMotion)
                return;

            transform.position += toMoveCamera;

            if (transform.position.x > Globals.CameraOffsetBounds)
            {
                transform.position = new Vector3(Globals.CameraOffsetBounds, transform.position.y, transform.position.z);
            }
            else if (transform.position.x < -Globals.CameraOffsetBounds)
            {
                transform.position = new Vector3(-Globals.CameraOffsetBounds, transform.position.y, transform.position.z);
            }
        }

        public void SetOrthographicSize(float screenWidthInWorld, Vector3 botPosition)
        {
            var orthographicSize = screenWidthInWorld * (Screen.height / (float) Screen.width) / 2;
            camera.orthographicSize = orthographicSize;

            CameraOffset(botPosition, false);

            startPos = transform.position;
            beginningLerpPos = transform.position;
            //targetPos = startPos;
            horzExtent = orthographicSize * Screen.width / Screen.height / 2;

            UpdateRect();
        }

        public void CameraOffset(Vector3 pos, bool useHorizontalOffset)
        {
            transform.position = pos + Vector3.back * 10;
            
            //Scrapyard wants the camera anchored differently, so it uses a different formula
            if (!useHorizontalOffset)
            {
                transform.position += Vector3.up * (camera.orthographicSize / 2);
            }

            UpdateRect();
        }
        
        //================================================================================================================//

        private void SetOrientation(ORIENTATION orientation)
        {
            switch (Globals.Orientation)
            {
                case ORIENTATION.VERTICAL:
                    transform.rotation = Quaternion.identity;
                    break;
                case ORIENTATION.HORIZONTAL:
                    transform.rotation = Quaternion.Euler(0,0,90);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(orientation), orientation, null);
            }

            UpdateRect();
        }
        
        #if UNITY_EDITOR

        [Button("Toggle Orientation"), DisableInEditorMode]
        private void ToggleOrientation()
        {
            switch (Globals.Orientation)
            {
                case ORIENTATION.VERTICAL:
                    Globals.Orientation = ORIENTATION.HORIZONTAL;
                    break;
                case ORIENTATION.HORIZONTAL:
                    Globals.Orientation = ORIENTATION.VERTICAL;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(Globals.Orientation), Globals.Orientation, null);
            }
        }

        private void OnDrawGizmosSelected()
        {
            GizmoExtensions.DrawRect(_cameraRect, Color.cyan);
        }

#endif
        

        //IMoveOnInput functions
        //================================================================================================================//

        public void RegisterMoveOnInput()
        {
            InputManager.RegisterMoveOnInput(this);
        }
            
        public void Move(float direction)
        {
            if (!Globals.CameraUseInputMotion)
                return;

            if (direction != 0)
            {
                beginningLerpPos = startPos;
                lerpValue = 0.0f;
            }
        }

        //====================================================================================================================//
        
    }
}

