using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using StarSalvager.Tutorial.Data;
using StarSalvager.Utilities.Inputs;
using StarSalvager.Utilities.UI;
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
        [SerializeField, Required]
        private TutorialDataScriptableObject tutorialRemoteData;
        /*[SerializeField, ListDrawerSettings(HideAddButton = true, HideRemoveButton = true)]
        private List<TutorialStepData> tutorialSteps = new List<TutorialStepData>
        {
            /* [0] #1#new TutorialStepData {title = "Intro Step"},
            /* [1] #1#new TutorialStepData {title = "Movement"},
            /* [2] #1#new TutorialStepData {title = "Rotate"},
            /* [3] #1#new TutorialStepData {title = "Falling Bits"},
            /* [4] #1#new TutorialStepData {title = "Combo"},
            /* [5] #1#new TutorialStepData {title = "Magnet"},

            /* [6] #1#new TutorialStepData {title = "Combo-magnet-1"},
            /* [7] #1#new TutorialStepData {title = "Combo-magnet-2"},

            /* [8] #1#new TutorialStepData {title = "Magnet-combo-1"},
            /* [9] #1#new TutorialStepData {title = "Magnet-combo-2"},

            /* [10] #1#new TutorialStepData {title = "Pulsar"},
            /* [11] #1#new TutorialStepData {title = "Pulsar-1"},

            /* [12] #1#new TutorialStepData {title = "Fuel"},
            /* [13] #1#new TutorialStepData {title = "Fuel-1"},
            /* [14] #1#new TutorialStepData {title = "Fuel-2"},
        };*/


        [SerializeField]
        private bool debug;
        
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
        private MonoBehaviour mono;

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
            mono = LevelManager.Instance;
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
            
            mono.StartCoroutine(MainTutorialCoroutine());

            _readyForInput = true;
            _isReady = true;
        }

        private void SetText(TutorialStepData tutorialStepData)
        {
            SetText(tutorialStepData.text);
        }
        
        private void SetText(string text)
        {
            CheckForSpriteReplacements(ref text);
            
            this.text.text = text;
        }

        //FIXME This is gross...
        private static void CheckForSpriteReplacements(ref string text)
        {
            text = text.Replace("#LEFT", TMP_SpriteMap.GetInputSprite("left"));
            text = text.Replace("#RIGHT", TMP_SpriteMap.GetInputSprite("right"));
            text = text.Replace("#UP", TMP_SpriteMap.GetInputSprite("up"));
            text = text.Replace("#DOWN", TMP_SpriteMap.GetInputSprite("down"));
        }

        //Tutorial Steps
        //====================================================================================================================//
        private IEnumerator MainTutorialCoroutine()
        {
            foreach (var stepCoroutine in _tutorialStepCoroutines)
            {
                yield return mono.StartCoroutine(stepCoroutine);
            }
            
            
        }
        
        private IEnumerator IntroStepCoroutine()
        {
            var bot = LevelManager.Instance.BotObject;
            bot.PROTO_GodMode = true;
            
            yield return mono.StartCoroutine(WaitStep(tutorialRemoteData[0], true));
        }
        private IEnumerator MoveStepCoroutine()
        {
            yield return mono.StartCoroutine(WaitStep(tutorialRemoteData[1], false));
            
            //TODO Need to wait for the movement of the Bot Left/Right
            bool left, right;
            left = right = false;

            while (!left || !right)
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
            yield return mono.StartCoroutine(WaitStep(tutorialRemoteData[2], false));
            
            bool left, right;
            left = right = false;

            while (!left || !right)
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
            yield return mono.StartCoroutine(WaitStep(tutorialRemoteData[3], true));
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
            
            SetText(tutorialRemoteData[3]);
            
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
                yield return mono.StartCoroutine(MagnetFirstCoroutine());
            }
            else if (combo)
            {
                //TODO Create Combo First Path
                yield return mono.StartCoroutine(ComboFirstCoroutine());
            }
            else
            {
                throw new Exception("Should not reach here");
            }

        }
        
        private IEnumerator ComboFirstCoroutine()
        {
            //tutorialSteps[4]
            yield return mono.StartCoroutine(PauseWaitTimerStep(tutorialRemoteData[4], true));
            
            SetText(tutorialRemoteData[6]);

            bool magnet = false;
            var bot = LevelManager.Instance.BotObject;

            void SetMagnet()
            {
                magnet = true;
            }

            bot.OnFullMagnet += SetMagnet;

            yield return new WaitUntil(() => magnet);
            
            bot.OnFullMagnet -= SetMagnet;
            
            yield return mono.StartCoroutine(PauseWaitTimerStep(tutorialRemoteData[7], true));
        }
        private IEnumerator MagnetFirstCoroutine()
        {
            //tutorialSteps[5]
            yield return mono.StartCoroutine(PauseWaitTimerStep(tutorialRemoteData[5], true));
            
            SetText(tutorialRemoteData[8]);

            bool combo = false;
            var bot = LevelManager.Instance.BotObject;

            void SetCombo()
            {
                combo = true;
            }

            bot.OnCombo += SetCombo;

            yield return new WaitUntil(() => combo);
            
            bot.OnCombo -= SetCombo;
            
            yield return mono.StartCoroutine(PauseWaitTimerStep(tutorialRemoteData[9], true));
        }

        private IEnumerator PulsarStepCoroutine()
        {
            SetText(tutorialRemoteData[10]);
            var bot = LevelManager.Instance.BotObject;
            
            bool bump = false;

            void SetBump()
            {
                bump = true;
            }

            bot.OnBitShift += SetBump;

            yield return new WaitUntil(() => bump);
            
            bot.OnBitShift -= SetBump;

            yield return mono.StartCoroutine(WaitStep(tutorialRemoteData[11], false));
        }
        
        private IEnumerator FuelStepCoroutine()
        {
            //TODO Set the bot able to use its fuel
            var bot = LevelManager.Instance.BotObject;
            bot.PROTO_GodMode = false;
            
            var playerData = PlayerPersistentData.PlayerData.liquidResource;
            
            yield return new WaitUntil(() => playerData[BIT_TYPE.RED] <= 0f);
            
            SetText(tutorialRemoteData[12]);
            
            //TODO Set the wave to spawn all reds
            
            yield return new WaitUntil(() => playerData[BIT_TYPE.RED] > 0f);

            yield return mono.StartCoroutine(WaitStep(tutorialRemoteData[13], false));
            
            SetText(tutorialRemoteData[14]);

        }

        private IEnumerator EndStepCoroutine()
        {
            yield return new WaitForSeconds(5f);

            Globals.UsingTutorial = false;
        }
        
        //Generic Tutorial Steps
        //====================================================================================================================//

        private IEnumerator PauseWaitTimerStep(TutorialStepData tutorialStepData, bool waitAnyKey)
        {
            Time.timeScale = 0f;

            yield return mono.StartCoroutine(WaitStep(tutorialStepData, waitAnyKey));
            
            Time.timeScale = 1f;
        }
        
        private IEnumerator WaitStep(TutorialStepData tutorialStepData, bool waitAnyKey)
        {
            fillImage.gameObject.SetActive(tutorialStepData.useWaitTime);
            pressAnyKeyText.gameObject.SetActive(!tutorialStepData.useWaitTime);
            
            SetText(tutorialStepData);

            if (tutorialStepData.useWaitTime)
            {
                _readyForInput = false;
                
                yield return new WaitTimeImage(tutorialStepData.waitTime, fillImage);

                pressAnyKeyText.gameObject.SetActive(true);
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
                if(_tutorialManager.debug) Debug.Log("Any Key Pressed");
                return false;
            }

            public void Reset() { }

            public object Current => null;

            public WaitForAnyKey(TutorialManager tutorialManager)
            {
                _tutorialManager = tutorialManager;
                
                if(_tutorialManager.debug) Debug.Log("Waiting for Any Key");

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
