using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Sirenix.OdinInspector;
using StarSalvager.Cameras;
using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using StarSalvager.UI.Hints;
using StarSalvager.Utilities;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.Helpers;
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
using Console = System.Console;
using Random = UnityEngine.Random;

namespace StarSalvager.UI
{
    public class GameUI : SceneSingleton<GameUI>, IHasHintElement, IBuildNavigationProfile
    {
        //Structs
        //====================================================================================================================//

        #region Structs

        [Serializable]
        public class SliderPartUI
        {
            [FormerlySerializedAs("backgroundImage")] [Required, FoldoutGroup("$NAME")] public Image buttonBackImage;
            [FormerlySerializedAs("foregroundImage")] [Required, FoldoutGroup("$NAME")] public Image buttonImage;
            [Required, FoldoutGroup("$NAME")] public Image partImage;
            [Required, FoldoutGroup("$NAME")] public Image leftDoorImage;
            [Required, FoldoutGroup("$NAME")] public Image rightDoorImage;
            [Required, FoldoutGroup("$NAME")] public Image secondPartImage;
            [Required, FoldoutGroup("$NAME")] public Image cooldownFillImage;
            [Required, FoldoutGroup("$NAME")] public Image cooldownBackgroundImage;

            [HideInInspector] public Image partBorderSprite;

            [Required, FoldoutGroup("$NAME")] public Image triggerInputImage;

            [Required, FoldoutGroup("$NAME")] public Slider slider;
            [Required, FoldoutGroup("$NAME")] public Image fillImage;

            private bool _isOpen;

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
                if (triggerInputImage is null || cooldownBackgroundImage is null)
                    return;

                //AnimateDoors(isTrigger);
                buttonImage.enabled = isTrigger;


                triggerInputImage.gameObject.SetActive(isTrigger && triggerSprite != null);
                triggerInputImage.sprite = triggerSprite;
                
            }

            public void AnimateDoors(in bool open, in float animationTime)
            {
                if (_isOpen == open)
                    return;
                
                Instance.StartCoroutine(TriggerCoroutine(open,animationTime));
                _isOpen = open;
            }

            private IEnumerator TriggerCoroutine(bool open, float animationTime)
            {
                if (open)
                {
                    yield return Instance.StartCoroutine(AnimateDoorsCoroutine(true, animationTime / 2));
                    yield return Instance.StartCoroutine(ScaleButtonCoroutine(false, animationTime / 2));
                    _isOpen = true;
                }
                else
                {
                    yield return Instance.StartCoroutine(ScaleButtonCoroutine(true, animationTime / 2));
                    yield return Instance.StartCoroutine(AnimateDoorsCoroutine(false, animationTime / 2));
                    _isOpen = false;
                }

            }
            /*private void AnimateDoors(bool opened)
            {
                if (leftDoorImage is null || rightDoorImage is null)
                    return;

                //leftDoorImage.transform.localPosition = new Vector3(-leftDoorImage.rectTransform.rect.width / 2f + doorOffset, 0, 0);
                //rightDoorImage.transform.localPosition = new Vector3(rightDoorImage.rectTransform.rect.width / 2f - doorOffset, 0, 0);
                //leftDoorImage.gameObject.SetActive(!opened);
                //rightDoorImage.gameObject.SetActive(!opened);

                Instance.StartCoroutine(DisplayTriggerButtonCoroutine(opened));
            }

            private IEnumerator DisplayTriggerButtonCoroutine(bool open)
            {
                if (buttonImage.enabled)
                {
                    yield return Instance.StartCoroutine(ScaleButtonCoroutine(true));
                    yield return Instance.StartCoroutine(AnimateDoorsCoroutine(false));
                }

                if (!open)
                    yield break;

                yield return new WaitForSeconds(0.2f);
                
                yield return Instance.StartCoroutine(AnimateDoorsCoroutine(true));
                yield return Instance.StartCoroutine(ScaleButtonCoroutine(false));
            }*/

