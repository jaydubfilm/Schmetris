using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

//Controls all map menu functions
public class LevelMenuUI : MonoBehaviour
{
    //Sub-menus
    public GameObject mainPanel;
    public GameObject helpPanel;
    public GameObject loadPanel;
    public GameObject savePanel;
    public GameObject confirmQuitPanel;

    //Levels UI
    public GameObject levelButtonPrefab;
    public HorizontalLayoutGroup levelGrid;
    List<GameObject> levelButtons = new List<GameObject>();

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

    //Update active levels
    void UpdateLevels()
    {
        //Remove old levels
        for(int i = 0;i<levelButtons.Count;i++)
        {
            Destroy(levelButtons[i]);
        }
        levelButtons = new List<GameObject>();

        //Add new level buttons
        for (int i = 0; i < GameController.Instance.game.levelDataArr.Length; i++)
        {
            GameObject newLevel = Instantiate(levelButtonPrefab, levelGrid.transform);
            newLevel.GetComponent<Text>().text = "Level " + (i + 1).ToString();
            if(i < GameController.Instance.highestScene)
            {
                int index = i + 1;
                newLevel.GetComponent<Button>().onClick.AddListener(()=> { PlayLevel(index); });
            }
            else
            {
                newLevel.GetComponent<Button>().interactable = false;
            }
            levelButtons.Add(newLevel);
        }

        //Update grid positioning to center
        if(levelButtons.Count > 0)
        {
            RectOffset newPadding = levelGrid.padding;
            newPadding.left = (int)(-levelButtons[0].GetComponent<RectTransform>().sizeDelta.x * levelButtons.Count / 2.0f);
            levelGrid.padding = newPadding;
        }
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
                UpdateLevels();
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
        GameController.Instance.LoadScrapyard();
    }

    //Load an existing game slot
    public void LoadGameSlot(int index)
    {
        GameController.Instance.LoadGame(index);
    }

    //Save to an existing game slot
    public void SaveGameSlot(int index)
    {
        GameController.Instance.SaveGame(index);
    }

    //Play selected level
    public void PlayLevel(int index)
    {
        GameController.Instance.StartLevel(index);
    }
}
