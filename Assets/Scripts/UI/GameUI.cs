using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using StarSalvager.Cameras;
using StarSalvager.Factories;
using StarSalvager.UI.Hints;
using StarSalvager.Utilities;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.Inputs;
using StarSalvager.Utilities.JsonDataTypes;
using StarSalvager.Utilities.Saving;
using StarSalvager.Utilities.UI;
using StarSalvager.Values;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

using Random = UnityEngine.Random;

namespace StarSalvager.UI
{
    public class GameUI : SceneSingleton<GameUI>, IHasHintElement
    {
        //Structs
        //====================================================================================================================//

        #region Structs

        [Serializable]
        public struct SliderPartUI
        {
            public bool isFilled => Math.Abs(foregroundImage.fillAmount - 1f) < 0.02f;

            [Required, FoldoutGroup("$NAME")]
            public Image backgroundImage;
            [Required, FoldoutGroup("$NAME")]
            public Image foregroundImage;
            
            [Required, FoldoutGroup("$NAME")]
            public Image secondPartImage;

            [Required, FoldoutGroup("$NAME")]
            public Image triggerInputImage;

            [Required, FoldoutGroup("$NAME")] public Slider slider;
            [Required, FoldoutGroup("$NAME")] public Image fillImage;
            
#if UNITY_EDITOR
            [SerializeField, PropertyOrder(-100), FoldoutGroup("$NAME")]
            private string NAME;
#endif
            public void Reset()
            {
                SetFill(1f);
            }

            public void SetIsTrigger(in bool isTrigger, in Sprite triggerSprite)
            {
                backgroundImage.gameObject.SetActive(isTrigger);
                triggerInputImage.gameObject.SetActive(isTrigger);

                if (!isTrigger)
                    return;

                Debug.Log($"{nameof(SetIsTrigger)} is not implemented for this Prototype");
                triggerInputImage.gameObject.SetActive(false);
                //triggerInputImage.sprite = triggerSprite;
            }

            public void SetSprite(in Sprite partSprite)
            {
                backgroundImage.sprite = partSprite;
                foregroundImage.sprite = partSprite;
            }
            public void SetSecondSprite(in Sprite partSprite)
            {
                secondPartImage.sprite = partSprite;
            }

            public void SetColor(in Color color)
            {
                foregroundImage.color = color;
                secondPartImage.color = color;
            }
            
            public void SetBackgroundColor(in Color color)
            {
                backgroundImage.color = color;
            }

            public void SetFill(float val)
            {
                foregroundImage.fillAmount = val;
            }
        }

        //Used sprites from: https://thoseawesomeguys.com/prompts/
        [Serializable]
        public struct InputIcon
        {
#if UNITY_EDITOR
            [SerializeField, PropertyOrder(-100), FoldoutGroup("$NAME")]
            private string NAME;
#endif
            [SerializeField, Required, FoldoutGroup("$NAME")]
            private Sprite keyboardSprite;
            [SerializeField, Required, FoldoutGroup("$NAME")]
            private Sprite xboxControllerSprite;
            [SerializeField, Required, FoldoutGroup("$NAME")]
            private Sprite playstationControllerSprite;

            public Sprite GetInputSprite(in string deviceName)
            {
                if (deviceName.Equals("Keyboard") || deviceName.Equals("Mouse"))
                    return keyboardSprite;

                if (deviceName.Contains("XInputControllerWindows"))
                    return xboxControllerSprite;

                if (deviceName.Contains("DualShock"))
                    return playstationControllerSprite;

                throw new Exception();
            }
        }

        [Serializable]
        public struct WindowSpriteSet
        {
            public enum TYPE
            {
                DEFAULT,
                ORANGE,
                RED
            }

            [FoldoutGroup("$type")]
            public TYPE type;
            [FoldoutGroup("$type")]
            public Sprite backgroundImage;
            [FoldoutGroup("$type")]
            public Sprite crossbarImage;
            [FoldoutGroup("$type")]
            public Sprite verticalBarImage;

            [FoldoutGroup("$type")] public Color titleColor;
            [FoldoutGroup("$type")] public Color textColor;
        }

        #endregion //Structs

        //============================================================================================================//

