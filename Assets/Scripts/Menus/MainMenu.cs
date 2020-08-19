using System;
using UnityEngine;
using UnityEngine.UI;
using StarSalvager.Utilities.SceneManagement;
using UnityEngine.SceneManagement;
using StarSalvager.Values;
using StarSalvager.Cameras.Data;
using StarSalvager.Utilities;
using StarSalvager.Utilities.UI;
using System.IO;
using Sirenix.OdinInspector;
using StarSalvager.Factories;
using StarSalvager.Missions;

using CameraController = StarSalvager.Cameras.CameraController;
using StarSalvager.Utilities.JsonDataTypes;
using System.Collections;
using StarSalvager.Utilities.Saving;
using System.Linq;
using StarSalvager.Audio;

namespace StarSalvager.UI
{
    //FIXME Once the navigation style is decided, we can better solidify the data structure for the menus
    //FIXME All windows can be combined to reduce total images used
    public class MainMenu : MonoBehaviour
    {
        private enum MENU
        {
            MAIN,
            NEW,
            LOAD,
            OPTION
        }
        
        //============================================================================================================//
        
        [SerializeField]
        private CameraController m_cameraController;
        public CameraController CameraController => m_cameraController;
        
        //============================================================================================================//

        #region Menu Windows
        
        [SerializeField, Required, FoldoutGroup("Main Menu")]
        private GameObject mainMenuWindow;
        [SerializeField, Required, FoldoutGroup("Main Menu")]
        private Button newGameButton;
        [SerializeField, Required, FoldoutGroup("Main Menu")]
        private Button continueButton;
        [SerializeField, Required, FoldoutGroup("Main Menu")]
        private Button loadGameButton;
        [SerializeField, Required, FoldoutGroup("Main Menu")]
        private Button optionsButton;
        [SerializeField, Required, FoldoutGroup("Main Menu")]
        private Button quitButton;
        
        [SerializeField, Required, BoxGroup("Main Menu/Testing", Order = -1000)]
        private Button m_toggleOrientationButton;
        [SerializeField, Required, BoxGroup("Main Menu/Testing")]
        private Slider m_cameraZoomScaler;
        [SerializeField, Required, BoxGroup("Main Menu/Testing")]
        private SliderText _zoomSliderText;
        
        //============================================================================================================//
        
        [SerializeField, Required, FoldoutGroup("New Game Menu")]
        private GameObject newGameWindow;
        [SerializeField, Required, FoldoutGroup("New Game Menu")]
        private Button startGameButton;
        [SerializeField, Required, FoldoutGroup("New Game Menu")]
        private Button tutorialButton;
        [SerializeField, Required, FoldoutGroup("New Game Menu")]
        private Button ngBackButton;
        
        //============================================================================================================//
        
        [SerializeField, Required, FoldoutGroup("Load Game Menu")]
        private GameObject loadGameWindow;
        /*[SerializeField, Required, FoldoutGroup("Load Game Menu")]
        private Button slot1Button;
        [SerializeField, Required, FoldoutGroup("Load Game Menu")]
        private Button slot2Button;
        [SerializeField, Required, FoldoutGroup("Load Game Menu")]
        private Button slot3Button;
        [SerializeField, Required, FoldoutGroup("Load Game Menu")]
        private Button lgBackButton;*/

        //============================================================================================================//

        [SerializeField, Required, FoldoutGroup("Options Menu")]
        private GameObject optionsWindow;
        [SerializeField, Required, FoldoutGroup("Options Menu")]
        private Slider musicSlider;
        [SerializeField, Required, FoldoutGroup("Options Menu")]
        private Slider sfxSlider;
        [SerializeField, Required, FoldoutGroup("Options Menu")]
        private Button oBackButton;
        [SerializeField, Required, FoldoutGroup("Options Menu")]
        private Toggle testingFeaturesToggle;
        
        #endregion //Menu Windows
        
        //============================================================================================================//
        
        // Start is called before the first frame update
        private void Start()
        {
            StartCoroutine(Init());
        }

