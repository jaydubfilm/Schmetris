using System;
using System.Collections.Generic;
using System.Linq;
using Recycling;
using Sirenix.OdinInspector;
using StarSalvager.Audio;
using StarSalvager.Cameras;
using StarSalvager.Factories;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JsonDataTypes;
using StarSalvager.Utilities.Saving;
using StarSalvager.Utilities.SceneManagement;
using StarSalvager.Utilities.UI;
using StarSalvager.Values;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace StarSalvager.UI.Scrapyard
{
    public class ScrapyardUI : MonoBehaviour
    {
        //============================================================================================================//

        [SerializeField, Required]
        private GameObject workbenchWindow;

        //Prototype
        //====================================================================================================================//

        //FIXME This should be combined with the part choice UI
        [Serializable]
        private struct SelectionUI
        {
            public Button partButton;
            public PartChoiceButtonHover PartChoiceButtonHover;
            public TMP_Text partTitle;
        }
        
        [SerializeField, Required, FoldoutGroup("Prototype")]
        private GameObject partDisposeWindow;
        [SerializeField, Required, FoldoutGroup("Prototype")]
        private TMP_Text titleText;
        [SerializeField, Required, FoldoutGroup("Prototype")]
        private SelectionUI[] selectionUis;

        //FIXME This should be moving to the drone designer once its ready
        public void CheckForPartOverage()
        {
            //--------------------------------------------------------------------------------------------------------//

            void FindAndDestroyPart(in PART_TYPE partType)
            {
                var type = partType;

                var storage = new List<IBlockData>(PlayerDataManager.GetCurrentPartsInStorage());
                var index = storage.FindIndex(x => x is PartData p && p.Type == (int) type);
                if (index >= 0)
                {
                    //TODO From the part from the storage
                    PlayerDataManager.RemovePartFromStorageAtIndex(index);
                    return;
                }

                index = _droneDesigner._scrapyardBot.AttachedBlocks
                    .FindIndex(x => x is ScrapyardPart p && p.Type == type);

                if (index < 0)
                    throw new Exception();

                var scrapyardPart = (ScrapyardPart) _droneDesigner._scrapyardBot.AttachedBlocks[index];
                var coordinate = scrapyardPart.Coordinate;

                _droneDesigner._scrapyardBot.TryRemoveAttachableAt(coordinate);

                var attachable = FactoryManager.Instance.GetFactory<PartAttachableFactory>()
                    .CreateScrapyardObject<ScrapyardPart>(new PartData
                    {
                        Type = (int) PART_TYPE.EMPTY,
                        Coordinate = coordinate,
                        Patches = new PatchData[0]
                    });

                _droneDesigner._scrapyardBot.AttachNewBit(coordinate, attachable);

                PlayerDataManager.SetBlockData(_droneDesigner._scrapyardBot.AttachedBlocks.GetBlockDatas());
            }

            //--------------------------------------------------------------------------------------------------------//

            var bitProfile = FactoryManager.Instance.BitProfileData;
            
            var partRemote = FactoryManager.Instance.PartsRemoteData;
            var partProfile = FactoryManager.Instance.PartsProfileData;
            
            var currentParts = new List<PartData>(PlayerDataManager.GetCurrentPartsInStorage().OfType<PartData>());
            currentParts.AddRange(PlayerDataManager.GetBlockDatas().OfType<PartData>());

            foreach (BIT_TYPE bitType in Enum.GetValues(typeof(BIT_TYPE)))
            {
                if(bitType == BIT_TYPE.WHITE || bitType == BIT_TYPE.NONE)
                    continue;
                
                var parts = currentParts
                    .Where(x => partRemote.GetRemoteData((PART_TYPE) x.Type).category == bitType)
                    .ToList();
                
                if(parts.Count <= Globals.MaxPartTypeCount)
                    continue;
                
                partDisposeWindow.SetActive(true);
                titleText.text = "Discard 1 Part";
                
                var partOptions = parts
                    .Where(x => PartChoiceUI.LastPicked != (PART_TYPE)x.Type)
                    .Select(x => (PART_TYPE) x.Type)
                    .Take(2)
                    .ToArray();
                
                for (int i = 0; i < partOptions.Length; i++)
                {
                    var partType = partOptions[i];
                    var partRemoteData = partRemote.GetRemoteData(partType);
                    
                    selectionUis[i].partTitle.text = partRemoteData.name;
                    selectionUis[i].partButton.image.sprite = partProfile.GetProfile(partType).Sprite;
                    selectionUis[i].partButton.image.color = partRemoteData.category.GetColor();
                    
                    selectionUis[i].PartChoiceButtonHover.SetPartType(partType);
                    
                    selectionUis[i].partButton.onClick.RemoveAllListeners();
                    selectionUis[i].partButton.onClick.AddListener(() =>
                    {
                        _droneDesigner.DroneDesignUi.ShowPartDetails(false, new PartData(), null);
                        FindAndDestroyPart(partType);
                        partDisposeWindow.SetActive(false);
                    });
                }
                
                break;
            }
        }

        //====================================================================================================================//

        [SerializeField, Required, FoldoutGroup("Menu Window")]
        private GameObject settingsWindow;
        [SerializeField, Required, FoldoutGroup("Menu Window")]
        private Button resumeGameButton;
        [SerializeField, Required, FoldoutGroup("Menu Window")]
        private Button settingsButton;
        [SerializeField, Required, FoldoutGroup("Menu Window")]
        private Button quitGameButton;

        //====================================================================================================================//

        [SerializeField, Required, FoldoutGroup("Settings Window")]
        private GameObject settingsWindowObject;
        [SerializeField, Required, FoldoutGroup("Settings Window")]
        private Button settingsBackButton;
        [SerializeField, Required, FoldoutGroup("Settings Window")]
        private Slider musicVolumeSlider;
        [SerializeField, Required, FoldoutGroup("Settings Window")]
        private Slider sfxVolumeSlider;
        [SerializeField, Required, FoldoutGroup("Settings Window")]
        private Toggle testingFeaturesToggle;

        //====================================================================================================================//

        [SerializeField, Required, FoldoutGroup("Part Choice Window")]
        private GameObject partChoiceWindow;

        //============================================================================================================//
        [SerializeField, Required, FoldoutGroup("Navigation Buttons")]
        private Button menuButton;
        [SerializeField, Required, FoldoutGroup("Navigation Buttons")]
        private Button backButton;

        [SerializeField, Required, FoldoutGroup("Components Indicator")]
        private TMP_Text componentsNumber;

        //====================================================================================================================//

        [SerializeField]
        private CameraController CameraController;

        private DroneDesigner _droneDesigner;

        private PartChoiceUI _partChoice;

        private GameObject[] _windows;
        private enum Window
        {
            Workbench,
            Settings,
        }

        //============================================================================================================//

        // Start is called before the first frame update
        private void Start()
        {
            _droneDesigner = FindObjectOfType<DroneDesigner>();
            _partChoice = FindObjectOfType<PartChoiceUI>();

            _windows = new[]
            {
                workbenchWindow,
                settingsWindow
            };

            InitButtons();
            InitMenuButtons();
            InitSettings();

            SetWindowActive(Window.Workbench);


            partDisposeWindow.SetActive(false);
        }

        //FIXME This does not need to be in Update
        private void Update()
        {
            componentsNumber.text = $"{TMP_SpriteMap.GEAR_ICON} {PlayerDataManager.GetComponents()}";
        }

        private void OnEnable()
        {
            CameraController.CameraOffset(Vector3.zero, true);
            CameraController.SetOrthographicSize(31f, Vector3.down * 5f);

            backButton.onClick?.Invoke();

            partChoiceWindow.SetActive(PlayerDataManager.GetCanChoosePart());
            
            //--------------------------------------------------------------------------------------------------------//
            
            if (PlayerDataManager.GetCanChoosePart())
            {
                if (_partChoice == null)
                {
                    _partChoice = FindObjectOfType<PartChoiceUI>();
                }
                bool notYetStarted = PlayerDataManager.GetStarted();

                if (!notYetStarted)
                {
                    _partChoice.Init(PartAttachableFactory.PART_OPTION_TYPE.InitialSelection);
                    PlayerDataManager.ClearAllPatches();
                }
                else
                {
                    _partChoice.Init(PartAttachableFactory.PART_OPTION_TYPE.Any);
                    
                    PlayerDataManager.SetPatches(Globals.CurrentRing.GenerateRingPatches());
                    _droneDesigner.DroneDesignUi.InitPurchasePatches();
                }
            }

            //--------------------------------------------------------------------------------------------------------//
            
        }

        //============================================================================================================//

        private void InitButtons()
        {

            menuButton.onClick.AddListener(() =>
            {
                _windows[(int)Window.Settings].SetActive(true);
            });


            backButton.onClick.AddListener(() =>
            {
            });

            //--------------------------------------------------------------------------------------------------------//

        }

        private void InitMenuButtons()
        {
            resumeGameButton.onClick.AddListener(() =>
            {
                _windows[(int)Window.Settings].SetActive(false);
            });

            settingsButton.onClick.AddListener(() =>
            {
                settingsWindowObject.SetActive(true);
            });

            quitGameButton.onClick.AddListener(() =>
            {
                Alert.ShowAlert("Quitting",
                    "Are you sure you want to save & quit?",
                    "Desktop",
                    "Main Menu",
                    "Cancel",
                    quit =>
                    {
                        PlayerDataManager.SavePlayerAccountData();
                        //PlayerDataManager.ClearPlayerAccountData();

                        if (!quit)
                        {
                            ScreenFade.Fade(() =>
                            {
                                
                                settingsWindowObject.SetActive(false);
                                SceneLoader.ActivateScene(SceneLoader.MAIN_MENU, SceneLoader.SCRAPYARD, MUSIC.MAIN_MENU);
                            });

                            return;
                        }
#if UNITY_EDITOR
                        UnityEditor.EditorApplication.isPlaying = false;
#else
                        Application.Quit();
#endif
                    },
                    null);
            });
        }

        private void InitSettings()
        {
            musicVolumeSlider.onValueChanged.AddListener(AudioController.SetMusicVolume);

            sfxVolumeSlider.onValueChanged.AddListener(AudioController.SetSFXVolume);

            testingFeaturesToggle.onValueChanged.AddListener(toggle =>
            {
                Globals.TestingFeatures = toggle;
            });

            settingsBackButton.onClick.AddListener(() =>
            {
                settingsWindowObject.SetActive(false);
            });
        }

        //Launch Window Functions
        //============================================================================================================//

        private void TryLaunch()
        {
            if (PlayerDataManager.GetBlockDatas().CheckHasDisconnects())
            {
                Alert.ShowAlert("Alert!",
                    "A disconnected piece is active on your Bot! Please repair before continuing", "Fix",
                    () =>
                    {
                        backButton.gameObject.SetActive(true);

                        SetWindowActive(Window.Workbench);
                    });

                return;
            }

            //Checks to see if we need to display a window
            if (PlayerDataManager.GetCurrentPartsInStorage().Count > 0)
            {
                Alert.ShowAlert("Warning!",
                    "You have unused parts left in storage, are you sure you want to launch?",
                    "Launch!",
                    "Back",
                    state =>
                    {
                        if(state) Launch();

                    },
                    "PartsStorage");

                return;
            }



            Launch();
        }

        private void Launch()
        {
            _droneDesigner.ProcessScrapyardUsageEndAnalytics();

            ScreenFade.Fade(() =>
            {
                SceneLoader.ActivateScene(SceneLoader.UNIVERSE_MAP, SceneLoader.SCRAPYARD);
            });
        }

        //============================================================================================================//

        /*private Window _currentWindow;*/

        private void SetWindowActive(Window window)
        {
            //_currentWindow = window;
            SetWindowActive((int)window);

            menuButton.gameObject.SetActive(true);
        }

        private void SetWindowActive(int index)
        {
            for (var i = 0; i < _windows.Length; i++)
            {
                _windows[i].SetActive(i == index);
            }
        }

    }
}