        private const float MAGNET_FILL_VALUE = 0.02875f;

        #region Properties

        [SerializeField]
        private RectTransform viewableAreaTransform;

        //Top Left Window
        //============================================================================================================//

        [SerializeField, Required, FoldoutGroup("TL Window")]
        private TMP_Text gearsText;

        [SerializeField, Required, FoldoutGroup("TL Window")]
        private Slider gearsSlider;
        [FormerlySerializedAs("patchPointsText")] 
        [SerializeField, Required, FoldoutGroup("TL Window"), Space(10f)]
        private TMP_Text componentsText;

        //Top Right Window
        //====================================================================================================================//

        [SerializeField, Required, FoldoutGroup("TR Window")]
        private Slider progressSlider;

        [SerializeField, Required, FoldoutGroup("TR Window")]
        private TMP_Text sectorText;

        //Bottom Window
        //====================================================================================================================//
        [SerializeField, Required, FoldoutGroup("B Window")]
        private GameObject abortWindow;
        [SerializeField, Required, FoldoutGroup("B Window")]
        private Button abortButton;


        //Bottom Left Window
        //============================================================================================================//


        //Right Window
        //============================================================================================================//

        /*[SerializeField, Required, FoldoutGroup("Trigger Parts")]
        [FormerlySerializedAs("SmartWeaponIcons")]
        private TriggerPartIcon[] triggerPartIcons;

         [SerializeField, Required, FoldoutGroup("Trigger Parts")]
         [FormerlySerializedAs("SmartWeaponsUI")]
        private TriggerPartUI[] triggerPartUI;*/

        [SerializeField, Required, FoldoutGroup("Trigger Parts")]
        private SliderPartUI[] SliderPartUis;

        [SerializeField, Required, FoldoutGroup("Trigger Parts")]
        private InputIcon[] inputIcons;




        //Bottom Right Window
        //============================================================================================================//

        [SerializeField, Required, FoldoutGroup("BR Window")]
        [FormerlySerializedAs("heatFillImage")]
        private Image botHealthBarImage;

        [SerializeField, Required, FoldoutGroup("BR Window")]
        private Slider carryCapacitySlider;
        [SerializeField, Required, FoldoutGroup("BR Window")]
        private Image carryCapacityFillImage;

        //Wave Summary Window
        //====================================================================================================================//
        [SerializeField, Required, FoldoutGroup("Summary Window")]
        private Image fadeImage;

        [SerializeField, Required, FoldoutGroup("Summary Window")]
        private float fadeTime = 1f;

        [SerializeField, Required, FoldoutGroup("Summary Window")]
        private GameObject dancersObject;


        [SerializeField, Required, FoldoutGroup("Summary Window")]
        private RectTransform waveSummaryWindow;
        [SerializeField, Required, FoldoutGroup("Summary Window")]
        private TMP_Text waveSummaryTitle;
        [SerializeField, Required, FoldoutGroup("Summary Window")]
        private TMP_Text waveSummaryText;
        [SerializeField, Required, FoldoutGroup("Summary Window")]
        private Button confirmButton;
        [SerializeField, Required, FoldoutGroup("Summary Window")]
        private TMP_Text confirmButtonText;

        [Space(10f), SerializeField, Required, FoldoutGroup("Summary Window")]
        private Image backgroundImage;
        [SerializeField, Required, FoldoutGroup("Summary Window")]
        private Image crossbarImage;
        [SerializeField, Required, FoldoutGroup("Summary Window")]
        private Image[] verticalBarImages;

        [SerializeField, FoldoutGroup("Summary Window")]
        private WindowSpriteSet[] spriteSets;

        //Health Cracks
        //====================================================================================================================//
        [SerializeField, Required, FoldoutGroup("Extras"), FoldoutGroup("Extras/Cracks")]
        private CanvasGroup cracksCanvasGroup;
        [SerializeField, Required, FoldoutGroup("Extras/Cracks")]
        private Image[] crackImages;

        //Patch Point Effect
        //====================================================================================================================//

        #region Patch Point Effect

