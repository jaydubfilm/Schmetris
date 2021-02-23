using StarSalvager.Audio;
using StarSalvager.Utilities;
using StarSalvager.Utilities.SceneManagement;
using UnityEngine;

namespace StarSalvager.Prototype
{
    [System.Obsolete("This is exclusively for prototyping, and should not be part of the final product")]
    public class IntroScene : MonoBehaviour, IReset
    {
        private int introSceneStage = 0;

        /*public GameObject mainMenuWindow;
    public GameObject menuCharacters;*/

        public GameObject panel1;
        public GameObject panel1Character;

        public GameObject panel2;
        public GameObject panel2Character;

        public GameObject panelText1;

        private void Awake()
        {
            gameObject.SetActive(false);
        }

        public void Activate()
        {
            gameObject.SetActive(false);
        }

        public void Reset()
        {
            gameObject.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (introSceneStage == 0)
                {
                    introSceneStage++;
                    panelText1.SetActive(false);
                    panel1Character.SetActive(false);
                    panel2.SetActive(true);
                    panel2Character.SetActive(true);
                }
                else if (introSceneStage == 1)
                {
                    /*mainMenuWindow.SetActive(true);
                menuCharacters.SetActive(true);*/
                    gameObject.SetActive(false);
                    panel1.SetActive(true);
                    panelText1.SetActive(true);
                    panel2.SetActive(false);
                    introSceneStage = 0;

                    panel1Character.SetActive(true);
                    panel2Character.SetActive(true);
                    
                    
                    ScreenFade.Fade(() =>
                    {
                        SceneLoader.ActivateScene(SceneLoader.SCRAPYARD, SceneLoader.MAIN_MENU);
                    });
                }
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                /*mainMenuWindow.SetActive(true);
            menuCharacters.SetActive(true);*/
                gameObject.SetActive(false);
                panel1.SetActive(true);
                panel2.SetActive(false);
                introSceneStage = 0;
                ScreenFade.Fade(() =>
                {
                    SceneLoader.ActivateScene(SceneLoader.SCRAPYARD, SceneLoader.MAIN_MENU);
                });
            }
        }
    }
}
