using Sirenix.OdinInspector;
using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StarSalvager.UI
{
    public class BotLoadListUIElement : ButtonReturnUIElement<EditorBotGeneratorData, EditorBotGeneratorData>
    {
        [SerializeField, Required]
        private TMP_Text loadListNameText;
        
        //============================================================================================================//
        
        public override void Init(EditorBotGeneratorData data, Action<EditorBotGeneratorData> OnPressed)
        {
            this.data = data;

            loadListNameText.text = data.Name;

            button.onClick.AddListener(() =>
            {
                OnPressed?.Invoke(data);
            });
        }
        
        //============================================================================================================//
    }
}