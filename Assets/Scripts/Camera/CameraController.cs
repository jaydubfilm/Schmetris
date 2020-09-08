using System;
using StarSalvager.Values;
using Sirenix.OdinInspector;
using StarSalvager.Cameras.Data;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.Inputs;
using UnityEngine;
using UnityEngine.SceneManagement;
using StarSalvager.Utilities.SceneManagement;

namespace StarSalvager.Cameras
{
    [DefaultExecutionOrder(-1000)]
    public class CameraController : MonoBehaviour, IMoveOnInput
    {
        //============================================================================================================//
        
        private Vector3 startPos;
        private Vector3 edgePos;
        private Vector3 targetPos;
        private float horzExtent;

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

        //Smooth camera to center over bot
        private void Update()
        {
            if (!Globals.CameraUseInputMotion || gameObject.scene.name != SceneLoader.LEVEL)
                return;

            if (transform.position != startPos &&
                (InputManager.Instance.MostRecentSideMovement == 0 ||
                transform.position.x > Globals.CameraOffsetBounds ||
                transform.position.x < -Globals.CameraOffsetBounds))
            {
                transform.position = Vector3.Lerp(transform.position, startPos, Globals.CameraSmoothing * Time.deltaTime);
                if (Vector3.Distance(transform.position, startPos) < 0.05f)
                {
                    transform.position = startPos;
                }
            }
                //transform.position = Vector3.MoveTowards(transform.position, startPos, Globals.CameraSmoothing * Time.deltaTime);
        }

        private void OnDestroy()
        {
            Globals.OrientationChange -= SetOrientation;
        }

        //============================================================================================================//

        private static Rect _cameraRect;

        public static bool IsPointInCameraRect(Vector2 position)
        {
            return _cameraRect.Contains(position);
        }

        private void UpdateRect()
        {
            var width = camera.aspect * 2f * camera.orthographicSize;
            var height = 2f * camera.orthographicSize;
            
            _cameraRect = new Rect
            {
                center = -new Vector2(width / 2f, height / 2f) + (Vector2)transform.position,
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

            //Scrapyard wants the camera anchored differently, so it uses a different formula
            //if (!inScrapyard)
            //{
            //    transform.position += Vector3.up * (orthographicSize / 2);
            //}
            //else
            //{
            //    transform.position += Vector3.down * (orthographicSize / 2) / 4;
            //    transform.position += Vector3.right * (orthographicSize * Screen.width / Screen.height) / 4;
            //}

            CameraOffset(botPosition, false);

            startPos = transform.position;
            targetPos = startPos;
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
            //else
            //{
            //    transform.position += Vector3.down * (camera.orthographicSize / 2) / 4;
            //    transform.position += Vector3.right * (camera.orthographicSize * Screen.width / Screen.height) / 4;
            //}

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
            
            /*m_currentInput = direction;
            
            if (m_currentInput == 0)
            {
                targetPos = startPos;
                return;
            }
            
            Vector3 cameraOff = startPos;
            cameraOff.x += -direction * horzExtent;
            
            edgePos = cameraOff;
            targetPos = edgePos;*/
        }

        //============================================================================================================//
    }
}

