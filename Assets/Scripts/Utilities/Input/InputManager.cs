using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using StarSalvager.Audio;
using StarSalvager.Cameras;
using StarSalvager.Cameras.Data;
using StarSalvager.Missions;
using StarSalvager.Values;
using UnityEngine;
using UnityEngine.InputSystem;

namespace StarSalvager.Utilities.Inputs
{
    public class InputManager : Singleton<InputManager>, IInput, IPausable
    {
        private static List<IMoveOnInput> _moveOnInput;

        //Properties
        //====================================================================================================================//

        #region Properties

        private Bot[] _bots;
        private ScrapyardBot[] _scrapyardBots;

        public bool isPaused => GameTimer.IsPaused;

        [ShowInInspector, ReadOnly]
        public bool LockSideMovement
        {
            get => _lockSideMovement;
            set
            {
                //Only want to call this in the event that it's different
                if (_lockSideMovement == value) 
                    return;
                
                _lockSideMovement = value;
                
                if (value)
                {
                    TryApplyMove(0f);
                }
                else
                {
                    //Need to make sure that we reset the DasTimer otherwise it wont work!
                    dasMovementTimer = 0f;
                    ProcessMovementInput(_currentMoveInput);
                }

            } 
        }

        private bool _lockSideMovement;


        [SerializeField, BoxGroup("DAS"), ReadOnly]
        private float dasMovementTimer;
        [SerializeField, BoxGroup("DAS"), ReadOnly]
        private bool dasMovementTriggered;
        [SerializeField, BoxGroup("DAS"), ReadOnly]
        private float previousMovementInput;
        [SerializeField, BoxGroup("DAS"), ReadOnly]
        private float currentMovementInput;

        [SerializeField, BoxGroup("DAS"), ReadOnly]
        private float dasRotateTimer;
        [SerializeField, BoxGroup("DAS"), ReadOnly]
        private bool dasRotateTriggered;
        [SerializeField, BoxGroup("DAS"), ReadOnly]
        private float previousRotateInput;
        [SerializeField, BoxGroup("DAS"), ReadOnly]
        private float currentRotateInput;

        private Dictionary<InputAction, Action<InputAction.CallbackContext>> _inputMap;

        public float MostRecentSideMovement { get; private set; }
        private float _currentMoveInput;

        public float MostRecentRotateMovement { get; private set; }
        private float _currentRotateInput;

        public float PreviousInput => previousMovementInput;

        #endregion //Properties

        //Unity Functions
        //============================================================================================================//

        #region Unity Functions

        private void Start()
        {
            Globals.OrientationChange += SetOrientation;
            RegisterPausable();
        }

        private void Update()
        {
            DasChecksMovement();
            DasChecksRotate();
        }

        private void OnEnable()
        {
            _bots = FindObjectsOfType<Bot>();
            _scrapyardBots = FindObjectsOfType<ScrapyardBot>();
        } 

        private void OnDestroy()
        {
            Globals.OrientationChange -= SetOrientation;
        }

        private void OnApplicationQuit()
        {
            Debug.Log($"{nameof(InputManager)} called {nameof(OnApplicationQuit)}");
            GameTimer.CustomOnApplicationQuit();
            MissionManager.CustomOnApplicationQuit();
            PlayerPersistentData.CustomOnApplicationQuit();
        }

        #endregion //Unity Functions

        //============================================================================================================//
        
        public static void RegisterMoveOnInput(IMoveOnInput toAdd)
        {
            if(_moveOnInput == null)
                _moveOnInput = new List<IMoveOnInput>();
            
            _moveOnInput.Add(toAdd);
        }


