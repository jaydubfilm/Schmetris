using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using StarSalvager.Cameras;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StarSalvager.Utilities.UI
{
    [Serializable]
    public sealed class SliderText
    {
        [SerializeField, Required]
        private TMP_Text sliderText;
        public TMP_Text Text => sliderText;

        public Slider Slider => _slider;
        [SerializeField, Required]
        private Slider _slider;

        [SerializeField]
        private string format;

        public float value
        {
            get => _slider.value;

            set
            {
                _slider.value = value;
                
                if (_ignoreChanges)
                    return;
                sliderText.text = FormattedSliderText(format, value);
            }
        }

        
        private bool _showMax;
        private bool _hasFormat;
        private bool _ignoreChanges;

        public void Init(bool showMaxValue = false, bool ignoreChanges = false)
        {
            _hasFormat = !string.IsNullOrEmpty(format);
            _showMax = showMaxValue;
            _ignoreChanges = ignoreChanges;
            /*//Ensure that we have a nice clean slate
            _slider.onValueChanged.RemoveAllListeners();
            
            if(!ignoreChanges)
                _slider.onValueChanged.AddListener(ValueChanged);*/


        }

        public void SetBounds(float min, float max)
        {
            _slider.minValue = min;
            _slider.maxValue = max;
        }

        /*private void ValueChanged(float data)
        {
            sliderText.text = FormattedSliderText(format, data);
        }
        private void ValueChanged(int data)
        {
            sliderText.text = FormattedSliderText(format, data);
        }*/

        private string FormattedSliderText(string format, float data)
        {
            if (_hasFormat)
                return string.Format(format, data, Slider.maxValue);

            if (_showMax)
                return $"{data} / {Slider.maxValue}";

            return $"{data}";
            
            
            //return string.IsNullOrEmpty(format) ? $"{data}{(_showMax ? "/" + Slider.maxValue :"")}" : ;
        }
    }
}


