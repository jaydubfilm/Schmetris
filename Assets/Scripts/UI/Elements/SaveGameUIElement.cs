using System;
using Sirenix.OdinInspector;
using StarSalvager.Utilities.Saving;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StarSalvager.UI.Scrapyard
{
    public class SaveGameUIElement : ButtonReturnUIElement<SaveFileData, SaveFileData>
    {
        [SerializeField, Required]
        private Button deleteButton;
        [SerializeField, Required]
        private TMP_Text nameText;
        [SerializeField, Required]
        private TMP_Text dateText;
        
        public void Init(SaveFileData data, Action<SaveFileData> OnPressed, Action<SaveFileData> OnDeletePressed)
        {
            Init(data, OnPressed);
            
            deleteButton.onClick.RemoveAllListeners();
            deleteButton.onClick.AddListener(() =>
            {
                OnDeletePressed?.Invoke(this.data);
            });

            nameText.text = data.Name;
            dateText.text = data.Date.ToString(SaveGameUI.DATETIME_FORMAT);

        }
        
        /// <summary>
        /// Use the Init which included the OnDeletePressed
        /// </summary>
        /// <param name="data"></param>
        /// <param name="OnPressed"></param>
        public override void Init(SaveFileData data, Action<SaveFileData> OnPressed)
        {
            this.data = data;
            
            button.onClick.RemoveAllListeners();
            
            button.onClick.AddListener(() =>
            {
                OnPressed?.Invoke(this.data);
            });
        }
    }
}


