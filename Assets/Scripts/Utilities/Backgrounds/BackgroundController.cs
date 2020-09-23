using System;
using Sirenix.OdinInspector;
using StarSalvager.Cameras;
using StarSalvager.Cameras.Data;
using StarSalvager.Utilities.Inputs;
using StarSalvager.Values;
using UnityEngine;

namespace StarSalvager.Utilities.Backgrounds
{
    public class BackgroundController : MonoBehaviour, IPausable, IMoveOnInput
    {
        private static bool IgnoreInput => false;//Globals.CameraUseInputMotion;
        
        public bool isPaused => GameTimer.IsPaused && !ignorePaused;
        
        [SerializeField]
        private bool ignorePaused;
        
        private Transform _cameraTransform;
        private IBackground[] _backgrounds;

        //Unity Functions
        //================================================================================================================//

        #region Unity Functions

        private void Start()
        {
            RegisterPausable();
        }

        private void OnEnable()
        {
            FindCamera();
            
            _backgrounds = GetComponentsInChildren<IBackground>();

            InitBackgrounds();

            Globals.OrientationChange += SetOrientation;
            SetOrientation(Globals.Orientation);
        }
        private void LateUpdate()
        {
            //If the Camera is off, we're using a different one
            if (_cameraTransform.gameObject.activeInHierarchy == false)
            {
                FindCamera();
                InitBackgrounds();
            }

            if (isPaused)
                return;

            //var moveAmount = -(CameraController.CameraXOffset / Globals.BotHorizontalSpeed);
            //var moveAmount = -(CameraController.CameraXOffset / Globals.CameraOffsetBounds);
            var moveAmount = -ObstacleManager.TEST_MOVEDELTA;
            
            
            System.Console.WriteLine(string.Empty);
            foreach (var background in _backgrounds)
            {
                background.UpdatePosition(moveAmount, IgnoreInput);
            }

        }

        #endregion //Unity Functions

        //BackgroundController Functions
        //================================================================================================================//

        private void FindCamera()
        {
            _cameraTransform = FindObjectOfType<Camera>()?.transform;
            
            if(_cameraTransform == null)
                throw new NullReferenceException("No Active Camera found");
        }
        
        private void InitBackgrounds()
        {
            var count = _backgrounds.Length;
            
            for (var i = 0; i < count; i++)
            {
                _backgrounds[i].Init(_cameraTransform, count - i);
            }
        }

        private void SetOrientation(ORIENTATION newOrientation)
        {
            foreach (var background in _backgrounds)
            {
                background.SetOrientation(newOrientation);
            }
            
        }

        //================================================================================================================//

        public void SetActive(bool state)
        {
            foreach (var background in _backgrounds)
            {
                background.gameObject.SetActive(state);
            }   
        }

        //IPausable Functions
        //====================================================================================================================//

        #region IPausable

        public void RegisterPausable()
        {
            GameTimer.AddPausable(this);
        }

        public void OnResume()
        {
        }

        public void OnPause()
        {
        }

        #endregion //IPausable

        //IMoveOnInput Functions
        //====================================================================================================================//
        
        public void RegisterMoveOnInput()
        {
            throw new NotImplementedException();
        }

        public void Move(float direction)
        {
            throw new NotImplementedException();
        }

        //====================================================================================================================//
        
        #region Unity Editor

#if UNITY_EDITOR

        [Button("Disable Backgrounds"), HorizontalGroup("BackgroundEditor")]
        private void DisableBackgrounds()
        {
            SetActive(false);
        }
        
        [Button("Enable Backgrounds"), HorizontalGroup("BackgroundEditor")]
        private void EnableBackgrounds()
        {
            SetActive(true);
        }
        
#endif

        #endregion //Unity Editor


    }

}

