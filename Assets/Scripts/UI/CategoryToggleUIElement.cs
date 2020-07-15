using System;
using StarSalvager.Utilities.UI;
using TMPro;
using UnityEngine;

namespace StarSalvager.UI
{
    public class CategoryToggleUIElement : ToggleUIElement<string>
    {
        [SerializeField]
        private TMP_Text toggleTitle;

        public override void Init(string data, Action<string, bool> OnToggleChanged)
        {
            this.data = data;

            toggleTitle.text = data;
            
            Toggle.onValueChanged.AddListener(value =>
            {
                OnToggleChanged?.Invoke(data, value);
            });
            
        }

        public void SetToggle(bool state)
        {
            Toggle.isOn = state;
        }

        public bool GetToggleValue()
        {
            return Toggle.isOn;
        }
    }
}

