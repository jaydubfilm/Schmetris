using StarSalvager.Utilities;
using StarSalvager.Utilities.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class IntroScene : MonoBehaviour, IReset
{
    private int introSceneStage = 0;

    /*public GameObject mainMenuWindow;
    public GameObject menuCharacters;*/

    public GameObject panel1;
    public GameObject panelText1;
    public GameObject panel2;

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
                panel2.SetActive(true);
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
                SceneLoader.ActivateScene(SceneLoader.UNIVERSE_MAP, SceneLoader.MAIN_MENU);
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
            SceneLoader.ActivateScene(SceneLoader.UNIVERSE_MAP, SceneLoader.MAIN_MENU);
        }
    }
}
