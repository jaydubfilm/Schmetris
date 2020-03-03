using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class Scrapyard : MonoBehaviour
{
    //Prefabs used to build bot grid
    public GameObject botGrid;
    public GameObject botColumn;
    public GameObject botTile;
    public Transform botParent;
    GameObject botDisplay;

    //Bot zoom limits
    float currentSize = 20.0f;
    const float minSize = 10.0f;
    const float maxSize = 70.0f;
    const float sizeChange = 10.0f;

    //Scrapyard submenus
    public GameObject saveMenu;
    public GameObject loadMenu;
    public GameObject confirmPurchase;
    public GameObject failPurchase;

    //Saving and loading
    public Transform[] saveSlots;
    public Transform[] loadSlots;
    const string tileAtlasResource = "MasterDiceSprites";
    Sprite[] tilesAtlas;
    const float iconSize = 5.0f;
    const float iconPos = 100.0f;

    //Market
    public Text playerMoney;
    public Text transactionMoney;
    int transactionAmount = 0;

    //Init
    private void Start()
    {
        if (GameController.Instance.saveManager == null)
        {
            GameController.Instance.saveManager = new SaveManager();
            GameController.Instance.saveManager.Init();
        }
        tilesAtlas = Resources.LoadAll<Sprite>(tileAtlasResource);
    }

    //Update scrapyard UI on opening
    public void UpdateScrapyard()
    {
        transactionAmount = GameController.Instance.money;
        playerMoney.text = "Money: $" + GameController.Instance.money.ToString();
        transactionMoney.text = "After: $" + transactionAmount;
        CloseSubMenu();
        RefreshBotIcons();
        BuildBotGrid();
    }

    //Create editable bot grid
    public void BuildBotGrid()
    {
        //Destroy existing bot grid
        if (botParent.GetComponentInChildren<VerticalLayoutGroup>())
        {
            Destroy(botDisplay);
        }

        //Generate empty grid
        Sprite[,] botMap = GameController.Instance.bot.GetTileMap();
        botDisplay = Instantiate(botGrid, botParent);
        botDisplay.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        //Fill grid with existing bricks
        for (int y = botMap.GetLength(1) - 1; y >= 0; y--)
        {
            GameObject newColumn = Instantiate(botColumn, botDisplay.transform);
            for (int x = 0; x < botMap.GetLength(0); x++)
            {
                GameObject newTile = Instantiate(botTile, newColumn.transform);
                Image newTileImage = newTile.GetComponent<Image>();
                if (botMap[x, y])
                {
                    newTileImage.sprite = botMap[x, y];
                }
                else
                {
                    newTileImage.color = Color.clear;
                }
            }
        }
    }

    //Create bot icons for save files
    void RefreshBotIcons()
    {
        for (int i = 0; i < saveSlots.Length; i++)
        {
            BuildBotIcon(i, saveSlots[i]);
        }

        for (int i = 0; i < loadSlots.Length; i++)
        {
            BuildBotIcon(i, loadSlots[i]);
        }
    }

    //Create individual bot icon
    void BuildBotIcon(int index, Transform target)
    {
        //Remove existing bot icon
        if (target.GetComponentInChildren<VerticalLayoutGroup>())
        {
            Destroy(target.GetComponentInChildren<VerticalLayoutGroup>().gameObject);
        }

        //Load saved bot
        SaveData targetFile = GameController.Instance.saveManager.GetSave(index);
        if (targetFile != null && targetFile.game != "" && targetFile.bot.Length > 0)
        {
            //Position icon base
            GameObject newGrid = Instantiate(botGrid, target);
            RectTransform newGridTransform = newGrid.GetComponent<RectTransform>();
            newGridTransform.pivot = new Vector2(0, 0.5f);
            newGridTransform.sizeDelta = Vector2.one * iconSize;
            newGridTransform.anchoredPosition = new Vector2(iconPos, 0);

            int minX = -1;
            int maxX = -1;
            int minY = -1;
            int maxY = -1;

            //Determine the dimensions for a square bot icon
            for (int y = 0; y < targetFile.bot.Length; y++)
            {
                for (int x = 0; x < targetFile.bot[0].botRow.Length; x++)
                {
                    if (targetFile.bot[x].botRow[y] != "")
                    {
                        maxX = x;
                        maxY = y;
                        if (minX == -1)
                        {
                            minX = x;
                        }
                        if (minY == -1)
                        {
                            minY = y;
                        }
                    }
                }
            }

            minX = Mathf.Min(minX, minY);
            minY = minX;
            maxX = Mathf.Max(maxX, maxY);
            maxY = maxX;

            //Build bot icon
            if (minX > -1 && minY > -1)
            {
                for (int y = maxY; y >= minY; y--)
                {
                    GameObject newColumn = Instantiate(botColumn, newGrid.transform);
                    for (int x = minX; x <= maxX; x++)
                    {
                        GameObject newTile = Instantiate(botTile, newColumn.transform);
                        Image newTileImage = newTile.GetComponent<Image>();
                        if (targetFile.bot[x].botRow[y] != "")
                        {
                            newTileImage.sprite = tilesAtlas.Single<Sprite>(s => s.name == targetFile.bot[x].botRow[y]);
                        }
                        else
                        {
                            newTileImage.color = Color.clear;
                        }
                    }
                }
            }
        }
    }

    //Button for rotating bot 90 degrees clockwise
    public void RotateClockwise()
    {
        botParent.Rotate(Vector3.back, 90);
    }

    //Button for rotating bot 90 degrees counterclockwise
    public void RotateCounterclockwise()
    {
        botParent.Rotate(Vector3.back, -90);
    }

    //Button for zooming in on bot
    public void ZoomIn()
    {
        currentSize = Mathf.Min(currentSize + sizeChange, maxSize);
        botDisplay.GetComponent<RectTransform>().sizeDelta = Vector2.one * currentSize;
    }

    //Button for zooming out of bot
    public void ZoomOut()
    {
        currentSize = Mathf.Max(currentSize - sizeChange, minSize);
        botDisplay.GetComponent<RectTransform>().sizeDelta = Vector2.one * currentSize;
    }

    //Button for saving current bot as a blueprint
    public void SaveBlueprint()
    {

    }

    //Button for replacing bot with a loaded blueprint
    public void LoadBlueprint()
    {

    }

    //Button for opening save game menu
    public void SaveGameMenu()
    {
        saveMenu.SetActive(true);
    }

    //Button for opening load game menu
    public void LoadGameMenu()
    {
        loadMenu.SetActive(true);
    }

    //Button for confirming market purchases
    public void ConfirmPurchase()
    {
        if(transactionAmount >= 0)
        {
            confirmPurchase.SetActive(true);
        }
        else
        {
            failPurchase.SetActive(true);
        }
    }

    //Button for completing confirmed market purchases
    public void CompleteConfirmedPurchase()
    {
        GameController.Instance.money = transactionAmount;
        UpdateScrapyard();
    }

    //Button for closing the scrapyard and loading the next level
    public void NextLevel()
    {

    }

    //Button for quitting game
    public void QuitGame()
    {
        GameController.Instance.QuitGame();
    }

    //Buttons for saving game to a chosen slot
    public void SaveGame(int index)
    {
        GameController.Instance.SaveGame(index);
        RefreshBotIcons();
    }

    //Buttons for loading game from a chosen slot
    public void LoadGame(int index)
    {

    }

    //Button for closing a sub-menu and returning to the main scrapyard
    public void CloseSubMenu()
    {
        saveMenu.SetActive(false);
        loadMenu.SetActive(false);
        confirmPurchase.SetActive(false);
        failPurchase.SetActive(false);
    }
}
