﻿using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using StarSalvager.Audio;
using StarSalvager.Tutorial.Data;
using StarSalvager.UI;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.Helpers;
using StarSalvager.Utilities.Inputs;
using StarSalvager.Utilities.Saving;
using StarSalvager.Utilities.SceneManagement;
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

        [SerializeField]
        private bool debug;

        //====================================================================================================================//
        
        [SerializeField, BoxGroup("Tutorial UI"), Required]
        private GameObject window;
        
        [SerializeField, BoxGroup("Tutorial UI"), Required]
        private TMP_Text text;
         [FormerlySerializedAs("image")] [SerializeField, BoxGroup("Tutorial UI"), Required]
        private Image fillImage;
        [SerializeField, BoxGroup("Tutorial UI"), Required]
        private GameObject pauseImage;
        

        [SerializeField, BoxGroup("Tutorial UI"), Required]
        private TMP_Text pressAnyKeyText;

        [SerializeField, BoxGroup("Tutorial UI"), Required]
        private GameObject characterObject;
 
        [SerializeField, BoxGroup("Tutorial UI")]
        private AnimationCurve slideCurve;
        [SerializeField, BoxGroup("Tutorial UI")]
        private AnimationCurve scaleCurve;

        //====================================================================================================================//
        
        private bool _readyForInput;
        private bool _keyPressed;

        private float _currentMoveDirection;

        private List<IEnumerator> _tutorialStepCoroutines;

        private InputManager _inputManager;

        private bool _isReady;
        private MonoBehaviour mono;

        private float _playerStartFuel;
        
        private bool _dialogOpen;


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
            //glowImage.gameObject.SetActive(false);
            //SetDialogWindowActive(false);
            pauseImage.SetActive(false);
            InitPositions();
            
            mono = LevelManager.Instance;
            InitInput();

            _playerStartFuel = PlayerDataManager.GetResource(BIT_TYPE.RED).Ammo;
            PlayerDataManager.GetResource(BIT_TYPE.RED).SetAmmo(30);
            
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

            LevelManager.Instance.SetBotBelowScreen();
            
            mono.StartCoroutine(MainTutorialCoroutine());

            
            LevelManager.Instance.GameUi.SetCurrentWaveText("Simulator");
            
            _readyForInput = true;
            _isReady = true;
        }

        private void SetText(TutorialStepData tutorialStepData, bool hideAnyKey = false, bool hideFillImage = false)
        {
            SetText(tutorialStepData.text, hideAnyKey, hideFillImage);
        }
        
        private void SetText(string text, bool hideAnyKey, bool hideFillImage)
        {
            CheckForSpriteReplacements(ref text);
            
            if(hideAnyKey) pressAnyKeyText.gameObject.SetActive(false);
            if(hideFillImage) fillImage.gameObject.SetActive(false);
            
            this.text.text = text;
        }

        //FIXME This is gross...
        private static void CheckForSpriteReplacements(ref string text)
        {
            text = text.Replace("#LEFT", TMP_SpriteHelper.GetInputSprite("left"));
            text = text.Replace("#RIGHT", TMP_SpriteHelper.GetInputSprite("right"));
            text = text.Replace("#UP", TMP_SpriteHelper.GetInputSprite("up"));
            text = text.Replace("#DOWN", TMP_SpriteHelper.GetInputSprite("down"));
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
            var bot = LevelManager.Instance.BotInLevel;
            bot.IsInvulnerable = true;

            yield return mono.StartCoroutine(SlideCharacterCoroutine(true));
            yield return mono.StartCoroutine(WaitStep(tutorialRemoteData[0], true));
        }
        private IEnumerator MoveStepCoroutine()
        {
            
            //TODO Need to have the bot fly in
            LevelManager.Instance.SetBotEnterScreen(true);
            
            var bot = LevelManager.Instance.BotInLevel;
            bot.IsInvulnerable = true;
            
            yield return mono.StartCoroutine(WaitStep(tutorialRemoteData[1], false));
            
            //Wait for the movement of the Bot Left/Right
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
            LevelManager.Instance.SetStage(1);
            
            yield return mono.StartCoroutine(WaitStep(tutorialRemoteData[15], false));
            
            
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
            
            yield return mono.StartCoroutine(WaitStep(tutorialRemoteData[3], false));

            var bot = LevelManager.Instance.BotInLevel;
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
            yield return mono.StartCoroutine(WaitStep(tutorialRemoteData[4], true));
            
            yield return mono.StartCoroutine(WaitShowDialogCoroutine(tutorialRemoteData[6], true, true));
            //SetText(tutorialRemoteData[6], true, true);

            var bot = LevelManager.Instance.BotInLevel;
            bool magnet = bot.HasFullMagnet;
            

            void SetMagnet()
            {
                magnet = true;
            }

            bot.OnFullMagnet += SetMagnet;

            yield return new WaitUntil(() => magnet);
            
            yield return mono.StartCoroutine(SlideCharacterCoroutine(false));
            SetMagnetGlow(true);
            
            bot.OnFullMagnet -= SetMagnet;
            
            yield return mono.StartCoroutine(WaitStep(tutorialRemoteData[7], true));
            
            LevelManager.Instance.BotInLevel.ForceDisconnectAllDetachables();
            yield return new WaitForSeconds(0.5f);
            
            yield return mono.StartCoroutine(SlideCharacterCoroutine(true));
            //glowImage.gameObject.SetActive(false);
            SetMagnetGlow(false);
        }
        private IEnumerator MagnetFirstCoroutine()
        {
            yield return mono.StartCoroutine(SlideCharacterCoroutine(false));
            SetMagnetGlow(true);
            
            //tutorialSteps[5]
            yield return mono.StartCoroutine(WaitStep(tutorialRemoteData[5], true));
            
            LevelManager.Instance.BotInLevel.ForceDisconnectAllDetachables();
            
            yield return mono.StartCoroutine(SlideCharacterCoroutine(true));
            SetMagnetGlow(false);
            
            yield return new WaitForSeconds(0.5f);
            
            /*if(_dialogOpen)
                yield return mono.StartCoroutine(ShowDialogWindowCoroutine(false));
            SetText(tutorialRemoteData[8], true, true);
            yield return mono.StartCoroutine(ShowDialogWindowCoroutine(true));*/
            
            yield return mono.StartCoroutine(WaitShowDialogCoroutine(tutorialRemoteData[8], true, true));

            bool combo = false;
            bool magnet = false;
            var bot = LevelManager.Instance.BotInLevel;

            //TODO need to also wait for magnet, and loop back if that's the case
            void SetCombo()
            {
                combo = true;
            }
            void SetMagnet()
            {
                magnet = true;
            }

            bot.OnCombo += SetCombo;
            bot.OnFullMagnet += SetMagnet;
            
            LevelManager.Instance.SetStage(2);

            yield return new WaitUntil(() => combo || magnet);
            
            bot.OnCombo -= SetCombo;
            bot.OnFullMagnet -= SetMagnet;

            if (magnet && !combo)
            {
                yield return mono.StartCoroutine(MagnetFirstCoroutine());
                yield break;
            }
            
            yield return mono.StartCoroutine(WaitStep(tutorialRemoteData[9], true));
        }

        private IEnumerator PulsarStepCoroutine()
        {
            LevelManager.Instance.SetStage(3);
            
            /*if(_dialogOpen)
                yield return mono.StartCoroutine(ShowDialogWindowCoroutine(false));
            SetText(tutorialRemoteData[10], true, true);
            yield return mono.StartCoroutine(ShowDialogWindowCoroutine(true));*/
            
            yield return mono.StartCoroutine(WaitShowDialogCoroutine(tutorialRemoteData[10], true, true));

            var bot = LevelManager.Instance.BotInLevel;
            
            bool bump = false;

            void SetBump()
            {
                bump = true;
            }

            bot.OnBitShift += SetBump;

            yield return new WaitUntil(() => bump);
            
            bot.OnBitShift -= SetBump;
            
            bot.CanUseResources = true;
            PlayerDataManager.GetResource(BIT_TYPE.RED).SetAmmo(6f);

            yield return mono.StartCoroutine(WaitStep(tutorialRemoteData[11], false));
        }

        private IEnumerator FuelStepCoroutine()
        {
            //TODO Set the bot able to use its fuel
            var bot = LevelManager.Instance.BotInLevel;
            
            LevelManager.Instance.SetStage(0);
            
            yield return new WaitUntil(() => PlayerDataManager.GetResource(BIT_TYPE.RED).Ammo <= 0f);
            
            LevelManager.Instance.SetStage(4);
            
            /*if(_dialogOpen)
                yield return mono.StartCoroutine(ShowDialogWindowCoroutine(false));
            SetText(tutorialRemoteData[12], true, true);
            yield return mono.StartCoroutine(ShowDialogWindowCoroutine(true));*/
            
            yield return mono.StartCoroutine(WaitShowDialogCoroutine(tutorialRemoteData[12], true, true));

            //TODO Set the wave to spawn all reds
            
            yield return new WaitUntil(() => PlayerDataManager.GetResource(BIT_TYPE.RED).Ammo > 0f);
            
            bot.IsInvulnerable = true;
            LevelManager.Instance.SetStage(3);

            yield return mono.StartCoroutine(WaitStep(tutorialRemoteData[13], false));
            
            /*if(_dialogOpen)
                yield return mono.StartCoroutine(ShowDialogWindowCoroutine(false));
            SetText(tutorialRemoteData[14], true, true);
            yield return mono.StartCoroutine(ShowDialogWindowCoroutine(true));*/
            
            yield return mono.StartCoroutine(WaitShowDialogCoroutine(tutorialRemoteData[14], true, true));
            
            pressAnyKeyText.gameObject.SetActive(false);

        }

        private IEnumerator EndStepCoroutine()
        {
            yield return new WaitForSeconds(4f);
            
            GameManager.SetCurrentGameState(GameState.LevelEndWave);
            
            
            LevelManager.Instance.BotInLevel.SetColliderActive(false);
            
            
            //TODO Bot needs to fly away
            LevelManager.Instance.SetBotExitScreen(true);
            
            yield return mono.StartCoroutine(ShowDialogWindowCoroutine(false));
            yield return mono.StartCoroutine(SlideCharacterCoroutine(false));

            yield return new WaitForSeconds(4f);

            /*float t = 0f;
            while (t < 1f)
            {
                fadeImage.color = Color.Lerp(Color.clear, Color.black, t);

                t += Time.deltaTime;
                
                yield return null;
            }*/
            
            //yield return new WaitForSeconds(1f);

            ScreenFade.Fade(() =>
            {
                GameManager.SetCurrentGameState(GameState.AccountMenu);
                Globals.UsingTutorial = false;
                LevelManager.Instance.SetBotExitScreen(false);
                LevelManager.Instance.BotInLevel.IsInvulnerable = false;


                PlayerDataManager.GetResource(BIT_TYPE.RED).SetAmmo(_playerStartFuel);
                SceneLoader.ActivateScene(SceneLoader.MAIN_MENU, SceneLoader.LEVEL, MUSIC.MAIN_MENU);
            });
        }
        
        //Generic Tutorial Steps
        //====================================================================================================================//

        /*private IEnumerator PauseWaitTimerStep(TutorialStepData tutorialStepData, bool waitAnyKey)
        {
            pauseImage.SetActive(true);
            Time.timeScale = 0f;

            yield return mono.StartCoroutine(WaitStep(tutorialStepData, waitAnyKey));
            
            Time.timeScale = 1f;
            
            pauseImage.SetActive(false);
        }*/
        
        private IEnumerator WaitStep(TutorialStepData tutorialStepData, bool waitAnyKey)
        {
            
            /*if(_dialogOpen)
                yield return mono.StartCoroutine(ShowDialogWindowCoroutine(false));
            
            
            SetText(tutorialStepData);
            yield return mono.StartCoroutine(ShowDialogWindowCoroutine(true));*/
            
            yield return mono.StartCoroutine(WaitShowDialogCoroutine(tutorialStepData, false, false));
            fillImage.gameObject.SetActive(tutorialStepData.useWaitTime);

            if (tutorialStepData.useWaitTime)
            {
                pressAnyKeyText.gameObject.SetActive(false);
                
                _readyForInput = false;
                
                yield return new WaitTimeImage(tutorialStepData.waitTime, fillImage);
                
                fillImage.gameObject.SetActive(false);

                pressAnyKeyText.gameObject.SetActive(waitAnyKey);
            } 
            
            pressAnyKeyText.gameObject.SetActive(waitAnyKey);
            
            if(waitAnyKey)
                yield return new WaitForAnyKey(this);
            
            _readyForInput = true;
        }

        /*private IEnumerator PresentCharacter()
        {
            yield return mono.StartCoroutine(SlideCharacterCoroutine(true));
            yield return mono.StartCoroutine(ShowDialogWindowCoroutine(true));
        }

        private IEnumerator HideCharacter()
        {
            yield return mono.StartCoroutine(ShowDialogWindowCoroutine(false));
            yield return mono.StartCoroutine(SlideCharacterCoroutine(false));
        }*/
        private IEnumerator WaitShowDialogCoroutine(TutorialStepData tutorialStepData, bool hideAnyKey, bool hideFillImage)
        {
            if(_dialogOpen)
                yield return mono.StartCoroutine(ShowDialogWindowCoroutine(false));
            
            SetText(tutorialStepData, hideAnyKey, hideFillImage);
            
            yield return mono.StartCoroutine(ShowDialogWindowCoroutine(true));
        }

        private IEnumerator SlideCharacterCoroutine(bool moveOnScreen)
        {
            var rectTransform = (RectTransform)characterObject.transform;
            var size = rectTransform.sizeDelta;

            var startPosition = moveOnScreen ? Vector2.right * size.x : Vector2.left * (size.x / 2);
            var endPosition = moveOnScreen ? Vector2.left * (size.x / 2) : Vector2.right * size.x;

            yield return mono.StartCoroutine(SlideInAnchorCoroutine(rectTransform,
                startPosition,
                endPosition));
        }

        private IEnumerator ShowDialogWindowCoroutine(bool show)
        {
            var rectTransform = (RectTransform)window.transform;

            var startScale = show ? Vector3.zero : Vector3.one;
            var endScale = show ? Vector3.one : Vector3.zero;

            yield return mono.StartCoroutine(LerpScaleCoroutine(rectTransform, startScale, endScale, 0.5f));

            _dialogOpen = show;
        }

        private void InitPositions()
        {
            var characterRectTransform = (RectTransform)characterObject.transform;
            var size = characterRectTransform.sizeDelta;
            characterRectTransform.anchoredPosition = Vector2.right * size.x;
            
            var rectTransform = (RectTransform)window.transform;
            rectTransform.localScale = Vector3.zero;
            
        }


        //IInput Functions
        //====================================================================================================================//

        private void SetMagnetGlow(bool state)
        {
            GameUI.Instance.OutlineMagnet(state);
            /*glowImage.gameObject.SetActive(true);
            
            //glowImage.transform.SetParent(glowBar);

            var glowRectTransform = (RectTransform) glowImage.transform;
            glowRectTransform.anchorMin = Vector2.zero;
            glowRectTransform.anchorMax = Vector2.one;
            
            glowRectTransform.sizeDelta = new Vector2(20f, 20f);
            glowRectTransform.localPosition = Vector3.zero;

            glowImage.SetActive(true);*/

        }
        
        //TODO Need to setup AnyKey
        
        public void InitInput()
        {
            _inputManager = InputManager.Instance;
            
            Input.Actions.Default.Continue.Enable();
            Input.Actions.Default.Continue.performed += AnyKeyPressed;


            //Found: https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/manual/ActionBindings.html?_ga=2.228834015.217367981.1603324316-246071923.1589462724#showing-current-bindings
            /*var key = Input.Actions.Default.Continue.GetBindingDisplayString(InputBinding.DisplayStringOptions.DontUseShortDisplayNames | InputBinding.DisplayStringOptions.DontOmitDevice);
            pressAnyKeyText.text = $"{key} to continue...";*/
            pressAnyKeyText.text = "Press Space to continue...";
        }

        public void DeInitInput()
        {
            Input.Actions.Default.Continue.Disable();
            Input.Actions.Default.Continue.performed -= AnyKeyPressed;
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

        private IEnumerator SlideInAnchorCoroutine(RectTransform rectTransform, Vector2 startPosition,
            Vector2 endPosition, float time = 1f)
        {
            float t = 0;
            while (t / time < 1f)
            {

                rectTransform.anchoredPosition =
                    Vector2.Lerp(startPosition, endPosition, slideCurve.Evaluate(t / time));

                t += Time.deltaTime;

                yield return null;
            }

        }

        private IEnumerator LerpScaleCoroutine(RectTransform rectTransform, Vector2 startScale, Vector2 endScale,
            float time = 1f)
        {
            float t = 0;
            while (t / time < 1f)
            {

                rectTransform.localScale = Vector2.Lerp(startScale, endScale, scaleCurve.Evaluate(t / time));

                t += Time.deltaTime;

                yield return null;
            }

        }

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
