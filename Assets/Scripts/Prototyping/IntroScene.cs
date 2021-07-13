using System;
using StarSalvager.Audio;
using StarSalvager.UI.Wreckyard.PatchTrees;
using StarSalvager.Utilities;
using StarSalvager.Utilities.Saving;
using StarSalvager.Utilities.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace StarSalvager.Prototype
{
    [Obsolete("This is exclusively for prototyping, and should not be part of the final product")]
    public class IntroScene : MonoBehaviour, IReset
    {
        private int _introSceneStage;

        public GameObject panel1;
        public GameObject panel1Character;

        public GameObject panel2;
        public GameObject panel2Character;

        public GameObject panelText1;

        [SerializeField]
        private Sprite[] tutorialSlides;

        [SerializeField]
        private Image tutorialSlideImage;


        //====================================================================================================================//
        private void OnEnable()
        {
            Utilities.Inputs.Input.Actions.MenuControls.Submit.performed += OnSubmitPressed;
            Utilities.Inputs.Input.Actions.MenuControls.Pause.performed += OnSkipPressed;
            Utilities.Inputs.Input.Actions.MenuControls.Cancel.performed += OnBackPressed;
        }

        private void Awake()
        {
            
            gameObject.SetActive(false);
            tutorialSlideImage.gameObject.SetActive(false);
        }
        private void OnDisable()
        {
            Utilities.Inputs.Input.Actions.MenuControls.Submit.performed -= OnSubmitPressed;
            Utilities.Inputs.Input.Actions.MenuControls.Pause.performed -= OnSkipPressed;
            Utilities.Inputs.Input.Actions.MenuControls.Cancel.performed -= OnBackPressed;
        }

        //IntroScene Functions
        //====================================================================================================================//

        public void Init()
        {
            if (PlayerDataManager.IntroCompleted())
            {
                Skip();
                return;
            }
            
            gameObject.SetActive(true);
        }
        
        //Intro Scene Functions
        //====================================================================================================================//

        private void OnSubmitPressed(InputAction.CallbackContext ctx)
        {
            if (!ctx.ReadValueAsButton()) return;
            
            NextStep();
        }
        private void OnSkipPressed(InputAction.CallbackContext ctx)
        {
            if (!ctx.ReadValueAsButton()) return;
            
            Skip();
        }

        private void OnBackPressed(InputAction.CallbackContext ctx)
        {
            if (!ctx.ReadValueAsButton()) return;

            ReturnStep();
        }

        private void ReturnStep()
        {
            switch (_introSceneStage)
            {
                case 0:
                    //do not increment the slides
                    return;
                case 1:
                    panel1.SetActive(true);
                    panelText1.SetActive(true);
                    panel1Character.SetActive(true);
                    panel2.SetActive(false);
                    panel2Character.SetActive(false);
                    break;
                case 2:
                    panel1.SetActive(false);
                    panelText1.SetActive(false);
                    panel2.SetActive(true);

                    //Skip();
                    tutorialSlideImage.gameObject.SetActive(false);
                    tutorialSlideImage.sprite = tutorialSlides[0];
                    break;
                default:
                    if (_introSceneStage - 1 >= tutorialSlides.Length)
                    {
                        Skip();
                        return;
                    }
                    tutorialSlideImage.sprite = tutorialSlides[_introSceneStage - 1];
                    break;
            }

            _introSceneStage--;
        }
        private void NextStep()
        {
            switch (_introSceneStage)
            {
                case 0:
                    panelText1.SetActive(false);
                    panel1Character.SetActive(false);
                    panel2.SetActive(true);
                    panel2Character.SetActive(true);
                    break;
                case 1:
                    panel1.SetActive(true);
                    panelText1.SetActive(true);
                    panel2.SetActive(false);

                    //Skip();
                    tutorialSlideImage.gameObject.SetActive(true);
                    tutorialSlideImage.sprite = tutorialSlides[0];
                    break;
                default:
                    if (_introSceneStage - 1 >= tutorialSlides.Length)
                    {
                        Skip();
                        return;
                    }
                    tutorialSlideImage.sprite = tutorialSlides[_introSceneStage - 1];
                    break;
            }
            
            _introSceneStage++;
        }

        private void Skip()
        {
            PlayerDataManager.SetIntroCompleted(true);
            _introSceneStage = 0;
            
            gameObject.SetActive(false);
            panel1Character.SetActive(true);
            panel1.SetActive(true);
            panel2.SetActive(false);

            tutorialSlideImage.gameObject.SetActive(false);
            
            ScreenFade.Fade(FadedCallback);
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
        
        private static void FadedCallback()
        {
            SceneLoader.ActivateScene(SceneLoader.WRECKYARD, SceneLoader.MAIN_MENU, MUSIC.SCRAPYARD);
            var patchTreeUI = FindObjectOfType<PatchTreeUI>();
            patchTreeUI.InitWreck("Base", null);
        }

        //====================================================================================================================//
        
    }
}
