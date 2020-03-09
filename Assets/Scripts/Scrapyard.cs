using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;
using System.Collections.Generic;

public class Scrapyard : MonoBehaviour
{
    //Prefabs used to build bot grid
    public GameObject botGrid;
    public GameObject botColumn;
    public GameObject botTile;
    public Transform botParent;
    GameObject botDisplay;
    List<GameObject> botBricks = new List<GameObject>();

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
    public GameObject confirmSell;

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
    public Transform marketParent;
    public List<string> marketList = new List<string>();
    public List<string> tempMarketList = new List<string>();
    List<GameObject> marketSelection = new List<GameObject>();

    //Brick moving
    GraphicRaycaster raycaster;
    GameObject selectedBrick = null;
    float holdingScreenTimer = 0;
    const float maxTapTimer = 0.15f;
    bool isMarketBrick = false;
    GameObject botBrick = null;
    bool canMove = true;

    //Init
    private void Start()
    {
        raycaster = GetComponent<GraphicRaycaster>();
        if (GameController.Instance.saveManager == null)
        {
            GameController.Instance.saveManager = new SaveManager();
            GameController.Instance.saveManager.Init();
        }
        tilesAtlas = Resources.LoadAll<Sprite>(tileAtlasResource);
        foreach(string MarketItem in marketList)
        {
            tempMarketList.Add(MarketItem);
        }
    }

