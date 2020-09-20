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

        #region Properties

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

        #endregion //Properties

        //============================================================================================================//

        #region Unity Functions

        private void Start()
        {
            Globals.OrientationChange += SetOrientation;
            
            if(Globals.CameraUseInputMotion)
                RegisterMoveOnInput();
        }

        private void OnEnable()
        {
            SetOrthographicSize(Constants.gridCellSize * Globals.ColumnsOnScreen, Vector3.zero);
            SetOrientation(Globals.Orientation);
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

                _cameraXOffset = transform.position.x;
            }

            tempPosition = transform.position;
        }

        private void LateUpdate()
        {
            CheckForCanBeSeen();
        }

        private void OnDestroy()
        {
            Globals.OrientationChange -= SetOrientation;
        }

        #endregion //Unity Functions

        //============================================================================================================//

        #region Camera Rect

        private static float _cameraXOffset;
        private static Rect _cameraRect;
        private static Vector2 center;
        private static Vector2 pos;

        private static Dictionary<float, Rect> checkRects;

        private static List<ICanBeSeen> _canBeSeens;
        

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

                //Offset by the remaining area
                rect.x += (_cameraRect.width * (1f - xTotal)) / 2f;
                
                checkRects.Add(xTotal, rect);
            }
            
            
            var tempRect = rect;
            tempRect.x += _cameraXOffset * -1f;
            
            GizmoExtensions.DrawDebugRect(tempRect, Color.red);
            
            return tempRect.Contains(position);
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

        //====================================================================================================================//
        
        //TODO May want to change the naming of some of the CanBeSeen properties
        public static void RegisterCanBeSeen(ICanBeSeen canBeSeen)
        {
            if(_canBeSeens == null)
                _canBeSeens = new List<ICanBeSeen>();
            
            _canBeSeens.Add(canBeSeen);
        }
        
        public static void UnRegisterCanBeSeen(ICanBeSeen canBeSeen)
        {
            if (_canBeSeens == null || _canBeSeens.Count == 0)
                return;

            if (!_canBeSeens.Contains(canBeSeen))
                return;

            canBeSeen.ExitedCamera();
            canBeSeen.IsSeen = false;
            
            _canBeSeens.Remove(canBeSeen);
        }

        private static void CheckForCanBeSeen()
        {
            if (_canBeSeens == null)
                return;
            
            foreach (var canBeSeen in _canBeSeens)
            {
                CheckCanBeSeen(canBeSeen);
            }
        }

        private static void CheckCanBeSeen(ICanBeSeen canBeSeen)
        {
            var seen = IsPointInCameraRect(canBeSeen.transform.position, canBeSeen.CameraCheckArea);

            if (canBeSeen.IsSeen == seen)
                return;

            canBeSeen.IsSeen = seen;
                
            if(seen) canBeSeen.EnteredCamera();
            else canBeSeen.ExitedCamera();
        }

        #endregion //Camera Rect


        //================================================================================================================//

        public void MoveCameraWithObstacles(Vector3 toMoveCamera)
        {
            if (!Globals.CameraUseInputMotion)
                return;

            var newPosition = transform.position;
            newPosition += toMoveCamera;

            if (newPosition.x > Globals.CameraOffsetBounds)
            {
                newPosition = new Vector3(Globals.CameraOffsetBounds, newPosition.y, newPosition.z);
            }
            else if (newPosition.x < -Globals.CameraOffsetBounds)
            {
                newPosition = new Vector3(-Globals.CameraOffsetBounds, newPosition.y, newPosition.z);
            }

            transform.position = newPosition;

            _cameraXOffset = newPosition.x;
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

        #region Unity Editor Functions

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

        #endregion //Unity Editor Functions

        //====================================================================================================================//
        
    }
}

