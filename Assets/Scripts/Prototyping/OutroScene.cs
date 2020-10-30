using System.Collections;
using System.Collections.Generic;
using StarSalvager.Audio;
using StarSalvager.UI;
using StarSalvager.Utilities;
using StarSalvager.Utilities.Saving;
using StarSalvager.Utilities.SceneManagement;
using StarSalvager.Values;
using UnityEngine;

namespace StarSalvager.Prototype
{
    [System.Obsolete("This is exclusively for prototyping, and should not be part of the final product")]
    public class OutroScene : MonoBehaviour, IReset
    {
        private int _outroSceneStage;

        public GameObject panel1;
        public GameObject panel1Character;

        public GameObject panel2;
        public GameObject panel2Character;

        public GameObject panelText1;

        //Unity Functions
        //====================================================================================================================//
        
        private void Awake()
        {
            gameObject.SetActive(false);
        }


        // Update is called once per frame
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (_outroSceneStage == 0)
                {
                    _outroSceneStage++;
                    panelText1.SetActive(false);
                    panel1Character.SetActive(false);
                    panel2.SetActive(true);
                    panel2Character.SetActive(true);
                }
                else if (_outroSceneStage == 1)
                {
                    gameObject.SetActive(false);
                    panel1.SetActive(true);
                    panelText1.SetActive(true);
                    panel2.SetActive(false);
                    _outroSceneStage = 0;

                    panel1Character.SetActive(true);
                    panel2Character.SetActive(true);
                    
                    //TODO Need to open the Game Summary Screen
                    ShowFinalScreen();
                    //SceneLoader.ActivateScene(SceneLoader.UNIVERSE_MAP, SceneLoader.MAIN_MENU);
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
                    PlayerDataManager.ResetPlayerRunData();
                    PlayerDataManager.SavePlayerAccountData();
                    PlayerDataManager.ClearCurrentSaveFile();
                    SceneLoader.ActivateScene(SceneLoader.MAIN_MENU, SceneLoader.LEVEL);
                });
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
