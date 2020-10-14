using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using StarSalvager.Utilities.Inputs;
using StarSalvager.Values;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Input = StarSalvager.Utilities.Inputs.Input;

namespace StarSalvager.Tutorial
{
    public class TutorialManager : MonoBehaviour, IInput
    {
        [Serializable]
        public struct TutorialStepData
        {
            //[FoldoutGroup("$title", false), DisplayAsString]
            [HideInInspector]
            public string title;
            [HorizontalGroup("$title/UseWait"), ToggleLeft, LabelWidth(50f)]
            public bool useWaitTime;
            [HorizontalGroup("$title/UseWait"), EnableIf("useWaitTime"), HideLabel, SuffixLabel("Seconds", true)]
            public float waitTime;
            [TextArea,FoldoutGroup("$title")]
            public string text;
        }

        [SerializeField, ListDrawerSettings(HideAddButton = true, HideRemoveButton = true)]
        private List<TutorialStepData> tutorialSteps = new List<TutorialStepData>
        {
            /* [0] */new TutorialStepData {title = "Intro Step"},
            /* [1] */new TutorialStepData {title = "Movement"},
            /* [2] */new TutorialStepData {title = "Rotate"},
            /* [3] */new TutorialStepData {title = "Falling Bits"},
            /* [4] */new TutorialStepData {title = "Combo"},
            /* [5] */new TutorialStepData {title = "Magnet"},

            /* [6] */new TutorialStepData {title = "Combo-magnet-1"},
            /* [7] */new TutorialStepData {title = "Combo-magnet-2"},

            /* [8] */new TutorialStepData {title = "Magnet-combo-1"},
            /* [9] */new TutorialStepData {title = "Magnet-combo-2"},

            /* [10] */new TutorialStepData {title = "Pulsar"},
            /* [11] */new TutorialStepData {title = "Pulsar-1"},

            /* [12] */new TutorialStepData {title = "Fuel"},
            /* [13] */new TutorialStepData {title = "Fuel-1"},
            /* [14] */new TutorialStepData {title = "Fuel-2"},
        };
        

        
        [SerializeField, BoxGroup("Tutorial UI")]
        private TMP_Text text;
         [FormerlySerializedAs("image")] [SerializeField, BoxGroup("Tutorial UI")]
        private Image fillImage;

        [SerializeField, BoxGroup("Tutorial UI")]
        private TMP_Text pressAnyKeyText;

        [SerializeField, BoxGroup("Tutorial UI")]
        private GameObject characterObject;
        
        private bool _readyForInput;
        private bool _keyPressed;

        private float _currentMoveDirection;

        private List<IEnumerator> _tutorialStepCoroutines;

        private InputManager _inputManager;

        private bool _isReady;

        //Unity Functions
        //====================================================================================================================//

        private void OnDisable()
        {
            if (!_isReady)
                return;
            
            DeInitInput();
        }

        //Tutorial Functions
        //====================================================================================================================//

        public void SetupTutorial()
        {
            InitInput();
            
            _tutorialStepCoroutines = new List<IEnumerator>
            {
                IntroStepCoroutine(),
                MoveStepCoroutine(),
                RotateStepCoroutine(),
                StartFallingBitsCoroutine(),
                BotCollectionsCoroutine(),
                PulsarStepCoroutine(),
                FuelStepCoroutine(),
                EndStepCoroutine()
            };
            
            StartCoroutine(MainTutorialCoroutine());

            _readyForInput = true;
            _isReady = true;
        }

        private void SetText(TutorialStepData tutorialStepData)
        {
            SetText(tutorialStepData.text);
        }
        
        private void SetText(string text)
        {
            this.text.text = text;
        }

        //Tutorial Steps
        //====================================================================================================================//
        private IEnumerator MainTutorialCoroutine()
        {
            foreach (var stepCoroutine in _tutorialStepCoroutines)
            {
                yield return StartCoroutine(stepCoroutine);
            }
            
            
        }
        
