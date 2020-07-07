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
    public class SliderText
    {
        [SerializeField, Required]
        private TMP_Text sliderText;

        [SerializeField, Required]
        private Slider _slider;

        [SerializeField]
        private string format;

        private CameraController m_cameraController;

        public void Init(CameraController cameraController)
        {
            m_cameraController = cameraController;
            _slider.onValueChanged.AddListener(ValueChanged);
            sliderText.text = string.Format(format, 1f / _slider.value);
        }

        private void ValueChanged(float data)
        {
            sliderText.text = string.Format(format, 1f / data);

            m_cameraController.SetOrthographicSize(Values.Constants.gridCellSize * Values.Globals.ColumnsOnScreen * data, Vector3.zero, true);
        }
    }
}


