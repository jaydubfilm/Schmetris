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

        [SerializeField, Required]
        private Slider _slider;

        [SerializeField]
        private string format;

        public float value
        {
            get => _slider.value;

            set => _slider.value = value;
        }

        public void Init()
        {
            //Ensure that we have a nice clean slate
            _slider.onValueChanged.RemoveAllListeners();
            
            _slider.onValueChanged.AddListener(ValueChanged);
            sliderText.text = FormattedSliderText(format, _slider.value);
        }

        public void SetBounds(float min, float max)
        {
            _slider.minValue = min;
            _slider.maxValue = max;
        }

        private void ValueChanged(float data)
        {
            sliderText.text = FormattedSliderText(format, data);
        }
        private void ValueChanged(int data)
        {
            sliderText.text = FormattedSliderText(format, data);
        }

        private static string FormattedSliderText(string format, float data)
        {
            return string.IsNullOrEmpty(format) ? $"{data}" : string.Format(format, data);
        }
    }
}


