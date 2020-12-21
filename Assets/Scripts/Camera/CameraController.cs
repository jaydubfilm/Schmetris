using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using StarSalvager.Values;
using Sirenix.OdinInspector;
using StarSalvager.Cameras.Data;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.Inputs;
using UnityEngine;
using StarSalvager.Utilities.SceneManagement;

namespace StarSalvager.Cameras
{
    //TODO The addition of the second camera, made this messy it needs to be cleaned
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
        
        [Required]
        public CinemachineVirtualCamera CinemachineVirtualCamera;
        [Required]
        public CinemachineVirtualCamera CinemachineReCenterVirtualCamera;

        public float blendTime = 1f;

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
            
            transform.position = Vector3.back * 10f;
        }


        private Vector3 tempPosition;

        //Smooth camera to center over bot
        private void Update()
        {
            UpdateRect();
            
            if (!Globals.CameraUseInputMotion || gameObject.scene.name != SceneLoader.LEVEL)
                return;
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
            /*if (checkRects == null)
                checkRects = new Dictionary<float, Rect>();


            //Don't want to be calculating the dimensions of the rectangle all the time, so store it for future use
            if (!checkRects.TryGetValue(xTotal, out var rect))
            {
                
                rect = _cameraRect;
                rect.width *= xTotal;

                //Offset by the remaining area
                rect.x += (_cameraRect.width * (1f - xTotal)) / 2f;
                
                checkRects.Add(xTotal, rect);
            }*/
            
            var rect = _cameraRect;
            rect.width *= xTotal;

            //Offset by the remaining area
            rect.x += (_cameraRect.width * (1f - xTotal)) / 2f;
            
            
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

            
            pos = camera.transform.position;
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

            SetOrthoSize(orthographicSize);
        }

        public void CameraOffset(Vector3 pos, bool useHorizontalOffset)
        {
            transform.position = pos + Vector3.back * 10;
            
            //Scrapyard wants the camera anchored differently, so it uses a different formula
            if (!useHorizontalOffset)
            {
                transform.position += Vector3.up * (camera.orthographicSize / 2);
            }
            else
            {
                transform.position += Vector3.down * 2;
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


        //Virtual Camera
        //====================================================================================================================//

        public void SetLookAtFollow(Transform target)
        {
            CinemachineVirtualCamera.LookAt = target;
            CinemachineVirtualCamera.Follow = target;
            
            CinemachineReCenterVirtualCamera.LookAt = target;
            CinemachineReCenterVirtualCamera.Follow = target;

            StartCoroutine(RecenterCameraCoroutine());
        }

        private IEnumerator RecenterCameraCoroutine()
        {
            CinemachineReCenterVirtualCamera.Priority = 100;
            
            yield return new WaitForSeconds(blendTime);
            
            CinemachineReCenterVirtualCamera.Priority = 0;
            
            yield return new WaitForSeconds(blendTime);
        }

        /*public void SetDeadzone(float width = 0.1f, float height = 0f)
        {
            var framingTransposer = CinemachineVirtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
            
            framingTransposer.m_DeadZoneWidth = width;
            framingTransposer.m_DeadZoneHeight = height;

            framingTransposer.m_SoftZoneHeight = Mathf.Max(height, 0.8f);
        }*/

        public void SetTrackedOffset(float x = 0f, float y = 0f, float z = 0f)
        {
            /*var framingTransposers = new[]
            {
                CinemachineVirtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>(),
                CinemachineReCenterVirtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>(),
            };


            /*var targetPosition = CinemachineVirtualCamera.m_LookAt.position + new Vector3(x, y * 2f, z);
            var newPos = CinemachineVirtualCamera.m_LookAt.InverseTransformPoint(targetPosition);#1#

            var targetPosition = CinemachineVirtualCamera.m_LookAt.position + new Vector3(x, y, z);
            var newPos = CinemachineVirtualCamera.m_LookAt.InverseTransformPoint(targetPosition);
            
            //framingTransposer.m_TrackedObjectOffset = newPos;

            foreach (var transposer in framingTransposers)
            {
                transposer.m_TrackedObjectOffset = newPos;
            }*/
        }

        public void SetOrthoSize(float size)
        {
            if (!CinemachineVirtualCamera)
                return;

            CinemachineVirtualCamera.m_Lens.OrthographicSize = size;
            
            CinemachineReCenterVirtualCamera.m_Lens.OrthographicSize = size;
        }

        public void ResetCameraPosition()
        {
            SetTrackedOffset();
            CinemachineVirtualCamera.transform.position = new Vector3(0f, 13.39f, -10f);
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
            /*if (!Globals.CameraUseInputMotion)
                return;

            if (direction == 0) 
                return;
            
            _beginningLerpPos = _startPos;
            _lerpValue = 0.0f;*/
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

