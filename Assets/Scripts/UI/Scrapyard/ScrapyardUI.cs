﻿using System;
using System.Collections.Generic;
using System.Linq;
using Recycling;
using Sirenix.OdinInspector;
using StarSalvager.Audio;
using StarSalvager.Cameras;
using StarSalvager.Factories;
using StarSalvager.UI.Hints;
using StarSalvager.Utilities;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.Helpers;
using StarSalvager.Utilities.JsonDataTypes;
using StarSalvager.Utilities.Saving;
using StarSalvager.Utilities.SceneManagement;
using StarSalvager.Utilities.UI;
using StarSalvager.Values;
using TMPro;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace StarSalvager.UI.Wreckyard
{
    [Obsolete]
    public class ScrapyardUI : MonoBehaviour
    {
        //============================================================================================================//

        [SerializeField, Required]
        private GameObject workbenchWindow;

        //Prototype
        //====================================================================================================================//

        /*[SerializeField, Required, FoldoutGroup("Prototype")]
        private GameObject partDisposeWindow;
        [SerializeField, Required, FoldoutGroup("Prototype")]
        private TMP_Text titleText;
        [SerializeField, Required, FoldoutGroup("Prototype")]
        private PartChoiceUI.PartSelectionUI[] selectionUis;

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
                        Patches = new List<PatchData>()
                    });

                _droneDesigner._scrapyardBot.AttachNewBit(coordinate, attachable);

                PlayerDataManager.SetDroneBlockData(_droneDesigner._scrapyardBot.AttachedBlocks.GetBlockDatas());
            }

            //--------------------------------------------------------------------------------------------------------//

            var currentParts = new List<PartData>(PlayerDataManager.GetCurrentPartsInStorage().OfType<PartData>());
            currentParts.AddRange(PlayerDataManager.GetBotBlockDatas().OfType<PartData>());

            foreach (BIT_TYPE bitType in Enum.GetValues(typeof(BIT_TYPE)))
            {
                if(bitType == BIT_TYPE.WHITE || bitType == BIT_TYPE.NONE)
                    continue;

                var parts = currentParts
                    .Where(x => ((PART_TYPE) x.Type).GetCategory() == bitType)
                    .ToList();

                if(parts.Count <= Globals.MaxPartTypeCount)
                    continue;

                partDisposeWindow.SetActive(true);
                titleText.text = "Discard 1 Part";

                var partOptions = parts
                    .Where(x => PartChoiceUI.LastPicked != (PART_TYPE)x.Type)
                    .Take(2)
                    .ToArray();

                for (int i = 0; i < partOptions.Length; i++)
                {
                    var partData = partOptions[i];
                    var partType = (PART_TYPE)partData.Type;
                    var category = partType.GetCategory();

                    selectionUis[i].optionText.text = partType.GetRemoteData().name;
                    selectionUis[i].optionImage.sprite = partType.GetSprite();

                    selectionUis[i].PartChoiceButtonHover.SetPartType(partType);
                    
                    selectionUis[i].categoryImage.color = category.GetColor();
                    selectionUis[i].categoryText.text = category.GetCategoryName();

                    selectionUis[i].optionButton.onClick.RemoveAllListeners();
                    selectionUis[i].optionButton.onClick.AddListener(() =>
                    {
                        PartDetailsUI.ShowPartDetails(false, partData, null);
                        FindAndDestroyPart(partType);
                        partDisposeWindow.SetActive(false);
                    });
                }

                break;
            }
        }*/

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

        [FormerlySerializedAs("componentsNumber")] [SerializeField, Required, FoldoutGroup("Gears Indicator")]
        private TMP_Text gearsAmountText;
        [SerializeField, Required, FoldoutGroup("Silver Indicator")]
        private TMP_Text silverAmountText;
        
        
        private PartDetailsUI PartDetailsUI
        {
            get
            {
                if (_partDetailsUI == null)
                    _partDetailsUI = FindObjectOfType<PartDetailsUI>();

                return _partDetailsUI;
            }
        }
        private PartDetailsUI _partDetailsUI;

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

        private void OnEnable()
        {
            if (!PlayerDataManager.HasRunData) return;

            PlayerDataManager.OnValuesChanged += OnValuesChanged;
            OnValuesChanged();

            musicVolumeSlider.value = PlayerPrefs.GetFloat(AudioController.MUSIC_VOLUME, 1f);
            sfxVolumeSlider.value = PlayerPrefs.GetFloat(AudioController.SFX_VOLUME, 1f);

            CameraController.CameraOffset(Vector3.zero, true);
            CameraController.SetOrthographicSize(31f, Vector3.down * 5f);

            backButton.onClick?.Invoke();

            partChoiceWindow.SetActive(PlayerDataManager.CanChoosePart);

            //--------------------------------------------------------------------------------------------------------//

            if (PlayerDataManager.CanChoosePart)
            {
                if (_partChoice == null)
                {
                    _partChoice = FindObjectOfType<PartChoiceUI>();
                }
                bool notYetStarted = PlayerDataManager.HasRunStarted();

                if (!notYetStarted)
                {
                    _partChoice.Init(PartAttachableFactory.PART_OPTION_TYPE.InitialSelection);
                    PlayerDataManager.ClearAllPatches();
                }
                else
                {
                    _partChoice.Init(PartAttachableFactory.PART_OPTION_TYPE.Any);

                    //PlayerDataManager.SetCurrentPatchOptions(Globals.CurrentRing.GenerateRingPatches());
                    _droneDesigner.DroneDesignUi.InitPurchasePatches();
                }
            }

            //--------------------------------------------------------------------------------------------------------//

        }

        // Start is called before the first frame update
        private void Start()
        {
            //partDisposeWindow.SetActive(false);
            
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
        }



        private void OnDisable()
        {
            PlayerDataManager.OnValuesChanged -= OnValuesChanged;
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

                                _windows[(int)Window.Settings].SetActive(false);
                                SceneLoader.ActivateScene(SceneLoader.MAIN_MENU, SceneLoader.WRECKYARD, MUSIC.MAIN_MENU);
                                AnalyticsManager.WreckEndEvent(AnalyticsManager.REASON.QUIT);
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
            if (PlayerDataManager.GetBotBlockDatas().CheckHasDisconnects())
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
            throw new NotImplementedException();
            //_droneDesigner.ProcessScrapyardUsageEndAnalytics();
        }

        //============================================================================================================//

        private void OnValuesChanged()
        {
            gearsAmountText.text = $"{TMP_SpriteHelper.GEAR_ICON} {PlayerDataManager.GetGears()}";
            silverAmountText.text = $"{TMP_SpriteHelper.SILVER_ICON} {PlayerDataManager.GetSilver()}";
        }

        //====================================================================================================================//


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