            private IEnumerator AnimateDoorsCoroutine(bool open, float animationTime = 0.5f)
            {
                var leftDoorRectTransform = (RectTransform) leftDoorImage.transform;
                var rightDoorRectTransform = (RectTransform) rightDoorImage.transform;

                Vector3 leftStart = leftDoorRectTransform.localPosition;
                Vector3 rightStart = rightDoorRectTransform.localPosition;

                Vector3 leftEnd = Vector3.right *
                                  (open
                                      ? -leftDoorRectTransform.rect.width - doorOffset
                                      : -leftDoorRectTransform.rect.width / 2f + doorOffset);
                Vector3 rightEnd = Vector3.right *
                                   (open
                                       ? rightDoorImage.rectTransform.rect.width + doorOffset
                                       : rightDoorRectTransform.rect.width / 2f - doorOffset);
                
                for (float time = 0; time < animationTime; time += Time.deltaTime)
                {
                    var t = time / animationTime;
                    rightDoorImage.transform.localPosition = Vector3.Lerp(rightStart, rightEnd, t);
                    leftDoorImage.transform.localPosition = Vector3.Lerp(leftStart, leftEnd, t);
                    
                    yield return null;
                }
            }

            private IEnumerator ScaleButtonCoroutine(bool shrink, float animationTime = 0.5f)
            {
                var buttonRectTransform = (RectTransform)buttonImage.transform;
                var defaultScale = Vector3.one;
                var targetScale = shrink ? Vector3.one * 0.1f : defaultScale;
                var startScale = shrink ? defaultScale : Vector3.one * 0.1f;

                buttonRectTransform.localScale = startScale;
                
                for (var time = 0f; time <= animationTime; time += Time.deltaTime)
                {
                    var t = time / animationTime;
                    buttonRectTransform.localScale = Vector3.Lerp(startScale, targetScale, t);
                    yield return null;
                }

            }

            public void SetSprite(in Sprite partSprite)
            {
                if (partImage is null) return;

                partImage.sprite = partSprite;

                if (partBorderSprite != null)
                    partBorderSprite.enabled = partSprite != null;
            }

            public void SetSecondSprite(in Sprite partSprite)
            {
                //FIXME Need to determine when to actually start showing this stuff
                //if (secondPartImage is null) return;
                //
                //secondPartImage.gameObject.SetActive(partSprite != null);
                //secondPartImage.sprite = partSprite;
            }

            public void SetColor(in Color color)
            {
                if (buttonImage is null || secondPartImage is null)
                    return;
                buttonImage.color = color;
                secondPartImage.color = color;
            }

            public void SetBackgroundColor(in Color color)
            {
                
                if (buttonBackImage is null)
                    return;

                buttonBackImage.color = color;
            }

            public void SetFill(float val)
            {
                if (cooldownFillImage is null)
                    return;

                cooldownFillImage.fillAmount = val;
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
                ORANGE
            }

            [FoldoutGroup("$type")] public TYPE type;
            [FoldoutGroup("$type")] public Sprite backgroundImage;
            [FoldoutGroup("$type")] public Sprite crossbarImage;
            [FoldoutGroup("$type")] public Sprite verticalBarImage;

            [FoldoutGroup("$type")] public Color titleColor;
            [FoldoutGroup("$type")] public Color textColor;
        }

        #endregion //Structs

        //============================================================================================================//

        private const float MAGNET_FILL_VALUE = 0.02875f;

        private static int[] _gameUIBitIndices;
        private const float doorOffset = 5f;

        #region Properties

        [SerializeField] private RectTransform viewableAreaTransform;

        //Top Left Window
        //============================================================================================================//

        [FormerlySerializedAs("gearsText")] [SerializeField, Required, FoldoutGroup("TL Window")]
        private TMP_Text xpText;

        //[SerializeField, Required, FoldoutGroup("TL Window")]
        //private Slider gearsSlider;
        [FormerlySerializedAs("componentsText")]
        [FormerlySerializedAs("patchPointsText")]
        [SerializeField, Required, FoldoutGroup("TL Window"), Space(10f)]
        private TMP_Text gearsText;

        [SerializeField, Required, FoldoutGroup("TL Window"), Space(10f)]
        private TMP_Text silverText;

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
        private Slider botHealthBarSlider;

        [SerializeField, Required, FoldoutGroup("BR Window")]
        private Image botHealthBarSliderImage;

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
        private RectTransform summaryWindowFrame;

        [SerializeField, Required, FoldoutGroup("Summary Window")]
        private TMP_Text waveSummaryTitle;

        [SerializeField, Required, FoldoutGroup("Summary Window")]
        private TMP_Text waveSummaryText;

        [SerializeField, Required, FoldoutGroup("Summary Window")]
        private Button confirmButton;

        [SerializeField, Required, FoldoutGroup("Summary Window")]
        private TMP_Text confirmButtonText;

        [SerializeField, FoldoutGroup("Summary Window")]
        private WindowSpriteSet[] spriteSets;

