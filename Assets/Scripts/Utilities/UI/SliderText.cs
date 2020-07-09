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

        [SerializeField, Required]
        private Slider _slider;

        [SerializeField]
        private string format;

        public void Init()
        {
            _slider.onValueChanged.AddListener(ValueChanged);
            sliderText.text = string.Format(format, _slider.value);
        }

        private void ValueChanged(float data)
        {
            sliderText.text = string.Format(format, data);
        }
        private void ValueChanged(int data)
        {
            sliderText.text = string.Format(format, data);
        }
    }
}


