using System;
using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector;
using StarSalvager.Factories;
using StarSalvager.Missions;
using StarSalvager.Utilities.Saving;
using StarSalvager.Utilities.SceneManagement;
using StarSalvager.Values;
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

        [SerializeField]
        private bool IsLoadMode;
        //----------------------------------------------------------//

        private string path;
        
        //============================================================================================================//
        
        // Start is called before the first frame update
        private void Start()
        {
            if (IsLoadMode)
                SaveButton.onClick.AddListener(LoadPressed);
            else
                SaveButton.onClick.AddListener(SavePressed);
            CancelButton.onClick.AddListener(CancelPressed);
        }

        private void OnEnable()
        {
            _selectedSaveFileData = null;
            
            path = Application.dataPath + "/RemoteData/";

            if (PlayerPersistentData.PlayerMetadata.CurrentSaveFile != null)
            {
                nameInputField.text = PlayerPersistentData.PlayerMetadata.CurrentSaveFile.Value.Name;
                _selectedSaveFileData = PlayerPersistentData.PlayerMetadata.CurrentSaveFile;
            }
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
            foreach (var saveFile in PlayerPersistentData.PlayerMetadata.SaveFiles)
            {
                var element = SaveGameContentScrollView.AddElement<SaveGameUIElement>(saveFile, $"{saveFile.Name}_UIElement");
                element.Init(saveFile, SaveFilePressed, DeleteSaveFilePressed);
            }

            SaveFileData emptyFile = new SaveFileData
            {
                Name = "New File",
                //Date = DateTime.Now,
                //FilePath = PlayerPersistentData.GetNextAvailableSaveSlot(),
                //MissionFilePath = PlayerPersistentData.GetNextAvailableSaveSlot()
            };

            var emptyElement = SaveGameContentScrollView.AddElement<SaveGameUIElement>(emptyFile, $"{emptyFile.Name}_UIElement");
            emptyElement.Init(emptyFile, SaveFilePressed, DeleteSaveFilePressed);
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
                    File.Delete(data.FilePath);
                    File.Delete(data.MissionFilePath);
                    PlayerPersistentData.PlayerMetadata.SaveFiles.Remove(data);
                    //TODO Delete the file here

                    UpdateScrollView();
                });
        }

        private void LoadPressed()
        {
            if (_selectedSaveFileData.HasValue)
            {
                PlayerPersistentData.SetCurrentSaveFile(_selectedSaveFileData.Value.FilePath);
                MissionManager.SetCurrentSaveFile(_selectedSaveFileData.Value.MissionFilePath);

                CancelPressed();

                FactoryManager.Instance.currentModularDataIndex = PlayerPersistentData.PlayerData.currentModularSectorIndex;
                SceneLoader.ActivateScene("UniverseMapScene", "MainMenuScene");
            }
        }

        private void SavePressed()
        {
            if (!_selectedSaveFileData.HasValue || _selectedSaveFileData.Value.Name != nameInputField.text || _selectedSaveFileData.Value.Name == "New File")
            {
                string playerPath = PlayerPersistentData.GetNextAvailableSaveSlot();
                string missionPath = MissionManager.GetNextAvailableSaveSlot();

                if (playerPath != string.Empty && missionPath != string.Empty)
                {
                    SaveFileData newSaveFile = new SaveFileData
                    {
                        Name = nameInputField.text,
                        Date = DateTime.Now,
                        FilePath = playerPath,
                        MissionFilePath = missionPath
                    };
                    print("CREATING FILE " + playerPath);

                    PlayerPersistentData.PlayerMetadata.SaveFiles.Add(newSaveFile);
                    PlayerPersistentData.PlayerMetadata.CurrentSaveFile = newSaveFile;

                    PlayerPersistentData.ExportPlayerPersistentData(PlayerPersistentData.PlayerData, playerPath);
                    MissionManager.ExportMissionsCurrentRemoteData(MissionManager.MissionsCurrentData, missionPath);

                    PlayerPersistentData.SetCurrentSaveFile(playerPath);
                    MissionManager.SetCurrentSaveFile(missionPath);

                    _selectedSaveFileData = newSaveFile;

                    Alert.ShowAlert("Save Successful", "Game Saved. Click to Continue", "Continue", CancelPressed);
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
                        
                        SaveGameContentScrollView.RemoveElement<SaveGameUIElement>(_selectedSaveFileData.Value);
                        string playerPath = _selectedSaveFileData.Value.FilePath;
                        string missionPath = _selectedSaveFileData.Value.MissionFilePath;

                        PlayerPersistentData.PlayerMetadata.SaveFiles.Remove(_selectedSaveFileData.Value);

                        SaveFileData newSaveFile = new SaveFileData
                        {
                            Name = nameInputField.text,
                            Date = DateTime.Now,
                            FilePath = playerPath,
                            MissionFilePath = missionPath
                        };
                        print("OVERWRITING FILE " + playerPath);

                        PlayerPersistentData.PlayerMetadata.SaveFiles.Add(newSaveFile);
                        PlayerPersistentData.PlayerMetadata.CurrentSaveFile = newSaveFile;

                        PlayerPersistentData.ExportPlayerPersistentData(PlayerPersistentData.PlayerData, playerPath);
                        MissionManager.ExportMissionsCurrentRemoteData(MissionManager.MissionsCurrentData, missionPath);

                        _selectedSaveFileData = newSaveFile;

                        UpdateScrollView();
                    });
            }
            
        }

        private void CancelPressed()
        {
            _selectedSaveFileData = null;
            gameObject.SetActive(false);
        }
        
        //============================================================================================================//

    }
    
    [Serializable]
    public class SaveGameContentScrollView : UIElementContentScrollView<SaveFileData>
    {}
}