        [SerializeField, FoldoutGroup("Patch Point Effect"), Required]
        private RectTransform effectArea;
        [SerializeField, FoldoutGroup("Patch Point Effect"), Required]
        private RectTransform moveTargetTransform;
        [SerializeField, FoldoutGroup("Patch Point Effect"), Required]
        private Image imagePrefab;
        [SerializeField, FoldoutGroup("Patch Point Effect"), Required]
        private float imageSize = 50;
        [SerializeField, FoldoutGroup("Patch Point Effect"), Range(0.1f, 20f)]
        private float effectRadius;
        [SerializeField, FoldoutGroup("Patch Point Effect"), Range(1, 10)]
        private int effectCount;

        [SerializeField, FoldoutGroup("Patch Point Effect")]
        private float rotationSpeed;

        [SerializeField, FoldoutGroup("Patch Point Effect"), Range(0.01f, 2f)]
        private float spawnTime;
        [SerializeField, FoldoutGroup("Patch Point Effect")]
        private AnimationCurve spawnCurve;

        [SerializeField, FoldoutGroup("Patch Point Effect"), Range(0.01f, 2f)]
        private float moveTime;
        [SerializeField, FoldoutGroup("Patch Point Effect")]
        private AnimationCurve moveCurve;

        #endregion //Patch Point Effect

        //Combo Effect Properties
        //====================================================================================================================//

        #region Combo Effect Properties

        [SerializeField, BoxGroup("Combo Effect"), MinMaxSlider(1,10,true)]
        private Vector2Int effectElementCount;
        [SerializeField, BoxGroup("Combo Effect"), MinMaxSlider(0,2,true)]
        private Vector2 moveTimeRange;
        [SerializeField, BoxGroup("Combo Effect")]
        private Sprite[] bitEffectSprites;
        [SerializeField, BoxGroup("Combo Effect")]
        private RectTransform[] sliderTargets;

        #endregion //Combo Effect Properties

        //Other
        //============================================================================================================//
        [SerializeField, Required, FoldoutGroup("Extras")]
        private FadeUIImage magnetFlash;

        [SerializeField, MinMaxSlider(0.2f, 2f, true), FoldoutGroup("Extras/Neon Border")]
        private Vector2 flashTimeRange;
        [SerializeField, Required, FoldoutGroup("Extras/Neon Border")]
        private Image borderGlow;
        [SerializeField, Required, FoldoutGroup("Extras/Neon Border")]
        private Image border;
        [SerializeField, FoldoutGroup("Extras/Neon Border")]
        private AnimationCurve flashCurve;
        private bool _flashingBorder;
        
        
        private Image[] glowImages;
        private float _alpha;
        private float speed = 4f;
        
        private Canvas _canvas;

        #endregion //Properties
        
        //Unity Functions
        //============================================================================================================//

        #region Unity Functions

        private void Start()
        {
            InputManager.InputDeviceChanged += TryUpdateInputSprites;
            ShowWaveSummaryWindow(false, string.Empty, string.Empty, null, instantMove: true);

            InitValues();
        }

        private void OnEnable()
        {
            Toast.SetToastArea(viewableAreaTransform);
            SetupPlayerValues();

            PlayerDataManager.OnCapacitiesChanged += SetupPlayerValues;
            PlayerDataManager.OnValuesChanged += ValuesUpdated;
        }

        private void OnDisable()
        {
            Toast.SetToastArea(transform as RectTransform);

            PlayerDataManager.OnCapacitiesChanged -= SetupPlayerValues;
            PlayerDataManager.OnValuesChanged -= ValuesUpdated;
        }

        #endregion //Unity Functions

        //Hint UI
        //============================================================================================================//

        public object[] GetHintElements(HINT hint)
        {
            switch (hint)
            {
                case HINT.NONE:
                    return null;
                case HINT.MAGNET:
                    return null;
                    /*return new object[]
                    {
                        magnetFlash.transform as RectTransform
                    };*/
                default:
                    throw new ArgumentOutOfRangeException(nameof(hint), hint, null);
            }
        }

        //Init UI
        //====================================================================================================================//

        #region Init UI

