using System;
using StarSalvager.Utilities.UI;
using TMPro;
using UnityEngine;

namespace StarSalvager.Prototype
{
    public class ToggleUIElementExample : ToggleUIElement<string>
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
    }
}

