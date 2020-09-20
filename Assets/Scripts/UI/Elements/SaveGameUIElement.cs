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
        
        public void Init(SaveFileData data, Action<SaveFileData> onPressedCallback, Action<SaveFileData> OnDeletePressed, bool hideDateAndDelete = false)
        {
            Init(data, onPressedCallback);
            
            deleteButton.onClick.RemoveAllListeners();
            deleteButton.onClick.AddListener(() =>
            {
                OnDeletePressed?.Invoke(this.data);
            });

            nameText.text = data.Name;
            dateText.text = data.Date.ToString(SaveGameUI.DATETIME_FORMAT);

            if (hideDateAndDelete)
            {
                dateText.gameObject.SetActive(false);
                deleteButton.gameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// Use the Init which included the OnDeletePressed
        /// </summary>
        /// <param name="data"></param>
        /// <param name="onPressedCallback"></param>
        public override void Init(SaveFileData data, Action<SaveFileData> onPressedCallback)
        {
            this.data = data;
            
            button.onClick.RemoveAllListeners();
            
            button.onClick.AddListener(() =>
            {
                onPressedCallback?.Invoke(this.data);
            });
        }
    }
}


