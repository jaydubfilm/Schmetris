using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

//Controls all map menu functions
public class LevelMenuUI : MonoBehaviour
{
    //Sub-menus
    public GameObject mainPanel;
    public GameObject helpPanel;
    public GameObject loadPanel;
    public GameObject savePanel;
    public GameObject savedPanel;
    public GameObject confirmQuitPanel;
    public GameObject confirmFuelPanel;

    //Levels UI
    public GameObject levelButtonPrefab;
    public HorizontalLayoutGroup levelGrid;
    List<GameObject> levelButtons = new List<GameObject>();

    //Pre-level fuel check
    public List<Sprite> fuelSprites = new List<Sprite>();
    int tempLevelIndex = 0;

    //Menu state for determining active controls
    enum MenuState
    {
        None,
        Main,
        Help,
        Load,
        Save,
        QuitGame,
        ConfirmLevel
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
        savedPanel.SetActive(false);
        confirmFuelPanel.SetActive(false);

        //Activate main menu
        UpdateLevels();
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
            case MenuState.ConfirmLevel:
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    ConfirmPlayLevel();
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
            case MenuState.ConfirmLevel:
                confirmFuelPanel.SetActive(false);
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
            case MenuState.ConfirmLevel:
                confirmFuelPanel.SetActive(true);
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
        StartCoroutine(SavedGameUI(index));
    }

    //Play selected level
    public void PlayLevel(int index)
    {
        bool hasFuel = GameController.Instance.bot.totalReddite > 0;

        //Look for fuel bricks in bot map
        Sprite[,] botMap = GameController.Instance.bot.GetTileMap();
        for (int x = 0; x < botMap.GetLength(0); x++)
        {
            for (int y = 0; y < botMap.GetLength(1); y++)
            {
                if(fuelSprites.Contains(botMap[x,y]))
                {
                    hasFuel = true;
                    break;
                }
            }
        }

        if (!hasFuel)
        {
            tempLevelIndex = index;
            SetMenuState(MenuState.ConfirmLevel);
        }
        else
        {
           
            GameController.Instance.StartLevel(index);
        }
    }

    //Play selected level ignoring fuel popup
    public void ConfirmPlayLevel()
    {
        GameController.Instance.StartLevel(tempLevelIndex);
    }

    //Game saved indicator
    IEnumerator SavedGameUI(int index)
    {
        savedPanel.SetActive(true);
        yield return 0;
        GameController.Instance.SaveGame(index);
        yield return new WaitForSecondsRealtime(0.25f);
        savedPanel.SetActive(false);
        SetMenuState(MenuState.Main);
    }
}
