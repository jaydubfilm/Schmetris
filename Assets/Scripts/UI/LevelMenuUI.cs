using UnityEngine;

//Controls all map menu functions
public class LevelMenuUI : MonoBehaviour
{
    //Sub-menus
    public GameObject mainPanel;
    public GameObject helpPanel;
    public GameObject loadPanel;
    public GameObject savePanel;
    public GameObject confirmQuitPanel;

    //Menu state for determining active controls
    enum MenuState
    {
        None,
        Main,
        Help,
        Load,
        Save,
        QuitGame
    }
    MenuState activeState = MenuState.None;

    //Open map menu
    public void OpenMenu()
    {
        //Close all sub-menus
        helpPanel.SetActive(false);
        confirmQuitPanel.SetActive(false);
        loadPanel.SetActive(false);
        savePanel.SetActive(false);

        //Activate main menu
        MainMenu();
    }

    //Update keyboard controls
    private void Update()
    {
        switch (activeState)
        {
            case MenuState.Main:
                if(Input.GetKeyDown(KeyCode.Alpha1))
                {
                    SaveMenu();
                }
                else if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    LoadMenu();
                }
                else if (Input.GetKeyDown(KeyCode.Alpha3))
                {
                    HelpPanel();
                }
                else if (Input.GetKeyDown(KeyCode.Alpha4))
                {
                    Scrapyard();
                }
                else if (Input.GetKeyDown(KeyCode.Alpha5))
                {
                    QuitGameMenu();
                }
                break;
            case MenuState.Help:
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    MainMenu();
                }
                break;
            case MenuState.Load:
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
            case MenuState.Save:
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    SaveGameSlot(0);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    SaveGameSlot(1);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha3))
                {
                    SaveGameSlot(2);
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
            case MenuState.Help:
                helpPanel.SetActive(false);
                break;
            case MenuState.Load:
                loadPanel.SetActive(false);
                break;
            case MenuState.Save:
                savePanel.SetActive(false);
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
            case MenuState.Help:
                helpPanel.SetActive(true);
                break;
            case MenuState.Load:
                loadPanel.SetActive(true);
                break;
            case MenuState.Save:
                savePanel.SetActive(true);
                break;
            case MenuState.QuitGame:
                confirmQuitPanel.SetActive(true);
                break;
        }
    }

    //Open main map menu
    public void MainMenu()
    {
        SetMenuState(MenuState.Main);
    }

    //Open help screen
    public void HelpPanel()
    {
        SetMenuState(MenuState.Help);
    }

    //Open save menu
    public void SaveMenu()
    {
        SetMenuState(MenuState.Save);
    }

    //Open load menu
    public void LoadMenu()
    {
        SetMenuState(MenuState.Load);
    }

    //Open quit confirmation
    public void QuitGameMenu()
    {
        SetMenuState(MenuState.QuitGame);
    }

    //Exit game and return to start menu
    public void QuitGame()
    {
        GameController.Instance.StartMenu();
    }

    //Open the scrapyard
    public void Scrapyard()
    {
    }

    //Load an existing game slot
    public void LoadGameSlot(int index)
    {
        GameController.Instance.LoadGame(index);
    }

    //Save to an existing game slot
    public void SaveGameSlot(int index)
    {
    }
}
