using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using StarSalvager.Audio;
using StarSalvager.Cameras;
using StarSalvager.Factories;
using StarSalvager.Utilities;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.Inputs;
using StarSalvager.Utilities.Saving;
using StarSalvager.Utilities.UI;
using StarSalvager.Values;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Input = UnityEngine.Input;
using Random = UnityEngine.Random;

namespace StarSalvager.UI
{
    public class GameUI : SceneSingleton<GameUI>
    {
        [Serializable]
        private struct SliderCover
        {
            [Required, HorizontalGroup("Row 1"), LabelWidth(40)]
            public GameObject Slider;
            [Required, HorizontalGroup("Row 1"), LabelWidth(40)]
            public GameObject Cover;

            public void SetHidden(bool hidden)
            {
                Slider.SetActive(!hidden);
                Cover.SetActive(hidden);
            }
        }
        
        [Serializable]
        public struct SmartWeaponV2
        {
            [Required, FoldoutGroup("$NAME")]
            public Button buttonObject;
            [Required, FoldoutGroup("$NAME")]
            public Image buttonImage;
            [Required, FoldoutGroup("$NAME")]
            public Image iconImage;
            [Required, FoldoutGroup("$NAME")]
            public Slider Slider;
            [Required, FoldoutGroup("$NAME")]
            public GameObject buttonUnCover;
            
            [Required, FoldoutGroup("$NAME")]
            public GameObject buttonCover;

            //public Sprite[] sprites;

#if UNITY_EDITOR
            [SerializeField, PropertyOrder(-100), FoldoutGroup("$NAME")]
            private string NAME;
#endif

            public void Reset()
            {
                SetFill(1f);
                //SetHasResource(true);
                SetActive(false);
            }

            public void SetActive(bool state)
            {
                buttonCover.SetActive(!state);
                buttonUnCover.SetActive(state);
            }

            public void SetColor(Color color)
            {
                buttonImage.color = color;
            }

            public void SetFill(float val)
            {
                Slider.value = val;
                
                buttonObject.interactable = val >= 1f;
            }
        }

        [Serializable]
        public struct SmartWeaponIcon
        {
            #if UNITY_EDITOR
            private string NAME => Type.ToString();
            #endif
            
            [FoldoutGroup("$NAME")]
            public PART_TYPE Type;
            [Required, FoldoutGroup("$NAME")]
            public Sprite UISprite;

            public Color Color => GetPartColor(Type);

