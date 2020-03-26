using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;
using System.Collections.Generic;

//Bot buidling/purchasing area
public class Scrapyard : MonoBehaviour
{
    //Sub-menus
    public GameObject saveLayoutMenu;
    public GameObject loadLayoutMenu;
    public GameObject confirmPurchase;
    public GameObject failPurchase;
    public GameObject confirmSell;
    public GameObject confirmLevel;
    public GameObject helpPanel;
    public GameObject confirmMap;
    public GameObject confirmUpgrade;
    public GameObject confirmConvert;
    public GameObject brickOptions;
    public Text sellText;
    public Text convertText;
    public Text upgradeText;
    public GameObject brickOptionsPrefab;
    List<GameObject> brickOptionButtons = new List<GameObject>();
    public Transform brickOptionsGrid;

    //Resources UI
    public Text playerMoney;
    public Text transactionMoney;
    float maxBarWidth = 0;
    public RectTransform fuelBar;
    public RectTransform blueBar;
    public RectTransform greenBar;
    public RectTransform yellowBar;
    public RectTransform greyBar;
    public Text redAmount;
    public Text blueAmount;
    public Text yellowAmount;
    public Text greenAmount;
    public Text greyAmount;
    public Text redBurnRate;
    public Text blueBurnRate;
    public Text yellowBurnRate;
    public Text greenBurnRate;
    public Text greyBurnRate;

    //Bot display UI
    public GameObject botGrid;
    public GameObject botColumn;
    public GameObject botTile;
    public Transform botParent;
    const string tileAtlasResource = "MasterDiceSprites";
    const string craftingAtlasResource = "PartSprites";
    Sprite[] tilesAtlas;
    GameObject botDisplay;

    //Market UI
    public Transform marketParent;
    public GameObject pricePrefab;
    List<GameObject> marketSelection = new List<GameObject>();
    public List<string> tempMarketList = new List<string>();
    List<int> marketPrices = new List<int>();
    public List<string> marketList = new List<string>();
    int maxMarketItems = 5;
    int marketIndex = 0;

    //Crafting components
    public GameObject[] containerParts;
    public GameObject[] convertableParts;
    public GameObject[] craftableParts;

    //Bot zoom controls
    float currentSize = 40.0f;
    const float minSize = 10.0f;
    const float maxSize = 70.0f;
    const float sizeChange = 10.0f;

    //Brick movement controls
    GraphicRaycaster raycaster;
    bool canMove = true;
    bool isTranslating = false;
    bool isMarketBrick = false;
    float holdingScreenTimer = 0;
    const float maxTapTimer = 0.15f;
    const float botBounds = 100.0f;
    Vector3 prevMousePos = Vector3.zero;
    GameObject coreBrick = null;
    GameObject selectedBrick = null;
    GameObject botBrick = null;
    GameObject sellBrick = null;

    //Temporary components loaded from in-game bot
    bool hasResources = false;
    List<GameObject> botBricks = new List<GameObject>();
    float maxCapacity = 0;
    float currentFuel = 0;
    float currentBlue = 0;
    float currentGreen = 0;
    float currentYellow = 0;
    float currentGrey = 0;
    float excessRed = 0;
    float excessBlue = 0;
    float excessGreen = 0;
    float excessYellow = 0;
    float excessGrey = 0;
    int currentMoney = 0;
    int transactionAmount = 0;
    Sprite[,] botMap;
    List<GameObject> uncommittedBricks = new List<GameObject>();

    //Temp prices
    int resourceChange = 1;
    int resourceCost = 2;
    int resourceSell = 1;
    int brickCost = 20;
    int brickSell = 10;

    //Sub-menu text amounts
    int tempMoneyAmount = 0;
    int tempRedAmount = 0;
    int tempBlueAmount = 0;
    int tempGreenAmount = 0;
    int tempYellowAmount = 0;
    int tempGreyAmount = 0;
    List<CraftedPart> tempUpgrades = new List<CraftedPart>();
    string tempUpgrade = "";

    //Init - Load resources before building UI
    void LoadComponents()
    {
        if (GameController.Instance.saveManager == null)
        {
            GameController.Instance.saveManager = new SaveManager();
            GameController.Instance.saveManager.Init();
        }

        tilesAtlas = Resources.LoadAll<Sprite>(tileAtlasResource);
        tilesAtlas = tilesAtlas.Concat<Sprite>(Resources.LoadAll<Sprite>(craftingAtlasResource)).ToArray<Sprite>();

        foreach (string MarketItem in marketList)
        {
            tempMarketList.Add(MarketItem);
        }

        raycaster = GetComponent<GraphicRaycaster>();
        maxBarWidth = fuelBar.sizeDelta.x;
        hasResources = true;
    }

    //Temporarily save all bot resources in scrapyard
    public void LoadBotComponents()
    {
        //Load bot resources
        currentMoney = GameController.Instance.money;
        transactionAmount = currentMoney;
        currentFuel = GameController.Instance.bot.GetSavedResource(ResourceType.Red);
        currentBlue = GameController.Instance.bot.GetSavedResource(ResourceType.Blue);
        currentYellow = GameController.Instance.bot.GetSavedResource(ResourceType.Yellow);
        currentGreen = GameController.Instance.bot.GetSavedResource(ResourceType.Green);
        currentGrey = GameController.Instance.bot.GetSavedResource(ResourceType.Grey);
        excessRed = GameController.Instance.bot.hangarRed;
        excessBlue = GameController.Instance.bot.hangarBlue;
        excessGreen = GameController.Instance.bot.hangarGreen;
        excessYellow = GameController.Instance.bot.hangarYellow;
        excessGrey = GameController.Instance.bot.hangarGrey;

        //Load bot map
        botMap = GameController.Instance.bot.GetTileMap();
    }