        //Game Over Window
        //====================================================================================================================//

        [SerializeField, Required, FoldoutGroup("Game Over Window")]
        private RectTransform gameoverWindowFrame;

        [SerializeField, Required, FoldoutGroup("Game Over Window")]
        private TMP_Text gameOverTitle;

        [SerializeField, Required, FoldoutGroup("Game Over Window")]
        private Button gameoverButton;

        [SerializeField, Required, FoldoutGroup("Game Over Window")]
        private TMP_Text gameoverButtonText;

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

        [SerializeField, BoxGroup("Combo Effect"), MinMaxSlider(1, 10, true)]
        private Vector2Int effectElementCount;

        [SerializeField, BoxGroup("Combo Effect"), MinMaxSlider(0, 2, true)]
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
            ShowWaveSummaryWindow(false, false, string.Empty, string.Empty, null, instantMove: true);

            InitValues();
        }

        private void OnEnable()
        {
            if (!PlayerDataManager.HasRunData)
                return;

            Toast.SetToastArea(viewableAreaTransform);
            SetupPlayerValues();

            PlayerDataManager.OnCapacitiesChanged += SetupPlayerValues;
            PlayerDataManager.OnValuesChanged += ValuesUpdated;
            PlayerDataManager.OnItemUnlocked += UnlockItem;
        }

        private void OnDisable()
        {
            Toast.SetToastArea(transform as RectTransform);

            PlayerDataManager.OnCapacitiesChanged -= SetupPlayerValues;
            PlayerDataManager.OnValuesChanged -= ValuesUpdated;
            PlayerDataManager.OnItemUnlocked -= UnlockItem;
        }

        #endregion //Unity Functions

        //Hint UI
        //============================================================================================================//

        #region Hint UI

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
                case HINT.HEALTH:
                    return new object[]
                    {
                        botHealthBarSliderImage.transform as RectTransform,
                    };
                default:
                    throw new ArgumentOutOfRangeException(nameof(hint), hint, null);
            }
        }

        #endregion //Hint UI

        //Init UI
        //====================================================================================================================//

        #region Init UI

        private void InitValues()
        {
            _gameUIBitIndices = new int[5];
            var bitList = Constants.BIT_ORDER.ToList();
            for (var i = 1; i <= 5; i++)
            {
                var bitType = (BIT_TYPE) i;
                var index = bitList.FindIndex(x => x == bitType);
                _gameUIBitIndices[i - 1] = index;
            }

            SetupAmmoSliders();

            //InitSmartWeaponUI();
            ResetIcons();


            SetCarryCapacity(0f, 1);

            SetHealthValue(1f);
            SetLevelProgressSlider(0f);


            SetPlayerGears(0);
            SetPlayerSilver(0);
            SetPlayerXP(0);

            OutlineMagnet(false);

            SetDancersActive(false);
            FadeBackground(false, true);
        }

        private void SetupPlayerValues()
        {

            SetPlayerXP(PlayerDataManager.GetXPThisRun());
            SetPlayerGears(PlayerDataManager.GetGears());
            SetPlayerSilver(PlayerDataManager.GetSilver());

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
                //Do not need to set the colors when the sprites are already colored SS-312
                //SliderPartUis[i].fillImage.color = Constants.BIT_ORDER[i].GetColor();

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

        //Update UI
        //============================================================================================================//

        #region Update UI

        private void UnlockItem(PlayerLevelRemoteData.UnlockData unlockData)
        {
            switch (unlockData.Unlock)
            {
                case PlayerLevelRemoteData.UNLOCK_TYPE.PART:
                    Debug.Log($"Unlocked {unlockData.PartType}");
                    break;
                case PlayerLevelRemoteData.UNLOCK_TYPE.PATCH:
                    Debug.Log($"Unlocked {unlockData.PatchType} {Mathfx.ToRoman(unlockData.Level)}");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

        }

        private void ValuesUpdated()
        {
            SetPlayerXP(PlayerDataManager.GetXPThisRun());
            SetPlayerGears(PlayerDataManager.GetGears());
            SetPlayerSilver(PlayerDataManager.GetSilver());
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


            //botHealthBarSliderImage.color = Color.Lerp(Color.red, Color.green, value);
            //Since the green color is already on the sprite, we'll only fade between red and white SS-312
            botHealthBarSliderImage.color = Color.Lerp(Color.red, Color.white, value);
            botHealthBarSlider.value = value;

        }

        public void SetPlayerXP(in int xp)
        {
            xpText.text = $"{xp} {TMP_SpriteHelper.STARDUST_ICON}";
        }

        public void SetPlayerGears(in int gears)
        {
            gearsText.text = $"{TMP_SpriteHelper.GEAR_ICON} {gears}";
        }

        public void SetPlayerSilver(in int silver)
        {
            silverText.text = $"{TMP_SpriteHelper.SILVER_ICON} {silver}";
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
            FlashNeonBorder(Random.Range(flashTimeRange.x, flashTimeRange.y));
        }

        public void FlashNeonBorder(in float time)
        {
            if (_flashingBorder)
                return;

            _flashingBorder = true;

            StartCoroutine(NeonBorderFlashingCoroutine(time));
        }

        private IEnumerator NeonBorderFlashingCoroutine(float time)
        {
            float t = 0f;
            Color startBorderGlowColor = borderGlow.color;
            Color endGlowColor = Color.clear;

            Color startBorderColor = Color.white;
            Color darkBorderColor = new Color(0.5f, 0.5f, 0.5f);

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
            
            var sprite = partType.GetSprite();
            SliderPartUis[index].SetSprite(sprite);
            
            if (partType == PART_TYPE.EMPTY)
            {
                SliderPartUis[index].SetIsTrigger(false, null);
                //SliderPartUis[index].SetColor(Color.clear);
                return;
            }

            var partRemoteData = partType.GetRemoteData();

            var isTrigger = partRemoteData.isManual;

            SliderPartUis[index].SetIsTrigger(isTrigger, isTrigger ? GetInputSprite(index) : null);

            //If the part icon needs a border, be sure to add it!
            /*if (SliderPartUis[index].partBorderSprite == null && SliderPartUis[index].buttonImage != null)
                SliderPartUis[index].partBorderSprite = PartAttachableFactory.CreateUIPartBorder(
                    (RectTransform) SliderPartUis[index].buttonImage.transform,
                    partType);*/

            SliderPartUis[index].SetColor(Globals.UsePartColors ? partRemoteData.category.GetColor() : Color.white);
        }
        
        public void StartAnimation(int index, in bool openDoors, in float animationTime)
        {
            SliderPartUis[index].AnimateDoors(openDoors, animationTime);
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
            throw new NotImplementedException();
            // if (index < 0)
            //     return false;
            // return SliderPartUis[index].isFilled;
        }

        public void SetFill(in BIT_TYPE bitType, in float fillValue)
        {
            switch (bitType)
            {
                case BIT_TYPE.BLUE:
                case BIT_TYPE.GREEN:
                case BIT_TYPE.GREY:
                case BIT_TYPE.RED:
                case BIT_TYPE.YELLOW:
                    SetFill(_gameUIBitIndices[(int) bitType - 1], fillValue);
                    break;
                default:
                    return;
            }
        }

        private void SetFill(in int index, in float fillValue)
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
            in bool isGameOverScreen,
            in string title,
            in string text,
            Action onConfirmCallback,
            string buttonText = "Continue",
            float moveTime = 1f,
            bool instantMove = false)
        {

            //--------------------------------------------------------------------------------------------------------//

            void CloseWindow()
            {
                ShowWaveSummaryWindow(false, false, string.Empty, string.Empty, null, instantMove: true);
                onConfirmCallback?.Invoke();
            }

            //--------------------------------------------------------------------------------------------------------//

            if (_movingSummaryWindow)
                return;

            var type = isGameOverScreen ? WindowSpriteSet.TYPE.ORANGE : WindowSpriteSet.TYPE.DEFAULT;
            _showingGameOver = isGameOverScreen;

            InputManager.SwitchCurrentActionMap(show ? ACTION_MAP.MENU : ACTION_MAP.DEFAULT);

            summaryWindowFrame.gameObject.SetActive(!isGameOverScreen);
            gameoverWindowFrame.gameObject.SetActive(isGameOverScreen);


            float targetY;
            if (show)
            {
                targetY = -waveSummaryWindow.sizeDelta.y / 4f;

                if (isGameOverScreen)
                {
                    gameoverButtonText.text = buttonText;

                    gameoverButton.onClick.RemoveAllListeners();
                    gameoverButton.onClick.AddListener(CloseWindow);

                    var spriteSet = spriteSets.FirstOrDefault(ss => ss.type == type);

                    gameoverButtonText.color = spriteSet.titleColor;
                }
                else
                {
                    confirmButtonText.text = buttonText;

                    confirmButton.onClick.RemoveAllListeners();
                    confirmButton.onClick.AddListener(CloseWindow);

                    var spriteSet = spriteSets.FirstOrDefault(ss => ss.type == type);

                    waveSummaryTitle.color = spriteSet.titleColor;
                    waveSummaryText.color = spriteSet.textColor;
                }
            }
            else
            {
                targetY = waveSummaryWindow.sizeDelta.y * 1.5f;
            }

            if (isGameOverScreen)
            {
                gameOverTitle.text = title;
            }
            else
            {
                waveSummaryTitle.text = title;
                waveSummaryText.text = text;
            }


            if (instantMove)
            {
                var newPos = waveSummaryWindow.anchoredPosition;
                newPos.y = targetY;
                waveSummaryWindow.anchoredPosition = newPos;

                return;
            }

            StartCoroutine(PositionWaveSummaryWindow(waveSummaryWindow,
                targetY,
                moveTime,
                isGameOverScreen ? gameoverButton.gameObject : confirmButton.gameObject));

        }

        private IEnumerator PositionWaveSummaryWindow(RectTransform rectTransform, float targetYPos, float time,
            GameObject focusTarget)
        {
            confirmButton.interactable = false;
            gameoverButton.interactable = false;
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
            gameoverButton.interactable = true;

            EventSystem.current.SetSelectedGameObject(focusTarget);
            UISelectHandler.SetBuildTarget(this);
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

        public void CreatePatchPointEffect(int count)
        {
            throw new NotImplementedException();
            /*if (LevelManager.Instance is null || LevelManager.Instance.BotInLevel is null)
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
            }#1#*/
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

                var trans = (RectTransform) image.transform;
                trans.sizeDelta = Vector2.one * imageSize;
                trans.SetParent(effectArea, false);
                trans.localScale = Vector3.zero;
                trans.localPosition = startPosition;
                transforms[i] = trans;

                spawnPositions[i] = startPosition +
                                    new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized *
                                    (effectRadius * 10f);
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
                    transforms[i].localEulerAngles +=
                        Vector3.forward * (rotationSpeed * (rotateDirection[i] ? 1f : -1f) * deltaTime);
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
                    transforms[i].localScale =
                        Vector3.Lerp(Vector3.one, Vector3.zero, spawnCurve.Evaluate(t / moveTime));
                    transforms[i].localEulerAngles +=
                        Vector3.forward * (rotationSpeed * (rotateDirection[i] ? 1f : -1f) * deltaTime);
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

        public void CreateAmmoEffect(in BIT_TYPE bitType, in float amount, in Vector2 startPosition,
            [CallerMemberName] string calledMemberName = "")
        {
            CreateAmmoEffect(bitType,
                amount,
                startPosition,
                effectElementCount.x, effectElementCount.y,
                moveTimeRange,
                calledMemberName);
        }

        private void CreateAmmoEffect(in BIT_TYPE bitType, in float amount, in Vector2 startPosition, in int minCount,
            in int maxCount, in Vector2 moveTimeRange, string calledMemberName)
        {
            if (bitType == BIT_TYPE.WHITE)
                throw new ArgumentException(
                    $"Trying to {nameof(CreateAmmoEffect)} for {BIT_TYPE.WHITE}. Called from {calledMemberName}");

            const float RADIUS = 50;
            Sprite sprite;
            Transform targetTransform;

            try
            {
                sprite = bitEffectSprites[(int) bitType - 1];
                targetTransform = sliderTargets[(int) bitType - 1];
            }
            catch (IndexOutOfRangeException)
            {
                Debug.LogError(
                    $"{bitType}[{(int) bitType - 1}]\n{nameof(bitEffectSprites)}[{bitEffectSprites.Length}]");
                throw;
            }


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
                //Changed: https://trello.com/c/65Xj4DlA/1469-ammo-graphic-shouldnt-obscur-upgrade
                trans.localPosition = startPosition + Random.insideUnitCircle.normalized * radius;
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
                moveTimeRange,
                nameof(TestComboEffect));
        }
#endif

        #endregion //Ammo Effect

        //====================================================================================================================//

        public static void ClearEventSelected()
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        //====================================================================================================================//

        private bool _showingGameOver;

        public NavigationProfile BuildNavigationProfile()
        {
            var toSelect = _showingGameOver ? gameoverButton : confirmButton;
            
            return new NavigationProfile(toSelect, new[] {toSelect}, null, null);
        }
    }
}