            private static Color GetPartColor(PART_TYPE type)
            {
                var factoryManager = FactoryManager.Instance;
                var burnType = factoryManager.PartsRemoteData.GetRemoteData(type).burnType;

                var color = factoryManager.BitProfileData.GetProfile(burnType).color;

                return color;
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
        
        //============================================================================================================//

        private const float MAGNET_FILL_VALUE = 0.02875f;
        
        #region Properties

        [SerializeField]
        private RectTransform viewableAreaTransform;

        [SerializeField, Required, FoldoutGroup("Slider Glows")]
        private Image redSliderGlow;

        [SerializeField, Required, FoldoutGroup("Slider Glows")]
        private Image greenSliderGlow;

        [SerializeField, Required, FoldoutGroup("Slider Glows")]
        private Image greySliderGlow;

        [SerializeField, Required, FoldoutGroup("Slider Glows")]
        private Image blueSliderGlow;

        [SerializeField, Required, FoldoutGroup("Slider Glows")]
        private Image yellowSliderGlow;

        //Top Left Window
        //============================================================================================================//

        [SerializeField, Required, FoldoutGroup("TL Window")]
        private TMP_Text gearsText;
        
        [SerializeField, Required, FoldoutGroup("TL Window")]
        private Slider gearsSlider;
        [SerializeField, Required, FoldoutGroup("TL Window"), Space(10f)]
        private TMP_Text patchPointsText;

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
        [FormerlySerializedAs("recoveryDronBanner")] [SerializeField, Required, FoldoutGroup("B Window")]
        private GameObject recoveryDroneBanner;
        

        //Bottom Left Window
        //============================================================================================================//

        [SerializeField, Required, FoldoutGroup("BL Window")]
        private SliderCover[] sliderCovers;
        
        [SerializeField, Required, FoldoutGroup("BL Window")]
        private SliderText fuelSlider;

        [SerializeField, Required, FoldoutGroup("BL Window")]
        private SliderText repairSlider;

        [SerializeField, Required, FoldoutGroup("BL Window")]
        private SliderText ammoSlider;
        
        [SerializeField, Required, FoldoutGroup("BL Window")]
        private SliderText waterSlider;
        [SerializeField, Required, FoldoutGroup("BL Window")]
        private SliderText powerSlider;
        


        //Right Window
        //============================================================================================================//

        [SerializeField, Required, FoldoutGroup("Smart Weapons")]
        private SmartWeaponIcon[] SmartWeaponIcons;

        [SerializeField, Required, FoldoutGroup("Smart Weapons")]
        //private SmartWeapon[] SmartWeaponsUI;
        private SmartWeaponV2[] SmartWeaponsUI;
        



        //Bottom Right Window
        //============================================================================================================//

        [SerializeField, Required, FoldoutGroup("BR Window")]
        private Image heatFillImage;

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


        //Heat Vignette
        //============================================================================================================//

        [SerializeField, Required, ToggleGroup("Extras/useVignette", "Vignette")]
        private bool useVignette;

        [SerializeField, Required, ToggleGroup("Extras/useVignette")]
        private Image vignetteImage;

        [SerializeField, Required, ToggleGroup("Extras/useVignette")]
        private Color vignetteMinColor;

        [SerializeField, Required, ToggleGroup("Extras/useVignette")]
        private Color vignetteMaxColor;

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

        #endregion //Properties

        //====================================================================================================================//

        private Image[] glowImages;
        private float _alpha;
        private float speed = 4f;

        //============================================================================================================//

        private void Start()
        {
            InitSliderText();

            vignetteImage.gameObject.SetActive(useVignette);

            ShowWaveSummaryWindow(false, string.Empty, string.Empty, null, instantMove: true);
            
            InitValues();

            glowImages = new[]
            {
                redSliderGlow,
                blueSliderGlow,
                greenSliderGlow,
                greySliderGlow,
                yellowSliderGlow,
                //heatSliderGlow
            };
        }

        private void OnEnable()
        {
            Toast.SetToastArea(viewableAreaTransform);
            SetupPlayerValues();

            PlayerDataManager.OnCapacitiesChanged += SetupPlayerValues;
            PlayerDataManager.OnValuesChanged += UpdatePlayerGearsLevel;
        }

        private void LateUpdate()
        {
            var value = 1f / (speed);

            _alpha = Mathf.PingPong(Time.time, value) / value;

            foreach (var image in glowImages)
            {
                if (!image.enabled)
                    continue;

                var color = image.color;
                color.a = _alpha;

                image.color = color;
            }
        }

        private void OnDisable()
        {
            Toast.SetToastArea(transform as RectTransform);
            
            PlayerDataManager.OnCapacitiesChanged -= SetupPlayerValues;
            PlayerDataManager.OnValuesChanged -= UpdatePlayerGearsLevel;
        }

        //============================================================================================================//

        private Canvas _canvas;
        public Vector2 GetViewSizeNormalize()
        {
            if(_canvas is null)
                _canvas = GetComponentInParent<Canvas>();

            var canvasSize = (_canvas.transform as RectTransform).sizeDelta;

            var size = viewableAreaTransform.rect.size;
            
            return new Vector2
            {
                x = size.x / canvasSize.x,
                y = size.y / canvasSize.y,  
            };
        }
        
        
        //============================================================================================================//


        private void InitValues()
        {
            
            InitSmartWeaponUI();
            ResetIcons();

            SetWaterValue(0f);
            SetPowerValue(0f);

            SetHeatSliderValue(0f);
            SetCarryCapacity(0f, 1);
            
            SetHealthValue(1f);

            SetFuelValue(0f);
            SetRepairValue(0f);
            SetAmmoValue(0f);

            SetProgressValue(0f);
            //SetTimeString("0:00");
            
            SetPlayerPatchPoints(0);
            SetPlayerGearsProgress((0, 0));
            ShowAbortWindow(false);

            ShowRecoveryBanner(false);
            ShowLiquidSliders(null);

            OutlineMagnet(false);

            SetDancersActive(false);
            FadeBackground(false, true);
        }

        private void InitSliderText()
        {
            fuelSlider.Init();
            repairSlider.Init();
            ammoSlider.Init();
            
            waterSlider.Init();
            powerSlider.Init();

        }

        private void InitSmartWeaponUI()
        {

            for (var i = 0; i < SmartWeaponsUI.Length; i++)
            {
                int index = i;
                var temp = SmartWeaponsUI[i];

                //temp.sprites = sprites;

                SmartWeaponsUI[i] = temp;
                SmartWeaponsUI[i].buttonObject.onClick.RemoveAllListeners();
                SmartWeaponsUI[i].buttonObject.onClick.AddListener(() =>
                {
                    InputManager.Instance.TriggerSmartWeapon(index);
                });

            }
        }

        //============================================================================================================//

        private void SetupPlayerValues()
        {
            ShowAbortWindow(false);

            SetResourceSliderBounds(BIT_TYPE.RED, 0, PlayerDataManager.GetResource(BIT_TYPE.RED).liquidCapacity);
            SetResourceSliderBounds(BIT_TYPE.GREEN, 0, PlayerDataManager.GetResource(BIT_TYPE.GREEN).liquidCapacity);
            SetResourceSliderBounds(BIT_TYPE.GREY, 0, PlayerDataManager.GetResource(BIT_TYPE.GREY).liquidCapacity);

            SetResourceSliderBounds(BIT_TYPE.BLUE, 0, PlayerDataManager.GetResource(BIT_TYPE.BLUE).resourceCapacity);
            SetResourceSliderBounds(BIT_TYPE.YELLOW, 0, PlayerDataManager.GetResource(BIT_TYPE.YELLOW).liquidCapacity);

            SetFuelValue(PlayerDataManager.GetResource(BIT_TYPE.RED).liquid);
            SetRepairValue(PlayerDataManager.GetResource(BIT_TYPE.GREEN).liquid);
            SetAmmoValue(PlayerDataManager.GetResource(BIT_TYPE.GREY).liquid);

            SetPlayerGearsProgress(PlayerDataManager.GetPatchPointProgress());
        }

        //============================================================================================================//

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
        
        //============================================================================================================//

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

        //============================================================================================================//

        public void AbortPressed()
        {
            LevelManager.Instance.BotObject.TrySelfDestruct();
                
            //If the bot was able to be killed, hide this window
            if(LevelManager.Instance.BotObject.Destroyed)
                ShowAbortWindow(false);
        }

        //TODO I should look into the NotifyPropertyChanged for setting up this functionality
        private void UpdatePlayerGearsLevel()
        {
            SetPlayerGearsProgress(PlayerDataManager.GetPatchPointProgress());

            //TODO Need to add the Patch Points connection here
            SetPlayerPatchPoints(PlayerDataManager.GetAvailablePatchPoints());
        }

        public void SetPlayerGearsProgress((int, int) patchPointProgress)
        {
            gearsSlider.minValue = 0;
            gearsSlider.maxValue = patchPointProgress.Item2;
            gearsSlider.value = patchPointProgress.Item1;
            
            //levelText.text = $"lvl {}";
            gearsText.text = $"{patchPointProgress.Item1} / {patchPointProgress.Item2}";
        }

        public void SetPlayerPatchPoints(int points)
        {
            patchPointsText.text = $"{points}";
        }

        public void SetResourceSliderBounds(BIT_TYPE type, int min, int max)
        {
            switch (type)
            {
                case BIT_TYPE.GREEN:
                    repairSlider.SetBounds(min, max);
                    break;
                case BIT_TYPE.GREY:
                    ammoSlider.SetBounds(min, max);
                    break;
                case BIT_TYPE.RED:
                    fuelSlider.SetBounds(min, max);
                    break;
                case BIT_TYPE.YELLOW:
                    powerSlider.SetBounds(min, max);
                    break;
                case BIT_TYPE.BLUE:
                    waterSlider.SetBounds(min, max);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }


        private bool _fuel;
        public void SetFuelValue(float value)
        {
            bool hasBitAttached = false;
            bool state = false;

            //FIXME This is inefficient, and I want to find a better way of reducing the calls here
            if (LevelManager.Instance != null && LevelManager.Instance.BotObject)
            {
                hasBitAttached = LevelManager.Instance.BotObject.attachedBlocks.HasBitAttached(BIT_TYPE.RED);
            }
            
            fuelSlider.value = value;

            //Only if there are not any bits attached
            if (!hasBitAttached)
            {
                state = CheckActivateGlow(fuelSlider, redSliderGlow);
            }

            //If we're glowing and we weren't before, play resource warning sound
            if (state && _fuel == false)
            {
                AudioController.PlaySound(SOUND.RESOURCE_WARNING);
            }

            _fuel = state;
        }

        public void SetRepairValue(float value)
        {
            repairSlider.value = value;
            CheckActivateGlow(repairSlider, greenSliderGlow);
        }

        public void SetAmmoValue(float value)
        {
            ammoSlider.value = value;
            CheckActivateGlow(ammoSlider, greySliderGlow);
        }
        
        public void SetWaterValue(float value)
        {
            
            waterSlider.value = value;

            CheckActivateGlow(waterSlider, blueSliderGlow);
        }

        public void SetPowerValue(float value)
        {
            powerSlider.value = value;
            CheckActivateGlow(powerSlider, yellowSliderGlow);
        }


        //====================================================================================================================//

        public void ShowLiquidSliders(IEnumerable<BIT_TYPE> types)
        {
            foreach (var sliderCover in sliderCovers)
            {
                sliderCover.SetHidden(true);
            }
            
            if(types.IsNullOrEmpty())
                return;

            foreach (var bitType in types)
            {
                UncoverSlider(bitType);
            }
            
        }

        private void UncoverSlider(BIT_TYPE bitType)
        {
            switch (bitType)
            {
                case BIT_TYPE.BLUE:
                    sliderCovers[4].SetHidden(false);
                    break;
                case BIT_TYPE.GREEN:
                    sliderCovers[2].SetHidden(false);
                    break;
                case BIT_TYPE.GREY:
                    sliderCovers[1].SetHidden(false);
                    break;
                case BIT_TYPE.RED:
                    sliderCovers[0].SetHidden(false);
                    break;
                case BIT_TYPE.YELLOW:
                    sliderCovers[3].SetHidden(false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(bitType), bitType, null);
            }
        }

        //============================================================================================================//


        public void SetProgressValue(float value)
        {
            progressSlider.value = value;
        }


        //============================================================================================================//

        [Button, DisableIf("_flashingBorder"), DisableInEditorMode, FoldoutGroup("Extras/Neon Border")]
        public void FlashBorder()
        {
            if (_flashingBorder)
                return;

            _flashingBorder = true;

            var time = Random.Range(flashTimeRange.x, flashTimeRange.y);
            
            StartCoroutine(BorderFlashingCoroutine(time));
        }

        private IEnumerator BorderFlashingCoroutine(float time)
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

        public void SetIconImage(int index, PART_TYPE partType)
        {
            if (index < 0) return;

            var smartWeaponIcon = SmartWeaponIcons.FirstOrDefault(x => x.Type == partType);

            if (smartWeaponIcon.UISprite != null)
            {
                var color = smartWeaponIcon.Color;
                SmartWeaponsUI[index].buttonImage.color = color;
                
                SmartWeaponsUI[index].iconImage.color = color;
                SmartWeaponsUI[index].iconImage.sprite = smartWeaponIcon.UISprite;

                return;
            }
            
            SmartWeaponsUI[index].buttonImage.color = Color.white;
            SmartWeaponsUI[index].iconImage.sprite = null;
        }
        public void ShowIcon(int index, bool state)
        {
            if (index < 0) return;
            SmartWeaponsUI[index].SetActive(state);
        }

        public void SetFill(int index, float fillValue)
        {
            if (index < 0) return;
            SmartWeaponsUI[index].SetFill(fillValue);
        }

        public void ResetIcons()
        {
            for (var i = 0; i < SmartWeaponsUI.Length; i++)
            {
                //FIXME Need to determine if we're still using the number text here
                /*SmartWeaponsUI[i].keyText.text = $"{i + 1}";*/
                SmartWeaponsUI[i].Reset();
            }
        }

        //============================================================================================================//

        public void SetCurrentWaveText(int sector, int wave)
        {
            sectorText.text = $"Sector {sector}.{wave}";
        }

        public void SetCurrentWaveText(string text)
        {
            sectorText.text = text;
        }

        //============================================================================================================//

        public void SetHealthValue(float value)
        {
            var inverse = 1f - value;

            var crackIncrement = 1f / crackImages.Length;

            for (int i = 0; i < crackImages.Length; i++)
            {
                crackImages[i].enabled = inverse >= crackIncrement * (i + 1);
            }
            
        }
        
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
            
            InputManager.SwitchCurrentActionMap(show ? "Menu Controls" : "Default");
            
            
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
        
        #region Dancers

        public void SetDancersActive(bool state)
        {
            dancersObject.SetActive(state);
        }
        
        #endregion

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
        


        /// <summary>
        /// Value sent should be normalized
        /// </summary>
        /// <param name="value"></param>
        public void SetHeatSliderValue(float value)
        {
            heatFillImage.fillAmount = value;

            if (useVignette)
                vignetteImage.color = Color.Lerp(vignetteMinColor, vignetteMaxColor, value);
        }

        //============================================================================================================//

        public void ShowRecoveryBanner(bool shown)
        {
            recoveryDroneBanner.SetActive(shown);
        }
        
        //====================================================================================================================//
        


        private static bool CheckActivateGlow(SliderText slider, Behaviour glowSlider)
        {
            return CheckActivateGlow(slider.Slider, glowSlider);
        }

        private static bool CheckActivateGlow(Slider slider, Behaviour glowSlider)
        {
            var value = slider.value / slider.maxValue;
            var glowing = value <= Globals.GameUIResourceThreshold;
            glowSlider.enabled = glowing;

            return glowing;
        }

        /*private static void CheckActivateGlowInverse(Slider slider, Behaviour glowSlider)
        {
            glowSlider.enabled = slider.value / slider.maxValue >= 0.75f;
        }*/

        //Patch point Effect
        //====================================================================================================================//

        [Button, DisableInEditorMode]
        public void CreatePatchPointEffect()
        {
            CreatePatchPointEffect(effectCount);
        }

        public void CreatePatchPointEffect(int count)
        {
            if (LevelManager.Instance is null || LevelManager.Instance.BotObject is null)
                return;

            if (GameManager.IsState(GameState.LevelEndWave) || GameManager.IsState(GameState.LevelBotDead))
                return;

            
            var patchSprite = FactoryManager.Instance.FacilityRemote.PatchSprite;

            
            var botWorldPosition = LevelManager.Instance.BotObject.transform.position;
            
            /*var viewportPoint = CameraController.Camera.WorldToViewportPoint(botWorldPosition);
            var canvasPoint = effectArea.sizeDelta * viewportPoint;*/
            var screenPoint = CameraController.Camera.WorldToScreenPoint(botWorldPosition);
            
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                effectArea,
                screenPoint,
                null,
                out var newPosition);

            StartCoroutine(PatchPointEffectCoroutine(newPosition, patchSprite, count));
        }

        private IEnumerator PatchPointEffectCoroutine(Vector2 startPosition,Sprite sprite, int count)
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


        //====================================================================================================================//
        

    }
}