        public void ForceMove(DIRECTION direction)
        {
            dasMovementTriggered = false;
            dasMovementTimer = 0f;
            
            switch (direction)
            {
                case DIRECTION.LEFT:
                    TryApplyMove(-1);
                    break;
                case DIRECTION.RIGHT:
                    TryApplyMove(1);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
            
            TryApplyMove(0);
            
        }
        
        //IInput Functions
        //============================================================================================================//

        #region IInput Functions

        public void InitInput()
        {
            //--------------------------------------------------------------------------------------------------------//
            
            if (_bots == null || _bots.Length == 0)
                _bots = FindObjectsOfType<Bot>();

            if (_scrapyardBots == null || _scrapyardBots.Length == 0)
                _scrapyardBots = FindObjectsOfType<ScrapyardBot>();

            //Ensure that we clear any previously registered Inputs
            DeInitInput();
            
            //Then we'll create our input map to easily init below
            SetupInputs();
            
            //--------------------------------------------------------------------------------------------------------//
            
            foreach (var func in _inputMap)
            {
                func.Key.Enable();
                func.Key.performed += func.Value;
            }
            
            //--------------------------------------------------------------------------------------------------------//
        }
        


        public void DeInitInput()
        {
            if (_inputMap == null)
                return;
            
            foreach (var func in _inputMap)
            {
                func.Key.Disable();
                func.Key.performed -= func.Value;
            }
        }
        
        #endregion //Input Setup
        
        //============================================================================================================//

        #region Inputs
        private void SetupInputs()
        {
            //Setup the unchanging inputs
            _inputMap = new Dictionary<InputAction, Action<InputAction.CallbackContext>>
            {
                {
                    Input.Actions.Default.Pause, Pause
                },
                {
                    Input.Actions.Default.SmartAction1, SmartAction1
                },
                {
                    Input.Actions.Default.SmartAction2, SmartAction2
                },
                {
                    Input.Actions.Default.SmartAction3, SmartAction3
                },
                {
                    Input.Actions.Default.SmartAction4, SmartAction4
                },
                {
                    Input.Actions.Default.LeftClick, LeftClick
                },
                {
                    Input.Actions.Default.RightClick, RightClick
                },
                //{
                //    Input.Actions.Default.SelfDestruct, SelfDestruct
                //}
            };
            
            //Here we setup the inputs dependent on the orientation
            switch (Globals.Orientation)
            {
                case ORIENTATION.VERTICAL:
                    _inputMap.Add(Input.Actions.Default.SideMovement, SideMovement);
                    _inputMap.Add(Input.Actions.Default.Rotate, RotateMovement);
                    break;
                case ORIENTATION.HORIZONTAL:
                    _inputMap.Add(Input.Actions.Vertical.SideMovement, SideMovement);
                    _inputMap.Add(Input.Actions.Vertical.Rotate, RotateMovement);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        private void SmartAction1(InputAction.CallbackContext ctx)
        {
            TriggerSmartWeapon(ctx, 0);
        }
        private void SmartAction2(InputAction.CallbackContext ctx)
        {
            TriggerSmartWeapon(ctx, 1);
        }
        private void SmartAction3(InputAction.CallbackContext ctx)
        {
            TriggerSmartWeapon(ctx, 2);
        }
        private void SmartAction4(InputAction.CallbackContext ctx)
        {
            TriggerSmartWeapon(ctx, 3);
        }

        private void TriggerSmartWeapon(InputAction.CallbackContext ctx, int index)
        {
            if (Console.Open)
                return;
            
            if (ctx.ReadValue<float>() != 1f)
                return;
            
            
            
            //FIXME Need to ensure that I map appropriate inputs to associated bots
            _bots[0].BotPartsLogic.TryTriggerSmartWeapon(index);
        }
        
        //Movement
        //============================================================================================================//

        #region Movement

        private void SideMovement(InputAction.CallbackContext ctx)
        {
            _currentMoveInput = ctx.ReadValue<float>();
            ProcessMovementInput(_currentMoveInput);
        }

        private void ProcessMovementInput(float moveDirection)
        {
            if (Console.Open)
                return;
            
            if (isPaused)
                return;

            if (LevelManager.Instance.BotDead)
                return;

            MostRecentSideMovement = moveDirection;

            if (LockSideMovement)
            {
                if (moveDirection != 0f)
                {
                    //TODO Sound to play if moving without fuel
                    //AudioController.PlaySound(SOUND.);
                }
                
                
                TryApplyMove(0f);
                return;
            }
            
            

            TryApplyMove(moveDirection);

            
            //This check needs to happen after TryApplyMove as it could cause the Move to never trigger
            if (moveDirection != 0f) 
                return;
            
            //If the user has released the key, we can reset the DAS system
            dasMovementTriggered = false;
            dasMovementTimer = 0f;
        }

        /// <summary>
        /// Considers DAS values when passing the input information to Move
        /// </summary>
        /// <param name="moveDirection"></param>
        private void TryApplyMove(float moveDirection)
        {
            
            currentMovementInput = moveDirection;
            
            //If we're trying to move, set things up for the DAS movement
            if (!dasMovementTriggered)
            {
                //If the timer is still counting down
                if (dasMovementTimer > 0f)
                    return;
            
                //If this is the first time its pressed, set the press directions
                previousMovementInput = currentMovementInput;

                //Set the countdown timer to the intended value
                dasMovementTimer = Globals.DASTime;
                
                //Quickly move the relevant managers, then reset their input, so that they will pause until DAS is ready
                Move(currentMovementInput);
                Move(0);
                return;
            }
            
            //If the DAS has triggered already, go ahead and update the relevant managers
            Move(currentMovementInput);
        }

        /// <summary>
        /// Applies the move value to relevant Managers
        /// </summary>
        /// <param name="value"></param>
        private void Move(float value)
        {
            if (_moveOnInput == null)
                return;

            if (LevelManager.Instance.BotDead)
                return;

            //_currentMovement = value;

            for (var i = _moveOnInput.Count - 1; i >= 0; i--)
            {
                var move = _moveOnInput[i];
                //Automatically unregister things that may have been deleted
                if (move == null)
                {
                    _moveOnInput.RemoveAt(i);
                    continue;
                }
                
                move.Move(value);
            }

        }

        #endregion //Movement

        //Rotation
        //====================================================================================================================//

        #region Rotation

        private void RotateMovement(InputAction.CallbackContext ctx)
        {
            _currentRotateInput = ctx.ReadValue<float>();
            ProcessRotateInput(_currentRotateInput);
        }

        private void ProcessRotateInput(float rotateDirection)
        {
            if (Console.Open)
                return;

            if (isPaused)
                return;

            if (LevelManager.Instance.BotDead)
                return;

            MostRecentRotateMovement = rotateDirection;

            TryApplyRotate(rotateDirection);

            //This check needs to happen after TryApplyRotate as it could cause the Rotate to never trigger
            if (rotateDirection != 0f)
                return;

            //If the user has released the key, we can reset the DAS system
            dasRotateTriggered = false;
            dasRotateTimer = 0f;
        }

        /// <summary>
        /// Considers DAS values when passing the input information to Move
        /// </summary>
        /// <param name="rotateDirection"></param>
        private void TryApplyRotate(float rotateDirection)
        {
            currentRotateInput = rotateDirection;

            //If we're trying to move, set things up for the DAS movement
            if (!dasRotateTriggered)
            {
                //If the timer is still counting down
                if (dasRotateTimer > 0f)
                    return;

                //If this is the first time its pressed, set the press directions
                previousRotateInput = currentRotateInput;

                //Set the countdown timer to the intended value
                dasRotateTimer = Globals.DASTime * 3f;

                //Quickly move the relevant managers, then reset their input, so that they will pause until DAS is ready
                Rotate(currentRotateInput);
                Rotate(0);
                return;
            }

            ////If the DAS has triggered already, go ahead and update the relevant managers
            //dasRotateTimer = Globals.DASTime * 1.2f;
            foreach (var bot in _bots)
            {
                if (bot.Rotating)
                {
                    return;
                }
            }

            Rotate(currentRotateInput);
        }

        /// <summary>
        /// Applies the move value to relevant Managers
        /// </summary>
        /// <param name="value"></param>
        private void Rotate(float value)
        {
            if (LevelManager.Instance.BotDead)
                return;

            foreach (var bot in _bots)
            {
                bot.Rotate(value);
            }

            /*foreach (var scrapyardBot in _scrapyardBots)
            {
                scrapyardBot.Rotate(value);
            }*/

            
        }

        #endregion //Rotation


        //====================================================================================================================//
        

        private void LeftClick(InputAction.CallbackContext ctx)
        {
            if (Console.Open)
                return;
            
            //var clicked = ctx.ReadValue<float>();
        }

        private void RightClick(InputAction.CallbackContext ctx)
        {
            if (Console.Open)
                return;
            
            //var clicked = ctx.ReadValue<float>();
        }

        private void Pause(InputAction.CallbackContext ctx)
        {
            if (Console.Open)
                return;
            
            if (LevelManager.Instance.EndWaveState)
                return;
            
            if(ctx.ReadValue<float>() == 1f)
                GameTimer.SetPaused(!isPaused);
        }
        
        //private void SelfDestruct(InputAction.CallbackContext ctx)
        //{
        //    _bots[0].TrySelfDestruct();
        //}

        //====================================================================================================================//
        

        public void CancelMove()
        {
            Move(0);
        }


        
        #endregion //Inputs

        //============================================================================================================//
        
        private void SetOrientation(ORIENTATION orientation)
        {
            //Update the current input setup
            InitInput();
        }
        
        //============================================================================================================//
        
        private void DasChecksMovement()
        {
            //If the user is no longer pressing a direction, these checks do not matter
            if (currentMovementInput == 0f)
                return;
            
            //If we've already triggered the DAS, don't bother with following checks
            if (dasMovementTriggered)
                return;

            //If timer hasn't reached zero, continue counting down
            if (dasMovementTimer > 0f)
            {
                dasMovementTimer -= Time.deltaTime;
                return;
            }

            dasMovementTriggered = true;
            dasMovementTimer = 0f;
            
            //If the User is still pressing the same input, go ahead and try and reapply it
            if(currentMovementInput == previousMovementInput)
                TryApplyMove(currentMovementInput);
        }

        private void DasChecksRotate()
        {
            //If the user is no longer pressing a direction, these checks do not matter
            if (currentRotateInput == 0f)
                return;
            
            //Commented out because a delay is required for rotate DAS to function correctly
            //If we've already triggered the DAS, don't bother with following checks
            //if (dasRotateTriggered)
            //    return;

            //If timer hasn't reached zero, continue counting down
            if (dasRotateTimer > 0f)
            {
                dasRotateTimer -= Time.deltaTime;
                return;
            }

            dasRotateTriggered = true;
            dasRotateTimer = 0f;

            //If the User is still pressing the same input, go ahead and try and reapply it
            if (currentRotateInput == previousRotateInput)
                TryApplyRotate(currentRotateInput);
        }

        //IPausable Functions
        //============================================================================================================//

        public void RegisterPausable()
        {
            GameTimer.AddPausable(this);
        }

        public void OnResume()
        {

        }

        public void OnPause()
        {
            Move(0);

            currentRotateInput = MostRecentRotateMovement = 0;
            Rotate(0);
        }

        //============================================================================================================//


    }
}
