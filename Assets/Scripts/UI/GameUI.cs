using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace StarSalvager.UI
{
    public class GameUI : MonoBehaviour
    {
        //============================================================================================================//
        
        [SerializeField, Required, BoxGroup("Heat Slider")]
        private Slider HeatSlider;
        [SerializeField, Required, BoxGroup("Heat Slider")]
        private Image heatSliderImage;
        [SerializeField, Required, BoxGroup("Heat Slider")]
        private Color minColor;
        [SerializeField, Required, BoxGroup("Heat Slider")]
        private Color maxColor;

        [SerializeField, Required, ToggleGroup("Heat Slider/useVignette")]
        private bool useVignette;
        [SerializeField, Required, ToggleGroup("Heat Slider/useVignette")]
        private Image vignetteImage;
        [SerializeField, Required, ToggleGroup("Heat Slider/useVignette")]
        private Color vignetteMinColor;
        [SerializeField, Required, ToggleGroup("Heat Slider/useVignette")]
        private Color vignetteMaxColor;

        //============================================================================================================//
        
        private void Start()
        {
            vignetteImage.gameObject.SetActive(useVignette);
            SetHeatSliderValue(0f);
        }
        
        //============================================================================================================//
        
        /// <summary>
        /// Value sent should be normalized
        /// </summary>
        /// <param name="value"></param>
        public void SetHeatSliderValue(float value)
        {
            HeatSlider.value = value;
            heatSliderImage.color = Color.Lerp(minColor, maxColor, value);

            if(useVignette)
                vignetteImage.color = Color.Lerp(vignetteMinColor, vignetteMaxColor, value);
        }
        
        //============================================================================================================//
        
    }
}


