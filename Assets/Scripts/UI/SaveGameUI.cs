using System;
using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector;
using StarSalvager.Missions;
using StarSalvager.Utilities.Saving;
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
            nameInputField.text = DateTime.Now.ToString(DATETIME_FORMAT);

            UpdateScrollView();
        }

        //============================================================================================================//


        
        private void UpdateScrollView()
        {
            //TODO Get all the save files here
            /*List<SaveFileData> saveFiles = new List<SaveFileData>();

            foreach (var fileName in Directory.GetFiles(path))
            {
                if (fileName.Contains("PlayerPersistentDataSaveFile") && !fileName.Contains(".meta"))
                {
                    saveFiles.Add(new SaveFileData
                    {
                        Name = fileName,
                        Date = System.IO.File.GetLastWriteTime(fileName),
                        FilePath = fileName,
                        MissionFilePath = fileName.Replace("PlayerPersistentDataSaveFile", "MissionsCurrentDataSaveFile")
                    });
                }
            }*/
            
            foreach (var saveFile in PlayerPersistentData.PlayerMetadata.SaveFiles)
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
                PlayerPersistentData.ImportPlayerPersistentData(_selectedSaveFileData.Value.FilePath);
                MissionManager.ImportMissionsCurrentRemoteData(_selectedSaveFileData.Value.MissionFilePath);

                CancelPressed();
            }

        }

        private void SavePressed()
        {

            if (!_selectedSaveFileData.HasValue)
            {
                //TODO Write a new save file
                string playerPath = PlayerPersistentData.GetNextAvailableSaveSlot();
                string missionPath = MissionManager.GetNextAvailableSaveSlot();

                if (playerPath != string.Empty && missionPath != string.Empty)
                {
                    PlayerPersistentData.PlayerMetadata.SaveFiles.Add(new SaveFileData
                    {
                        Name = DateTime.Now.ToString(),
                        Date = DateTime.Now,
                        FilePath = playerPath,
                        MissionFilePath = missionPath
                    });
                    print("CREATING FILE " + playerPath);

                    PlayerPersistentData.SetCurrentSaveFile(playerPath);
                    MissionManager.SetCurrentSaveFile(missionPath);
                    PlayerPersistentData.ResetPlayerData();
                    MissionManager.ResetMissionData();
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
                        //TODO Delete the old file here
                        //TODO Need to save new file here
                        string playerPath = _selectedSaveFileData.Value.FilePath;
                        string missionPath = _selectedSaveFileData.Value.MissionFilePath;

                        PlayerPersistentData.PlayerMetadata.SaveFiles.Remove(_selectedSaveFileData.Value);

                        PlayerPersistentData.PlayerMetadata.SaveFiles.Add(new SaveFileData
                        {
                            Name = DateTime.Now.ToString(),
                            Date = DateTime.Now,
                            FilePath = playerPath,
                            MissionFilePath = missionPath
                        });
                        print("OVERWRITING FILE " + playerPath);

                        PlayerPersistentData.ExportPlayerPersistentData(PlayerPersistentData.PlayerData, playerPath);
                        MissionManager.ExportMissionsCurrentRemoteData(MissionManager.MissionsCurrentData, missionPath);

                        UpdateScrollView();
                    });
            }
            
        }

        private void CancelPressed()
        {
            //TODO Need to close the window here
            gameObject.SetActive(false);
        }
        
        //============================================================================================================//

    }
    
    [Serializable]
    public class SaveGameContentScrollView : UIElementContentScrollView<SaveFileData>
    {}
}