        private IEnumerator Init()
        {
            while (!SceneLoader.IsReady)
                yield return null;
            
            AnalyticsManager.ReportAnalyticsEvent(AnalyticsManager.AnalyticsEventType.GameStart);

            InitButtons();

            OpenMenu(MENU.MAIN);

            if (gameObject.scene == SceneManager.GetActiveScene())
                Globals.ScaleCamera(m_cameraZoomScaler.value);

            testingFeaturesToggle.isOn = !FactoryManager.Instance.DisableTestingFeatures;
        }

        private void Update()
        {
            continueButton.interactable = PlayerPersistentData.PlayerMetadata.SaveFiles.Count > 0;

            m_toggleOrientationButton.gameObject.SetActive(!FactoryManager.Instance.DisableTestingFeatures);
            m_cameraZoomScaler.gameObject.SetActive(!FactoryManager.Instance.DisableTestingFeatures);
            _zoomSliderText.Text.gameObject.SetActive(!FactoryManager.Instance.DisableTestingFeatures);
        }

        //============================================================================================================//

        private void InitButtons()
        {
            m_toggleOrientationButton.onClick.AddListener(() =>
            {
                Globals.Orientation = Globals.Orientation == ORIENTATION.HORIZONTAL
                    ? ORIENTATION.VERTICAL
                    : ORIENTATION.HORIZONTAL;
            });
            
            m_cameraZoomScaler.onValueChanged.AddListener(Globals.ScaleCamera);
            _zoomSliderText.Init();

            
            //Main Menu Buttons
            //--------------------------------------------------------------------------------------------------------//
            
            newGameButton.onClick.AddListener(() => OpenMenu(MENU.NEW));

            continueButton.onClick.AddListener(() =>
            {
                string playerPath = PlayerPersistentData.PlayerMetadata.GetPathMostRecentFile();
                string missionPath = PlayerPersistentData.PlayerMetadata.GetPathMostRecentMissionFile();

                if (playerPath != string.Empty && missionPath != string.Empty)
                {
                    print("LOADING FILE " + playerPath);

                    PlayerPersistentData.PlayerMetadata.CurrentSaveFile = PlayerPersistentData.PlayerMetadata.SaveFiles.FirstOrDefault(s => s.FilePath == playerPath);
                    PlayerPersistentData.SetCurrentSaveFile(playerPath);
                    MissionManager.SetCurrentSaveFile(missionPath);
                    FactoryManager.Instance.currentModularDataIndex = PlayerPersistentData.PlayerData.currentModularSectorIndex;
                    SceneLoader.ActivateScene(SceneLoader.UNIVERSE_MAP, SceneLoader.MAIN_MENU);
                }
            });

            loadGameButton.onClick.AddListener(() => loadGameWindow.SetActive(true));
            
            optionsButton.onClick.AddListener(() => OpenMenu(MENU.OPTION));
            
            quitButton.onClick.AddListener(() =>
            {
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
                #else
                Application.Quit();
                #endif
            });
            
            //New Game Buttons
            //--------------------------------------------------------------------------------------------------------//
            
            startGameButton.onClick.AddListener(() =>
            {
                OpenMenu(MENU.MAIN);

                string playerPath = PlayerPersistentData.GetNextAvailableSaveSlot();
                string missionPath = MissionManager.GetNextAvailableSaveSlot();

                if (playerPath != string.Empty && missionPath != string.Empty)
                {
                    /*SaveFileData newSaveFile = new SaveFileData
                    {
                        Name = DateTime.Now.ToString(),
                        Date = DateTime.Now,
                        FilePath = playerPath,
                        MissionFilePath = missionPath
                    };
                    print("CREATING FILE " + playerPath);

                    PlayerPersistentData.PlayerMetadata.SaveFiles.Add(newSaveFile);
                    PlayerPersistentData.PlayerMetadata.CurrentSaveFile = newSaveFile;*/

                    PlayerPersistentData.SetCurrentSaveFile(playerPath);
                    MissionManager.SetCurrentSaveFile(missionPath);
                    PlayerPersistentData.ResetPlayerData();
                    MissionManager.ResetMissionData();
                    SceneLoader.ActivateScene(SceneLoader.UNIVERSE_MAP, SceneLoader.MAIN_MENU);
                }
                else
                {
                    print("NO EMPTY SLOTS");
                }
            });
            
            tutorialButton.onClick.AddListener(() => throw new NotImplementedException());
            tutorialButton.interactable = false;
            
            ngBackButton.onClick.AddListener(() => OpenMenu(MENU.MAIN));

            //Load Game Buttons
            //--------------------------------------------------------------------------------------------------------//

            //FIXME This will likely need to be scalable
           /* slot1Button.onClick.AddListener(() =>
            {
                OpenMenu(MENU.MAIN);
                PlayerPersistentData.SetCurrentSaveFile(0);
                MissionManager.SetCurrentSaveFile();
                PlayerPersistentData.IsNewFile = false;
                SceneLoader.ActivateScene("UniverseMapScene", "MainMenuScene");
            });

            slot2Button.onClick.AddListener(() =>
            {
                OpenMenu(MENU.MAIN);
                PlayerPersistentData.SetCurrentSaveFile(1);
                MissionManager.SetCurrentSaveFile();
                PlayerPersistentData.IsNewFile = false;
                SceneLoader.ActivateScene("UniverseMapScene", "MainMenuScene");
            });
            //slot2Button.interactable = false;
            
            slot3Button.onClick.AddListener(() =>
            {
                OpenMenu(MENU.MAIN);
                PlayerPersistentData.SetCurrentSaveFile(2);
                MissionManager.SetCurrentSaveFile();
                PlayerPersistentData.IsNewFile = false;
                SceneLoader.ActivateScene("UniverseMapScene", "MainMenuScene");
            });
            //slot3Button.interactable = false;

            lgBackButton.onClick.AddListener(() => OpenMenu(MENU.MAIN));*/
            
            //Options Buttons
            //--------------------------------------------------------------------------------------------------------//

            musicSlider.onValueChanged.AddListener(AudioController.SetMusicVolume);
            sfxSlider.onValueChanged.AddListener(AudioController.SetSFXVolume);
            
            oBackButton.onClick.AddListener(() => OpenMenu(MENU.MAIN));

            testingFeaturesToggle.onValueChanged.AddListener(delegate {
                FactoryManager.Instance.DisableTestingFeatures = !testingFeaturesToggle.isOn;
            });
            
            //--------------------------------------------------------------------------------------------------------//



        }
        
