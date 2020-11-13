﻿using System.Collections;
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
            dialogueLines.Add((1, "The drone is lost. May death take us quickly."));
            dialogueLines.Add((0, "It’s been a pleasure and an honour, Captain."));
            dialogueLines.Add((1, $"{PlayerDataManager.GetResource(BIT_TYPE.GREY).resource + 1} perfectly refined scrap metal bits. And for what?"));
            dialogueLines.Add((0, $"I thought it was {PlayerDataManager.GetResource(BIT_TYPE.GREY).resource}."));
            dialogueLines.Add((1, "Nah I’ve had this one in my pocket since yesterday. What? I like how it feels."));
            dialogueLines.Add((0, $"{ PlayerDataManager.GetResource(BIT_TYPE.GREY).resource + 1} metal bits is enough to craft another drone core."));
            dialogueLines.Add((0, "(ahem) Captain… it’ill put a dent in our cargo stores, but -"));
            dialogueLines.Add((1, "Are you saying we’re still in the game?"));
            dialogueLines.Add((0, "We’re still in the game!"));

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

        private static void ShowFinalScreen()
        {
            Alert.ShowDancers(true);
            AudioController.PlayMusic(MUSIC.GAME_OVER, true);
            Alert.ShowAlert("GAME OVER",
                PlayerDataManager.GetRunSummaryString(),
                "Finish",
                () =>
                {
                    Alert.ShowDancers(false);
                    Globals.IsRecoveryBot = false;
                    GameUI.Instance.ShowRecoveryBanner(false);
                    Globals.CurrentWave = 0;
                    GameTimer.SetPaused(false);
                    LevelManager.Instance.IsWaveProgressing = true;
                    
                    PlayerDataManager.ResetPlayerRunData();
                    PlayerDataManager.SavePlayerAccountData();
                    
                    ScreenFade.Fade(() =>
                    {
                        SceneLoader.ActivateScene(SceneLoader.MAIN_MENU, SceneLoader.LEVEL);
                    });
                    
                    
                });
            Alert.SetLineHeight(90f);
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
