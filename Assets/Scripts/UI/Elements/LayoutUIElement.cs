using Sirenix.OdinInspector;
using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using StarSalvager.Utilities.JsonDataTypes;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StarSalvager.UI
{
    public class LayoutUIElement : ButtonReturnUIElement<ScrapyardLayout, ScrapyardLayout>
    {
        [SerializeField, Required]
        private TMP_Text loadListNameText;
        
        //============================================================================================================//
        
        public override void Init(ScrapyardLayout data, Action<ScrapyardLayout> onPressedCallback)
        {
            this.data = data;

            loadListNameText.text = data.Name;

            button.onClick.AddListener(() =>
            {
                onPressedCallback?.Invoke(data);
            });
        }
        
        //============================================================================================================//
    }
}