using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using StarSalvager.Audio;
using StarSalvager.Cameras;
using StarSalvager.Cameras.Data;
using StarSalvager.UI;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.Saving;
using StarSalvager.Values;
using UnityEngine;
using UnityEngine.InputSystem;

namespace StarSalvager.Utilities.Inputs
{
    public enum ACTION_MAP
    {
        NULL = -1,
        DEFAULT,
        MENU
    }
    public class InputManager : Singleton<InputManager>, IInput, IPausable
    {
        public static Action<int, bool> TriggerWeaponStateChange;
        public static Action<string> InputDeviceChanged;

        [SerializeField, ReadOnly, BoxGroup("Debug", order: -1000)]
        private ACTION_MAP currentActionMap;

        [Button, DisableInEditorMode, HorizontalGroup("Debug/Row1")]
        private void ForceMenuControls()
        {
            SwitchCurrentActionMap(ACTION_MAP.MENU);
        }
        [Button, DisableInEditorMode, HorizontalGroup("Debug/Row1")]
        private void ForceDefaultControls()
        {
            SwitchCurrentActionMap(ACTION_MAP.DEFAULT);
        }

        //====================================================================================================================//

        private static List<IMoveOnInput> _moveOnInput;

        [SerializeField, Required]
        private PlayerInput playerInput;

        //Properties
        //====================================================================================================================//

        #region Properties

        private readonly bool[] _triggersPressed = new bool[5];

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

        [ShowInInspector, ReadOnly]
        public bool LockRotation
        {
            get => _lockRotation;
            set
            {
                //Only want to call this in the event that it's different
                if (_lockRotation == value)
                    return;

                _lockRotation = value;

                if (value)
                {
                    TryApplyRotate(0f);
                }
                else
                {
                    //Need to make sure that we reset the DasTimer otherwise it wont work!
                    _darTimer = 0f;
                    ProcessRotateInput(_currentRotateInput);
                }

            }
        }

        private bool _lockSideMovement;
        private bool _lockRotation;

        [SerializeField, BoxGroup("DAS"), ReadOnly]
        private float dasMovementTimer;
        [SerializeField, BoxGroup("DAS"), ReadOnly]
        private bool dasMovementTriggered;
        [SerializeField, BoxGroup("DAS"), ReadOnly]
        private float previousMovementInput;
        /*[SerializeField, BoxGroup("DAS"), ReadOnly]
        private float currentMovementInput;*/

        [ShowInInspector, BoxGroup("DAS"), ReadOnly]
        private float _darTimer;
        [ShowInInspector, BoxGroup("DAS"), ReadOnly]
        private bool _dasRotateTriggered;
        [ShowInInspector, BoxGroup("DAS"), ReadOnly]
        private float _previousRotateInput;
        /*[ShowInInspector, BoxGroup("DAS"), ReadOnly]
        private float currentRotateInput;*/

        private Dictionary<InputAction, Action<InputAction.CallbackContext>> _inputMap;

        public float MostRecentSideMovement { get; private set; }

        public float CurrentMoveInput => _currentMoveInput;
        private float _currentMoveInput;

        public float MostRecentRotateMovement { get; private set; }

        public float CurrentRotateInput => _currentRotateInput;
        private float _currentRotateInput;

        public float PreviousInput => previousMovementInput;

        #endregion //Properties

        //Unity Functions
        //============================================================================================================//

        private bool _controlPressed;

        #region Unity Functions

