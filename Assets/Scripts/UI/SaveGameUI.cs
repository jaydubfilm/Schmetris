using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using StarSalvager.Utilities.Saving;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StarSalvager.UI.Scrapyard
{
    public class SaveGameUI : MonoBehaviour
    {
        public static readonly string DATETIME_FORMAT = "ddd, dd MMMM yyyy, hh:mm tt";
        
        //============================================================================================================//
        
        [SerializeField]
        private SaveGameContentScrollView SaveGameContentScrollView;
        
        [SerializeField, Required]
        private TMP_InputField nameInputField;

        [SerializeField, Required]
        private Button SaveButton;
        [SerializeField, Required]
        private Button CancelButton;

        private SaveFileData? _selectedSaveFileData;
        
        //============================================================================================================//
        
        // Start is called before the first frame update
        private void Start()
        {
            SaveButton.onClick.AddListener(SavePressed);
            CancelButton.onClick.AddListener(CancelPressed);
        }

        private void OnEnable()
        {
            _selectedSaveFileData = null;
            
            nameInputField.text = DateTime.Now.ToString(DATETIME_FORMAT);

            UpdateScrollView();
        }

        //============================================================================================================//

        private void UpdateScrollView()
        {
            //TODO Get all the save files here
            //----------------------------------------------------------//
            var TEMP_FILES = new List<SaveFileData>
            {
                new SaveFileData
                {
                    Name = "Test Save 1",
                    Date = DateTime.Now,
                    FilePath = Application.dataPath
                },
                new SaveFileData
                {
                    Name = "Test Save 2",
                    Date = DateTime.Now.AddDays(-1),
                    FilePath = Application.dataPath
                },
                new SaveFileData
                {
                    Name = "Test Save 3",
                    Date = DateTime.Now.AddDays(-2),
                    FilePath = Application.dataPath
                }
            };
            //----------------------------------------------------------//

            foreach (var saveFile in TEMP_FILES)
            {
                var element = SaveGameContentScrollView.AddElement<SaveGameUIElement>(saveFile, $"{saveFile.Name}_UIElement");
                element.Init(saveFile, SaveFilePressed, DeleteSaveFilePressed);
            }
            
        }
        
        //============================================================================================================//


        private void SaveFilePressed(SaveFileData data)
        {
            _selectedSaveFileData = data;
            nameInputField.text = data.Name;
        }

        private void DeleteSaveFilePressed(SaveFileData data)
        {
            Alert.ShowAlert("Delete Save File", $"Are you sure you want to Delete {_selectedSaveFileData.Value.Name}?", "Delete", "Cancel",
                answer =>
                {
                    //Decided to cancel action
                    if(!answer)
                        return;
                        
                    SaveGameContentScrollView.RemoveElement<SaveGameUIElement>(data);
                    //TODO Delete the file here

                    UpdateScrollView();
                });
        }

        private void SavePressed()
        {

            if (!_selectedSaveFileData.HasValue)
            {
                //TODO Write a new save file

                UpdateScrollView();
            }
            else
            {
                Alert.ShowAlert("Overwriting", $"Are you sure you want to overwrite {_selectedSaveFileData.Value.Name}?", "Overwrite", "Cancel",
                    answer =>
                    {
                        //Decided to cancel action
                        if(!answer)
                            return;
                        
                        SaveGameContentScrollView.RemoveElement<SaveGameUIElement>(_selectedSaveFileData.Value);
                        //TODO Delete the old file here
                        //TODO Need to save new file here

                        UpdateScrollView();
                    });
            }
            
        }

        private void CancelPressed()
        {
            //TODO Need to close the window here
        }
        
        //============================================================================================================//

    }
    
    [Serializable]
    public class SaveGameContentScrollView : UIElementContentScrollView<SaveFileData>
    {}
}