        private void InitValues()
        {
            SetupAmmoSliders();

            //InitSmartWeaponUI();
            ResetIcons();


            SetCarryCapacity(0f, 1);

            SetHealthValue(1f);
            SetLevelProgressSlider(0f);


            SetPlayerComponents(0);
            SetPlayerXP(0);
            ShowAbortWindow(false);

            OutlineMagnet(false);

            SetDancersActive(false);
            FadeBackground(false, true);
        }
        
        private void SetupPlayerValues()
        {
            ShowAbortWindow(false);

            SetPlayerXP(PlayerDataManager.GetXPThisRun());
            SetPlayerComponents(PlayerDataManager.GetComponents());

            UpdateAmmoSliders();
        }

        #endregion //Init UI
        
        //Ammo Sliders
        //============================================================================================================//

        #region Ammo Sliders

        private void SetupAmmoSliders()
        {
            for (var i = 0; i < Constants.BIT_ORDER.Length; i++)
            {
                SliderPartUis[i].fillImage.color = Constants.BIT_ORDER[i].GetColor();

                SliderPartUis[i].slider.minValue = 0;
            }
        }

        private void UpdateAmmoSliders()
        {
            for (var i = 0; i < Constants.BIT_ORDER.Length; i++)
            {
                var resource = PlayerDataManager.GetResource(Constants.BIT_ORDER[i]);

                SliderPartUis[i].slider.maxValue = resource.AmmoCapacity;
                SliderPartUis[i].slider.value = resource.Ammo;
            }
        }

        #endregion //Ammo Sliders

        //Magnets
        //============================================================================================================//

        #region Magnets

        public void SetCarryCapacity(float value, int max)
        {
            carryCapacityFillImage.pixelsPerUnitMultiplier = max * MAGNET_FILL_VALUE;
            carryCapacitySlider.value = value;

        }

        public void OutlineMagnet(bool state)
        {
            magnetFlash.SetActive(state);
        }

        public void FlashMagnet()
        {
            magnetFlash.FlashOnce();
        }

        #endregion //Magnets

        //Abort Window
        //============================================================================================================//

        #region Abort Window

        private bool _abortWindowShown = true;
        public void ShowAbortWindow(bool shown)
        {
            //Prevent repeated calls
            if (_abortWindowShown == shown)
                return;

            _abortWindowShown = shown;

            if (!shown)
            {
                abortButton.onClick.RemoveAllListeners();
                abortWindow.SetActive(false);
                return;
            }

            abortWindow.SetActive(true);

            if (Globals.UsingTutorial)
            {
                abortButton.gameObject.SetActive(false);
                return;
            }

            abortButton.gameObject.SetActive(true);

            abortButton.onClick.AddListener(AbortPressed);
        }
        
        public void AbortPressed()
        {
            LevelManager.Instance.BotInLevel.TrySelfDestruct();

            //If the bot was able to be killed, hide this window
            if(LevelManager.Instance.BotInLevel.Destroyed)
                ShowAbortWindow(false);
        }

        #endregion //Abort Window

        //Update UI
        //============================================================================================================//

        #region Update UI

        private void ValuesUpdated()
        {
            SetPlayerComponents(PlayerDataManager.GetComponents());
            SetPlayerXP(PlayerDataManager.GetXPThisRun());
            //SetPlayerXP(PlayerDataManager.get);

            UpdateAmmoSliders();
        }

        public void SetHealthValue(float value)
        {
            var inverse = 1f - value;

            var crackIncrement = 1f / crackImages.Length;

            for (int i = 0; i < crackImages.Length; i++)
            {
                crackImages[i].enabled = inverse >= crackIncrement * (i + 1);
            }


            botHealthBarImage.color = Color.Lerp(Color.red, Color.green, value);
            botHealthBarImage.fillAmount = value;

        }

        public void SetPlayerXP(in int xp)
        {
            gearsText.text = $"{xp} XP";
        }

        public void SetPlayerComponents(in int points)
        {
            componentsText.text = $"{points}";
        }


        public void SetLevelProgressSlider(in float value)
        {
            progressSlider.value = value;
        }

        public void SetCurrentWaveText(int sector, int wave)
        {
            sectorText.text = $"Sector {sector}.{wave}";
        }

        public void SetCurrentWaveText(string text)
        {
            sectorText.text = text;
        }
        
        #endregion //Update UI

        //Neon Border Flashing
        //============================================================================================================//

