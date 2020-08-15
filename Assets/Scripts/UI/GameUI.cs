using System;
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
        //============================================================================================================//

        #region Properties
        
        //[SerializeField, Required, BoxGroup("Heat Slider")]
        //private Slider HeatSlider;
        //[SerializeField, Required, BoxGroup("Heat Slider")]
        //private Image heatSliderImage;
        //[SerializeField, Required, BoxGroup("Heat Slider")]
        //private Color minColor;
        //[SerializeField, Required, BoxGroup("Heat Slider")]
        //private Color maxColor;

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
        private SliderText fuelSlider;
        [SerializeField, Required, FoldoutGroup("BL Window")]
        private SliderText repairSlider;
        [SerializeField, Required, FoldoutGroup("BL Window")]
        private SliderText ammoSlider;
        
        [SerializeField, Required, FoldoutGroup("BL Window"), Space(10f)]
        private Slider heatSlider;
        [SerializeField, Required, FoldoutGroup("BL Window")]
        private Slider carryCapacitySlider;

        //Bottom Right Window
        //============================================================================================================//
        
        [SerializeField, Required, FoldoutGroup("BR Window")]
        private Slider waterSlider;
        [SerializeField, Required, FoldoutGroup("BR Window")]
        private Slider powerSlider;
        
        [SerializeField, Required, FoldoutGroup("BR Window")]
        private Image bombImageIcon;
        
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
        
        //============================================================================================================//
        
        private void Start()
        {
            InitSliderText();
            
            vignetteImage.gameObject.SetActive(useVignette);
            
            InitValues();
        }

        private void OnEnable()
        {
            SetupPlayerValues();
        }

        //============================================================================================================//

        private void InitValues()
        {
            ShowBombIcon(false);
            SetBombFill(1f);
            
            SetWaterValue(0f);
            SetPowerValue(0f);
            
            SetHeatSliderValue(0f);
            SetCarryCapacity(0f);
            
            SetFuelValue(0f);
            SetRepairValue(0f);
            SetAmmoValue(0f);
            
            SetClockValue(1f);
            SetTimeString("0:00");
        }

        private void InitSliderText()
        {
            fuelSlider.Init();
            repairSlider.Init();
            ammoSlider.Init();

            //FIXME This should be set using a capacity value instead of hard set here
            SetResourceSliderBounds(0, 250);
        }
        
        //============================================================================================================//

        private void SetupPlayerValues()
        {
            var playerData = PlayerPersistentData.PlayerData;

            if (playerData == null)
                return;
            
            SetFuelValue(playerData.liquidResource[BIT_TYPE.RED]);
            SetRepairValue(playerData.liquidResource[BIT_TYPE.GREEN]);
            SetAmmoValue(playerData.liquidResource[BIT_TYPE.GREY]);
        }
        
        //============================================================================================================//
        

        public void SetWaterValue(float value)
        {
            waterSlider.value = value;
        }
        public void SetPowerValue(float value)
        {
            powerSlider.value = value;
        }

        public void SetCarryCapacity(float value)
        {
            carryCapacitySlider.value = value;
        }
        
        //============================================================================================================//
        
        public void SetResourceSliderBounds(int min, int max)
        {
            fuelSlider.SetBounds(min, max);
            repairSlider.SetBounds(min, max);
            ammoSlider.SetBounds(min, max);
        }

        public void SetFuelValue(float value)
        {
            fuelSlider.value = value;
        }
        public void SetRepairValue(float value)
        {
            repairSlider.value = value;
        }
        public void SetAmmoValue(float value)
        {
            ammoSlider.value = value;
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

        public void ShowBombIcon(bool state)
        {
            bombImageIcon.gameObject.SetActive(state);
        }
        
        public void SetBombFill(float fillValue)
        {
            bombImageIcon.fillAmount = fillValue;
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

            if(useVignette)
                vignetteImage.color = Color.Lerp(vignetteMinColor, vignetteMaxColor, value);
        }
        
        //============================================================================================================//
        
    }
}