    //Update bot with scrapyard changes
    public void SaveBotComponents()
    {
        //Make sure all bricks are attached to bot
        SnapBricksToBot();

        //Save bot map
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

        //Save bot resources
        currentMoney = transactionAmount;
        GameController.Instance.money = currentMoney;
        GameController.Instance.bot.SetSavedResource(ResourceType.Red, currentFuel);
        GameController.Instance.bot.SetSavedResource(ResourceType.Blue, currentBlue);
        GameController.Instance.bot.SetSavedResource(ResourceType.Yellow, currentYellow);
        GameController.Instance.bot.SetSavedResource(ResourceType.Green, currentGreen);
        GameController.Instance.bot.SetSavedResource(ResourceType.Grey, currentGrey);
        GameController.Instance.bot.hangarRed = excessRed;
        GameController.Instance.bot.hangarBlue = excessBlue;
        GameController.Instance.bot.hangarGreen = excessGreen;
        GameController.Instance.bot.hangarYellow = excessYellow;
        GameController.Instance.bot.hangarGrey = excessGrey;
    }

    //Attach all floating bricks to the main bot before saving
    void SnapBricksToBot()
    {
        //~Does sprite have a sprite-filled path back to core?
        //~If not, find shortest route to core, move sprite to last spriteless space before bot

        List<Vector2Int> checkedCoords = new List<Vector2Int>();
        Vector2Int startPoint = GameController.Instance.bot.coreV2;
        for(int i = 0;i<GameController.Instance.bot.maxBotRadius;i++)
        {
            for(int x = 0;x <= i; x++)
            {
                Vector2Int testCoord = new Vector2Int(x, i);
                if(!checkedCoords.Contains(testCoord))
                {
                    if(!IsCoreConnected(testCoord))
                    {
                        ConnectToCore();
                    }
                    checkedCoords.Add(testCoord);
                }
                if (!checkedCoords.Contains(testCoord * -1))
                {
                    if (!IsCoreConnected(testCoord * -1))
                    {
                        ConnectToCore();
                    }
                    checkedCoords.Add(testCoord * -1);
                }
            }

            for (int y = 0; y <= i; y++)
            {
                Vector2Int testCoord = new Vector2Int(-i, y);
                if (!checkedCoords.Contains(testCoord))
                {
                    if (!IsCoreConnected(testCoord))
                    {
                        ConnectToCore();
                    }
                    checkedCoords.Add(testCoord);
                }
                if (!checkedCoords.Contains(testCoord * -1))
                {
                    if (!IsCoreConnected(testCoord * -1))
                    {
                        ConnectToCore();
                    }
                    checkedCoords.Add(testCoord * -1);
                }
            }
        }
    }

    //Check if a brick has a route back to the core
    bool IsCoreConnected(Vector2Int coords)
    {
        for (int x = 0; x < botMap.GetLength(0); x++)
        {
            for (int y = 0; y < botMap.GetLength(1); y++)
            {
            }
        }
        return true;
    }

    //Connect unconnected sprite to closest possible point to core
    void ConnectToCore()
    {

    }