        #region Neon Border Flashing

        [Button, DisableIf("_flashingBorder"), DisableInEditorMode, FoldoutGroup("Extras/Neon Border")]
        public void FlashNeonBorder()
        {
            if (_flashingBorder)
                return;

            _flashingBorder = true;

            var time = Random.Range(flashTimeRange.x, flashTimeRange.y);

            StartCoroutine(NeonBorderFlashingCoroutine(time));
        }

        private IEnumerator NeonBorderFlashingCoroutine(float time)
        {
            float t = 0f;
            Color startBorderGlowColor = borderGlow.color;
            Color endGlowColor = Color.clear;

            Color startBorderColor = Color.white;
            Color darkBorderColor = new Color(0.5f,0.5f,0.5f);

            var mult = Random.Range(1f, 5f);

            while (t / time < 1f)
            {
                var td = 1f - flashCurve.Evaluate((t / time) * mult);
                borderGlow.color = Color.Lerp(startBorderGlowColor, endGlowColor, td);
                border.color = Color.Lerp(startBorderColor, darkBorderColor, td);

                t += Time.deltaTime;

                yield return null;
            }

            borderGlow.color = startBorderGlowColor;
            border.color = startBorderColor;

            _flashingBorder = false;
        }

        #endregion //Neon Border Flashing

        //Part UI
        //====================================================================================================================//

        #region Part UI

        private void TryUpdateInputSprites(string newDeviceName)
        {
            Debug.Log($"{nameof(TryUpdateInputSprites)} is not implemented for this Prototype");
            return;
            var indices = new[]
            {
                0, 1, 3, 4
            };
            
            for (var i = 0; i < indices.Length; i++)
            {
                var index = indices[i];
                var sliderPartUi = SliderPartUis[index];

                if (!sliderPartUi.triggerInputImage.gameObject.activeInHierarchy)
                    continue;

                sliderPartUi.triggerInputImage.sprite =
                    inputIcons[index].GetInputSprite(newDeviceName);

            }
        }

        public void SetIconImage(int index, in PART_TYPE partType)
        {
            //--------------------------------------------------------------------------------------------------------//
            
            Sprite GetInputSprite(in int bitIndex)
            {
                return inputIcons[bitIndex].GetInputSprite(InputManager.CurrentInputDeviceName);
            }

            //--------------------------------------------------------------------------------------------------------//
            
            if (index < 0) return;

            if (partType == PART_TYPE.EMPTY)
            {
                SliderPartUis[index].SetIsTrigger(false, null);
                SliderPartUis[index].SetSprite(null);
                SliderPartUis[index].SetColor(Color.clear);
                return;
            }

            var partRemoteData = FactoryManager.Instance.PartsRemoteData.GetRemoteData(partType);

            var isTrigger = partRemoteData.isManual;
            var sprite = FactoryManager.Instance.PartsProfileData.GetProfile(partType).GetSprite(0);

            SliderPartUis[index].SetIsTrigger(isTrigger, isTrigger ? GetInputSprite(index) : null);
            SliderPartUis[index].SetSprite(sprite);
            SliderPartUis[index].SetColor(partRemoteData.category.GetColor());
        }
        public void SetSecondIconImage(int index, in PART_TYPE partType)
        {
            //--------------------------------------------------------------------------------------------------------//
            
            if (index < 0) return;

            var sprite = FactoryManager.Instance.PartsProfileData.GetProfile(partType).GetSprite(0);

            SliderPartUis[index].SetSecondSprite(sprite);
        }

        public bool GetIsFilled(in int index)
        {
            if (index < 0) 
                return false;
            return SliderPartUis[index].isFilled;
        }
        
        public void SetFill(int index, float fillValue)
        {
            if (index < 0) return;
            SliderPartUis[index].SetFill(fillValue);
        }

        public void ResetIcons()
        {
            for (var i = 0; i < SliderPartUis.Length; i++)
            {
                //FIXME Need to determine if we're still using the number text here
                /*SmartWeaponsUI[i].keyText.text = $"{i + 1}";*/
                SliderPartUis[i].Reset();
            }
        }

        #endregion //Part UI

        //Wave Summary Window Functions
        //====================================================================================================================//

