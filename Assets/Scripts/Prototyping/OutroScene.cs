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

namespace StarSalvager.Prototype
{
    [System.Obsolete("This is exclusively for prototyping, and should not be part of the final product")]
    public class OutroScene : MonoBehaviour, IReset
    {
        private List<(int, string)> dialogueLines = new List<(int, string)>();
        
        private int _outroSceneStage;

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
        
        private void Awake()
        {
            gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            dialogueLines.Clear();
            //dialogueLines.Add((1, "The drone is lost..."));
            dialogueLines.Add((0, "Well, that wasn't an optimal outcome, was it?"));
            dialogueLines.Add((1, "Shame to lose all those parts, but I have a back up around here somewhere. Lemme dust it off and get it ready to fly."));
            /*dialogueLines.Add((1, $"{PlayerDataManager.GetResource(BIT_TYPE.GREY).resource + 1} perfectly refined scrap metal bits. And for what?"));
            dialogueLines.Add((0, $"I thought it was {PlayerDataManager.GetResource(BIT_TYPE.GREY).resource}."));
            dialogueLines.Add((1, "Nah I’ve had this one in my pocket since yesterday. What? I like how it feels."));
            dialogueLines.Add((0, $"{ PlayerDataManager.GetResource(BIT_TYPE.GREY).resource + 1} metal bits is enough to craft another drone core."));
            dialogueLines.Add((0, "(ahem) Captain… it’ll put a dent in our cargo stores, but -"));
            dialogueLines.Add((1, "Are you saying we’re still in the game?"));*/
            dialogueLines.Add((0, "Try to be more careful next time, Captain. "));

            _outroSceneStage = 0;

            panel1Character.SetActive(false);
            panel2Character.SetActive(true);
            panelText1GameObject.SetActive(false);
            panelText2GameObject.SetActive(true);
            panel1.SetActive(false);
            panel2.SetActive(true);

            panelText2.text = dialogueLines[0].Item2;
            
        }


        // Update is called once per frame
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                _outroSceneStage++;
                if (_outroSceneStage >= dialogueLines.Count)
                {
                    ShowFinalScreen();
                    return;
                }

                bool isLeftCharacter = dialogueLines[_outroSceneStage].Item1 == 0;

                panel1Character.SetActive(isLeftCharacter);
                panel2Character.SetActive(!isLeftCharacter);
                panelText1GameObject.SetActive(isLeftCharacter);
                panelText2GameObject.SetActive(!isLeftCharacter);
                panel1.SetActive(isLeftCharacter);
                panel2.SetActive(!isLeftCharacter);

                if (isLeftCharacter)
                {
                    panelText1.text = dialogueLines[_outroSceneStage].Item2;
                }
                else
                {
                    panelText2.text = dialogueLines[_outroSceneStage].Item2;
                }
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                gameObject.SetActive(false);
                panel1.SetActive(true);
                panel2.SetActive(false);
                _outroSceneStage = 0;
                
                //TODO Need to open the Game Summary Screen
                ShowFinalScreen();
                //SceneLoader.ActivateScene(SceneLoader.UNIVERSE_MAP, SceneLoader.MAIN_MENU);
            }
        }

        //OutroScene Functions
        //====================================================================================================================//

        private void ShowFinalScreen()
        {
            GameUI.Instance.SetDancersActive(true);
            AudioController.CrossFadeTrack(MUSIC.GAME_OVER);
            /*AudioController.PlayMusic(MUSIC.GAME_OVER, true);*/
            
            gameObject.SetActive(false);
            
            LevelManager.Instance.GameUi.ShowWaveSummaryWindow(true,
                "Game Over",
                PlayerDataManager.GetRunSummaryString(),
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
    }
}
