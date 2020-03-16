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
    float currentSize = 40.0f;
    const float minSize = 10.0f;
    const float maxSize = 70.0f;
    const float sizeChange = 10.0f;

    //Scrapyard submenus
    public GameObject saveMenu;
    public GameObject loadMenu;
    public GameObject saveLayoutMenu;
    public GameObject loadLayoutMenu;
    public GameObject confirmPurchase;
    public GameObject failPurchase;
    public GameObject confirmSell;
    public GameObject confirmLevel;

    //Saving and loading
    public Transform[] saveSlots;
    public Transform[] loadSlots;
    public Transform[] saveLayoutSlots;
    public Transform[] loadLayoutSlots;
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
    List<int> marketPrices = new List<int>();
    public GameObject pricePrefab;

    //Brick moving
    GraphicRaycaster raycaster;
    GameObject selectedBrick = null;
    float holdingScreenTimer = 0;
    const float maxTapTimer = 0.15f;
    bool isMarketBrick = false;
    GameObject botBrick = null;
    GameObject sellBrick = null;
    bool canMove = true;
    bool isTranslating = false;
    GameObject coreBrick = null;
    const float botBounds = 100.0f;
    Vector3 prevMousePos = Vector3.zero;

    //Resource
    public RectTransform fuelBar;
    float maxFuelWidth = 0;
    float currentFuel = 0;
    float currentFuelMax = 0;
    public RectTransform blueBar;
    float maxBlueWidth = 0;
    float currentBlue = 0;
    float currentBlueMax = 0;
    public RectTransform greenBar;
    float maxGreenWidth = 0;
    float currentGreen = 0;
    float currentGreenMax = 0;
    public RectTransform yellowBar;
    float maxYellowWidth = 0;
    float currentYellow = 0;
    float currentYellowMax = 0;
    public RectTransform greyBar;
    float maxGreyWidth = 0;
    float currentGrey = 0;
    float currentGreyMax = 0;
    bool hasResources = false;

    //Init
    void Init()
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
        maxFuelWidth = fuelBar.sizeDelta.x;
        maxBlueWidth = blueBar.sizeDelta.x;
        maxGreenWidth = greenBar.sizeDelta.x;
        maxYellowWidth = yellowBar.sizeDelta.x;
        maxGreyWidth = greyBar.sizeDelta.x;
        hasResources = true;
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
                    isTranslating = false;
                    break;
                }
                else if (botBricks.Contains(targets[i].gameObject) && targets[i].gameObject.GetComponent<Image>().color != Color.clear)
                {
                    isMarketBrick = false;
                    botBrick = targets[i].gameObject;
                    isTranslating = botBrick == coreBrick;
                    prevMousePos = Input.mousePosition;
                    break;
                }
            }
        }
        else if (Input.GetMouseButtonUp(0) && canMove)
        {
            if (holdingScreenTimer < maxTapTimer)
            {
                if (botBrick && !isMarketBrick)
                {
                    if (!isTranslating)
                    {
                        sellBrick = botBrick;
                        botBrick = null;
                        ConfirmSell();
                    }
                    else
                    {
                        botBrick = null;
                    }
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
                            //~For now, don't remove purchased bricks from market
                            //tempMarketList.Remove(selectedBrick.GetComponent<Image>().sprite.name);
                            transactionAmount -= marketPrices[tempMarketList.IndexOf(selectedBrick.GetComponent<Image>().sprite.name)];
                            UpdateUI();
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
                if(botBrick && !selectedBrick)
                {
                    if (!isTranslating)
                    {
                        selectedBrick = Instantiate(botTile, transform.parent);
                        selectedBrick.GetComponent<Image>().sprite = botBrick.GetComponent<Image>().sprite;
                        botBrick.GetComponent<Image>().color = Color.clear;
                    }
                    else
                    {
                        Vector2 newBotPos = botDisplay.GetComponent<RectTransform>().anchoredPosition + new Vector2(Input.mousePosition.x - prevMousePos.x, Input.mousePosition.y - prevMousePos.y);
                        newBotPos.x = Mathf.Clamp(newBotPos.x, -botBounds, botBounds);
                        newBotPos.y = Mathf.Clamp(newBotPos.y, -botBounds, botBounds);
                        botDisplay.GetComponent<RectTransform>().anchoredPosition = newBotPos;
                        prevMousePos = Input.mousePosition;
                    }
                }
                UpdateBrickSnap();
            }
        }

        //Keyboard controls
        if(Input.GetKeyDown(KeyCode.Equals))
        {
            ZoomIn();
        }
        else if (Input.GetKeyDown(KeyCode.Minus))
        {
            ZoomOut();
        }
    }

    //Snap brick to block closest to player drag position
    void UpdateBrickSnap()
    {
        if(selectedBrick)
        {
            if(selectedBrick.GetComponentInChildren<Text>())
                selectedBrick.GetComponentInChildren<Text>().enabled = false;
            selectedBrick.transform.SetParent(transform.parent);
            selectedBrick.GetComponent<RectTransform>().anchorMin = Vector2.zero;
            selectedBrick.GetComponent<RectTransform>().anchorMax = Vector2.zero;
            selectedBrick.GetComponent<RectTransform>().sizeDelta = Vector2.one * currentSize;
            selectedBrick.GetComponent<RectTransform>().anchoredPosition = Input.mousePosition;
        }
    }
    
    //Add player stored resources to scrapyard upon opening
    public void LoadScrapyardResources()
    {
        currentFuel = GameController.Instance.bot.GetStoredResource(ResourceType.Red);
        currentFuelMax = GameController.Instance.bot.GetResourceCapacity(ResourceType.Red);
        currentBlue = GameController.Instance.bot.GetStoredResource(ResourceType.Blue);
        currentBlueMax = GameController.Instance.bot.GetResourceCapacity(ResourceType.Blue);
        currentYellow = GameController.Instance.bot.GetStoredResource(ResourceType.Yellow);
        currentYellowMax = GameController.Instance.bot.GetResourceCapacity(ResourceType.Yellow);
        currentGreen = GameController.Instance.bot.GetStoredResource(ResourceType.Green);
        currentGreenMax = GameController.Instance.bot.GetResourceCapacity(ResourceType.Green);
        currentGrey = GameController.Instance.bot.GetStoredResource(ResourceType.Grey);
        currentGreyMax = GameController.Instance.bot.GetResourceCapacity(ResourceType.Grey);
    }

    //Update money UI
    void UpdateUI()
    {
        playerMoney.text = "Money: $" + GameController.Instance.money.ToString();
        transactionMoney.text = "After: $" + transactionAmount;
    }

    //Update scrapyard UI on opening
    public void UpdateScrapyard()
    {
        if(!hasResources)
        {
            Init();
        }

        transactionAmount = GameController.Instance.money;
        UpdateUI();

        Vector2 blueSize = blueBar.sizeDelta;
        blueSize.x = maxBlueWidth * (currentBlueMax > 0 ? currentBlue / currentBlueMax : 0);
        blueBar.sizeDelta = blueSize;

        Vector2 yellowSize = yellowBar.sizeDelta;
        yellowSize.x = maxYellowWidth * (currentYellowMax > 0 ? currentYellow / currentYellowMax : 0);
        yellowBar.sizeDelta = yellowSize;

        Vector2 greenSize = greenBar.sizeDelta;
        greenSize.x = maxGreenWidth * (currentGreenMax > 0 ? currentGreen / currentGreenMax : 0);
        greenBar.sizeDelta = greenSize;

        Vector2 greySize = greyBar.sizeDelta;
        greySize.x = maxGreyWidth * (currentGreyMax > 0 ? currentGrey / currentGreyMax : 0);
        greyBar.sizeDelta = greySize;

        Vector2 fuelSize = fuelBar.sizeDelta;
        fuelSize.x = maxFuelWidth * (currentFuelMax > 0 ? currentFuel / currentFuelMax : 0);
        fuelBar.sizeDelta = fuelSize;

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

            int price = 20;
            GameObject newPrice = Instantiate(pricePrefab, newTile.transform);
            newPrice.GetComponent<Text>().text = "$" + price;
            marketPrices.Add(price);
        }
    }

    //Update player's bot from editable bot grid
    void UpdateGameplayBot()
    {
        //Update bot map
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

        //Update bot resources
        GameController.Instance.bot.SetStoredResource(ResourceType.Red, currentFuel);
        GameController.Instance.bot.SetStoredResource(ResourceType.Blue, currentBlue);
        GameController.Instance.bot.SetStoredResource(ResourceType.Yellow, currentYellow);
        GameController.Instance.bot.SetStoredResource(ResourceType.Green, currentGreen);
        GameController.Instance.bot.SetStoredResource(ResourceType.Grey, currentGrey);
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
        botDisplay.GetComponent<RectTransform>().sizeDelta = Vector2.one * currentSize;

        //Fill grid with existing bricks
        for (int y = 0; y < botMap.GetLength(1); y++)
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
                if (x == 6 && y == 6)
                    coreBrick = newTile;
                botBricks.Add(newTile);
            }
        }
    }

    //Create bot icons for save files
    void RefreshBotIcons()
    {
        for (int i = 0; i < saveSlots.Length; i++)
        {
            BuildBotIcon(i, saveSlots[i], false);
        }

        for (int i = 0; i < loadSlots.Length; i++)
        {
            BuildBotIcon(i, loadSlots[i], false);
        }

        for (int i = 0; i < saveLayoutSlots.Length; i++)
        {
            BuildBotIcon(i, saveLayoutSlots[i], true);
        }

        for (int i = 0; i < loadLayoutSlots.Length; i++)
        {
            BuildBotIcon(i, loadLayoutSlots[i], true);
        }
    }

    //Create individual bot icon
    void BuildBotIcon(int index, Transform target, bool isLayout)
    {
        //Remove existing bot icon
        if (target.GetComponentInChildren<VerticalLayoutGroup>())
        {
            Destroy(target.GetComponentInChildren<VerticalLayoutGroup>().gameObject);
        }

        //Load saved bot
        SaveData targetFile = isLayout ? GameController.Instance.saveManager.GetLayout(index) : GameController.Instance.saveManager.GetSave(index);
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
                for (int y = minX; y <= maxY; y++)
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
    public void SaveLayoutMenu()
    {
        canMove = false;
        saveLayoutMenu.SetActive(true);
    }

    //Button for replacing bot with a loaded layout
    public void LoadLayoutMenu()
    {
        canMove = false;
        loadLayoutMenu.SetActive(true);
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
    public void ConfirmSell()
    {
        canMove = false;
        confirmSell.SetActive(true);
    }

    //Button for selling confirmed bricks
    public void CompleteConfirmedSell()
    {
        canMove = true;
        sellBrick.GetComponent<Image>().color = Color.clear;
        sellBrick = null;
        UpdateGameplayBot();
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
        canMove = false;
        if (transactionAmount >= 0)
        {
            confirmLevel.SetActive(true);
        }
        else
        {
            failPurchase.SetActive(true);
        }
    }

    //Button for confirm move to next level
    public void ConfirmNextLevel()
    {
        CompleteConfirmedPurchase();
        GameController.Instance.LoadNewLevel();
    }

    //Button for return bot to state of confirmed changes
    public void ResetChanges()
    {
        UpdateScrapyard();
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
        CloseSubMenu();
    }

    //Buttons for loading game from a chosen slot
    public void LoadGame(int index)
    {
        GameController.Instance.LoadGame(index);
    }

    //Buttons for saving layout to a chosen slot
    public void SaveLayout(int index)
    {
        GameController.Instance.SaveLayout(index);
        RefreshBotIcons();
        CloseSubMenu();
    }

    //Buttons for loading layout from a chosen slot
    public void LoadLayout(int index)
    {
        GameController.Instance.LoadLayout(index);
    }

    //Button for closing a sub-menu and returning to the main scrapyard
    public void CloseSubMenu()
    {
        canMove = true;
        saveMenu.SetActive(false);
        loadMenu.SetActive(false);
        saveLayoutMenu.SetActive(false);
        loadLayoutMenu.SetActive(false);
        confirmPurchase.SetActive(false);
        failPurchase.SetActive(false);
        confirmSell.SetActive(false);
        confirmLevel.SetActive(false);
    }

    //Button for buying resources
    public void BuyResource(string resource)
    {
        switch (resource)
        {
            case "RED":
                currentFuel = Mathf.Min(currentFuelMax, currentFuel + 10);
                break;
            case "BLUE":
                currentBlue = Mathf.Min(currentBlueMax, currentBlue + 10);
                break;
            case "YELLOW":
                currentYellow = Mathf.Min(currentYellowMax, currentYellow + 10);
                break;
            case "GREEN":
                currentGreen = Mathf.Min(currentGreenMax, currentGreen + 10);
                break;
            case "GREY":
                currentGrey = Mathf.Min(currentGreyMax, currentGrey + 10);
                break;
        }
        UpdateGameplayBot();
        UpdateScrapyard();
    }

    //Button for selling resources
    public void SellResource(string resource)
    {
        switch (resource)
        {
            case "RED":
                currentFuel = Mathf.Max(0, currentFuel - 10);
                break;
            case "BLUE":
                currentBlue = Mathf.Max(0, currentBlue - 10);
                break;
            case "YELLOW":
                currentYellow = Mathf.Max(0, currentYellow - 10);
                break;
            case "GREEN":
                currentGreen = Mathf.Max(0, currentGreen - 10);
                break;
            case "GREY":
                currentGrey = Mathf.Max(0, currentGrey - 10);
                break;
        }
        UpdateGameplayBot();
        UpdateScrapyard();
    }
}
