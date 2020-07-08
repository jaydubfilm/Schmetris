using System;
using System.Collections;
using StarSalvager.Cameras;
using StarSalvager.Cameras.Data;
using StarSalvager.Values;
using UnityEngine;
using UnityEngine.InputSystem;

namespace StarSalvager.Utilities.Inputs
{
    public class InputManager : Singleton<InputManager>, IInput, IPausable
    {
        private Bot[] _bots;
        private ScrapyardBot[] _scrapyardBots;
        private Scrapyard _scrapyard;

        public bool isPaused => GameTimer.IsPaused;

        private ObstacleManager obstacleManager
        {
            get
            {
                if (_obstacleManager == null)
                    _obstacleManager = FindObjectOfType<ObstacleManager>();

                return _obstacleManager;
            }
        }
        private ObstacleManager _obstacleManager;

        private EnemyManager enemyManager
        {
            get
            {
                if (_enemyManager == null)
                    _enemyManager = FindObjectOfType<EnemyManager>();
                return _enemyManager;
            }
        }
        private EnemyManager _enemyManager;

        private CameraController cameraController
        {
            get
            {
                if (_cameraController == null)
                    _cameraController = FindObjectOfType<CameraController>();

                return _cameraController;
            }
        }
        private CameraController _cameraController;

        //============================================================================================================//

        private void Start()
        {
            Globals.OrientationChange += SetOrientation;
            GameTimer.AddPausable(this);
        }

        private void OnEnable()
        {
            _bots = FindObjectsOfType<Bot>();
            _scrapyardBots = FindObjectsOfType<ScrapyardBot>();
            _scrapyard = FindObjectOfType<Scrapyard>();
        } 

        private void OnDestroy()
        {
            Globals.OrientationChange -= SetOrientation;
        }


        //============================================================================================================//

        public void InitInput()
        {
            if (_bots == null || _bots.Length == 0)
                _bots = FindObjectsOfType<Bot>();

            if (_scrapyardBots == null || _scrapyardBots.Length == 0)
                _scrapyardBots = FindObjectsOfType<ScrapyardBot>();

            DeInitInput();
            
            switch (Globals.Orientation)
            {
                case ORIENTATION.VERTICAL:
                    Input.Actions.Default.SideMovement.Enable();
                    Input.Actions.Default.SideMovement.performed += SideMovement;

                    Input.Actions.Default.Rotate.Enable();
                    Input.Actions.Default.Rotate.performed += Rotate;

                    Input.Actions.Default.LeftClick.Enable();
                    Input.Actions.Default.LeftClick.performed += LeftClick;

                    Input.Actions.Default.RightClick.Enable();
                    Input.Actions.Default.RightClick.performed += RightClick;
                    break;
                case ORIENTATION.HORIZONTAL:
                    Input.Actions.Vertical.SideMovement.Enable();
                    Input.Actions.Vertical.SideMovement.performed += SideMovement;

                    Input.Actions.Vertical.Rotate.Enable();
                    Input.Actions.Vertical.Rotate.performed += Rotate;

                    Input.Actions.Default.LeftClick.Enable();
                    Input.Actions.Default.LeftClick.performed += LeftClick;

                    Input.Actions.Default.RightClick.Enable();
                    Input.Actions.Default.RightClick.performed += RightClick;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

        }

        public void DeInitInput()
        {
            Input.Actions.Default.SideMovement.Disable();
            Input.Actions.Default.SideMovement.performed -= SideMovement;

            Input.Actions.Default.Rotate.Disable();
            Input.Actions.Default.Rotate.performed -= Rotate;
            
            
            Input.Actions.Vertical.SideMovement.Disable();
            Input.Actions.Vertical.SideMovement.performed -= SideMovement;

            Input.Actions.Vertical.Rotate.Disable();
            Input.Actions.Vertical.Rotate.performed -= Rotate;

            Input.Actions.Default.LeftClick.Disable();
            Input.Actions.Default.LeftClick.performed -= LeftClick;

            Input.Actions.Default.RightClick.Disable();
            Input.Actions.Default.RightClick.performed -= RightClick;
        }
        
        //============================================================================================================//

        private void SetOrientation(ORIENTATION orientation)
        {
            InitInput();
        }
        
        //============================================================================================================//


        float _prevMove = 0.0f;

        private void SideMovement(InputAction.CallbackContext ctx)
        {
            if (isPaused)
                return;
            
            var move = ctx.ReadValue<float>();
            _prevMove = move;

            var noObstacles = obstacleManager is null;
            
            foreach (var bot in _bots)
            {
                bot.Move(move, noObstacles);
            }

            if (noObstacles)
                return;

            obstacleManager.Move(move);
            enemyManager.Move(move);
            cameraController.Move(move);
            LevelManager.Instance.ProjectileManager.Move(move);

            StartCoroutine(dasTimer(move));
        }

        private void SideMovement(float move)
        {
            if (isPaused)
                return;

            var noObstacles = obstacleManager is null;

            foreach (var bot in _bots)
            {
                bot.Move(move, noObstacles);
            }


            if (noObstacles)
                return;

            obstacleManager.Move(move);
            enemyManager.Move(move);
            cameraController.Move(move);

            LevelManager.Instance.ProjectileManager.Move(move);
        }

        IEnumerator dasTimer(float direction)
        {
            SideMovement(0.0f);
            yield return new WaitForSeconds(0.15f);

            if (_prevMove == direction)
            {
                SideMovement(direction);
            }
        }

        private void Rotate(InputAction.CallbackContext ctx)
        {
            if (isPaused)
                return;

            var rot = ctx.ReadValue<float>();

            foreach (var bot in _bots)
            {
                bot.Rotate(rot);
            }

            foreach (var scrapyardBot in _scrapyardBots)
            {
                scrapyardBot.Rotate(rot);
            }
        }

        private void LeftClick(InputAction.CallbackContext ctx)
        {
            var clicked = ctx.ReadValue<float>();

            if (clicked == 1)
            {
                if (_scrapyard != null)
                {
                    _scrapyard.OnLeftMouseButtonDown();
                }
            }
        }

        private void RightClick(InputAction.CallbackContext ctx)
        {

            var clicked = ctx.ReadValue<float>();

            if (clicked == 1)
            {
                if (_scrapyard != null)
                {
                    _scrapyard.OnRightMouseButtonDown();
                }
            }
        }

        //============================================================================================================//

        public void OnResume()
        {

        }

        public void OnPause()
        {

        }

        //============================================================================================================//
    }
}
