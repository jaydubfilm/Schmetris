using UnityEngine;

//Controls all main menu functions
public class MainMenuUI : MonoBehaviour
{
    //Sub-menus
    public GameObject mainPanel;
    public GameObject newGamePanel;
    public GameObject loadGamePanel;
    public GameObject confirmQuitPanel;

    //Menu state for determining active controls
    enum MenuState
    {
        None,
        Main,
        NewGame,
        LoadGame,
        QuitGame
    }
    MenuState activeState = MenuState.None;

    //Open menu
    public void OpenMenu()
    {
        //Close all sub-menus
        newGamePanel.SetActive(false);
        loadGamePanel.SetActive(false);
        confirmQuitPanel.SetActive(false);

        //Activate main menu
        MainMenu();
    }

    //Update keyboard controls
    private void Update()
    {
        switch (activeState)
        {
            case MenuState.Main:
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    NewGameMenu();
                }
                else if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    LoadGameMenu();
                }
                else if (Input.GetKeyDown(KeyCode.Alpha3))
                {
                    QuitGameMenu();
                }
                break;
            case MenuState.NewGame:
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    NewEasyGame();
                }
                else if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    NewMediumGame();
                }
                /*else if (Input.GetKeyDown(KeyCode.Alpha3))
                {
                    NewHardGame();
                }*/
                else if (Input.GetKeyDown(KeyCode.Alpha3))
                {
                    MainMenu();
                }
                break;
            case MenuState.LoadGame:
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    LoadGameSlot(0);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    LoadGameSlot(1);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha3))
                {
                    LoadGameSlot(2);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha4))
                {
                    MainMenu();
                }
                break;
            case MenuState.QuitGame:
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    QuitGame();
                }
                else if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    MainMenu();
                }
                break;
        }
    }

    //Change active menu state
    void SetMenuState(MenuState newState)
    {
        //State has not changed - no steps required
        if (activeState == newState)
            return;

        //Transition out of old menu state
        switch (activeState)
        {
            case MenuState.Main:
                mainPanel.SetActive(false);
                break;
            case MenuState.LoadGame:
                loadGamePanel.SetActive(false);
                break;
            case MenuState.NewGame:
                newGamePanel.SetActive(false);
                break;
            case MenuState.QuitGame:
                confirmQuitPanel.SetActive(false);
                break;
        }

        //Transition into new menu state
        activeState = newState;
        switch (activeState)
        {
            case MenuState.Main:
                mainPanel.SetActive(true);
                break;
            case MenuState.LoadGame:
                loadGamePanel.SetActive(true);
                break;
            case MenuState.NewGame:
                newGamePanel.SetActive(true);
                break;
            case MenuState.QuitGame:
                confirmQuitPanel.SetActive(true);
                break;
        }
    }

    //Open main menu
    public void MainMenu()
    {
        SetMenuState(MenuState.Main);
    }

    //Open new game menu
    public void NewGameMenu()
    {
        SetMenuState(MenuState.NewGame);
    }

    //Open load game menu
    public void LoadGameMenu()
    {
        SetMenuState(MenuState.LoadGame);
    }

    //Open quit confirmation
    public void QuitGameMenu()
    {
        SetMenuState(MenuState.QuitGame);
    }

    //Start an easy game
    public void NewEasyGame()
    {
        GameController.Instance.EasyGame();
    }

    //Start a medium game
    public void NewMediumGame()
    {
        GameController.Instance.MediumGame();
    }

    //Start a difficult game
    public void NewHardGame()
    {
        GameController.Instance.HardGame();
    }

    //Load an existing game slot
    public void LoadGameSlot(int index)
    {
        GameController.Instance.LoadGame(index);
    }

    //Quit application
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}