        #region Wave Summary Window

        private bool _movingSummaryWindow;

        public void ShowWaveSummaryWindow(bool show,
            in string title,
            in string text,
            Action onConfirmCallback,
            string buttonText = "Continue",
            WindowSpriteSet.TYPE type = WindowSpriteSet.TYPE.DEFAULT,
            float moveTime = 1f,
            bool instantMove = false)
        {
            if (_movingSummaryWindow)
                return;

            InputManager.SwitchCurrentActionMap(show ? ACTION_MAP.MENU : ACTION_MAP.DEFAULT);


            float targetY;
            if (show)
            {
                targetY = -waveSummaryWindow.sizeDelta.y / 4f;

                confirmButtonText.text = buttonText;

                confirmButton.onClick.RemoveAllListeners();
                confirmButton.onClick.AddListener(() =>
                {
                    ShowWaveSummaryWindow(false, string.Empty, string.Empty, null, instantMove: true);
                    onConfirmCallback?.Invoke();
                });

                var spriteSet = spriteSets.FirstOrDefault(ss => ss.type == type);

                backgroundImage.sprite = spriteSet.backgroundImage;
                crossbarImage.sprite = spriteSet.crossbarImage;

                foreach (var verticalBarImage in verticalBarImages)
                {
                    verticalBarImage.sprite = spriteSet.verticalBarImage;
                }

                waveSummaryTitle.color = spriteSet.titleColor;
                waveSummaryText.color = spriteSet.textColor;



            }
            else
            {
                targetY = waveSummaryWindow.sizeDelta.y * 1.5f;
            }

            waveSummaryTitle.text = title;
            waveSummaryText.text = text;

            if (instantMove)
            {
                var newPos = waveSummaryWindow.anchoredPosition;
                newPos.y = targetY;
                waveSummaryWindow.anchoredPosition = newPos;

                return;
            }

            StartCoroutine(PositionWaveSummaryWindow(waveSummaryWindow, targetY, moveTime));

        }

        private IEnumerator PositionWaveSummaryWindow(RectTransform rectTransform, float targetYPos, float time)
        {
            confirmButton.interactable = false;
            _movingSummaryWindow = true;

            var t = 0f;

            var startPos = rectTransform.anchoredPosition;
            var endPos = startPos;
            endPos.y = targetYPos;

            while (t / time < 1f)
            {
                rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, t / time);

                t += Time.deltaTime;
                yield return null;
            }

            _movingSummaryWindow = false;
            confirmButton.interactable = true;

            EventSystem.current.SetSelectedGameObject(confirmButton.gameObject);

        }

        #endregion //Wave Summary Window

        //Dancers
        //====================================================================================================================//
        
        #region Dancers

        public void SetDancersActive(bool state)
        {
            dancersObject.SetActive(state);
        }

        #endregion

        //Fading
        //====================================================================================================================//

        #region Fading

        public void FadeBackground(bool fadeIn, bool instant = false)
        {


            var startColor = fadeIn ? Color.clear : Color.black;
            var endColor = fadeIn ? Color.black : Color.clear;

            if (instant)
            {
                fadeImage.color = endColor;
                return;
            }

            if (_fading)
                return;

            StartCoroutine(FadeBackground(fadeTime, startColor, endColor));
        }

        private bool _fading;
        private IEnumerator FadeBackground(float time, Color startColor, Color endColor)
        {
            _fading = true;
            float t = 0;

            fadeImage.color = startColor;

            while (t / time <= 1f)
            {
                fadeImage.color = Color.Lerp(startColor, endColor, t / time);


                t += Time.deltaTime;

                yield return null;
            }


            fadeImage.color = endColor;

            _fading = false;
        }

        #endregion //Fading

        //Patch point Effect
        //====================================================================================================================//

        #region Patch Point Effects

        [Button, DisableInEditorMode]
        public void CreatePatchPointEffect()
        {
            CreatePatchPointEffect(effectCount);
        }

