using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
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
        private BotShapeEditor _botShapeEditor;

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
        
        
        [SerializeField, BoxGroup("DAS"), DisableInPlayMode]
        private float DASTime = 0.15f;
        [SerializeField, BoxGroup("DAS"), ReadOnly]
        private float dasTimer;
        [SerializeField, BoxGroup("DAS"), ReadOnly]
        private bool dasTriggered;
        [SerializeField, BoxGroup("DAS"), ReadOnly]
        private float previousInput;
        [SerializeField, BoxGroup("DAS"), ReadOnly]
        private float currentInput;

        //============================================================================================================//

        private void Start()
        {
            Globals.DASTime = DASTime;

            Globals.OrientationChange += SetOrientation;
            GameTimer.AddPausable(this);
        }

        private void Update()
        {
            DasChecks();
        }

        private void OnEnable()
        {
            _bots = FindObjectsOfType<Bot>();
            _scrapyardBots = FindObjectsOfType<ScrapyardBot>();
            _scrapyard = FindObjectOfType<Scrapyard>();
            _botShapeEditor = FindObjectOfType<BotShapeEditor>();
        } 

        private void OnDestroy()
        {
            Globals.OrientationChange -= SetOrientation;
        }


        //============================================================================================================//

        private static List<IMoveOnInput> moveOnInput;
        
        public static void RegisterMoveOnInput(IMoveOnInput toAdd)
        {
            if(moveOnInput == null)
                moveOnInput = new List<IMoveOnInput>();
            
            moveOnInput.Add(toAdd);
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

        

        //float _prevMove = 0.0f;

        private void DasChecks()
        {
            //If the user is no longer pressing a direction, these checks do not matter
            if (currentInput == 0f)
                return;
            
            //If we've already triggered the DAS, don't bother with following checks
            if (dasTriggered)
                return;

            //If timer hasn't reached zero, continue counting down
            if (dasTimer > 0f)
            {
                dasTimer -= Time.deltaTime;
                return;
            }

            dasTriggered = true;
            dasTimer = 0f;
            
            //If the User is still pressing the same input, go ahead and try and reapply it
            if(currentInput == previousInput)
                TryApplyMove(currentInput);
        }

        private void SideMovement(InputAction.CallbackContext ctx)
        {
            if (isPaused)
                return;
            
            var moveDirection = ctx.ReadValue<float>();

            TryApplyMove(moveDirection);

            
            //This check needs to happen after TryApplyMove as it could cause the Move to never trigger
            if (moveDirection != 0f) 
                return;
            
            //If the user has released the key, we can reset the DAS system
            dasTriggered = false;
            dasTimer = 0f;

        }

        /// <summary>
        /// Considers DAS values when passing the input information to Move
        /// </summary>
        /// <param name="moveDirection"></param>
        private void TryApplyMove(float moveDirection)
        {
            currentInput = moveDirection;
            
            //If we're trying to move, set things up for the DAS movement
            if (!dasTriggered)
            {
                //If the timer is still counting down
                if (dasTimer > 0f)
                    return;
            
                //If this is the first time its pressed, set the press directions
                previousInput = currentInput;

                //Set the countdown timer to the intended value
                dasTimer = DASTime;
                
                //Quickly move the relevant managers, then reset their input, so that they will pause until DAS is ready
                Move(currentInput);
                Move(0);
                return;
            }
            
            //If the DAS has triggered already, go ahead and update the relevant managers
            Move(currentInput);
        }

        /// <summary>
        /// Applies the move value to relevant Managers
        /// </summary>
        /// <param name="value"></param>
        private void Move(float value)
        {
            for (var i = moveOnInput.Count - 1; i >= 0; i--)
            {
                var move = moveOnInput[i];
                //Automatically unregister things that may have been deleted
                if (move == null)
                {
                    moveOnInput.RemoveAt(i);
                    continue;
                }
                
                move.Move(value);
            }
            //if (obstacleManager != null)
            //    obstacleManager.Move(value);
            //if (enemyManager != null)
            //    enemyManager.Move(value);
            //if (cameraController != null)
            //    cameraController.Move(value);
            //if (LevelManager.Instance != null)
            //    LevelManager.Instance.ProjectileManager.Move(value);
        }

        /*private void SideMovement(float move)
        {
            if (isPaused)
                return;

            var noObstacles = obstacleManager is null;

            foreach (var bot in _bots)
            {
                bot.Move(move, noObstacles);
            }


            if (noObstacles)
            {
                foreach (var bot in _bots)
                {
                    bot.Move(move, noObstacles);
                }
                return;
            }
            
            if(!obstacleManager.isMoving)
            {
                foreach (var bot in _bots)
                {
                    bot.Move(move, noObstacles);
                }
            }

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
        }*/

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

                if (_botShapeEditor != null)
                {
                    _botShapeEditor.OnLeftMouseButtonDown();
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

                if (_botShapeEditor != null)
                {
                    _botShapeEditor.OnRightMouseButtonDown();
                }
            }
        }

        //============================================================================================================//

        public void OnResume()
        {

        }

        public void OnPause()
        {
            Move(0);
        }

        //============================================================================================================//
    }
}
