using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using StarSalvager.Utilities.UI;
using StarSalvager.Values;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StarSalvager.UI
{
    public class GameUI : MonoBehaviour
    {
        /*[Serializable, Obsolete]
        public struct SmartWeapon
        {
            public Image image;
            public Image NoResourceIcon;

            public TMP_Text keyText;

            public void Reset()
            {
                SetFill(1f);
                SetHasResource(true);
                SetActive(false);
            }

            public void SetActive(bool state)
            {
                image.gameObject.SetActive(state);
            }

            public void SetHasResource(bool hasResource)
            {
                //Doesn't matter if the thing isn't showing
                if (!image.gameObject.activeInHierarchy)
                    return;

                //Prevent constantly setting the below values
                if (NoResourceIcon.gameObject.activeInHierarchy == !hasResource)
                    return;

                image.color = hasResource ? Color.white : Color.gray;
                NoResourceIcon.gameObject.SetActive(!hasResource);
            }

            public void SetFill(float fillValue)
            {
                image.fillAmount = fillValue;
            }
        }*/
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
            public Image fillImage;

            public Sprite[] sprites;
            
            /*[Required] 
            public Image noResourceIcon;*/
            /*[Required, FoldoutGroup("$NAME")] 
            public TMP_Text keyText;*/

#if UNITY_EDITOR
            private string NAME => buttonObject ? buttonObject.gameObject.name : "Null";
            /*[SerializeField, HorizontalGroup("$NAME/Row1")]
            private Color color;

            [Button, HorizontalGroup("$NAME/Row1")]
            private void SetColors()
            {
                var images = new List<Image>(buttonObject.GetComponentsInChildren<Image>())
                {
                    buttonObject.targetGraphic as Image
                };

                foreach (var image in images)
                {
                    image.color = color;
                }
                
            }*/
#endif

            public void Reset()
            {
                SetFill(1f);
                //SetHasResource(true);
                SetActive(false);
            }

            public void SetActive(bool state)
            {
                buttonObject.gameObject.SetActive(state);
            }
            
            /*//FIXME I'll need to prevent constant setting here
            public void SetHasResource(bool hasResource)
            {
                //Doesn't matter if the thing isn't showing
                if (!buttonObject.gameObject.activeInHierarchy)
                    return;

                /#1#/Prevent constantly setting the below values
                if (buttonObject.image.sprite == _sprites[2])
                    return;#1#

                //If the resource is full & we have resources, set the sprite to glow
                if (fillImage.fillAmount >= 1f && hasResource)
                    buttonImage.sprite = sprites[1];
                else
                {
                    buttonImage.sprite = hasResource ? sprites[1] : sprites[2];
                }
                    
                
                //fillImage.color = hasResource ? Color.white : Color.gray;
                //noResourceIcon.gameObject.SetActive(!hasResource);
            }*/

            public void SetFill(float fillValue)
            {
                fillImage.fillAmount = fillValue;
                
                buttonImage.sprite = fillValue >= 1f ? sprites[1] : sprites[2];
            }
        }
        //============================================================================================================//

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

        [SerializeField, Required, FoldoutGroup("Slider Glows")]
        private Image heatSliderGlow;

        //Top Left Window
        //============================================================================================================//

        [SerializeField, Required, FoldoutGroup("TL Window")]
        private TMP_Text sectorText;

        [SerializeField, Required, FoldoutGroup("TL Window")]
        private TMP_Text timeText;

        [SerializeField, Required, FoldoutGroup("TL Window")]
        private Image clockImage;

        //Bottom Left Window
        //============================================================================================================//

        [SerializeField, Required, FoldoutGroup("BL Window")]
        private TMP_Text levelText;
        [SerializeField, Required, FoldoutGroup("BL Window")]
        private TMP_Text gearsText;
        [SerializeField, Required, FoldoutGroup("BL Window")]
        private Slider gearsSlider;
        
        [SerializeField, Required, FoldoutGroup("BL Window")]
        private SliderText fuelSlider;

        [SerializeField, Required, FoldoutGroup("BL Window")]
        private SliderText repairSlider;

        [SerializeField, Required, FoldoutGroup("BL Window")]
        private SliderText ammoSlider;

        [SerializeField, Required, FoldoutGroup("BL Window"), Space(10f)]
        private Slider heatSlider;

        [SerializeField, Required, FoldoutGroup("BL Window")]
        private Slider carryCapacitySlider;

        //Right Window
        //============================================================================================================//
        
        [SerializeField, Required, FoldoutGroup("R Window")]
        private Sprite normalSprite;
        [SerializeField, Required, FoldoutGroup("R Window")]
        private Sprite readySprite;
        [SerializeField, Required, FoldoutGroup("R Window")]
        private Sprite disabledSprite;
       
        [SerializeField, Required, FoldoutGroup("R Window")]
        //private SmartWeapon[] SmartWeaponsUI;
        private SmartWeaponV2[] SmartWeaponsUI;



        //Bottom Right Window
        //============================================================================================================//

        [SerializeField, Required, FoldoutGroup("BR Window")]
        private Slider waterSlider;

        [SerializeField, Required, FoldoutGroup("BR Window")]
        private Slider powerSlider;

        [SerializeField, Required, FoldoutGroup("BR Window"), Space(10f)]
        private Image bombImageIcon;

        [SerializeField, Required, FoldoutGroup("BR Window")]
        private Image bombNoResourceIcon;

        //Heat Vignette
        //============================================================================================================//

        [SerializeField, Required, ToggleGroup("useVignette")]
        private bool useVignette;

        [SerializeField, Required, ToggleGroup("useVignette")]
        private Image vignetteImage;

        [SerializeField, Required, ToggleGroup("useVignette")]
        private Color vignetteMinColor;

        [SerializeField, Required, ToggleGroup("useVignette")]
        private Color vignetteMaxColor;

        #endregion //Properties
        
        private Image[] glowImages;
        private float _alpha;
        private float speed = 4f;

        //============================================================================================================//

        private void Start()
        {
            InitSliderText();

            vignetteImage.gameObject.SetActive(useVignette);

            
            InitValues();

            

            glowImages = new[]
            {
                redSliderGlow,
                blueSliderGlow,
                greenSliderGlow,
                greenSliderGlow,
                yellowSliderGlow,
                heatSliderGlow
            };
        }

        private void OnEnable()
        {
            SetupPlayerValues();

            PlayerData.OnCapacitiesChanged += SetupPlayerValues;
            PlayerData.OnValuesChanged += UpdatePlayerGearsLevel;
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
            PlayerData.OnCapacitiesChanged -= SetupPlayerValues;
            PlayerData.OnValuesChanged -= UpdatePlayerGearsLevel;
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
            SetCarryCapacity(0f);

            SetFuelValue(0f);
            SetRepairValue(0f);
            SetAmmoValue(0f);

            SetClockValue(1f);
            SetTimeString("0:00");
            
            SetPlayerGearsLevel(0,0, 0);
        }

        private void InitSliderText()
        {
            fuelSlider.Init();
            repairSlider.Init();
            ammoSlider.Init();

        }

        private void InitSmartWeaponUI()
        {
            var sprites = new []
            {
                normalSprite,
                readySprite, 
                disabledSprite
            };

            for (var i = 0; i < SmartWeaponsUI.Length; i++)
            {
                var temp = SmartWeaponsUI[i];

                temp.sprites = sprites;

                SmartWeaponsUI[i] = temp;
            }
        }

        //============================================================================================================//

        private void SetupPlayerValues()
        {
            var playerData = PlayerPersistentData.PlayerData;

            if (playerData == null)
                return;

            SetResourceSliderBounds(BIT_TYPE.RED, 0, playerData.liquidCapacity[BIT_TYPE.RED]);
            SetResourceSliderBounds(BIT_TYPE.GREEN, 0, playerData.liquidCapacity[BIT_TYPE.GREEN]);
            SetResourceSliderBounds(BIT_TYPE.GREY, 0, playerData.liquidCapacity[BIT_TYPE.GREY]);

            SetResourceSliderBounds(BIT_TYPE.BLUE, 0, playerData.ResourceCapacities[BIT_TYPE.BLUE]);
            SetResourceSliderBounds(BIT_TYPE.YELLOW, 0, playerData.liquidCapacity[BIT_TYPE.YELLOW]);

            SetFuelValue(playerData.liquidResource[BIT_TYPE.RED]);
            SetRepairValue(playerData.liquidResource[BIT_TYPE.GREEN]);
            SetAmmoValue(playerData.liquidResource[BIT_TYPE.GREY]);

            SetPlayerGearsLevel(playerData.Level, playerData.Gears, 999);
        }

        //============================================================================================================//


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

        public void SetCarryCapacity(float value)
        {
            carryCapacitySlider.value = value;
        }

        //============================================================================================================//

        //TODO I should look into the NotifyPropertyChanged for setting up this functionality
        private void UpdatePlayerGearsLevel()
        {
            var playerData = PlayerPersistentData.PlayerData;
            
            var gearsRequired = LevelManager.Instance.PlayerlevelRemoteDataScriptableObject
                .GetRemoteData(playerData.Level).GearsToLevelUp;

            SetPlayerGearsLevel(playerData.Level, playerData.Gears, gearsRequired);
        }

        public void SetPlayerGearsLevel(int playerLevel, int gears, int gearsRequired)
        {
            gearsSlider.minValue = 0;
            gearsSlider.maxValue = gearsRequired;
            gearsSlider.value = gears;
            
            levelText.text = $"lvl {playerLevel}";
            gearsText.text = $"{gears} / {gearsRequired}";
        }

        /*public void SetAllResourceSliderBounds(int min, int max)
        {
            fuelSlider.SetBounds(min, max);
            repairSlider.SetBounds(min, max);
            ammoSlider.SetBounds(min, max);
        }*/

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
                    powerSlider.minValue = min;
                    powerSlider.maxValue = max;
                    break;
                case BIT_TYPE.BLUE:
                    waterSlider.minValue = min;
                    waterSlider.maxValue = max;
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

        //============================================================================================================//


        public void SetClockValue(float value)
        {
            clockImage.fillAmount = value;
        }

        public void SetTimeString(int seconds)
        {
            TimeSpan time = TimeSpan.FromSeconds(seconds);

            //here backslash is must to tell that colon is
            //not the part of format, it just a character that we want in output
            SetTimeString(time.ToString(@"m\:ss"));
        }

        public void SetTimeString(string time)
        {
            timeText.text = time;
        }



        //============================================================================================================//

        /*public void ShowBombIcon(bool state)
        {
            bombImageIcon.gameObject.SetActive(state);
        }

        public void SetHasBombResource(bool hasAmmo)
        {
            //Doesn't matter if the thing isn't showing
            if (!bombImageIcon.gameObject.activeInHierarchy)
                return;

            //Prevent constantly setting the below values
            if (bombNoResourceIcon.gameObject.activeInHierarchy == !hasAmmo)
                return;
            
            bombImageIcon.color = hasAmmo ? Color.white : Color.gray;
            bombNoResourceIcon.gameObject.SetActive(!hasAmmo);
        }
        
        public void SetBombFill(float fillValue)
        {
            bombImageIcon.fillAmount = fillValue;
        }*/
        public void SetIconImage(int index, Sprite sprite)
        {
            if (index < 0) return;
            SmartWeaponsUI[index].iconImage.sprite = sprite;
        }

        public void ShowIcon(int index, bool state)
        {
            if (index < 0) return;
            SmartWeaponsUI[index].SetActive(state);
        }

        //FIXME Need to determine what's happening with this
        /*[Obsolete("Currently not using the resource indicator for the Smart Weapons")]
        public void SetHasResource(int index, bool hasResource)
        {
            if (index < 0) return;
            SmartWeaponsUI[index].SetHasResource(hasResource);
        }*/

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
            sectorText.text = $"Sector {sector} Wave {wave}";

            //m_currentWaveText.text = "Sector " + (Values.Globals.CurrentSector + 1) + " Wave " + endString;
        }

        public void SetCurrentWaveText(string text)
        {
            sectorText.text = text;
        }

        //============================================================================================================//


        /// <summary>
        /// Value sent should be normalized
        /// </summary>
        /// <param name="value"></param>
        public void SetHeatSliderValue(float value)
        {
            //HeatSlider.value = value;
            //heatSliderImage.color = Color.Lerp(minColor, maxColor, value);

            heatSlider.value = value;

            CheckActivateGlowInverse(heatSlider, heatSliderGlow);

            if (useVignette)
                vignetteImage.color = Color.Lerp(vignetteMinColor, vignetteMaxColor, value);
        }

        //============================================================================================================//


        private static void CheckActivateGlow(SliderText slider, Behaviour glowSlider)
        {
            CheckActivateGlow(slider.Slider, glowSlider);
        }

        private static void CheckActivateGlow(Slider slider, Behaviour glowSlider)
        {
            var value = slider.value / slider.maxValue;
            glowSlider.enabled = value <= 0.25f;
        }

        private static void CheckActivateGlowInverse(Slider slider, Behaviour glowSlider)
        {
            glowSlider.enabled = slider.value / slider.maxValue >= 0.75f;
        }

        //====================================================================================================================//

    }
}