        private IEnumerator IntroStepCoroutine()
        {
            var bot = LevelManager.Instance.BotObject;
            bot.PROTO_GodMode = true;
            
            yield return StartCoroutine(WaitStep(tutorialSteps[0], true));
        }
        private IEnumerator MoveStepCoroutine()
        {
            yield return StartCoroutine(WaitStep(tutorialSteps[1], false));
            
            //TODO Need to wait for the movement of the Bot Left/Right
            bool left, right;
            left = right = false;

            while (!left && !right)
            {
                if (_inputManager.CurrentMoveInput < 0)
                    left = true;
                
                if (_inputManager.CurrentMoveInput > 0)
                    right = true;
                
                yield return null;
            }
        }
        private IEnumerator RotateStepCoroutine()
        {
            yield return StartCoroutine(WaitStep(tutorialSteps[2], false));
            
            bool left, right;
            left = right = false;

            while (!left && !right)
            {
                if (_inputManager.CurrentRotateInput < 0)
                    left = true;
                
                if (_inputManager.CurrentRotateInput > 0)
                    right = true;
                
                yield return null;
            }
        }
        private IEnumerator StartFallingBitsCoroutine()
        {
            yield return StartCoroutine(WaitStep(tutorialSteps[3], true));
        }
        private IEnumerator BotCollectionsCoroutine()
        {
            bool magnet, combo;
            magnet = combo = false;
            
            void SetMagnet()
            {
                magnet = true;
            }
            void SetCombo()
            {
                combo = true;
            }
            
            SetText(tutorialSteps[3]);
            
            var bot = LevelManager.Instance.BotObject;
            bot.OnFullMagnet += SetMagnet;
            bot.OnCombo += SetCombo;

            while (!magnet && !combo)
            {
                yield return null;
            }
            
            bot.OnFullMagnet -= SetMagnet;
            bot.OnCombo -= SetCombo;

            if (magnet)
            {
                //TODO Create Magnet First Path
                yield return StartCoroutine(MagnetFirstCoroutine());
            }
            else if (combo)
            {
                //TODO Create Combo First Path
                yield return StartCoroutine(ComboFirstCoroutine());
            }
            else
            {
                throw new Exception("Should not reach here");
            }

        }
        
        private IEnumerator ComboFirstCoroutine()
        {
            //tutorialSteps[4]
            yield return StartCoroutine(PauseWaitTimerStep(tutorialSteps[4], true));
            
            SetText(tutorialSteps[6]);

            bool magnet = false;
            var bot = LevelManager.Instance.BotObject;

            void SetMagnet()
            {
                magnet = true;
            }

            bot.OnFullMagnet += SetMagnet;

            yield return new WaitUntil(() => magnet);
            
            bot.OnFullMagnet -= SetMagnet;
            
            yield return StartCoroutine(PauseWaitTimerStep(tutorialSteps[7], true));
        }
        private IEnumerator MagnetFirstCoroutine()
        {
            //tutorialSteps[5]
            yield return StartCoroutine(PauseWaitTimerStep(tutorialSteps[5], true));
            
            SetText(tutorialSteps[8]);

            bool combo = false;
            var bot = LevelManager.Instance.BotObject;

            void SetCombo()
            {
                combo = true;
            }

            bot.OnCombo += SetCombo;

            yield return new WaitUntil(() => combo);
            
            bot.OnCombo -= SetCombo;
            
            yield return StartCoroutine(PauseWaitTimerStep(tutorialSteps[9], true));
        }

        private IEnumerator PulsarStepCoroutine()
        {
            SetText(tutorialSteps[10]);
            var bot = LevelManager.Instance.BotObject;
            
            bool bump = false;

            void SetBump()
            {
                bump = true;
            }

            bot.OnBitShift += SetBump;

            yield return new WaitUntil(() => bump);
            
            bot.OnBitShift -= SetBump;

            yield return StartCoroutine(WaitStep(tutorialSteps[11], false));
        }
        