    //Check for brick dragging
    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && canMove)
        {
            holdingScreenTimer = 0;
            PointerEventData pointer = new PointerEventData(EventSystem.current);
            pointer.position = Input.mousePosition;
            List<RaycastResult> targets = new List<RaycastResult>();
            raycaster.Raycast(pointer, targets);
            for (int i = 0; i < targets.Count; i++)
            {
                if (marketSelection.Contains(targets[i].gameObject))
                {
                    isMarketBrick = true;
                    selectedBrick = targets[i].gameObject;
                    UpdateBrickSnap();
                    break;
                }
                else if (botBricks.Contains(targets[i].gameObject) && targets[i].gameObject.GetComponent<Image>().color != Color.clear)
                {
                    isMarketBrick = false;
                    botBrick = targets[i].gameObject;
                    selectedBrick = Instantiate(botTile, transform.parent);
                    selectedBrick.GetComponent<Image>().sprite = botBrick.GetComponent<Image>().sprite;
                    botBrick.GetComponent<Image>().color = Color.clear;
                    UpdateBrickSnap();
                    break;
                }
            }
        }
        else if (Input.GetMouseButtonUp(0) && canMove)
        {
            if (holdingScreenTimer < maxTapTimer)
            {
                if (selectedBrick && !isMarketBrick)
                {
                    ConfirmSell(selectedBrick.GetComponent<Image>().sprite.name);
                }
            }
            else if (selectedBrick)
            {
                PointerEventData pointer = new PointerEventData(EventSystem.current);
                pointer.position = Input.mousePosition;
                List<RaycastResult> targets = new List<RaycastResult>();
                raycaster.Raycast(pointer, targets);
                for (int i = 0; i < targets.Count; i++)
                {
                    if (botBricks.Contains(targets[i].gameObject) && targets[i].gameObject.GetComponent<Image>().color == Color.clear)
                    {
                        targets[i].gameObject.GetComponent<Image>().color = Color.white;
                        targets[i].gameObject.GetComponent<Image>().sprite = selectedBrick.GetComponent<Image>().sprite;
                        if(isMarketBrick)
                        {
                            tempMarketList.Remove(selectedBrick.GetComponent<Image>().sprite.name);
                        }
                        else
                        {
                            botBrick = null;
                        }
                        break;
                    }
                }
                Destroy(selectedBrick);
                BuildMarketplace();
                if(botBrick)
                {
                    botBrick.GetComponent<Image>().color = Color.white;
                }
            }
            holdingScreenTimer = 0;
            selectedBrick = null;
            botBrick = null;
        }
        else if (Input.GetMouseButton(0) && canMove)
        {
            holdingScreenTimer += Time.unscaledDeltaTime;
            if (holdingScreenTimer >= maxTapTimer)
            {
                UpdateBrickSnap();
            }
        }
    }

    //Snap brick to block closest to player drag position
    void UpdateBrickSnap()
    {
        if(selectedBrick)
        {
            selectedBrick.transform.parent = transform.parent;
            selectedBrick.GetComponent<RectTransform>().anchorMin = Vector2.zero;
            selectedBrick.GetComponent<RectTransform>().anchorMax = Vector2.zero;
            selectedBrick.GetComponent<RectTransform>().sizeDelta = Vector2.one * currentSize;
            selectedBrick.GetComponent<RectTransform>().anchoredPosition = Input.mousePosition;
        }
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
        BuildMarketplace();
    }

    //Add available tiles to market
    void BuildMarketplace()
    {
        //Remove existing marketplace items
        Image[] buttons = marketParent.GetComponentsInChildren<Image>();
        for(int i = 0;i<buttons.Length;i++)
        {
            Destroy(buttons[i].gameObject);
        }
        marketSelection = new List<GameObject>();

        //Add new items to marketplace
        for (int i = 0;i<tempMarketList.Count;i++)
        {
            GameObject newTile = Instantiate(botTile, marketParent.transform);
            Image newTileImage = newTile.GetComponent<Image>();
            newTileImage.sprite = tilesAtlas.Single<Sprite>(s => s.name == tempMarketList[i]);
            marketSelection.Add(newTile);
        }
    }

    //Update player's bot from editable bot grid
    void UpdateGameplayBot()
    {
        Sprite[,] botMap = GameController.Instance.bot.GetTileMap();
        for (int x = 0; x < botMap.GetLength(0); x++)
        {
            for (int y = 0; y < botMap.GetLength(1); y++)
            {
                if (botBricks[x + y * botMap.GetLength(1)].GetComponent<Image>().color != Color.clear)
                    botMap[x, y] = botBricks[x + y * botMap.GetLength(1)].GetComponent<Image>().sprite;
                else
                    botMap[x, y] = null;
            }
        }
        GameController.Instance.bot.SetTileMap(botMap);
    }

    //Create editable bot grid
    public void BuildBotGrid()
    {
        //Destroy existing bot grid
        if (botParent.GetComponentInChildren<VerticalLayoutGroup>())
        {
            Destroy(botDisplay);
        }
        botBricks = new List<GameObject>();

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
                botBricks.Add(newTile);
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

    //Button for saving current bot as a layout
    public void SaveLayout()
    {

    }

    //Button for replacing bot with a loaded layout
    public void LoadLayout()
    {

    }

    //Button for opening save game menu
    public void SaveGameMenu()
    {
        canMove = false;
        saveMenu.SetActive(true);
    }

    //Button for opening load game menu
    public void LoadGameMenu()
    {
        canMove = false;
        loadMenu.SetActive(true);
    }

    //Button for confirming sold bricks
    public void ConfirmSell(string brick)
    {
        canMove = false;
        confirmSell.SetActive(true);
    }

    //Button for selling confirmed bricks
    public void CompleteConfirmedSell(string brick)
    {
        canMove = true;
        UpdateScrapyard();
    }

    //Button for confirming market purchases
    public void ConfirmPurchase()
    {
        canMove = false;
        if (transactionAmount >= 0)
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
        canMove = true;
        GameController.Instance.money = transactionAmount;
        UpdateGameplayBot();
        UpdateScrapyard();
    }

    //Button for closing the scrapyard and loading the next level
    public void NextLevel()
    {
        GameController.Instance.LoadNewLevel();
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
        canMove = true;
        saveMenu.SetActive(false);
        loadMenu.SetActive(false);
        confirmPurchase.SetActive(false);
        failPurchase.SetActive(false);
        confirmSell.SetActive(false);
    }
}
