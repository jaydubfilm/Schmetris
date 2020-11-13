using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using StarSalvager.Factories;
using StarSalvager.Utilities;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.Inputs;
using StarSalvager.Utilities.Saving;
using StarSalvager.Utilities.UI;
using StarSalvager.Values;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
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

        /*[SerializeField, Required, FoldoutGroup("Slider Glows")]
        private Image heatSliderGlow;*/

        //Top Left Window
        //============================================================================================================//

        [SerializeField, Required, FoldoutGroup("TL Window")]
        private TMP_Text gearsText;
        
        [SerializeField, Required, FoldoutGroup("TL Window")]
        private Slider gearsSlider;
        [SerializeField, Required, FoldoutGroup("TL Window"), Space(10f)]
        private TMP_Text patchPointsText;
        
        /*[SerializeField, Required, FoldoutGroup("TL Window")]
        private TMP_Text sectorText;

        [SerializeField, Required, FoldoutGroup("TL Window")]
        private TMP_Text timeText;

        [SerializeField, Required, FoldoutGroup("TL Window")]
        private Image clockImage;*/

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

        /*[SerializeField, Required, FoldoutGroup("BL Window")]
        private TMP_Text levelText;*/
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
        
        /*[SerializeField, Required, FoldoutGroup("Smart Weapons")]
        private Sprite normalSprite;
        [SerializeField, Required, FoldoutGroup("Smart Weapons")]
        private Sprite readySprite;
        [SerializeField, Required, FoldoutGroup("Smart Weapons")]
        private Sprite disabledSprite;*/
       
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

        [SerializeField, Required, FoldoutGroup("Wave Summary Window")]
        private RectTransform waveSummaryWindow;
        [SerializeField, Required, FoldoutGroup("Wave Summary Window")]
        private TMP_Text waveSummaryTitle;
        [SerializeField, Required, FoldoutGroup("Wave Summary Window")]
        private TMP_Text waveSummaryText;
        [SerializeField, Required, FoldoutGroup("Wave Summary Window")]
        private Button confirmButton;

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

        //Other
        //============================================================================================================//
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
            
            abortButton.onClick.AddListener(() =>
            {
                LevelManager.Instance.BotObject.TrySelfDestruct();
                
                //If the bot was able to be killed, hide this window
                if(LevelManager.Instance.BotObject.Destroyed)
                    ShowAbortWindow(false);

            });
        }

        //============================================================================================================//

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

        public void SetFuelValue(float value)
        {
            fuelSlider.value = value;
            CheckActivateGlow(fuelSlider, redSliderGlow);
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
        public void ShowWaveSummaryWindow(bool show,in string title, in string text, Action onConfirmCallback, float moveTime = 1f, bool instantMove = false)
        {
            if (_movingSummaryWindow)
                return;
            
            float targetY;
            if (show)
            {
                targetY = -waveSummaryWindow.sizeDelta.y / 4f;
                
                confirmButton.onClick.RemoveAllListeners();
                confirmButton.onClick.AddListener(() =>
                {
                    ShowWaveSummaryWindow(false,string.Empty, string.Empty, null, instantMove:true);
                    onConfirmCallback?.Invoke();
                });
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
        }

        #endregion //Wave Summary Window
        


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
        


        private static void CheckActivateGlow(SliderText slider, Behaviour glowSlider)
        {
            CheckActivateGlow(slider.Slider, glowSlider);
        }

        private static void CheckActivateGlow(Slider slider, Behaviour glowSlider)
        {
            var value = slider.value / slider.maxValue;
            glowSlider.enabled = value <= Globals.GameUIResourceThreshold;
        }

        private static void CheckActivateGlowInverse(Slider slider, Behaviour glowSlider)
        {
            glowSlider.enabled = slider.value / slider.maxValue >= 0.75f;
        }

        //====================================================================================================================//

    }
}


