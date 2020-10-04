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
        public enum STATE
        {
            NONE,
            RECENTER,
            MOTION
        }
        
        //============================================================================================================//
        
        
        #region Properties

        private Vector3 _startPos;
        private Vector3 _beginningLerpPos;
        
        private float _lerpValue;

        //============================================================================================================//

        private new Transform transform
        {
            get
            {
                if (_mTransform == null)
                    _mTransform = gameObject.transform;

                return _mTransform;
            }
        }

        private Transform _mTransform;

        public static Camera Camera;

        private new Camera camera
        {
            get
            {
                if (_camera == null)
                {
                    _camera = GetComponent<Camera>();
                    
                }

                Camera = _camera;
                return _camera;
            }
        }
        private Camera _camera;

        //Camera movement
        //====================================================================================================================//
        
        public static float CAMERA_DELTA => (_current - _last).magnitude;
        private static Vector2 _current, _last;
        
        public static STATE CurrentState { get; private set; }

        private bool _atBounds;

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

            //--------------------------------------------------------------------------------------------------------//

            if (InputManager.Instance.MostRecentSideMovement != 0)
            {
                CurrentState = _atBounds ? STATE.MOTION : STATE.NONE;
            }
            else if (_beginningLerpPos != _startPos && InputManager.Instance.MostRecentSideMovement == 0)
            {
                CurrentState = STATE.RECENTER;
            }
            else
            {
                CurrentState = STATE.NONE;
            }

            //--------------------------------------------------------------------------------------------------------//

            if (InputManager.Instance.MostRecentSideMovement == 0 && tempPosition == transform.position &&
                _lerpValue == 0.0f)
            {
                _beginningLerpPos = transform.position;
            }
            else if (_lerpValue == 0.0f)
            {
                _beginningLerpPos = _startPos;
            }

            if (_beginningLerpPos != _startPos &&
                (InputManager.Instance.MostRecentSideMovement == 0 ||
                 transform.position.x > Globals.CameraOffsetBounds ||
                 transform.position.x < -Globals.CameraOffsetBounds))
            {
                _lerpValue = Mathf.Min(1.0f, _lerpValue + Globals.CameraSmoothing * Time.deltaTime);
                transform.position = Vector3.Lerp(_beginningLerpPos, _startPos, Mathf.SmoothStep(0.0f, 1.0f, _lerpValue));
                if (_lerpValue == 1.0f)
                {
                    transform.position = _startPos;
                    _lerpValue = 0.0f;
                }

                _cameraXOffset = transform.position.x;
                
                _last = _current;
                _current = tempPosition;
            }
            else
            {
                _last = _current = Vector2.zero;
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

        [Obsolete("This should not move using the ObstacleManager")]
        public void MoveCameraWithObstacles(Vector3 toMoveCamera)
        {
            if (!Globals.CameraUseInputMotion)
                return;

            var newPosition = transform.position;
            newPosition += toMoveCamera;

            //_atBounds = ClampX(Globals.CameraOffsetBounds, -Globals.CameraOffsetBounds, ref newPosition);

            if (newPosition.x > Globals.CameraOffsetBounds)
            {
                newPosition = new Vector3(Globals.CameraOffsetBounds, newPosition.y, newPosition.z);
                _atBounds = true;
            }
            else if (newPosition.x < -Globals.CameraOffsetBounds)
            {
                newPosition = new Vector3(-Globals.CameraOffsetBounds, newPosition.y, newPosition.z);
                _atBounds = true;
            }
            else
            {
                _atBounds = false;
            }

            transform.position = newPosition;

            _cameraXOffset = newPosition.x;
        }

        public void SetOrthographicSize(float screenWidthInWorld, Vector3 botPosition)
        {
            var orthographicSize = screenWidthInWorld * (Screen.height / (float) Screen.width) / 2;
            camera.orthographicSize = orthographicSize;

            CameraOffset(botPosition, false);

            _startPos = transform.position;
            _beginningLerpPos = transform.position;
            //targetPos = startPos;
            //horzExtent = orthographicSize * Screen.width / Screen.height / 2;

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


        private static bool ClampX(float min, float max, ref Vector3 value)
        {
            var clamped = value.x < min || value.x > max;

            value.x = Mathf.Clamp(value.x, min, max);

            return clamped;
        }

        //IMoveOnInput functions
        //================================================================================================================//

        #region IMoveOnInput

        public void RegisterMoveOnInput()
        {
            InputManager.RegisterMoveOnInput(this);
        }
            
        public void Move(float direction)
        {
            if (!Globals.CameraUseInputMotion)
                return;

            if (direction == 0) 
                return;
            
            _beginningLerpPos = _startPos;
            _lerpValue = 0.0f;
        }

        #endregion //IMoveOnInput

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

