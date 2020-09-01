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
        private Bot[] _bots;
        private ScrapyardBot[] _scrapyardBots;

        public bool isPaused => GameTimer.IsPaused;

        [ShowInInspector, ReadOnly]
        public bool LockSideMovement
        {
            get => _LockSideMovement;
            set
            {
                if (value)
                    TryApplyMove(0f);
                
                _LockSideMovement = value;
            } 
        }

        private bool _LockSideMovement;


        [SerializeField, BoxGroup("DAS"), ReadOnly]
        private float dasTimer;
        [SerializeField, BoxGroup("DAS"), ReadOnly]
        private bool dasTriggered;
        [SerializeField, BoxGroup("DAS"), ReadOnly]
        private float previousInput;
        [SerializeField, BoxGroup("DAS"), ReadOnly]
        private float currentInput;
        
        private Dictionary<InputAction, Action<InputAction.CallbackContext>> _inputMap;

        //============================================================================================================//

        private void Start()
        {
            Globals.OrientationChange += SetOrientation;
            RegisterPausable();
        }

        private void Update()
        {
            DasChecks();
        }

        private void OnEnable()
        {
            _bots = FindObjectsOfType<Bot>();
            _scrapyardBots = FindObjectsOfType<ScrapyardBot>();
            /*_scrapyard = FindObjectOfType<Scrapyard>();
            _botShapeEditor = FindObjectOfType<BotShapeEditor>();*/
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

        //============================================================================================================//

        private static List<IMoveOnInput> moveOnInput;
        
        public static void RegisterMoveOnInput(IMoveOnInput toAdd)
        {
            if(moveOnInput == null)
                moveOnInput = new List<IMoveOnInput>();
            
            moveOnInput.Add(toAdd);
        }
            
        
        //============================================================================================================//

        #region Input Setup

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
                }
            };
            
            //Here we setup the inputs dependent on the orientation
            switch (Globals.Orientation)
            {
                case ORIENTATION.VERTICAL:
                    _inputMap.Add(Input.Actions.Default.SideMovement, SideMovement);
                    _inputMap.Add(Input.Actions.Default.Rotate, Rotate);
                    break;
                case ORIENTATION.HORIZONTAL:
                    _inputMap.Add(Input.Actions.Vertical.SideMovement, SideMovement);
                    _inputMap.Add(Input.Actions.Vertical.Rotate, Rotate);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
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
        
        //============================================================================================================//

        private void SideMovement(InputAction.CallbackContext ctx)
        {
            if (Console.Open)
                return;
            
            if (isPaused)
                return;

            var moveDirection = ctx.ReadValue<float>();
            
            if (LockSideMovement)
            {
                if (moveDirection != 0f)
                {
                    //TODO Sound to play if moving without fuel
                    //AudioController.PlaySound(SOUND);
                }
                
                
                TryApplyMove(0f);
                return;
            }
            
            

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
                dasTimer = Globals.DASTime;
                
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
            if (moveOnInput == null)
                return;
            
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

        }

        private void Rotate(InputAction.CallbackContext ctx)
        {
            if (Console.Open)
                return;
            
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
            
            AudioController.PlaySound(SOUND.BOT_ROTATE);
        }

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
        
        #endregion //Inputs

        //============================================================================================================//
        
        private void SetOrientation(ORIENTATION orientation)
        {
            //Update the current input setup
            InitInput();
        }
        
        //============================================================================================================//
        
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
        }

        //============================================================================================================//


    }
}
