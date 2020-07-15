using System;
using StarSalvager.Values;
using Sirenix.OdinInspector;
using StarSalvager.Cameras.Data;
using StarSalvager.Utilities.Inputs;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StarSalvager.Cameras
{
    public class CameraController : MonoBehaviour, IMoveOnInput
    {
        //============================================================================================================//

        private Vector3 startPos;
        private Vector3 edgePos;
        private Vector3 targetPos;
        public float smoothing = 4.0f;
        private float horzExtent;

        //Input Manager variables - -1.0f for left, 0 for nothing, 1.0f for right
        private float m_currentInput;

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
            RegisterMoveOnInput();
        }

        private void OnEnable()
        {
            SetOrthographicSize(Values.Constants.gridCellSize * Values.Globals.ColumnsOnScreen, Vector3.zero, gameObject.scene == SceneManager.GetSceneByName("ScrapyardScene"));
            SetOrientation(Values.Globals.Orientation);
        }

        //Smooth camera to center over bot
        private void Update()
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, smoothing * Time.deltaTime);
        }

        private void OnDestroy()
        {
            Globals.OrientationChange -= SetOrientation;
        }

        //============================================================================================================//

        public void SetOrthographicSize(float screenWidthInWorld, Vector3 botPosition, bool inScrapyard = false)
        {
            var orthographicSize = screenWidthInWorld * (Screen.height / (float) Screen.width) / 2;
            camera.orthographicSize = orthographicSize;
            transform.position =
                botPosition +
                Vector3.back * 10;

            //Scrapyard wants the camera anchored differently, so it uses a different formula
            if (!inScrapyard)
            {
                transform.position += Vector3.up * (orthographicSize / 2);
            }
            else
            {
                transform.position += Vector3.down * (orthographicSize / 2) / 4;
                transform.position += Vector3.right * (orthographicSize * Screen.width / Screen.height) / 4;
            }

            startPos = transform.position;
            targetPos = startPos;
            horzExtent = orthographicSize * Screen.width / Screen.height / 2;
        }

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
        
        #endif
        

        //IMoveOnInput functions
        //================================================================================================================//

        public void RegisterMoveOnInput()
        {
            InputManager.RegisterMoveOnInput(this);
        }

        public void Move(float direction)
        {
            m_currentInput = direction;

            if (m_currentInput == 0)
            {
                targetPos = startPos;
                return;
            }

            Vector3 cameraOff = startPos;
            cameraOff.x += -direction * horzExtent;

            edgePos = cameraOff;
            targetPos = edgePos;
        }

        //============================================================================================================//
    }
}