    //Update marketplace with available purchases
    void BuildMarketplace()
    {
        //Remove existing marketplace items
        Image[] buttons = marketParent.GetComponentsInChildren<Image>();
        for (int i = 0; i < buttons.Length; i++)
        {
            Destroy(buttons[i].gameObject);
        }
        marketSelection = new List<GameObject>();

        //Add new items to marketplace
        for (int i = marketIndex; i < marketIndex + maxMarketItems; i++)
        {
            GameObject newTile = Instantiate(botTile, marketParent.transform);
            Image newTileImage = newTile.GetComponent<Image>();
            newTileImage.sprite = tilesAtlas.Single<Sprite>(s => s.name == tempMarketList[i]);
            marketSelection.Add(newTile);

            int price = brickCost;
            GameObject newPrice = Instantiate(pricePrefab, newTile.transform);
            newPrice.GetComponent<Text>().text = "$" + price;
            marketPrices.Add(price);
        }
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

                if (GameController.Instance.bot.coreV2 == new Vector2Int(x,y))
                    coreBrick = newTile;

                botBricks.Add(newTile);
            }
        }

        UpdateUpgradeGlows();
    }

    //Update available upgrade glows
    void UpdateUpgradeGlows()
    {
        foreach(GameObject BotTile in botBricks)
        {
            bool canUpgrade = false;
            List<CraftedPart> checkUpgrades = GetUpgradeList(BotTile.GetComponent<Image>().sprite);
            foreach (CraftedPart CheckPart in checkUpgrades)
            {
                if (CanUpgrade(BotTile.GetComponent<Image>().sprite, CheckPart))
                    canUpgrade = true;
            }
            BotTile.transform.GetChild(0).gameObject.SetActive(canUpgrade);
        }
    }

    //Update scrapyard resource UI
    void UpdateResources()
    {
        if(currentFuel > maxCapacity)
        {
            excessRed += currentFuel - maxCapacity;
            currentFuel = maxCapacity;
        }
        else if (currentFuel < maxCapacity)
        {
            float shiftExcess = Mathf.Min(excessRed, maxCapacity - currentFuel);
            currentFuel += shiftExcess;
            excessRed -= shiftExcess;
        }

        if (currentBlue > maxCapacity)
        {
            excessBlue += currentBlue - maxCapacity;
            currentBlue = maxCapacity;
        }
        else if (currentBlue < maxCapacity)
        {
            float shiftExcess = Mathf.Min(excessBlue, maxCapacity - currentBlue);
            currentBlue += shiftExcess;
            excessBlue -= shiftExcess;
        }

        if (currentGreen > maxCapacity)
        {
            excessGreen += currentGreen - maxCapacity;
            currentGreen = maxCapacity;
        }
        else if (currentGreen < maxCapacity)
        {
            float shiftExcess = Mathf.Min(excessGreen, maxCapacity - currentGreen);
            currentGreen += shiftExcess;
            excessGreen -= shiftExcess;
        }

        if (currentYellow > maxCapacity)
        {
            excessYellow += currentYellow - maxCapacity;
            currentYellow = maxCapacity;
        }
        else if (currentYellow < maxCapacity)
        {
            float shiftExcess = Mathf.Min(excessYellow, maxCapacity - currentYellow);
            currentYellow += shiftExcess;
            excessYellow -= shiftExcess;
        }

        if (currentGrey > maxCapacity)
        {
            excessGrey += currentGrey - maxCapacity;
            currentGrey = maxCapacity;
        }
        else if (currentGrey < maxCapacity)
        {
            float shiftExcess = Mathf.Min(excessGrey, maxCapacity - currentGrey);
            currentGrey += shiftExcess;
            excessGrey -= shiftExcess;
        }

        playerMoney.text = "Money: $" + currentMoney.ToString();
        transactionMoney.text = "After: $" + transactionAmount;

        blueBar.sizeDelta = new Vector2(maxBarWidth * (maxCapacity > 0 ? currentBlue / maxCapacity : 0), blueBar.sizeDelta.y);
        blueAmount.text = Mathf.RoundToInt(currentBlue).ToString();
        blueBurnRate.text = "-" + Mathf.RoundToInt(GameController.Instance.bot.GetBurnRate(ResourceType.Blue)).ToString() + "/s";
        blueAmount.text += " (+" + Mathf.RoundToInt(excessBlue).ToString() + ")";

        yellowBar.sizeDelta = new Vector2(maxBarWidth * (maxCapacity > 0 ? currentYellow / maxCapacity : 0), yellowBar.sizeDelta.y);
        yellowAmount.text = Mathf.RoundToInt(currentYellow).ToString();
        yellowBurnRate.text = "-" + Mathf.RoundToInt(GameController.Instance.bot.GetBurnRate(ResourceType.Yellow)).ToString() + "/s";
        yellowAmount.text += " (+" + Mathf.RoundToInt(excessYellow).ToString() + ")";

        greenBar.sizeDelta = new Vector2(maxBarWidth * (maxCapacity > 0 ? currentGreen / maxCapacity : 0), greenBar.sizeDelta.y);
        greenAmount.text = Mathf.RoundToInt(currentGreen).ToString();
        greenBurnRate.text = "-" + Mathf.RoundToInt(GameController.Instance.bot.GetBurnRate(ResourceType.Green)).ToString() + "/s";
        greenAmount.text += " (+" + Mathf.RoundToInt(excessGreen).ToString() + ")";

        greyBar.sizeDelta = new Vector2(maxBarWidth * (maxCapacity > 0 ? currentGrey / maxCapacity : 0), greyBar.sizeDelta.y);
        greyAmount.text = Mathf.RoundToInt(currentGrey).ToString();
        greyBurnRate.text = "-" + Mathf.RoundToInt(GameController.Instance.bot.GetBurnRate(ResourceType.Grey)).ToString() + "/s";
        greyAmount.text += " (+" + Mathf.RoundToInt(excessGrey).ToString() + ")";

        fuelBar.sizeDelta = new Vector2(maxBarWidth * (maxCapacity > 0 ? currentFuel / maxCapacity : 0), fuelBar.sizeDelta.y);
        redAmount.text = Mathf.RoundToInt(currentFuel).ToString();
        redBurnRate.text = "-" + Mathf.RoundToInt(GameController.Instance.bot.GetBurnRate(ResourceType.Red)).ToString() + "/s";
        redAmount.text += " (+" + Mathf.RoundToInt(excessRed).ToString() + ")";

        UpdateUpgradeGlows();
    }

    //Update temp bot capacity based on assets in the bot grid
    void UpdateCapacity()
    {
        maxCapacity = 0;
        foreach (GameObject GridObject in botBricks)
        {
            foreach (GameObject ContainerPart in containerParts)
            {
                Brick targetContainer = ContainerPart.GetComponent<Brick>();
                for (int i = 0; i < targetContainer.spriteArr.Length; i++)
                {
                    if (targetContainer.spriteArr[i] == GridObject.GetComponent<Image>().sprite)
                    {
                        maxCapacity += targetContainer.GetComponent<Container>().capacity[i];
                    }
                }
            }
        }
    }

    //Update all scrapyard UI on loading or saving bot
    public void UpdateScrapyard()
    {
        if (!hasResources)
        {
            LoadComponents();
        }

        CloseSubMenu();
        BuildBotGrid();
        UpdateCapacity();
        UpdateResources();
        GameController.Instance.RefreshBotIcons();
        BuildMarketplace();
    }

    //Snap brick to block closest to player drag position
    void UpdateBrickSnap()
    {
        selectedBrick.GetComponent<RectTransform>().sizeDelta = Vector2.one * currentSize;
        selectedBrick.GetComponent<RectTransform>().anchoredPosition = Input.mousePosition;
    }

    //Update keyboard and mouse controls
    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && canMove)
        {
            //Start tap timer - is player trying to click something or drag something?
            holdingScreenTimer = 0;
            PointerEventData pointer = new PointerEventData(EventSystem.current);
            pointer.position = Input.mousePosition;
            List<RaycastResult> targets = new List<RaycastResult>();
            raycaster.Raycast(pointer, targets);
            for (int i = 0; i < targets.Count; i++)
            {
                //An item is being purchased from the marketplace
                if (marketSelection.Contains(targets[i].gameObject))
                {
                    isMarketBrick = true;
                    selectedBrick = targets[i].gameObject;
                    isTranslating = false;
                    break;
                }

                //An existing brick is being edited
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
            //A brick has been clicked - show interact options
            if (holdingScreenTimer < maxTapTimer)
            {
                if (botBrick && !isMarketBrick)
                {
                    sellBrick = botBrick;
                    botBrick = null;
                    BrickOptions();
                }
            }
            else if (selectedBrick)
            {
                //A brick has been dragged - drop it in the most appropriate spot
                PointerEventData pointer = new PointerEventData(EventSystem.current);
                pointer.position = Input.mousePosition;
                List<RaycastResult> targets = new List<RaycastResult>();
                raycaster.Raycast(pointer, targets);
                for (int i = 0; i < targets.Count; i++)
                {
                    if (botBricks.Contains(targets[i].gameObject) && targets[i].gameObject.GetComponent<Image>().color == Color.clear)
                    {
                        uncommittedBricks.Add(targets[i].gameObject);
                        targets[i].gameObject.GetComponent<Image>().color = Color.white;
                        targets[i].gameObject.GetComponent<Image>().sprite = selectedBrick.GetComponent<Image>().sprite;
                        if (isMarketBrick)
                        {
                            //~For now, don't remove purchased bricks from market
                            //tempMarketList.Remove(selectedBrick.GetComponent<Image>().sprite.name);
                            transactionAmount -= marketPrices[tempMarketList.IndexOf(selectedBrick.GetComponent<Image>().sprite.name)];
                            UpdateResources();
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
                if (botBrick)
                {
                    uncommittedBricks.Add(botBrick);
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
                if (botBrick && !selectedBrick)
                {
                    //Reposition selected brick (if able)
                    if (!isTranslating && uncommittedBricks.Contains(botBrick))
                    {
                        selectedBrick = Instantiate(botTile, transform.parent);
                        selectedBrick.GetComponent<Image>().sprite = botBrick.GetComponent<Image>().sprite;
                        botBrick.GetComponent<Image>().color = Color.clear;
                        uncommittedBricks.Remove(botBrick);

                        if (selectedBrick.GetComponentInChildren<Text>())
                            selectedBrick.GetComponentInChildren<Text>().enabled = false;
                        selectedBrick.transform.SetParent(transform.parent);
                        selectedBrick.GetComponent<RectTransform>().anchorMin = Vector2.zero;
                        selectedBrick.GetComponent<RectTransform>().anchorMax = Vector2.zero;
                    }

                    //Core selected - reposition bot instead
                    else if (isTranslating)
                    {
                        Vector2 newBotPos = botDisplay.GetComponent<RectTransform>().anchoredPosition + new Vector2(Input.mousePosition.x - prevMousePos.x, Input.mousePosition.y - prevMousePos.y);
                        newBotPos.x = Mathf.Clamp(newBotPos.x, -botBounds, botBounds);
                        newBotPos.y = Mathf.Clamp(newBotPos.y, -botBounds, botBounds);
                        botDisplay.GetComponent<RectTransform>().anchoredPosition = newBotPos;
                        prevMousePos = Input.mousePosition;
                    }
                }
                else if (selectedBrick)
                {
                    if (selectedBrick.transform.parent != transform.parent)
                    {
                        if (selectedBrick.GetComponentInChildren<Text>())
                            selectedBrick.GetComponentInChildren<Text>().enabled = false;
                        selectedBrick.transform.SetParent(transform.parent);
                        selectedBrick.GetComponent<RectTransform>().anchorMin = Vector2.zero;
                        selectedBrick.GetComponent<RectTransform>().anchorMax = Vector2.zero;
                    }
                    UpdateBrickSnap();
                }
            }
        }

        //Keyboard controls
        if (Input.GetKeyDown(KeyCode.Equals))
        {
            ZoomIn();
        }
        else if (Input.GetKeyDown(KeyCode.Minus))
        {
            ZoomOut();
        }
    }

    //Button for buying resources
    public void BuyResource(string resource)
    {
        switch (resource)
        {
            case "RED":
                currentFuel += resourceChange;
                break;
            case "BLUE":
                currentBlue += resourceChange;
                break;
            case "YELLOW":
                currentYellow += resourceChange;
                break;
            case "GREEN":
                currentGreen += resourceChange;
                break;
            case "GREY":
                currentGrey += resourceChange;
                break;
        }

        transactionAmount -= resourceCost;
        UpdateResources();
    }

    //Button for selling resources
    public void SellResource(string resource)
    {
        switch (resource)
        {
            case "RED":
                if (currentFuel < resourceChange)
                    return;
                currentFuel = Mathf.Max(0, currentFuel - resourceChange);
                break;
            case "BLUE":
                if (currentBlue < resourceChange)
                    return;
                currentBlue = Mathf.Max(0, currentBlue - resourceChange);
                break;
            case "YELLOW":
                if (currentYellow < resourceChange)
                    return;
                currentYellow = Mathf.Max(0, currentYellow - resourceChange);
                break;
            case "GREEN":
                if (currentGreen < resourceChange)
                    return;
                currentGreen = Mathf.Max(0, currentGreen - resourceChange);
                break;
            case "GREY":
                if (currentGrey < resourceChange)
                    return;
                currentGrey = Mathf.Max(0, currentGrey - resourceChange);
                break;
        }

        transactionAmount += resourceSell;
        UpdateResources();
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

    //Button for scrolling left in the market
    public void MarketLeft()
    {
        marketIndex = Mathf.Max(0, marketIndex - 1);
        BuildMarketplace();
    }

    //Button for scrolling right in the market
    public void MarketRight()
    {
        marketIndex = Mathf.Min(marketList.Count - maxMarketItems, marketIndex + 1);
        BuildMarketplace();
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

    //Button for showing help panel
    public void HelpPanel()
    {
        canMove = false;
        helpPanel.SetActive(true);
    }

    //Button for confirming sold bricks
    public void ConfirmSell()
    {
        canMove = false;
        tempMoneyAmount = uncommittedBricks.Contains(sellBrick) ? brickCost : brickSell;
        sellText.text = "SELL FOR $" + tempMoneyAmount + "?";
        brickOptions.SetActive(false);
        confirmSell.SetActive(true);
    }

    //Button for confirming conversion of bricks to resources
    public void ConfirmConvert()
    {
        canMove = false;
        Sprite targetPart = sellBrick.GetComponent<Image>().sprite;
        foreach (GameObject Resource in convertableParts)
        {
            Brick resourceBrick = Resource.GetComponent<Brick>();
            for (int i = 0; i < resourceBrick.spriteArr.Length; i++)
            {
                if (targetPart == resourceBrick.spriteArr[i])
                {
                    tempRedAmount = Resource.GetComponent<Fuel>() ? Resource.GetComponent<Fuel>().maxFuelArr[i] : 0;
                    tempYellowAmount = Resource.GetComponent<Yellectrons>() ? Resource.GetComponent<Yellectrons>().maxResource[i] : 0;
                    tempGreyAmount = Resource.GetComponent<Greyscale>() ? Resource.GetComponent<Greyscale>().maxResource[i] : 0;
                    tempGreenAmount = Resource.GetComponent<Repair>() ? Resource.GetComponent<Repair>().maxResource[i] : 0;
                    tempBlueAmount = Resource.GetComponent<Gun>() ? Resource.GetComponent<Gun>().maxResource[i] : 0;
                    break;
                }
            }
        }

        convertText.text = "CONVERT FOR ";
        if(tempRedAmount > 0)
        {
            convertText.text += tempRedAmount.ToString() + " RED, ";
        }
        if (tempBlueAmount > 0)
        {
            convertText.text += tempBlueAmount.ToString() + " BLUE, ";
        }
        if (tempGreenAmount > 0)
        {
            convertText.text += tempGreenAmount.ToString() + " GREEN, ";
        }
        if (tempYellowAmount > 0)
        {
            convertText.text += tempYellowAmount.ToString() + " YELLOW, ";
        }
        if (tempGreyAmount > 0)
        {
            convertText.text += tempGreyAmount.ToString() + " GREY, ";
        }
        convertText.text = convertText.text.Substring(0, convertText.text.Length - 2);
        convertText.text += "?";
        brickOptions.SetActive(false);
        confirmConvert.SetActive(true);
    }

    //Button for confirming brick upgrade
    public void ConfirmUpgrade(string upgradeName)
    {
        canMove = false;
        tempUpgrade = upgradeName;
        foreach (CraftedPart TempPart in tempUpgrades)
        {
            for(int i = 0;i<TempPart.scrapyardName.Length;i++)
            {
                if(tempUpgrade == TempPart.scrapyardName[i])
                {
                    tempMoneyAmount = TempPart.moneyToCraft[i];
                    tempRedAmount = TempPart.redToCraft[i];
                    tempBlueAmount = TempPart.blueToCraft[i];
                    tempGreenAmount = TempPart.greenToCraft[i];
                    tempYellowAmount = TempPart.yellowToCraft[i];
                    tempGreyAmount = TempPart.greyToCraft[i];
                    break;
                }
            }
        }

        upgradeText.text = "UPGRADE TO " + upgradeName + " FOR ";
        if(tempMoneyAmount > 0)
        {
            upgradeText.text += "$" + tempMoneyAmount.ToString() + ", ";
        }
        if (tempRedAmount > 0)
        {
            upgradeText.text += tempRedAmount.ToString() + " RED, ";
        }
        if (tempBlueAmount > 0)
        {
            upgradeText.text += tempBlueAmount.ToString() + " BLUE, ";
        }
        if (tempGreenAmount > 0)
        {
            upgradeText.text += tempGreenAmount.ToString() + " GREEN, ";
        }
        if (tempYellowAmount > 0)
        {
            upgradeText.text += tempYellowAmount.ToString() + " YELLOW, ";
        }
        if (tempGreyAmount > 0)
        {
            upgradeText.text += tempGreyAmount.ToString() + " GREY, ";
        }
        upgradeText.text = upgradeText.text.Substring(0, upgradeText.text.Length - 2);
        upgradeText.text += "?";

        brickOptions.SetActive(false);
        confirmUpgrade.SetActive(true);
    }

    //Button for returning to map screen
    public void MapScreen()
    {
        canMove = false;
        confirmMap.SetActive(true);
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

    //Button for opening the brick options menu
    public void BrickOptions()
    {
        canMove = false;

        for (int i = 0; i < brickOptionButtons.Count; i++)
        {
            Destroy(brickOptionButtons[i]);
        }
        brickOptionButtons = new List<GameObject>();

        GameObject sellButton = Instantiate(brickOptionsPrefab, brickOptionsGrid);
        sellButton.GetComponent<Text>().text = "SELL";
        if (CanSell(sellBrick))
        {
            sellButton.GetComponent<Button>().onClick.AddListener(() => { ConfirmSell(); });
        }
        else
        {
            sellButton.GetComponent<Button>().interactable = false;
        }
        brickOptionButtons.Add(sellButton);

        GameObject convertButton = Instantiate(brickOptionsPrefab, brickOptionsGrid);
        convertButton.GetComponent<Text>().text = "CONVERT";
        if (CanConvert(sellBrick))
        {
            convertButton.GetComponent<Button>().onClick.AddListener(() => { ConfirmConvert(); });
        }
        else
        {
            convertButton.GetComponent<Button>().interactable = false;
        }
        brickOptionButtons.Add(convertButton);

        tempUpgrades = GetUpgradeList(sellBrick.GetComponent<Image>().sprite);
        foreach (CraftedPart TempPart in tempUpgrades)
        {
            GameObject upgradeButton = Instantiate(brickOptionsPrefab, brickOptionsGrid);
            string targetUpgrade = GetUpgradeName(sellBrick.GetComponent<Image>().sprite, TempPart);
            upgradeButton.GetComponent<Text>().text = "UPGRADE: " + targetUpgrade;
            if (CanUpgrade(sellBrick.GetComponent<Image>().sprite, TempPart))
            {
                upgradeButton.GetComponent<Button>().onClick.AddListener(() => { ConfirmUpgrade(targetUpgrade); });
            }
            else
            {
                upgradeButton.GetComponent<Button>().interactable = false;
            }
            brickOptionButtons.Add(upgradeButton);
        }

        GameObject backButton = Instantiate(brickOptionsPrefab, brickOptionsGrid);
        backButton.GetComponent<Text>().text = "BACK";
        backButton.GetComponent<Button>().onClick.AddListener(() => { CloseSubMenu(); });
        brickOptionButtons.Add(backButton);

        brickOptions.SetActive(true);
    }

    //Button for closing a sub-menu and returning to the main scrapyard
    public void CloseSubMenu()
    {
        canMove = true;
        helpPanel.SetActive(false);
        saveLayoutMenu.SetActive(false);
        loadLayoutMenu.SetActive(false);
        confirmPurchase.SetActive(false);
        failPurchase.SetActive(false);
        confirmSell.SetActive(false);
        confirmLevel.SetActive(false);
        confirmMap.SetActive(false);
        confirmConvert.SetActive(false);
        confirmUpgrade.SetActive(false);
        brickOptions.SetActive(false);
    }

    //Check if this part can be sold
    bool CanSell(GameObject selectedPart)
    {
        return coreBrick != selectedPart;
    }

    //Check if this part can be converted for resources
    bool CanConvert(GameObject selectedPart)
    {
        if (coreBrick != selectedPart)
        {
            Sprite targetPart = selectedPart.GetComponent<Image>().sprite;
            foreach (GameObject Resource in convertableParts)
            {
                Brick resourceBrick = Resource.GetComponent<Brick>();
                for (int i = 0; i < resourceBrick.spriteArr.Length; i++)
                {
                    if (targetPart == resourceBrick.spriteArr[i])
                    {
                        return Resource.GetComponent<Fuel>() || Resource.GetComponent<Yellectrons>() || Resource.GetComponent<Greyscale>() || Resource.GetComponent<Repair>() || Resource.GetComponent<Gun>();
                    }
                }
            }
        }
        return false;
    }

    //Check what upgrades are available for the selected part
    List<CraftedPart> GetUpgradeList(Sprite selectedPart)
    {
        List<CraftedPart> upgradeList = new List<CraftedPart>();
        foreach (GameObject Upgrade in craftableParts)
        {
            CraftedPart targetPart = Upgrade.GetComponent<CraftedPart>();
            for (int i = 0; i < targetPart.basePartToCraft.Length; i++)
            {
                if (selectedPart == targetPart.basePartToCraft[i])
                {
                    upgradeList.Add(targetPart);
                }
            }
        }
        return upgradeList;
    }

    //Check if the player can afford the selected upgrade
    bool CanUpgrade(Sprite selectedPart, CraftedPart selectedUpgrade)
    {
        for (int i = 0; i < selectedUpgrade.basePartToCraft.Length; i++)
        {
            if (selectedPart == selectedUpgrade.basePartToCraft[i])
            {
                return (currentBlue + excessBlue) >= selectedUpgrade.blueToCraft[i] && (currentFuel + excessRed) >= selectedUpgrade.redToCraft[i] && (currentGreen + excessGreen) >= selectedUpgrade.greenToCraft[i] && (currentYellow + excessYellow) >= selectedUpgrade.yellowToCraft[i] && (currentGrey + excessGrey) >= selectedUpgrade.greyToCraft[i] && GameController.Instance.money >= selectedUpgrade.moneyToCraft[i];
            }
        }
        return false;
    }

    //Get the name of the player's target upgrade
    string GetUpgradeName(Sprite selectedPart, CraftedPart selectedUpgrade)
    {
        for (int i = 0; i < selectedUpgrade.basePartToCraft.Length; i++)
        {
            if (selectedPart == selectedUpgrade.basePartToCraft[i])
            {
                return selectedUpgrade.scrapyardName[i];
            }
        }
        return "";
    }

    //Button for selling confirmed bricks
    public void CompleteConfirmedSell()
    {
        canMove = true;
        if(uncommittedBricks.Contains(sellBrick))
        {
            uncommittedBricks.Remove(sellBrick);
        }
        sellBrick.GetComponent<Image>().color = Color.clear;
        sellBrick = null;
        transactionAmount += tempMoneyAmount;
        tempMoneyAmount = 0;
        UpdateResources();
        CloseSubMenu();
    }

    //Button for completing a brick conversion to resources
    public void CompleteConfirmedConvert()
    {
        canMove = true;
        currentFuel += tempRedAmount;
        currentBlue += tempBlueAmount;
        currentYellow += tempYellowAmount;
        currentGreen += tempGreenAmount;
        currentGrey += tempGreyAmount;
        tempRedAmount = 0;
        tempBlueAmount = 0;
        tempYellowAmount = 0;
        tempGreyAmount = 0;
        tempGreenAmount = 0;
        sellBrick.GetComponent<Image>().color = Color.clear;
        sellBrick = null;
        UpdateResources();
        CloseSubMenu();
    }

    //Button for completing a brick upgrade
    public void CompleteConfirmedUpgrade()
    {
        canMove = true;

        foreach (CraftedPart TempPart in tempUpgrades)
        {
            for (int i = 0; i < TempPart.scrapyardName.Length; i++)
            {
                if (tempUpgrade == TempPart.scrapyardName[i])
                {
                    currentBlue -= TempPart.blueToCraft[i];
                    currentFuel -= TempPart.redToCraft[i];
                    currentGreen -= TempPart.greenToCraft[i];
                    currentYellow -= TempPart.yellowToCraft[i];
                    currentGrey -= TempPart.greyToCraft[i];
                    transactionAmount -= TempPart.moneyToCraft[i];
                    sellBrick.GetComponent<Image>().sprite = TempPart.GetComponent<Brick>().spriteArr[i];
                    break;
                }
            }
        }

        tempUpgrade = "";
        tempUpgrades = new List<CraftedPart>();
        tempMoneyAmount = 0;
        tempRedAmount = 0;
        tempBlueAmount = 0;
        tempYellowAmount = 0;
        tempGreyAmount = 0;
        tempGreenAmount = 0;
        sellBrick = null;
        UpdateResources();
        CloseSubMenu();
    }

    //Button for confirming return to map screen
    public void ConfirmMapScreen()
    {
        CompleteConfirmedPurchase();
        GameController.Instance.LoadMapScreen();
    }

    //Button for completing confirmed market purchases
    public void CompleteConfirmedPurchase()
    {
        canMove = true;
        uncommittedBricks = new List<GameObject>();
        SaveBotComponents();
        UpdateScrapyard();
    }

    //Button for return bot to state of confirmed changes
    public void ResetChanges()
    {
        uncommittedBricks = new List<GameObject>();
        LoadBotComponents();
        UpdateScrapyard();
    }

    //Buttons for saving layout to a chosen slot
    public void SaveLayout(int index)
    {
        Sprite[,] layoutMap = new Sprite[botMap.GetLength(0), botMap.GetLength(1)];
        for (int x = 0; x < layoutMap.GetLength(0); x++)
        {
            for (int y = 0; y < layoutMap.GetLength(1); y++)
            {
                if (botBricks[x + y * layoutMap.GetLength(1)].GetComponent<Image>().color != Color.clear)
                    layoutMap[x, y] = botBricks[x + y * layoutMap.GetLength(1)].GetComponent<Image>().sprite;
                else
                    layoutMap[x, y] = null;
            }
        }
        GameController.Instance.SaveLayout(index, layoutMap);
        CloseSubMenu();
    }

    //Buttons for loading layout from a chosen slot
    public void LoadLayout(int index)
    {
        Sprite[,] newMap = GameController.Instance.LoadLayout(index);
        if (newMap != null)
        {
            //Remove all exising bot changes - we're overwriting with the layout
            ResetChanges();

            //Calculate costs of layout
            int totalMoneyCost = 0;
            float totalRedCost = 0;
            float totalBlueCost = 0;
            float totalGreenCost = 0;
            float totalYellowCost = 0;
            float totalGreyCost = 0;
            List<Sprite> unmatchedBricks = new List<Sprite>();
            foreach (GameObject CheckBrick in botBricks)
            {
                unmatchedBricks.Add(CheckBrick.GetComponent<Image>().sprite);
            }

            for (int x = 0; x < newMap.GetLength(0); x++)
            {
                for (int y = 0; y < newMap.GetLength(1); y++)
                {
                    //For each sprite in the new layout, look for matches in the old bot
                    Brick craftedPart = GetCraftedPart(newMap[x, y]);
                    List<Sprite> partUpgrades = GetSpriteUpgradeList(newMap[x, y]);
                    for (int i = partUpgrades.Count - 1; i >= 0; i--)
                    {
                        //If player already owns the part in question, waive costs and use the old part instead
                        if(unmatchedBricks.Contains(partUpgrades[i]))
                        {
                            unmatchedBricks.Remove(partUpgrades[i]);
                            break;
                        }

                        //Otherwise, player must pay the buy/upgrade costs associated with the new brick
                        else
                        {
                            if(!craftedPart || i == 0)
                            {
                                totalMoneyCost += brickCost;
                            }
                            else if (craftedPart)
                            {
                                CraftedPart upgradedPart = craftedPart.GetComponent<CraftedPart>();
                                totalMoneyCost += upgradedPart.moneyToCraft[i];
                                totalRedCost += upgradedPart.redToCraft[i];
                                totalBlueCost += upgradedPart.blueToCraft[i];
                                totalGreenCost += upgradedPart.greenToCraft[i];
                                totalYellowCost += upgradedPart.yellowToCraft[i];
                                totalGreyCost += upgradedPart.greyToCraft[i];
                            }
                        }
                    }
                }
            }

            //Sell unused player bricks
            for (int i = 0; i < unmatchedBricks.Count; i++)
            {
                totalMoneyCost -= brickSell;
            }

            //If player is short on resources, add their purchase cost to the amount
            if (totalRedCost <= excessRed)
            {
                excessRed -= totalRedCost;
                totalRedCost = 0;
            }
            else
            {
                totalRedCost -= excessRed;
                excessRed = 0;
                currentFuel -= totalRedCost;
                if (currentFuel < 0)
                {
                    int resourceIncrease = Mathf.CeilToInt(-currentFuel / (float)resourceChange);
                    currentFuel += resourceIncrease * resourceChange;
                    totalMoneyCost += resourceIncrease * resourceCost;
                }
            }

            if (totalBlueCost <= excessBlue)
            {
                excessBlue -= totalBlueCost;
                totalBlueCost = 0;
            }
            else
            {
                totalBlueCost -= excessBlue;
                excessBlue = 0;
                currentBlue -= totalBlueCost;
                if (currentBlue < 0)
                {
                    int resourceIncrease = Mathf.CeilToInt(-currentBlue / (float)resourceChange);
                    currentBlue += resourceIncrease * resourceChange;
                    totalMoneyCost += resourceIncrease * resourceCost;
                }
            }

            if (totalGreenCost <= excessGreen)
            {
                excessGreen -= totalGreenCost;
                totalGreenCost = 0;
            }
            else
            {
                totalGreenCost -= excessGreen;
                excessGreen = 0;
                currentGreen -= totalGreenCost;
                if (currentGreen < 0)
                {
                    int resourceIncrease = Mathf.CeilToInt(-currentGreen / (float)resourceChange);
                    currentGreen += resourceIncrease * resourceChange;
                    totalMoneyCost += resourceIncrease * resourceCost;
                }
            }

            if (totalYellowCost <= excessYellow)
            {
                excessYellow -= totalYellowCost;
                totalYellowCost = 0;
            }
            else
            {
                totalYellowCost -= excessYellow;
                excessYellow = 0;
                currentYellow -= totalYellowCost;
                if (currentYellow < 0)
                {
                    int resourceIncrease = Mathf.CeilToInt(-currentYellow / (float)resourceChange);
                    currentYellow += resourceIncrease * resourceChange;
                    totalMoneyCost += resourceIncrease * resourceCost;
                }
            }

            if (totalGreyCost <= excessGrey)
            {
                excessGrey -= totalGreyCost;
                totalGreyCost = 0;
            }
            else
            {
                totalGreyCost -= excessGrey;
                excessGrey = 0;
                currentGrey -= totalGreyCost;
                if (currentGrey < 0)
                {
                    int resourceIncrease = Mathf.CeilToInt(-currentGrey / (float)resourceChange);
                    currentGrey += resourceIncrease * resourceChange;
                    totalMoneyCost += resourceIncrease * resourceCost;
                }
            }

            //Update bot map and transaction amounts
            transactionAmount -= totalMoneyCost;
            botMap = newMap;
            BuildBotGrid();
            UpdateResources();
        }

        CloseSubMenu();
    }

    //Return the crafted brick associated with this sprite
    Brick GetCraftedPart(Sprite targetSprite)
    {
        Brick craftedMatch = null;
        foreach (GameObject CraftCheck in craftableParts)
        {
            Brick craftedPart = CraftCheck.GetComponent<Brick>();
            for (int i = 0; i < craftedPart.spriteArr.Length; i++)
            {
                if (targetSprite == craftedPart.spriteArr[i])
                {
                    craftedMatch = craftedPart;
                    break;
                }
            }
        }
        return craftedMatch;
    }

    //Return the upgrades required in order to get to this sprite
    List<Sprite> GetSpriteUpgradeList(Sprite targetSprite)
    {
        //Find the correct crafted brick
        Brick craftedMatch = GetCraftedPart(targetSprite);

        //Find levels before reaching this upgrade
        List<Sprite> upgradeList = new List<Sprite>();
        if (craftedMatch)
        {
            for (int i = 0; i < craftedMatch.spriteArr.Length; i++)
            {
                if (targetSprite == craftedMatch.spriteArr[i])
                {
                    break;
                }
                else
                {
                    upgradeList.Add(craftedMatch.spriteArr[i]);
                }
            }
        }

        upgradeList.Add(targetSprite);
        return upgradeList;
    }
}
