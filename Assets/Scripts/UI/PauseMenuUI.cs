using UnityEngine;

//Controls all pause menu functions
public class PauseMenuUI : MonoBehaviour
{
    //Sub-menus
    public GameObject mainPanel;
    public GameObject helpPanel;
    public GameObject confirmQuitPanel;

    //Menu state for determining active controls
    enum MenuState
    {
        None,
        Main,
        Help,
        QuitGame
    }
    MenuState activeState = MenuState.None;

    //Open pause menu
    public void OpenMenu()
    {
        //Close all sub-menus
        helpPanel.SetActive(false);
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
                    ResumeGame();
                }
                else if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    HelpPanel();
                }
                else if (Input.GetKeyDown(KeyCode.Alpha3))
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
            case MenuState.QuitGame:
                confirmQuitPanel.SetActive(true);
                break;
        }
    }

    //Open main pause menu
    public void MainMenu()
    {
        SetMenuState(MenuState.Main);
    }

    //Open help screen
    public void HelpPanel()
    {
        SetMenuState(MenuState.Help);
    }

    //Open quit confirmation
    public void QuitGameMenu()
    {
        SetMenuState(MenuState.QuitGame);
    }

    //Close pause menu and return to gameplay
    public void ResumeGame()
    {
        GameController.Instance.ResumeGame();
    }

    //Exit game and return to start menu
    public void QuitGame()
    {
        GameController.Instance.StartMenu();
    }
}
