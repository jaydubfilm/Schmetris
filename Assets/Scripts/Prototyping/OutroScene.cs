using System.Collections;
using System.Collections.Generic;
using StarSalvager.Audio;
using StarSalvager.UI;
using StarSalvager.Utilities;
using StarSalvager.Utilities.Saving;
using StarSalvager.Utilities.SceneManagement;
using StarSalvager.Values;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace StarSalvager.Prototype
{
    [System.Obsolete("This is exclusively for prototyping, and should not be part of the final product")]
    public class OutroScene : MonoBehaviour, IReset
    {
        private List<(int, string)> _dialogueLines;
        
        private int _outroSceneSlide;

        public GameObject panel1;
        public GameObject panel1Character;

        public GameObject panel2;
        public GameObject panel2Character;

        public GameObject panelText1GameObject;
        public GameObject panelText2GameObject;

        public TMP_Text panelText1;
        public TMP_Text panelText2;

        //Unity Functions
        //====================================================================================================================//
        private void OnEnable()
        {
            Utilities.Inputs.Input.Actions.MenuControls.Submit.performed += OnSubmitPressed;
            Utilities.Inputs.Input.Actions.MenuControls.Pause.performed += OnSkipPressed;
        }
        
        private void Awake()
        {
            SetupScene();
            gameObject.SetActive(false);
        }

        // Update is called once per frame
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
                NextStep();

            if (Input.GetKeyDown(KeyCode.Escape))
                Skip();
        }
        
        private void OnDisable()
        {
            Utilities.Inputs.Input.Actions.MenuControls.Submit.performed -= OnSubmitPressed;
            Utilities.Inputs.Input.Actions.MenuControls.Pause.performed -= OnSkipPressed;
        }

        //Inputs
        //====================================================================================================================//
        
        private void OnSubmitPressed(InputAction.CallbackContext ctx)
        {
            if (!ctx.ReadValueAsButton())
                return;
            
            NextStep();
        }



        private void OnSkipPressed(InputAction.CallbackContext ctx)
        {
            if (!ctx.ReadValueAsButton())
                return;
            
            Skip();
        }

        

        private void NextStep()
        {
            _outroSceneSlide++;
            if (_outroSceneSlide >= _dialogueLines.Count)
            {
                ShowFinalScreen();
                return;
            }

            SetCurrentSlide(_outroSceneSlide);
        }
        
        private void Skip()
        {
            gameObject.SetActive(false);
            panel1.SetActive(true);
            panel2.SetActive(false);
            _outroSceneSlide = 0;
                
            //TODO Need to open the Game Summary Screen
            ShowFinalScreen();
        }
        
        //OutroScene Functions
        //====================================================================================================================//

        private void SetupScene()
        {
            //0: Mushroom Character
            //1: Mechanic Character
            _dialogueLines = new List<(int, string)>
            {
                (0, "Well, that wasn't an optimal outcome, was it?"),
                (1,
                    "Shame to lose all those parts, but I have a back up around here somewhere. Lemme dust it off and get it ready to fly."),
                (0, "Try to be more careful next time, Captain. ")
            };

            _outroSceneSlide = 0;

            SetCurrentSlide(0);
        }

        private void SetCurrentSlide(in int index)
        {
            bool isLeftCharacter = _dialogueLines[index].Item1 == 0;

            panel1Character.SetActive(isLeftCharacter);
            panel2Character.SetActive(!isLeftCharacter);
            panelText1GameObject.SetActive(isLeftCharacter);
            panelText2GameObject.SetActive(!isLeftCharacter);
            panel1.SetActive(isLeftCharacter);
            panel2.SetActive(!isLeftCharacter);

            if (isLeftCharacter)
            {
                panelText1.text = _dialogueLines[index].Item2;
            }
            else
            {
                panelText2.text = _dialogueLines[index].Item2;
            }
        }

        private void ShowFinalScreen()
        {
            GameUI.Instance.SetDancersActive(true);
            AudioController.CrossFadeTrack(MUSIC.GAME_OVER);
            /*AudioController.PlayMusic(MUSIC.GAME_OVER, true);*/
            
            SetupScene();
            gameObject.SetActive(false);
            
            LevelManager.Instance.GameUi.ShowWaveSummaryWindow(true,
                "Game Over",
                string.Empty/*PlayerDataManager.GetRunSummaryString()*/,
                () =>
                {
                    Globals.CurrentWave = 0;
                    GameTimer.SetPaused(false);
                    
                    PlayerDataManager.CompleteCurrentRun();
                    PlayerDataManager.SavePlayerAccountData();
                    
                    ScreenFade.Fade(() =>
                    {
                        GameUI.Instance.SetDancersActive(false);
                        GameUI.Instance.FadeBackground(false, true);
                        GameManager.SetCurrentGameState(GameState.AccountMenu);
                        SceneLoader.ActivateScene(SceneLoader.MAIN_MENU, SceneLoader.LEVEL, MUSIC.MAIN_MENU);
                    });
                    
                    
                },
                "Main Menu",
                GameUI.WindowSpriteSet.TYPE.ORANGE,
                0.5f);
        }
        

        //IReset Functions
        //====================================================================================================================//
        
        public void Activate()
        {
            gameObject.SetActive(false);
        }

        public void Reset()
        {
            gameObject.SetActive(false);
        }

        //====================================================================================================================//
        
    }
}