        //============================================================================================================//

        private void OpenMenu(MENU menu)
        {
            mainMenuWindow.SetActive(false);
            newGameWindow.SetActive(false);
            optionsWindow.SetActive(false);
            
            switch (menu)
            {
                case MENU.MAIN:
                    mainMenuWindow.SetActive(true);
                    break;
                case MENU.NEW:
                    newGameWindow.SetActive(true);
                    break;
                case MENU.OPTION:
                    optionsWindow.SetActive(true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(menu), menu, null);
            }
        }
        
        
        //============================================================================================================//

        /*private void ScaleCamera(float cameraZoomScalerValue)
        {
            Globals.ColumnsOnScreen = (int)cameraZoomScalerValue;
            if (Globals.ColumnsOnScreen % 2 == 0)
                Globals.ColumnsOnScreen += 1;
            
            CameraController.SetOrthographicSize(Constants.gridCellSize * Globals.ColumnsOnScreen, Vector3.zero);

            if (Globals.Orientation == ORIENTATION.VERTICAL)
            {
                Globals.GridSizeX = (int)(Globals.ColumnsOnScreen * Constants.GridWidthRelativeToScreen);
                Globals.GridSizeY = (int)((Camera.main.orthographicSize * Constants.GridHeightRelativeToScreen * 2) / Constants.gridCellSize);
            }
            else
            {
                Globals.GridSizeX = (int)(Globals.ColumnsOnScreen * Constants.GridWidthRelativeToScreen * (Screen.height / (float)Screen.width));
                Globals.GridSizeY = (int)((Camera.main.orthographicSize * Constants.GridHeightRelativeToScreen * 2 * (Screen.width / (float)Screen.height)) / Constants.gridCellSize);
            }
        }*/
        
        //============================================================================================================//
        
        #if UNITY_EDITOR
        
        [Button("Clear Remote Data"), DisableInPlayMode]
        private void ClearRemoteData()
        {
            FindObjectOfType<FactoryManager>().ClearRemoteData();
        }
        
        #endif
    }
}