        private IEnumerator FuelStepCoroutine()
        {
            //TODO Set the bot able to use its fuel
            var bot = LevelManager.Instance.BotObject;
            bot.PROTO_GodMode = false;
            
            var playerData = PlayerPersistentData.PlayerData.liquidResource;
            
            yield return new WaitUntil(() => playerData[BIT_TYPE.RED] <= 0f);
            
            SetText(tutorialSteps[12]);
            
            //TODO Set the wave to spawn all reds
            
            yield return new WaitUntil(() => playerData[BIT_TYPE.RED] > 0f);

            yield return StartCoroutine(WaitStep(tutorialSteps[13], false));
            
            SetText(tutorialSteps[12]);

        }

        private IEnumerator EndStepCoroutine()
        {
            yield return new WaitForSeconds(5f);
        }
        
        //Generic Tutorial Steps
        //====================================================================================================================//

        private IEnumerator PauseWaitTimerStep(TutorialStepData tutorialStepData, bool waitAnyKey)
        {
            Time.timeScale = 0f;

            yield return StartCoroutine(WaitStep(tutorialStepData, waitAnyKey));
            
            Time.timeScale = 1f;
        }
        
        private IEnumerator WaitStep(TutorialStepData tutorialStepData, bool waitAnyKey)
        {
            SetText(tutorialStepData);

            if (tutorialStepData.useWaitTime)
            {
                _readyForInput = false;
                yield return new WaitTimeImage(tutorialStepData.waitTime, fillImage);
            } 
            
            if(waitAnyKey)
                yield return new WaitForAnyKey(this);
            
            _readyForInput = true;
        }


        //IInput Functions
        //====================================================================================================================//
        
        //TODO Need to setup AnyKey
        
        public void InitInput()
        {
            _inputManager = InputManager.Instance;
            
            Input.Actions.Default.Any.Enable();
            Input.Actions.Default.Any.performed += AnyKeyPressed;
        }

        public void DeInitInput()
        {
            Input.Actions.Default.Any.Disable();
            Input.Actions.Default.Any.performed -= AnyKeyPressed;
        }

        //Input Functions
        //====================================================================================================================//

        private void AnyKeyPressed(InputAction.CallbackContext ctx)
        {
            var pressed = ctx.ReadValue<float>() == 1f;
            
            _keyPressed = pressed;
        }

        //Utility Coroutines
        //====================================================================================================================//

        #region Utility IEnumerators

        private sealed class WaitForAnyKey : IEnumerator
        {
            private readonly TutorialManager _tutorialManager;
            
            public bool MoveNext()
            {
                if (!_tutorialManager._keyPressed) 
                    return true;
                
                _tutorialManager._keyPressed = false;
                Debug.Log("Any Key Pressed");
                return false;
            }

            public void Reset() { }

            public object Current => null;

            public WaitForAnyKey(TutorialManager tutorialManager)
            {
                Debug.Log("Waiting for Any Key");
                _tutorialManager = tutorialManager;
            }
        }
        private sealed class WaitTimeSlider : IEnumerator
        {
            private readonly Slider _slider;
            private readonly float _startTime;
            private float _timePast;
            
            public bool MoveNext()
            {
                if (_timePast >= _startTime)
                    return false;
                
                _timePast += Time.unscaledDeltaTime;
                _slider.value = _timePast / _startTime;
                
                return true;
            }

            public void Reset() { }

            public object Current => null;

            public WaitTimeSlider(float time, Slider slider)
            {
                _startTime = time;
                _timePast = 0;
                _slider = slider;
            }
        }
        private sealed class WaitTimeImage : IEnumerator
        {
            private readonly Image _image;
            private readonly float _startTime;
            private float _timePast;
            
            public bool MoveNext()
            {
                if (_timePast >= _startTime)
                    return false;
                
                _timePast += Time.unscaledDeltaTime;
                _image.fillAmount = _timePast / _startTime;
                
                return true;
            }

            public void Reset() { }

            public object Current => null;

            public WaitTimeImage(float time, Image image)
            {
                _startTime = time;
                _timePast = 0;
                _image = image;
            }
        }

        #endregion //Utility IEnumerators

        //====================================================================================================================//
        
    }
}
