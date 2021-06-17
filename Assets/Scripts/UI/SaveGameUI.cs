using System;
using Sirenix.OdinInspector;
using StarSalvager.Audio;
using StarSalvager.Factories;
using StarSalvager.Utilities.FileIO;
using StarSalvager.Utilities.Saving;
using StarSalvager.Utilities.SceneManagement;
using StarSalvager.Values;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StarSalvager.UI.Wreckyard
{
    [System.Obsolete]
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

        [SerializeField]
        private bool IsLoadMode;
        //----------------------------------------------------------//

        //private string path;
        
        //============================================================================================================//
        
        // Start is called before the first frame update
        private void Start()
        {
            /*if (IsLoadMode)
                SaveButton.onClick.AddListener(LoadPressed);
            else
                SaveButton.onClick.AddListener(SavePressed);
            CancelButton.onClick.AddListener(CloseMenu);*/
        }

        private void OnEnable()
        {
            _selectedSaveFileData = null;
            
           // path = Application.dataPath + "/RemoteData/";

            if (IsLoadMode)
                nameInputField.gameObject.SetActive(false);
            else
                nameInputField.text = DateTime.Now.ToString(DATETIME_FORMAT);

            UpdateScrollView();
        }

        private void Update()
        {
            SaveButton.interactable = _selectedSaveFileData != null;
            nameInputField.interactable = _selectedSaveFileData != null;
        }

        //============================================================================================================//



        private void UpdateScrollView()
        {
            SaveGameContentScrollView.ClearElements();
            
            foreach (var saveFile in PlayerDataManager.GetSaveFiles())
            {
                var element = SaveGameContentScrollView.AddElement(saveFile, $"{saveFile.Name}_UIElement");
                element.Init(saveFile, SaveFilePressed, DeleteSaveFilePressed);
            }

            if (!IsLoadMode && (SaveGameContentScrollView.Elements == null || SaveGameContentScrollView.Elements.Count < 6))
            {
                SaveFileData emptyFile = new SaveFileData
                {
                    Name = "New File",
                };

                var emptyElement = SaveGameContentScrollView.AddElement(emptyFile, $"{emptyFile.Name}_UIElement");
                emptyElement.Init(emptyFile, SaveFilePressed, DeleteSaveFilePressed, true);
            }
        }
        
        //============================================================================================================//


        private void SaveFilePressed(SaveFileData data)
        {
            _selectedSaveFileData = data;
            if (data.Name == "New File")
                nameInputField.text = DateTime.Now.ToString(DATETIME_FORMAT);
            else
                nameInputField.text = data.Name;
        }

        private void DeleteSaveFilePressed(SaveFileData data)
        {
            Alert.ShowAlert("Delete Save File", $"Are you sure you want to Delete {data.Name}?", "Delete", "Cancel",
                answer =>
                {
                    //Decided to cancel action
                    if(!answer)
                        return;
                        
                    SaveGameContentScrollView.RemoveElement(data);
                    Files.TryDeleteFile(Files.GetPlayerAccountSavePath(PlayerDataManager.CurrentSaveSlotIndex));
                    PlayerDataManager.ClearSaveFileData(data);
                    //TODO Delete the file here

                    UpdateScrollView();
                });
        }

        private void LoadPressed()
        {
            /*if (_selectedSaveFileData.HasValue)
            {
                PlayerDataManager.SetCurrentSaveSlotIndex(_selectedSaveFileData.Value.SaveSlotIndex);

                CloseMenu();

                FactoryManager.Instance.currentModularDataIndex = 0;
                SceneLoader.ActivateScene(SceneLoader.SCRAPYARD, SceneLoader.MAIN_MENU, MUSIC.SCRAPYARD);
            }*/
        }

        private void SavePressed()
        {
            if (!_selectedSaveFileData.HasValue || _selectedSaveFileData.Value.Name != nameInputField.text || _selectedSaveFileData.Value.Name == "New File")
            {
                int saveSlotIndex = Files.GetNextAvailableSaveSlot();

                if (saveSlotIndex >= 0)
                {
                    SaveFileData newSaveFile = new SaveFileData
                    {
                        Name = nameInputField.text,
                        Date = DateTime.Now,
                        SaveSlotIndex = saveSlotIndex
                    };
                    print("CREATING FILE " + saveSlotIndex);

                    PlayerDataManager.AddSaveFileData(newSaveFile);
                    PlayerDataManager.SetCurrentSaveFile(saveSlotIndex);

                    PlayerDataManager.ExportPlayerAccountData(saveSlotIndex);
                    PlayerDataManager.SetCurrentSaveSlotIndex(saveSlotIndex);

                    _selectedSaveFileData = newSaveFile;

                    Alert.ShowAlert("Save Successful", "Game Saved. Click to Continue", "Continue", CloseMenu);
                }
                else
                {
                    print("NO EMPTY SLOTS");
                }

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
                        
                        SaveGameContentScrollView.RemoveElement(_selectedSaveFileData.Value);
                        int saveSlotIndex = _selectedSaveFileData.Value.SaveSlotIndex;

                        PlayerDataManager.RemoveSaveFileData(_selectedSaveFileData.Value);

                        SaveFileData newSaveFile = new SaveFileData
                        {
                            Name = nameInputField.text,
                            Date = DateTime.Now,
                            SaveSlotIndex = saveSlotIndex
                        };
                        print("OVERWRITING FILE " + saveSlotIndex);

                        PlayerDataManager.AddSaveFileData(newSaveFile);
                        PlayerDataManager.SetCurrentSaveFile(saveSlotIndex);

                        PlayerDataManager.ExportPlayerAccountData(saveSlotIndex);

                        _selectedSaveFileData = newSaveFile;

                        UpdateScrollView();

                        CloseMenu();
                    });
            }
            
        }

        private void CloseMenu()
        {
            _selectedSaveFileData = null;
            gameObject.SetActive(false);
        }
        
        //============================================================================================================//

    }
    
    [Serializable, Obsolete]
    public class SaveGameContentScrollView : UIElementContentScrollView<SaveGameUIElement, SaveFileData>
    {}
}

