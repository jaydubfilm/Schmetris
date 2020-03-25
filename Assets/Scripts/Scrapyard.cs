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
    public Button sellOption;
    public Button upgradeOption;
    public Button convertOption;
    public Text sellText;
    public Text convertText;

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
    float totalResources
    {
        get
        {
            return currentFuel + currentBlue + currentGreen + currentYellow + currentGrey;
        }
    }
    int currentMoney = 0;
    int transactionAmount = 0;
    Sprite[,] botMap;

    //Temp prices
    int resourceChange = 1;
    int resourceCost = 2;
    int resourceSell = 1;
    int brickCost = 20;
    int brickSell = 10;

    //Sub-menu text amounts
    int tempSellAmount = 0;
    int tempRedAmount = 0;
    int tempBlueAmount = 0;
    int tempGreenAmount = 0;
    int tempYellowAmount = 0;
    int tempGreyAmount = 0;

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

        //Load bot map
        botMap = GameController.Instance.bot.GetTileMap();
    }

    //Update bot with scrapyard changes
    public void SaveBotComponents()
    {
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
                    if (CanUpgrade(newTileImage.sprite))
                        newTile.transform.GetChild(0).gameObject.SetActive(true);
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
    }

    //Update scrapyard resource UI
    void UpdateResources()
    {
        playerMoney.text = "Money: $" + currentMoney.ToString();
        transactionMoney.text = "After: $" + transactionAmount;

        blueBar.sizeDelta = new Vector2(maxBarWidth * (maxCapacity > 0 ? currentBlue / maxCapacity : 0), blueBar.sizeDelta.y);
        blueAmount.text = Mathf.RoundToInt(currentBlue).ToString();
        blueBurnRate.text = "-" + Mathf.RoundToInt(GameController.Instance.bot.GetBurnRate(ResourceType.Blue)).ToString() + "/s";

        yellowBar.sizeDelta = new Vector2(maxBarWidth * (maxCapacity > 0 ? currentYellow / maxCapacity : 0), yellowBar.sizeDelta.y);
        yellowAmount.text = Mathf.RoundToInt(currentYellow).ToString();
        yellowBurnRate.text = "-" + Mathf.RoundToInt(GameController.Instance.bot.GetBurnRate(ResourceType.Yellow)).ToString() + "/s";

        greenBar.sizeDelta = new Vector2(maxBarWidth * (maxCapacity > 0 ? currentGreen / maxCapacity : 0), greenBar.sizeDelta.y);
        greenAmount.text = Mathf.RoundToInt(currentGreen).ToString();
        greenBurnRate.text = "-" + Mathf.RoundToInt(GameController.Instance.bot.GetBurnRate(ResourceType.Green)).ToString() + "/s";

        greyBar.sizeDelta = new Vector2(maxBarWidth * (maxCapacity > 0 ? currentGrey / maxCapacity : 0), greyBar.sizeDelta.y);
        greyAmount.text = Mathf.RoundToInt(currentGrey).ToString();
        greyBurnRate.text = "-" + Mathf.RoundToInt(GameController.Instance.bot.GetBurnRate(ResourceType.Grey)).ToString() + "/s";

        fuelBar.sizeDelta = new Vector2(maxBarWidth * (maxCapacity > 0 ? currentFuel / maxCapacity : 0), fuelBar.sizeDelta.y);
        redAmount.text = Mathf.RoundToInt(currentFuel).ToString();
        redBurnRate.text = "-" + Mathf.RoundToInt(GameController.Instance.bot.GetBurnRate(ResourceType.Red)).ToString() + "/s";
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

    //Button for buying resources
    public void BuyResource(string resource)
    {
        switch (resource)
        {
            case "RED":
                if (totalResources - currentFuel > maxCapacity - resourceChange)
                    return;
                currentFuel = Mathf.Min(maxCapacity, currentFuel + resourceChange);
                break;
            case "BLUE":
                if (totalResources - currentBlue > maxCapacity - resourceChange)
                    return;
                currentBlue = Mathf.Min(maxCapacity, currentBlue + resourceChange);
                break;
            case "YELLOW":
                if (totalResources - currentYellow > maxCapacity - resourceChange)
                    return;
                currentYellow = Mathf.Min(maxCapacity, currentYellow + resourceChange);
                break;
            case "GREEN":
                if (totalResources - currentGreen > maxCapacity - resourceChange)
                    return;
                currentGreen = Mathf.Min(maxCapacity, currentGreen + resourceChange);
                break;
            case "GREY":
                if (totalResources - currentGrey > maxCapacity - resourceChange)
                    return;
                currentGrey = Mathf.Min(maxCapacity, currentGrey + resourceChange);
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
        tempSellAmount = brickSell;
        sellText.text = "SELL FOR $" + tempSellAmount + "?";
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
            convertText.text += tempRedAmount.ToString() + " RED,";
        }
        if (tempBlueAmount > 0)
        {
            convertText.text += tempBlueAmount.ToString() + " BLUE,";
        }
        if (tempGreenAmount > 0)
        {
            convertText.text += tempGreenAmount.ToString() + " GREEN,";
        }
        if (tempYellowAmount > 0)
        {
            convertText.text += tempYellowAmount.ToString() + " YELLOW,";
        }
        if (tempGreyAmount > 0)
        {
            convertText.text += tempGreyAmount.ToString() + " GREY,";
        }
        convertText.text = convertText.text.Substring(0, convertText.text.Length - 1);
        convertText.text += "?";
        brickOptions.SetActive(false);
        confirmConvert.SetActive(true);
    }

    //Button for confirming brick upgrade
    public void ConfirmUpgrade()
    {
        canMove = false;
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
        sellOption.interactable = CanSell(sellBrick);
        convertOption.interactable = CanConvert(sellBrick);
        upgradeOption.interactable = CanUpgrade(sellBrick.GetComponent<Image>().sprite);
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

    //Button for selling confirmed bricks
    public void CompleteConfirmedSell()
    {
        canMove = true;
        sellBrick.GetComponent<Image>().color = Color.clear;
        sellBrick = null;
        transactionAmount += tempSellAmount;
        tempSellAmount = 0;
        UpdateResources();
        CloseSubMenu();
    }

    //Button for completing a brick conversion to resources
    public void CompleteConfirmedConvert()
    {
        canMove = true;
        currentFuel = Mathf.Min(currentFuel + tempRedAmount, maxCapacity - (totalResources - currentFuel));
        currentBlue = Mathf.Min(currentBlue + tempBlueAmount, maxCapacity - (totalResources - currentBlue));
        currentYellow = Mathf.Min(currentYellow + tempYellowAmount, maxCapacity - (totalResources - currentYellow));
        currentGreen = Mathf.Min(currentGreen + tempGreenAmount, maxCapacity - (totalResources - currentGreen));
        currentGrey = Mathf.Min(currentGrey + tempGreyAmount, maxCapacity - (totalResources - currentGrey));
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
        SaveBotComponents();
        UpdateScrapyard();
    }

    //Button for return bot to state of confirmed changes
    public void ResetChanges()
    {
        LoadBotComponents();
        UpdateScrapyard();
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
                    sellBrick = botBrick;
                    botBrick = null;
                    BrickOptions();
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

    //Check if this part is upgradeable with player's existing resources
    bool CanUpgrade(Sprite selectedPart)
    {
        foreach(GameObject Upgrade in craftableParts)
        {
            CraftedPart targetPart = Upgrade.GetComponent<CraftedPart>();
            for (int i = 0;i< targetPart.basePartToCraft.Length;i++)
            {
                if(selectedPart == targetPart.basePartToCraft[i])
                {
                    if(currentBlue >= targetPart.blueToCraft[i] && currentFuel >= targetPart.redToCraft[i] && currentGreen >= targetPart.greenToCraft[i] && currentYellow >= targetPart.yellowToCraft[i] && currentGrey >= targetPart.greyToCraft[i] && GameController.Instance.money >= targetPart.moneyToCraft[i])
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    //Button for completing a brick upgrade
    public void CompleteConfirmedUpgrade()
    {
        canMove = true;

        Sprite selectedPart = sellBrick.GetComponent<Image>().sprite;
        foreach (GameObject Upgrade in craftableParts)
        {
            CraftedPart targetPart = Upgrade.GetComponent<CraftedPart>();
            for (int i = 0; i < targetPart.basePartToCraft.Length; i++)
            {
                if (selectedPart == targetPart.basePartToCraft[i])
                {
                    if (currentBlue >= targetPart.blueToCraft[i] && currentFuel >= targetPart.redToCraft[i] && currentGreen >= targetPart.greenToCraft[i] && currentYellow >= targetPart.yellowToCraft[i] && currentGrey >= targetPart.greyToCraft[i] && GameController.Instance.money >= targetPart.moneyToCraft[i])
                    {
                        currentBlue -= targetPart.blueToCraft[i];
                        currentFuel -= targetPart.redToCraft[i];
                        currentGreen -= targetPart.greenToCraft[i];
                        currentYellow -= targetPart.yellowToCraft[i];
                        currentGrey -= targetPart.greyToCraft[i];
                        transactionAmount -= targetPart.moneyToCraft[i];
                        sellBrick.GetComponent<Image>().sprite = targetPart.GetComponent<Brick>().spriteArr[i];
                        break;
                    }
                }
            }
        }

        CompleteConfirmedPurchase();
    }

    //Buttons for saving layout to a chosen slot
    public void SaveLayout(int index)
    {
        GameController.Instance.SaveLayout(index);
        CloseSubMenu();
    }

    //Buttons for loading layout from a chosen slot
    public void LoadLayout(int index)
    {
        GameController.Instance.LoadLayout(index);
    }
}