        private void Start()
        {
            Globals.OrientationChange += SetOrientation;
            RegisterPausable();

        }

        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.LeftControl))
                _controlPressed = true;
            else if(UnityEngine.Input.GetKeyUp(KeyCode.LeftControl))
                _controlPressed = false;

            DasChecksMovement();
            DasChecksRotate();

            UpdateShuffleCountdown();
            //TryUpdateTriggers();
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


        #endregion //Unity Functions

        //============================================================================================================//

        public static ACTION_MAP CurrentActionMap { get; private set; }


        public static void SwitchCurrentActionMap(in ACTION_MAP actionMap)
        {
            switch (actionMap)
            {
                case ACTION_MAP.DEFAULT:

                    Input.Actions.MenuControls.Disable();
                    Input.Actions.Default.Enable();
                    break;
                case ACTION_MAP.MENU:
                    if (Instance) Instance.OnPause();

                    Input.Actions.Default.Disable();
                    Input.Actions.MenuControls.Enable();
                    break;
            }

            Instance.currentActionMap = CurrentActionMap = actionMap;

            var actionMapName = GetActionMapName(actionMap);
            Instance.playerInput.SwitchCurrentActionMap(actionMapName);

        }

        public static void SetToExpectedActionMap()
        {
            ACTION_MAP actionMap;

            if (GameTimer.IsPaused)
                actionMap = ACTION_MAP.MENU;
            else
            {
                switch (GameManager.CurrentGameState)
                {
                    case GameState.MainMenu:
                    case GameState.AccountMenu:
                    case GameState.Scrapyard:
                    case GameState.UniverseMap:
                        actionMap = ACTION_MAP.MENU;
                        break;
                    case GameState.LevelActive:
                    case GameState.LevelActiveEndSequence:
                    case GameState.LevelEndWave:
                    case GameState.LevelBotDead:
                    case GameState.LEVEL_ACTIVE:
                    case GameState.LEVEL:
                        actionMap = ACTION_MAP.DEFAULT;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            switch (actionMap)
            {
                case ACTION_MAP.DEFAULT:

                    Input.Actions.MenuControls.Disable();
                    Input.Actions.Default.Enable();
                    break;
                case ACTION_MAP.MENU:
                    if(Instance)
                    {
                        Instance.ProcessMovementInput(0);
                        Instance.ProcessRotateInput(0);
                    }

                    Input.Actions.Default.Disable();
                    Input.Actions.MenuControls.Enable();
                    break;
            }

            Instance.currentActionMap = CurrentActionMap = actionMap;

            var actionMapName = GetActionMapName(actionMap);
            Instance.playerInput.SwitchCurrentActionMap(actionMapName);
        }

        private static string GetActionMapName(in ACTION_MAP actionMap)
        {
            switch (actionMap)
            {
                case ACTION_MAP.DEFAULT:
                    return "Default";
                case ACTION_MAP.MENU:
                    return "Menu Controls";
                default:
                    throw new ArgumentOutOfRangeException(nameof(actionMap), actionMap, null);
            }
        }

        public static void RegisterMoveOnInput(IMoveOnInput toAdd)
        {
            if(_moveOnInput == null)
                _moveOnInput = new List<IMoveOnInput>();

            _moveOnInput.Add(toAdd);
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
                func.Key.canceled += func.Value;
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

        //FIXME This functions but could use reorganizing
        public static string CurrentInputDeviceName => Instance._currentInputDevice;
        private string _currentInputDevice = "Keyboard";
        private void CheckForInputDeviceChange(in InputAction.CallbackContext callbackContext)
        {
            CheckForInputDeviceChange(callbackContext.control.device);
        }
        private void CheckForInputDeviceChange(in InputDevice inputDevice)
        {
            const string KEYBOARD = "Keyboard";
            const string MOUSE = "Mouse";

            var deviceName = inputDevice.name;

            if (deviceName.Equals(KEYBOARD) || deviceName.Equals(MOUSE))
                deviceName = KEYBOARD;

            if (_currentInputDevice.Equals(deviceName))
                return;

            _currentInputDevice = deviceName;

            Debug.Log($"New Device Name: {deviceName}");
            //TODO Notify whoever that the
            InputDeviceChanged?.Invoke(deviceName);
        }



        private void SetupInputs()
        {
            //var actionMap = playerInput.currentActionMap.actions;

            //Setup the unchanging inputs
            _inputMap = new Dictionary<InputAction, Action<InputAction.CallbackContext>>
            {
                {
                    Input.Actions.Default.SwapPart, TrySwapPart
                },
                /*{
                    Input.Actions.Default.ShuffleAlt, ShuffleInput
                },*/
                {
                    Input.Actions.Default.SideMovement, MovementDelegator
                },
                {
                    Input.Actions.Default.Rotate, MovementDelegator
                },
                {
                    Input.Actions.Default.Pause, Pause
                },
                {
                    Input.Actions.Default.TriggerPart, TriggerPart
                },
                {
                    Input.Actions.Default.LeftClick, LeftClick
                },
                {
                    Input.Actions.Default.RightClick, RightClick
                },
                {
                    Input.Actions.Default.SpeedChange, SpeedChange
                },
                {
                    Input.Actions.Default.Dash, Dash
                }
            };
        }

        private void TrySwapPart(InputAction.CallbackContext ctx)
        {
            var vector2 = ctx.ReadValue<Vector2>();

            if (vector2 == Vector2.zero)
                return;

            var direction = vector2.ToDirection();

            Debug.Log($"Try swap {direction} part");

            _bots[0].BotPartsLogic.TrySwapPart(direction);
        }

        //Smart Actions
        //====================================================================================================================//

        private void TriggerPart(InputAction.CallbackContext ctx)
        {
            if (Console.Open)
                return;

            CheckForInputDeviceChange(ctx);

            var rawDirection = ctx.ReadValue<Vector2>();
            var direction = rawDirection.ToDirection();

            if (direction == DIRECTION.NULL)
            {
                for (var i = 0; i < 5; i++)
                {
                    TriggerWeaponStateChange?.Invoke(i, false);
                }
                return;
            }

            var input = Mathfx.Abs(rawDirection);//new Vector2(Mathf.Abs(rawDirection.x), Mathf.Abs(rawDirection.y));
            int index;
            bool state;
            switch (direction)
            {
                case DIRECTION.UP:
                    index = 0;
                    state = input.y == 1f;
                    break;
                case DIRECTION.DOWN:
                    index = 1;
                    state = input.y == 1f;
                    break;
                case DIRECTION.LEFT:
                    index = 3;
                    state = input.x == 1f;
                    break;
                case DIRECTION.RIGHT:
                    index = 4;
                    state = input.x == 1f;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            TriggerWeaponStateChange?.Invoke(index, state);
        }

        //====================================================================================================================//

        //Movement
        //============================================================================================================//

        #region Movement

        private void SpeedChange(InputAction.CallbackContext ctx)
        {
#if UNITY_EDITOR
            if (Console.Open)
                return;

            if (!GameManager.IsState(GameState.LEVEL_ACTIVE))
                return;

            var direction = ctx.ReadValue<float>();

            if (direction < 0)
            {
                Globals.DecreaseFallSpeed();
            }
            else if (direction > 0)
            {
                Globals.IncreaseFallSpeed();
            }
#endif
        }

        private void Dash(InputAction.CallbackContext ctx)
        {
            if (Console.Open)
                return;

            CheckForInputDeviceChange(ctx);

            if (!GameManager.IsState(GameState.LEVEL_ACTIVE))
                return;

            var direction = ctx.ReadValue<float>();

            _bots[0].Dash(direction, Globals.DashDistance);

            /*if (direction < 0)
            {
                Globals.DecreaseFallSpeed();
            }
            else if (direction > 0)
            {
                Globals.IncreaseFallSpeed();
            }*/
        }

        private void MovementDelegator(InputAction.CallbackContext ctx)
        {
            if (Console.Open)
                return;

            CheckForInputDeviceChange(ctx);

            switch (ctx.action.name)
            {
                case "Side Movement":
                    if (Globals.Orientation == ORIENTATION.VERTICAL) SideMovement(ctx);
                    else RotateMovement(ctx);
                    break;
                case "Rotate":
                    if (Globals.Orientation == ORIENTATION.VERTICAL) RotateMovement(ctx);
                    else SideMovement(ctx);
                    break;
            }
        }

        public float TEST_Input;
        private void SideMovement(InputAction.CallbackContext ctx)
        {
            var newValue = ctx.ReadValue<float>();

            //Rounding for Joystick Clamping. Helps prevent overshooting while moving
            if (newValue < -0.5f)
                newValue = -1f;
            else if (newValue > 0.5f)
                newValue = 1f;
            else
                newValue = 0f;


            TEST_Input = newValue;

            //If the input is already set to the updated value, we can ignore it.
            if (System.Math.Abs(newValue - _currentMoveInput) < 0.05f)
                return;

            if(Globals.UseShuffleDance)
                TrySideShuffleDance(Mathf.RoundToInt(newValue));

            //If the current movement is set to max, and we're trying to stop, do so immediately
            if (Mathf.Abs(_currentMoveInput) > 0.9f && Mathf.Abs(newValue) < 0.9f)
                _currentMoveInput = 0f;

            if (newValue < 0)
                _currentMoveInput = -1f;
            else if (newValue > 0)
                _currentMoveInput = 1f;

            ProcessMovementInput(_currentMoveInput);
        }

        private void ProcessMovementInput(float moveDirection)
        {
            if (Console.Open)
                return;

            if (isPaused)
                return;

            if (GameManager.IsState(GameState.LevelBotDead))
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

            _currentMoveInput = moveDirection;

            //If we're trying to move, set things up for the DAS movement
            if (!dasMovementTriggered)
            {
                //If the timer is still counting down
                if (dasMovementTimer > 0f)
                    return;

                //If this is the first time its pressed, set the press directions
                previousMovementInput = _currentMoveInput;

                //Set the countdown timer to the intended value
                dasMovementTimer = Globals.DASTime;

                //Quickly move the relevant managers, then reset their input, so that they will pause until DAS is ready
                Move(_currentMoveInput);
                Move(0);
                return;
            }

            //If the DAS has triggered already, go ahead and update the relevant managers
            Move(_currentMoveInput);
        }

        /// <summary>
        /// Applies the move value to relevant Managers
        /// </summary>
        /// <param name="value"></param>
        private void Move(float value)
        {
            if (_moveOnInput == null)
                return;

            if (value != 0 && !GameManager.IsState(GameState.LEVEL_ACTIVE))
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

        //Side Shuffle
        //====================================================================================================================//

        //private const float TIMER = 0.3f;
        private float countdown;
        private int direction;
        private int keyCount;

        /*private void ShuffleInput(InputAction.CallbackContext ctx)
        {
            if (!Globals.UseShuffleDance)
                return;

            var newValue = ctx.ReadValue<float>();

            //Rounding for Joystick Clamping. Helps prevent overshooting while moving
            if (newValue < -0.5f)
                newValue = -1f;
            else if (newValue > 0.5f)
                newValue = 1f;
            else
                newValue = 0f;

            //If the input is already set to the updated value, we can ignore it.
            if (System.Math.Abs(newValue - _currentMoveInput) < 0.05f)
                return;


        }*/

        private void UpdateShuffleCountdown()
        {
            if (countdown > 0f)
                countdown -= Time.deltaTime;
            else
            {

                keyCount = 0;
                countdown = 0f;
                direction = 0;
            }
        }

        /// <summary>
        /// Returns true if the shuffle steps are approved, and false if they've failed
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        private bool TrySideShuffleDance(int dir)
        {
            void SetFailed()
            {
                keyCount = 0;
                countdown = 0f;
                direction = 0;
            }

            switch (dir)
            {
                case 0 when countdown <= 0f:
                    SetFailed();
                    return false;
                case 0 when countdown > 0f:
                    return true;
            }

            //Debug.Log($"Compare Dir: {dir} Direction: {direction}");

            if (dir == direction)
            {
                 SetFailed();
            }
            else if (dir != direction && countdown <= 0f)
            {
                countdown = Globals.ShuffleTimeThreshold;
            }

            if (countdown <= 0f)
            {
                SetFailed();
                return false;
            }

            direction = dir;

            if (keyCount++ == 1)
            {
                Debug.Log($"Shuffle in {direction}");

                switch (direction)
                {
                    case -1:
                        _bots[0].CoreShuffle(DIRECTION.RIGHT);
                        break;
                    case 1:
                        _bots[0].CoreShuffle(DIRECTION.LEFT);
                        break;
                }
            }

            return true;
        }

        //Rotation
        //====================================================================================================================//

        #region Rotation

        private void RotateMovement(InputAction.CallbackContext ctx)
        {
            _currentRotateInput = ctx.ReadValue<float>();
            Debug.Log($"Current Rotation Input: {_currentRotateInput}");

            ProcessRotateInput(_currentRotateInput);
        }

        private void ProcessRotateInput(float rotateDirection)
        {
            if (Console.Open)
                return;

            if (isPaused)
                return;

            if (GameManager.IsState(GameState.LevelBotDead))
                return;

            if (rotateDirection != 0 && GameManager.IsState(GameState.LevelEndWave))
                return;

            MostRecentRotateMovement = rotateDirection;

            if (LockRotation)
            {
                TryApplyRotate(0f);
                return;
            }

            TryApplyRotate(rotateDirection);

            //This check needs to happen after TryApplyRotate as it could cause the Rotate to never trigger
            if (rotateDirection != 0f)
                return;

            //If the user has released the key, we can reset the DAS system
            _dasRotateTriggered = false;
            _darTimer = 0f;
        }

        /// <summary>
        /// Considers DAS values when passing the input information to Move
        /// </summary>
        /// <param name="rotateDirection"></param>
        private void TryApplyRotate(float rotateDirection)
        {
            _currentRotateInput = rotateDirection;

            //If we're trying to move, set things up for the DAS movement
            if (!_dasRotateTriggered)
            {
                //If the timer is still counting down
                if (_darTimer > 0f)
                    return;

                //If this is the first time its pressed, set the press directions
                _previousRotateInput = _currentRotateInput;

                //Set the countdown timer to the intended value
                _darTimer = Globals.DARTime;

                //Quickly move the relevant managers, then reset their input, so that they will pause until DAS is ready
                Rotate(_currentRotateInput);
                Rotate(0);
                return;
            }

            //If the DAS has triggered already, go ahead and update the relevant managers
            //dasRotateTimer = Globals.DASTime * 1.2f;
            foreach (var bot in _bots)
            {
                if (bot.Rotating)
                {
                    return;
                }
            }

            Rotate(_currentRotateInput);
        }

        /// <summary>
        /// Applies the move value to relevant Managers
        /// </summary>
        /// <param name="value"></param>
        private void Rotate(float value)
        {
            if (GameManager.IsState(GameState.LevelBotDead))
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

            CheckForInputDeviceChange(ctx);

            if (GameManager.IsState(GameState.LevelEndWave) || GameManager.IsState(GameState.LevelBotDead))
                return;

            if (ctx.ReadValue<float>() == 1f)
            {
                GameTimer.SetPaused(!isPaused);
                SwitchCurrentActionMap(isPaused ? ACTION_MAP.MENU: ACTION_MAP.DEFAULT);
            }
        }

        //private void SelfDestruct(InputAction.CallbackContext ctx)
        //{
        //    _bots[0].TrySelfDestruct();
        //}

        //====================================================================================================================//

        public void CancelMove()
        {
            Move(0);
            Rotate(0);
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
            if (_currentMoveInput == 0f)
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
            if(_currentMoveInput == previousMovementInput)
                TryApplyMove(_currentMoveInput);
        }

        private void DasChecksRotate()
        {
            //If the user is no longer pressing a direction, these checks do not matter
            if (_currentRotateInput == 0f)
                return;

            //Commented out because a delay is required for rotate DAS to function correctly
            //If we've already triggered the DAS, don't bother with following checks
            //if (dasRotateTriggered)
            //    return;

            //If timer hasn't reached zero, continue counting down
            if (_darTimer > 0f)
            {
                _darTimer -= Time.deltaTime;
                return;
            }

            _dasRotateTriggered = true;
            _darTimer = 0f;

            //If the User is still pressing the same input, go ahead and try and reapply it
            if (_currentRotateInput == _previousRotateInput)
                TryApplyRotate(_currentRotateInput);
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
            previousMovementInput = _currentMoveInput = MostRecentSideMovement = 0;
            Move(0);

            _previousRotateInput = _currentRotateInput = MostRecentRotateMovement = 0;
            Rotate(0);
        }

        //============================================================================================================//


    }
}
