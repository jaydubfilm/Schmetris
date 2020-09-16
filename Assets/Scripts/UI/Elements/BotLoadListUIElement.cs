using Sirenix.OdinInspector;
using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StarSalvager.UI
{
    public class BotLoadListUIElement : ButtonReturnUIElement<EditorGeneratorDataBase, EditorGeneratorDataBase>
    {
        [SerializeField, Required]
        private TMP_Text loadListNameText;
        
        //============================================================================================================//
        
        public override void Init(EditorGeneratorDataBase data, Action<EditorGeneratorDataBase> onPressedCallback)
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