        public void CreatePatchPointEffect(int count)
        {
            if (LevelManager.Instance is null || LevelManager.Instance.BotInLevel is null)
                return;

            if (GameManager.IsState(GameState.LevelEndWave) || GameManager.IsState(GameState.LevelBotDead))
                return;


            var patchSprite = FactoryManager.Instance.PatchSprite;

            var botWorldPosition = LevelManager.Instance.BotInLevel.transform.position;

            var screenPoint = CameraController.Camera.WorldToScreenPoint(botWorldPosition);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                effectArea,
                screenPoint,
                null,
                out var newPosition);

            StartCoroutine(PatchPointEffectCoroutine(newPosition, patchSprite, count));

            /*if (count >= 1 && HintManager.CanShowHint(HINT.PATCH_POINT))
            {
                HintManager.TryShowHint(HINT.PATCH_POINT, patchPointsText.transform as RectTransform);
            }*/
        }

        private IEnumerator PatchPointEffectCoroutine(Vector2 startPosition, Sprite sprite, int count)
        {
            var transforms = new RectTransform[count];
            var spawnPositions = new Vector2[count];
            var rotateDirection = new bool[count];

            for (var i = 0; i < count; i++)
            {
                var image = Instantiate(imagePrefab);
                image.sprite = sprite;

                var trans = (RectTransform)image.transform;
                trans.sizeDelta = Vector2.one * imageSize;
                trans.SetParent(effectArea, false);
                trans.localScale = Vector3.zero;
                trans.localPosition = startPosition;
                transforms[i] = trans;

                spawnPositions[i] = startPosition +
                                    new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized * (effectRadius * 10f);
                rotateDirection[i] = Random.value > 0.5f;
            }

            var t = 0f;

            while (t / spawnTime <= 1f)
            {
                var deltaTime = Time.deltaTime;
                var td = spawnCurve.Evaluate(t / spawnTime);

                for (int i = 0; i < count; i++)
                {
                    transforms[i].localPosition = Vector2.Lerp(startPosition, spawnPositions[i], td);
                    transforms[i].localScale = Vector3.Lerp(Vector3.zero, Vector3.one, td);
                    transforms[i].localEulerAngles += Vector3.forward * (rotationSpeed * (rotateDirection[i] ? 1f : -1f) * deltaTime);
                }

                t += deltaTime;
                yield return null;
            }

            for (int i = 0; i < count; i++)
            {
                spawnPositions[i] = transforms[i].localPosition;
            }

            t = 0f;
            var targetPosition = effectArea.transform.InverseTransformPoint(moveTargetTransform.position);

            while (t / moveTime <= 1f)
            {
                var deltaTime = Time.deltaTime;
                var td = moveCurve.Evaluate(t / moveTime);

                for (var i = 0; i < count; i++)
                {
                    transforms[i].localPosition = Vector2.Lerp(spawnPositions[i], targetPosition, td);
                    transforms[i].localScale = Vector3.Lerp(Vector3.one, Vector3.zero, spawnCurve.Evaluate(t/moveTime));
                    transforms[i].localEulerAngles += Vector3.forward * (rotationSpeed * (rotateDirection[i] ? 1f : -1f) * deltaTime);
                }

                t += deltaTime;
                yield return null;
            }

            for (int i = 0; i < count; i++)
            {
                Destroy(transforms[i].gameObject);
            }

        }

        #endregion //Patch Point Effects

        //Ammo Effect
        //====================================================================================================================//

        //FIXME Adding ammo in this method could cause a loss either from early destruction of the coroutine, or division
        #region Ammo Effect

        public void CreateAmmoEffect(in BIT_TYPE bitType, in float amount, in Vector2 startPosition)
        {
            CreateAmmoEffect(bitType, 
                amount,
                startPosition, 
                effectElementCount.x, effectElementCount.y,
                moveTimeRange);
        }
        private void CreateAmmoEffect(in BIT_TYPE bitType, in float amount, in Vector2 startPosition, in int minCount, in int maxCount, in Vector2 moveTimeRange)
        {
            const float RADIUS = 50;
            
            var sprite = bitEffectSprites[(int) bitType - 1];
            var targetTransform = sliderTargets[(int) bitType - 1];
            
            var count = Random.Range(minCount, maxCount);
            var dividedAmount = amount / count;
            
            var screenPoint = CameraController.Camera.WorldToScreenPoint(startPosition);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                effectArea,
                screenPoint,
                null,
                out var newPosition);


            StartCoroutine(AmmoEffectCoroutine(
                bitType,
                dividedAmount,
                targetTransform,
                newPosition, 
                sprite,
                RADIUS,
                count, 
                moveTimeRange));
        }

        private IEnumerator AmmoEffectCoroutine(
            BIT_TYPE bitType,
            float dividedAmount,
            Transform targetTransform, 
            Vector2 startPosition, 
            Sprite sprite,
            float radius, 
            int count, 
            Vector2 delayRange)
        {
            Vector3 TARGET_SCALE = Vector3.one * 0.2f;
            
            var transforms = new RectTransform[count];
            var rotateDirection = new bool[count];

            for (var i = 0; i < count; i++)
            {
                var image = Instantiate(imagePrefab);
                image.sprite = sprite;

                var trans = (RectTransform) image.transform;
                trans.sizeDelta = Vector2.one * imageSize;
                trans.SetParent(effectArea, false);
                trans.localScale = Vector3.zero;
                trans.localPosition = startPosition + Random.insideUnitCircle * radius;
                transforms[i] = trans;

                rotateDirection[i] = Random.value > 0.5f;
            }

            var t = 0f;
            var fastSpawnTime = spawnTime / 2f;

            while (t / fastSpawnTime <= 1f)
            {
                var deltaTime = Time.deltaTime;
                var td = spawnCurve.Evaluate(t / fastSpawnTime);

                for (int i = 0; i < count; i++)
                {
                    transforms[i].localScale = Vector3.Lerp(Vector3.zero, TARGET_SCALE, td);
                    transforms[i].localEulerAngles +=
                        Vector3.forward * (rotationSpeed * (rotateDirection[i] ? 1f : -1f) * deltaTime);
                }

                t += deltaTime;
                yield return null;
            }
            
            var targetPosition = effectArea.transform.InverseTransformPoint(targetTransform.position);
            
            for (int i = 0; i < count; i++)
            {
                StartCoroutine(AmmoElementMoveCoroutine(
                    bitType,
                    dividedAmount,
                    transforms[i],
                    targetPosition,
                    TARGET_SCALE,
                    rotateDirection[i],
                    Random.Range(moveTimeRange.x, moveTimeRange.y)
                ));
            }
        }

        private IEnumerator AmmoElementMoveCoroutine(
            BIT_TYPE bitType,
            float dividedAmount,
            Transform movingTransform,
            Vector2 targetPosition,
            Vector2 targetScale,
            bool rotationDirection,
            float moveTime)
        {
            var t = 0f;
            var startPosition = movingTransform.localPosition;

            yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
            
            while (t / moveTime <= 1f)
            {
                var deltaTime = Time.deltaTime;
                var td = moveCurve.Evaluate(t / moveTime);

                movingTransform.localPosition = Vector2.Lerp(startPosition, targetPosition, td);
                movingTransform.localScale =
                    Vector3.Lerp(targetScale, Vector3.zero, spawnCurve.Evaluate(t / moveTime));
                movingTransform.localEulerAngles +=
                    Vector3.forward * (rotationSpeed * (rotationDirection ? 1f : -1f) * deltaTime);

                t += deltaTime;
                yield return null;
            }

            var resource = PlayerDataManager.GetResource(bitType);
            resource.AddAmmo(dividedAmount);
            
            
            Destroy(movingTransform.gameObject);
        }
        
#if UNITY_EDITOR
        [Button, BoxGroup("Combo Effect"), DisableInEditorMode]
        private void TestComboEffect()
        {
            var bitType = (BIT_TYPE) Random.Range(1, 6);
            var count = LevelManager.Instance.BotInLevel.AttachedBlocks.Count;
            var startPosition = LevelManager.Instance.BotInLevel.AttachedBlocks[Random.Range(0, count)].transform
                .position;

            CreateAmmoEffect(bitType, 
                Random.Range(5, 50),
                startPosition, 
                effectElementCount.x, effectElementCount.y,
                moveTimeRange);
        }
#endif

        #endregion //Ammo Effect

        //====================================================================================================================//
        
        public static void ClearEventSelected()
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        //====================================================================================================================//
        
    }
}
