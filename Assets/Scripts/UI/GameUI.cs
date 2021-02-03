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
        [Serializable]
        public struct TriggerPartUI
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

#if UNITY_EDITOR
            [SerializeField, PropertyOrder(-100), FoldoutGroup("$NAME")]
            private string NAME;
#endif

            public void Reset()
            {
                SetFill(1f);
                //SetHasResource(true);
                SetActive(false);
                SetInteractable(buttonObject.interactable);
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
                
                //buttonObject.interactable = val >= 1f;
                //SetInteractable(val >= 1f);
            }

            public void SetInteractable(in bool interactable)
            {
                buttonObject.interactable = interactable;
            }
        }

        [Serializable]
        public struct TriggerPartIcon
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
        

        //Bottom Left Window
        //============================================================================================================//


        //Right Window
        //============================================================================================================//

        [SerializeField, Required, FoldoutGroup("Trigger Parts")]
        [FormerlySerializedAs("SmartWeaponIcons")] 
        private TriggerPartIcon[] triggerPartIcons;

         [SerializeField, Required, FoldoutGroup("Trigger Parts")]
         [FormerlySerializedAs("SmartWeaponsUI")]
        private TriggerPartUI[] triggerPartUI;
        



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

        [SerializeField]
        private RectTransform[] bitLevelContainerTransforms;
        private List<Image[]> _bitLevelImages;

        
        [SerializeField]
        private RectTransform[] partIconContainerTransforms;
        private List<Image[]> _partIconImages;

        //====================================================================================================================//

        private Image[] glowImages;
        private float _alpha;
        private float speed = 4f;

        //============================================================================================================//

        private void Start()
        {

            ShowWaveSummaryWindow(false, string.Empty, string.Empty, null, instantMove: true);
            
            InitValues();
        }

        private void OnEnable()
        {
            Toast.SetToastArea(viewableAreaTransform);
            SetupPlayerValues();

            PlayerDataManager.OnCapacitiesChanged += SetupPlayerValues;
            PlayerDataManager.OnValuesChanged += UpdatePlayerGearsLevel;
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

        public object[] GetHintElements(HINT hint)
        {
            switch (hint)
            {
                case HINT.NONE:
                    return null;
                case HINT.MAGNET:
                    return new object[]
                    {
                        magnetFlash.transform as RectTransform 
                    };
                default:
                    throw new ArgumentOutOfRangeException(nameof(hint), hint, null);
            }
        }

        //====================================================================================================================//
        
        private void InitValues()
        {
            SetupBitLevelImages();
            SetupPartIconImages();
            
            SetBitLevelImages(new Dictionary<BIT_TYPE, int>());
            
            InitSmartWeaponUI();
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

        private void InitSmartWeaponUI()
        {

            for (var i = 0; i < triggerPartUI.Length; i++)
            {
                int index = i;
                var temp = triggerPartUI[i];

                //temp.sprites = sprites;

                triggerPartUI[i] = temp;
                triggerPartUI[i].buttonObject.onClick.RemoveAllListeners();
                triggerPartUI[i].buttonObject.onClick.AddListener(() =>
                {
                    InputManager.Instance.TriggerSmartWeapon(index);
                });

            }
        }
        

        //============================================================================================================//
        
        readonly BIT_TYPE[] _bitTypes = {
            BIT_TYPE.RED,
            BIT_TYPE.YELLOW,
            BIT_TYPE.GREY,
            BIT_TYPE.BLUE,
            BIT_TYPE.GREEN
        };
        
        private void SetupPlayerValues()
        {
            ShowAbortWindow(false);

            SetPlayerXP(0);
            SetPlayerComponents(PlayerDataManager.GetComponents());
        }

        
        private void SetupBitLevelImages()
        {
            _bitLevelImages = new List<Image[]>();
            foreach (var bitLevelContainerTransform in bitLevelContainerTransforms)
            {
                var images = bitLevelContainerTransform.GetComponentsInChildren<Image>();
                
                _bitLevelImages.Add(images);
            }

            for (int i = 0; i < _bitTypes.Length; i++)
            {
                var bitType = _bitTypes[i];

                for (int ii = 0; ii < _bitLevelImages[i].Length; ii++)
                {
                    var level = 4 - ii;
                    var sprite = FactoryManager.Instance.BitProfileData.GetProfile(bitType).GetSprite(level);
                    var image = _bitLevelImages[i][ii];
                    
                    image.gameObject.name = sprite.name;
                    image.sprite = sprite;
                    image.enabled = false;
                }
            }
            
        }

        private void SetupPartIconImages()
        {
            _partIconImages = new List<Image[]>();
            foreach (var partIconContainerTransform in partIconContainerTransforms)
            {
                var images = partIconContainerTransform.GetComponentsInChildren<Image>();
                
                _partIconImages.Add(images);
            }
        }

        private struct PartIconState
        {
            public Sprite Sprite;
            public Color Color;
        }
        public void SetPartImages(in Dictionary<PART_TYPE, bool> partStates)
        {
            if (_partIconImages.IsNullOrEmpty())
                return;

            for (int i = 0; i < _partIconImages.Count; i++)
            {
                for (int ii = 0; ii < _partIconImages[i].Length; ii++)
                {
                    _partIconImages[i][ii].enabled = false;
                }
            }
            
            //TODO Get the parts on the bot, determine the colors they're using

            var partProfile = FactoryManager.Instance.PartsProfileData;
            var partRemoteData = FactoryManager.Instance.PartsRemoteData;
            /*var parts = PlayerDataManager
                .GetBlockDatas()
                .OfType<PartData>()
                .Select(x => (PART_TYPE) x.Type);*/

            var parts = partStates.Keys.ToList();
            
            var bitCategories = new Dictionary<BIT_TYPE, List<PartIconState>>();

            foreach (var partType in parts)
            {
                var partData = partRemoteData.GetRemoteData(partType);
                var sprite = partProfile.GetProfile(partType).Sprite;
                
                var types = partData.partGrade.Types;
                
                if(types.IsNullOrEmpty())
                    continue;

                if (types[0] == BIT_TYPE.NONE)
                    types = new List<BIT_TYPE>(_bitTypes);

                foreach (var bitType in types)
                {
                    if(!bitCategories.ContainsKey(bitType))
                        bitCategories.Add(bitType, new List<PartIconState>());
                    
                    bitCategories[bitType].Add(new PartIconState
                    {
                        Sprite = sprite,
                        Color = partStates[partType] ? Color.white : Color.gray
                    });
                }
            }
            
            for (int i = 0; i < _bitTypes.Length; i++)
            {
                var bitType = _bitTypes[i];

                if (!bitCategories.ContainsKey(bitType))
                    continue;

                var partIconStates = bitCategories[bitType];
                
                for (int ii = 0; ii < partIconStates.Count; ii++)
                {
                    var image = _partIconImages[i][ii];
                    image.enabled = true;
                    image.sprite = partIconStates[ii].Sprite;
                    image.color = partIconStates[ii].Color;
                }
            }
        }

        //====================================================================================================================//

        public void SetBitLevelImages(Dictionary<BIT_TYPE, int> bitLevels)
        {
            foreach (var bitLevel in bitLevels)
            {
                SetBitLevelImages(bitLevel.Key, bitLevel.Value);
            }
        }
        
        //Uses levels 0-4
        public void SetBitLevelImages(in BIT_TYPE type, in int level)
        {
            var bitType = type;
            var index = _bitTypes.ToList().FindIndex(x => x == bitType);

            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(type), type, null);

            var images = _bitLevelImages[index];

            for (var i = 0; i < images.Length; i++)
            {
                var active = (4 - i) <= level;
                
                //TODO Need to set the color depending on the level
                images[i].color = (4 - i) == level ? Color.white : Color.gray;
                images[i].enabled = active;

            }

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
            LevelManager.Instance.BotInLevel.TrySelfDestruct();
                
            //If the bot was able to be killed, hide this window
            if(LevelManager.Instance.BotInLevel.Destroyed)
                ShowAbortWindow(false);
        }

        //TODO I should look into the NotifyPropertyChanged for setting up this functionality
        private void UpdatePlayerGearsLevel()
        {
            //SetPlayerGearsProgress(PlayerDataManager.GetPatchPointProgress());

            //TODO Need to add the Patch Points connection here
            SetPlayerComponents(PlayerDataManager.GetComponents());
        }

        public void SetPlayerXP(int xp)
        {
            gearsText.text = $"{xp} XP";
        }

        public void SetPlayerComponents(int points)
        {
            patchPointsText.text = $"{points}";
        }


        public void SetLevelProgressSlider(float value)
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

            var triggerPartIcon = triggerPartIcons.FirstOrDefault(x => x.Type == partType);

            if (triggerPartIcon.UISprite != null)
            {
                var color = triggerPartIcon.Color;
                triggerPartUI[index].buttonImage.color = color;
                
                triggerPartUI[index].iconImage.color = color;
                triggerPartUI[index].iconImage.sprite = triggerPartIcon.UISprite;

                return;
            }
            
            triggerPartUI[index].buttonImage.color = Color.white;
            triggerPartUI[index].iconImage.sprite = null;
        }
        public void ShowIcon(int index, bool state)
        {
            if (index < 0) return;
            triggerPartUI[index].SetActive(state);
        }
        
        public void SetInteractable(int index, bool state)
        {
            if (index < 0) return;
            triggerPartUI[index].SetInteractable(state);
        }

        public void SetFill(int index, float fillValue)
        {
            if (index < 0) return;
            triggerPartUI[index].SetFill(fillValue);
        }

        public void ResetIcons()
        {
            for (var i = 0; i < triggerPartUI.Length; i++)
            {
                //FIXME Need to determine if we're still using the number text here
                /*SmartWeaponsUI[i].keyText.text = $"{i + 1}";*/
                triggerPartUI[i].Reset();
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


            botHealthBarImage.color = Color.Lerp(Color.red, Color.green, value);
            botHealthBarImage.fillAmount = value;

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
        


        //Patch point Effect
        //====================================================================================================================//

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

        public static void ClearEventSelected()
        {
            EventSystem.current.SetSelectedGameObject(null);
        }


        //====================================================================================================================//
        

    }
}


