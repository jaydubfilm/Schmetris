using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StarSalvager.Utilities.UI
{
    [Serializable]
    public class SliderText
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
        }

        private void ValueChanged(float data)
        {
            sliderText.text = string.Format(format, data);
        }
    }
}


