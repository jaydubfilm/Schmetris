using System;
using StarSalvager.Values;
using Sirenix.OdinInspector;
using StarSalvager.Cameras.Data;
using UnityEngine;

namespace StarSalvager.Cameras
{
    public class CameraController : MonoBehaviour
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
                    _camera = Camera.main;

                return _camera;
            }
        }

        private Camera _camera;

        //============================================================================================================//

        //Init
        private void Start()
        {
            //DontDestroyOnLoad(this);
            startPos = transform.position;
            targetPos = startPos;
            horzExtent = camera.orthographicSize * Screen.width / Screen.height / 2;

            Globals.OrientationChange += SetOrientation;
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

        public void SetOrthographicSize(float screenWidthInWorld)
        {
            var orthographicSize = camera.orthographicSize;
            
            orthographicSize = screenWidthInWorld * (Screen.height / (float) Screen.width) / 2;
            camera.orthographicSize = orthographicSize;
            transform.position =
                LevelManager.Instance.BotGameObject.transform.position +
                Vector3.back * 10 +
                Vector3.up * (orthographicSize / 2);

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
        

        //============================================================================================================//

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

