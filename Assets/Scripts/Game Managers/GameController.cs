using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class GameController : MonoBehaviour
{
    //Game-wide events - Individual assets can connect to these to perform actions on specific game/level events (end, restart, etc)
    public delegate void GameEvent();
    public static event GameEvent OnGameOver, OnGameRestart, OnLoseLife, OnLevelRestart, OnNewLevel, OnSpeedChange, OnLevelComplete;

    public static GameController Instance { get; private set; }
    public List<GameObject> blockList;
    public List<GameObject> enemyList;
    public List<GameObject> bitReference;
    public List<GameObject> enemyReference;

    bool tutorialHasStarted;

    //Player earned score/money - adjust UI to match every time money is updated
    int _money = 0;
    public int money
    {
        get
        {
            return _money;
        }
        set
        {
            _money = value;
            hud.SetMoney(_money);
        }
    }

    //Player lives remaining - adjust UI to match every time lives change
    int _lives = 0;
    public int lives
    {
        get
        {
            return _lives;
        }
        set
        {
            _lives = value;
            hud.SetLives(_lives);
        }
    }

    //Menu screens
    public GameObject startMenu;
    public GameObject mapMenu;
    public GameObject pauseMenu;
    public GameObject scrapyard;
    public GameUI hud;

    public bool isBotDead = false;
    public bool isPaused = false;

    //public static int bgAdjustFlag = 0;
    
    public GameObject scoreIncreasePrefab;

    //public LevelData[] allLevelData;
    public Game easyGame;
    public Game mediumGame;
    public Game hardGame;
    public Game game;

    public Bot bot;
    public GameSettings settings;

    public static float timeRemaining = 10.0f;
    public int currentScene = 1;
    public int highestScene = 1;
    public static int spawnRow = 25;
    //public static int bitCount = 10;

    public GameObject[] bgPanelArr;
    public GameObject bgPanel;

    int columnNum = ScreenStuff.cols;
  
    int[] blockProbArr;
    int[] speciesProbArr;

    LevelData levelData;
    int levelSection = 0;
    bool isTimedLevel = false;
    public bool isLevelCompleteQueued = false;
    BlockSpawnData[] blockSpawns;
    public SpeciesSpawnData[] speciesSpawnData;
   
    float blockSpawnTimer = 3f;
    float enemySpawnTimer;
    float enemySpawnRate;

    public float blockSpeed;

    bool isRestarting = false;
    public SaveManager saveManager = null;
    public Transform[] loadIcons;
    public Transform[] saveIcons;
    public Transform[] mapLoadIcons;
    public Transform[] saveLayoutSlots;
    public Transform[] loadLayoutSlots;
    public GameObject iconGrid;
    public GameObject iconColumn;
    public GameObject iconTile;
    const string atlasResource = "MasterDiceSprites";
    const string craftingAtlasResource = "PartSprites";
    Sprite[] tilesAtlas;

    AudioController audioController;

    //Carl Added...
    public DynamicPathfindingManager enemyPathfinding;

    public float costMultiplier
    {
        get
        {
            return game.storeCostMultiplier;
        }
    }

    int speedMultiplier = 2;
    public float adjustedSpeed
    {
        get
        {
            if(settings.speedLevels.Length == 0 || speedMultiplier >= settings.speedLevels.Length)
            {
                return 1.0f;
            }
            return settings.speedLevels[speedMultiplier];
        }
    }

    public void EndGame(string endgameMessage)
    {
        if (!isBotDead)
        {
            isBotDead = true;
            lives--;
            if (lives == 0)
            {
                hud.SetProgressText("Level " + currentScene + " attained. $" + money + " Salvaged.");
                if (OnGameOver != null)
                {
                    OnGameOver();
                }
                hud.SetGameOverPopup(true, endgameMessage);
            }
            else
            {
                if (OnLoseLife != null)
                {
                    OnLoseLife();
                }
                hud.SetLifeLostPopup(true, endgameMessage);
            }
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            if (saveManager == null)
            {
                saveManager = new SaveManager();
                saveManager.Init();
            }
            tilesAtlas = Resources.LoadAll<Sprite>(atlasResource);
            tilesAtlas = tilesAtlas.Concat<Sprite>(Resources.LoadAll<Sprite>(craftingAtlasResource)).ToArray<Sprite>();
            RefreshBotIcons();
            bot.Init();
            bot.gameObject.SetActive(false);
            audioController = GetComponent<AudioController>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start () {
        lives = 3;
        bgPanelArr = new GameObject[4];
        SpawnBGPanels();
        speedMultiplier = settings.defaultSpeedLevel;
        StartMenu();
    }

    public void RefreshBotIcons()
    {
        for (int i = 0; i < loadIcons.Length; i++)
        {
            BuildBotIcon(i, loadIcons[i], false);
        }
        for (int i = 0; i < mapLoadIcons.Length; i++)
        {
            BuildBotIcon(i, mapLoadIcons[i], false);
        }
        for (int i = 0; i < saveIcons.Length; i++)
        {
            BuildBotIcon(i, saveIcons[i], false);
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

    void BuildBotIcon(int index, Transform target, bool isLayout)
    {
        if(target.GetComponentInChildren<VerticalLayoutGroup>())
        {
            Destroy(target.GetComponentInChildren<VerticalLayoutGroup>().gameObject);
        }

        SaveData targetFile = isLayout ? saveManager.GetLayout(index) : saveManager.GetSave(index);
        if (targetFile != null && targetFile.game != "" && targetFile.bot.Length > 0)
        {
            //Position icon base
            GameObject newGrid = Instantiate(iconGrid, target);
            RectTransform newGridTransform = newGrid.GetComponent<RectTransform>();
            newGridTransform.pivot = new Vector2(0, 0.5f);
            newGridTransform.sizeDelta = Vector2.one * 5;
            newGridTransform.anchoredPosition = new Vector2(100, 0);

            int minX = -1;
            int maxX = -1;
            int minY = -1;
            int maxY = -1;

            for (int y = 0; y < targetFile.bot.Length; y++)
            {
                for (int x = 0; x < targetFile.bot[0].botRow.Length; x++)
                {
                    if(targetFile.bot[x].botRow[y] != "")
                    {
                        maxX = x;
                        maxY = y;
                        if(minX == -1)
                        {
                            minX = x;
                        }
                        if(minY == -1)
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

            if (minX > -1 && minY > -1)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    GameObject newColumn = Instantiate(iconColumn, newGrid.transform);
                    for (int x = minX; x <= maxX ; x++)
                    {
                        GameObject newTile = Instantiate(iconTile, newColumn.transform);
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

    public void StartLevel(int level)
    {
        bot.gameObject.SetActive(true);
        bot.OnLevelRestart();

        currentScene = level;
        if (OnNewLevel != null)
        {
            OnNewLevel();
        }

        LoadLevelData(level);
        if (level == 1)
        {
            if (TutorialManager.Instance != null && tutorialHasStarted == false)
            {
                TutorialManager.Instance.TutorialPopup(0, true, true, true);                
                tutorialHasStarted = true;
                TutorialManager.Instance.isBotDead = false;
                TutorialManager.Instance.playerPos.GetComponent<Bot>().SetFuelAmt(500);

            }
        }
    }

    public void LoadMapScreen()
    {
        startMenu.SetActive(false);
        scrapyard.SetActive(false);
        hud.gameObject.SetActive(false);
        pauseMenu.SetActive(false);
        bot.hasDamagedCells = false;
        if (highestScene == 1)
        {
            StartLevel(1);

        }
        else
        {
            mapMenu.GetComponent<LevelMenuUI>().OpenMenu();
            mapMenu.SetActive(true);
            audioController.FadeInMusic(audioController.menuMusic, 8.0f, 1.0f);
        }
    }

    public void SaveGame(int index)
    {
        saveManager.SetSave(index, lives, money, highestScene, game.name, bot);
        RefreshBotIcons();
    }

    public void SaveLayout(int index, Sprite[,] map, List<ContainerData> containers)
    {
        saveManager.SetLayout(index, map, containers);
        RefreshBotIcons();
    }

    public void LoadGame(int index)
    {
        SaveData loadData = saveManager.GetSave(index);
        if (loadData != null && loadData.game != "")
        {
            lives = loadData.lives;
            money = loadData.money;
            highestScene = loadData.level;
            currentScene = highestScene;

            //~Add in Resource loading?
            if (easyGame.name == loadData.game)
            {
                game = easyGame;
            }
            else if (mediumGame.name == loadData.game)
            {
                game = mediumGame;
            }
            else if (hardGame.name == loadData.game)
            {
                game = hardGame;
            }

            bot.savedContainerData = loadData.containers;
            if (loadData.bot.Length > 0)
            {
                Sprite[,] newMap = new Sprite[loadData.bot.Length, loadData.bot[0].botRow.Length];
                for (int x = 0; x < newMap.GetLength(0); x++)
                {
                    for (int y = 0; y < newMap.GetLength(1); y++)
                    {
                        if(loadData.bot[x].botRow[y] != "")
                            newMap[x, y] = tilesAtlas.Single<Sprite>(s => s.name == loadData.bot[x].botRow[y]);
                    }
                }
                bot.SetTileMap(newMap);
            }

            //Resources
            bot.SetSavedResource(ResourceType.Red, loadData.fuel, false);
            bot.SetSavedResource(ResourceType.Blue, loadData.blue, false);
            bot.SetSavedResource(ResourceType.Yellow, loadData.yellow, false);
            bot.SetSavedResource(ResourceType.Green, loadData.green, false);
            bot.SetSavedResource(ResourceType.Grey, loadData.grey, false);
            bot.SetSavedResource(ResourceType.Red, Mathf.Max(0, loadData.hangarRed), true);
            bot.SetSavedResource(ResourceType.Blue, Mathf.Max(0, loadData.hangarBlue), true);
            bot.SetSavedResource(ResourceType.Green, Mathf.Max(0, loadData.hangarGreen), true);
            bot.SetSavedResource(ResourceType.Yellow, Mathf.Max(0, loadData.hangarYellow), true);
            bot.SetSavedResource(ResourceType.Grey, Mathf.Max(0, loadData.hangarGrey), true);

            hud.gameObject.SetActive(false);
            isPaused = true;
            Time.timeScale = 0;
            bot.gameObject.SetActive(false);
            SceneManager.LoadScene(1);
            LoadMapScreen();
        }
    }

    public Sprite[,] LoadLayout(int index)
    {
        SaveData loadData = saveManager.GetLayout(index);
        if (loadData != null && loadData.game != "")
        {
            if (loadData.bot.Length > 0)
            {
                Sprite[,] newMap = new Sprite[loadData.bot.Length, loadData.bot[0].botRow.Length];
                for (int x = 0; x < newMap.GetLength(0); x++)
                {
                    for (int y = 0; y < newMap.GetLength(1); y++)
                    {
                        if (loadData.bot[x].botRow[y] != "")
                            newMap[x, y] = tilesAtlas.Single<Sprite>(s => s.name == loadData.bot[x].botRow[y]);
                    }
                }
                return newMap;
            }
        }
        return null;
    }

    //Used for external Canvas buttons for touchscreen controls
    public void SpeedUp()
    {
        speedMultiplier = Mathf.Min(speedMultiplier + 1, settings.speedLevels.Length - 1);
        hud.SetSpeed(adjustedSpeed);
        if (OnSpeedChange != null)
        {
            OnSpeedChange();
        }
    }

    //Used for external Canvas buttons for touchscreen controls
    public void SpeedDown()
    {
        speedMultiplier = Mathf.Max(speedMultiplier - 1, 0);
        hud.SetSpeed(adjustedSpeed);
        if (OnSpeedChange != null)
        {
            OnSpeedChange();
        }
    }

    //Used for external Canvas buttons for touchscreen controls
    public void EasyGame()
    {
        game = easyGame;
        highestScene = 1;
        lives = 3;
        money = 0;
        speedMultiplier = settings.defaultSpeedLevel;
        bot.ResetTileMap();
        LoadMapScreen();
    }

    //Used for external Canvas buttons for touchscreen controls
    public void MediumGame()
    {
        game = mediumGame;
        highestScene = 1;
        lives = 3;
        money = 0;
        speedMultiplier = settings.defaultSpeedLevel;
        bot.ResetTileMap();
        LoadMapScreen();

    }

    //Used for external Canvas buttons for touchscreen controls
    public void HardGame()
    {
        game = hardGame;
        highestScene = 1;
        lives = 3;
        money = 0;
        speedMultiplier = settings.defaultSpeedLevel;
        bot.ResetTileMap();
        LoadMapScreen();

    }

    //Open initial game menu
    public void StartMenu()
    {
        scrapyard.SetActive(false);
        pauseMenu.SetActive(false);
        hud.gameObject.SetActive(false);
        mapMenu.SetActive(false);
        startMenu.SetActive(true);
        hud.SetLifeLostPopup(false);
        hud.SetLevelCompletePopup(false);
        hud.SetGameOverPopup(false);
        isPaused = true;
        Time.timeScale = 0;
        bot.gameObject.SetActive(false);
        bot.ResetTileMap();
        SceneManager.LoadScene(1);
        startMenu.GetComponent<MainMenuUI>().OpenMenu();
        audioController.FadeInMusic(audioController.menuMusic, 8.0f, 1.0f);
    }

    //Used for external Canvas buttons for touchscreen controls
    public void PauseGame()
    {
        hud.gameObject.SetActive(false);
        pauseMenu.SetActive(true);
        pauseMenu.GetComponent<PauseMenuUI>().OpenMenu();
        isPaused = true;
        Time.timeScale = 0;
    }

    //Used for external Canvas buttons for touchscreen controls
    public void ResumeGame()
    {
        hud.gameObject.SetActive(true);
        pauseMenu.SetActive(false);
        isPaused = false;
        Time.timeScale = 1.0f;
    }

    public void LoadNewLevel()
    {
        scrapyard.SetActive(false);
        bot.gameObject.SetActive(true);
        bot.OnLevelRestart();
        currentScene = Mathf.Min(currentScene + 1, game.levelDataArr.Length);
        if (OnNewLevel != null)
        {
            OnNewLevel();
        }
        LoadLevelData(highestScene);
    }

    public void RestartOnDestroy()
    {
        hud.gameObject.SetActive(false);
        mapMenu.SetActive(false);
        isPaused = true;
        Time.timeScale = 0;
        bot.gameObject.SetActive(false);
        SceneManager.LoadScene(1);
        StartLevel(currentScene);
    }

    public void LoadScrapyard()
    {
        hud.gameObject.SetActive(false);
        mapMenu.SetActive(false);
        isPaused = true;
        Time.timeScale = 0;
        //bot.OnNewLevel();
        bot.gameObject.SetActive(false);
        SceneManager.LoadScene(1);
        scrapyard.SetActive(true);
        scrapyard.GetComponent<Scrapyard>().LoadBotComponents();
        scrapyard.GetComponent<Scrapyard>().UpdateScrapyard();
        audioController.FadeInMusic(audioController.menuMusic, 8.0f, 1.0f);
    }

    public void Update()
    {
        if (!isBotDead && !isPaused)
        {

            //Update time remaining
            timeRemaining -= Time.deltaTime;
            float totalTimeRemaining = timeRemaining;
            for(int i = levelSection + 1;i < levelData.levelSections.Length;i++)
            {
                totalTimeRemaining += levelData.levelSections[i].levelDuration;
            }
            hud.SetTimer(totalTimeRemaining);
            if (isTimedLevel && timeRemaining < 0)
            {
                LoadNextLevelSection();
            }
            if (isLevelCompleteQueued)
            {
                bool hasBlocks = false;
                for (int i = 0; i < blockList.Count; i++)
                {
                    if (blockList[i])
                    {
                        hasBlocks = true;
                        break;
                    }
                }
                if (!hasBlocks)
                {
                    highestScene = Mathf.Min(highestScene + 1, game.levelDataArr.Length);
                    blockList = new List<GameObject>();
                    bot.OnNewLevel();
                    LoadScrapyard();
                }
            }

            BlockSpawnCheck();

            if (enemySpawnRate > 0)
                EnemySpawnCheck();
        }

        if (!isPaused)
        {
            ScrollBackground();

            foreach (GameObject moveableObject in GameObject.FindGameObjectsWithTag("Moveable"))
            {
                Vector3 pos = moveableObject.transform.position;

                if (pos.x < ScreenStuff.leftEdgeOfWorld - ScreenStuff.colSize)
                    pos.x = ScreenStuff.rightEdgeOfWorld;
                if (pos.x > ScreenStuff.rightEdgeOfWorld + ScreenStuff.colSize)
                    pos.x = ScreenStuff.leftEdgeOfWorld;
                moveableObject.transform.position = pos;
                if (pos.y < ScreenStuff.bottomEdgeOfWorld)
                {
                    Block block = moveableObject.GetComponent<Block>();
                    if (block != null)
                    {
                        block.DestroyBlock();
                    }
                    else
                        Destroy(moveableObject);
                }
            }
        }
    }

    //Used for external Canvas buttons for touchscreen controls
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }


    public void CreateFloatingText(string message, Vector3 worldPos, int size, Color color)
    {
        GameObject scoreFX = Instantiate(scoreIncreasePrefab);
        scoreFX.transform.SetParent(GetComponentInChildren<Canvas>().transform);
        scoreFX.transform.rotation = Quaternion.identity;
        scoreFX.transform.position = Camera.main.WorldToScreenPoint(worldPos);
        scoreFX.GetComponent<FloatingText>().Init(message, hud.moneyText.transform.position, size, color);
    }

    public int[] GetSpawnProbabilities() {
        int[] pArr = new int[blockSpawns.Length];
        for (int d = 0; d < blockSpawns.Length; d++)
            pArr[d] = blockSpawns[d].probability;
        return pArr;
    }

    public int[] GetSpeciesProbabilities() {
        int[] pArr = new int[speciesSpawnData.Length];
        for (int d = 0; d < speciesSpawnData.Length; d++)
            pArr[d] = speciesSpawnData[d].probability;
        return pArr;
    }

    public void LoadNextLevelSection()
    {
        LoadLevelSection(levelSection + 1);
    }

    public void LoadLevelSection(int sectionNumber)
    {
        if(sectionNumber >= levelData.levelSections.Length)
        {
            if (!isLevelCompleteQueued)
            {
                isLevelCompleteQueued = true;
                if (OnLevelComplete != null)
                {
                    OnLevelComplete();
                }
            }
            return;
        }

        levelSection = sectionNumber;

        blockSpawns = levelData.levelSections[levelSection].blocks;
        speciesSpawnData = levelData.levelSections[levelSection].speciesSpawnData;
        blockSpeed = levelData.levelSections[levelSection].blockSpeed;
        blockProbArr = GetSpawnProbabilities();
        speciesProbArr = GetSpeciesProbabilities();

        blockSpawnTimer = levelData.levelSections[levelSection].blockSpawnRate;
        enemySpawnRate = levelData.levelSections[levelSection].enemySpawnRate;
        enemySpawnTimer = enemySpawnRate;
        timeRemaining = levelData.levelSections[levelSection].levelDuration;
        isTimedLevel = timeRemaining > 0;

        hud.SetGameOverPopup(false);
        hud.SetLifeLostPopup(false);
        hud.SetLevelCompletePopup(false);
        startMenu.SetActive(false);
        pauseMenu.SetActive(false);
        mapMenu.SetActive(false);
        hud.gameObject.SetActive(true);

        isBotDead = false;
        isPaused = false;
        Time.timeScale = 1.0f;
        if(TutorialManager.Instance)
            TutorialManager.Instance.OnLevelChange(sectionNumber);
    }

    public void LoadLevelData(int levelNumber) {
        isLevelCompleteQueued = false;
        speedMultiplier = settings.defaultSpeedLevel;
        hud.SetSpeed(adjustedSpeed);
        SceneManager.LoadScene(Mathf.Min(SceneManager.sceneCountInBuildSettings - 1,levelNumber));
        hud.SetLevel(levelNumber);
        levelData = game.levelDataArr[levelNumber-1];
        audioController.FadeInMusic(audioController.gameMusic, 17.0f, 1.0f);
        LoadLevelSection(0);
    }

    void ScrollBackground() {
        Vector3[] bV3 = new Vector3[4];
        Vector3 panelUpV3 = new Vector3(0,2*settings.bgHeight,0);
        Vector3 panelLeftV3 = new Vector3(-2*settings.bgWidth,0,0);
        Vector3 panelRightV3 = new Vector3(2*settings.bgWidth,0,0);

        if (bgPanelArr[0] == null)
            return;

        // scroll Background Down

        for (int x = 0;x<4;x++) {    
            bV3[x] = bgPanelArr[x].transform.position;
            bV3[x] += new Vector3 (0,-settings.bgScrollSpeed * Time.unscaledDeltaTime,0);
        }

        // flip bottom BG to top

        if (bV3[1].y < bV3[0].y && bV3[0].y < 0.0f) {
            bV3[1] += panelUpV3;
            bV3[3] += panelUpV3;
        }
        if (bV3[0].y < bV3[1].y && bV3[1].y < 0.0f) {
            bV3[0] += panelUpV3;
            bV3[2] += panelUpV3;
        }

        // on player move

        /*if (bgAdjustFlag!=0) {
            float xOffset = ScreenStuff.colSize*bgAdjustFlag;

            // adjust background position

            for (int x = 0;x<4;x++)
                bV3[x] += new Vector3(xOffset,0,0);
    
                
            // adjust all non-player object positions

            GameObject[] movingObjectArr;
            movingObjectArr = GameObject.FindGameObjectsWithTag("Moveable");
            foreach(GameObject mo in movingObjectArr) {
                Vector3 v = mo.transform.position; 
                v.x +=xOffset;
                mo.transform.position = v;
            }
            
            bgAdjustFlag = 0;
        }*/

        // flip left BG to right

        if (bV3[0].x < bV3[2].x && bV3[2].x < 0.0f) {
            bV3[0] += panelRightV3;
            bV3[1] += panelRightV3;
        }
        if (bV3[2].x < bV3[0].x && bV3[0].x < 0.0f) {
            bV3[2] += panelRightV3;
            bV3[3] += panelRightV3;
        }
    
        // flip right BG to left

        if (bV3[0].x > bV3[2].x && bV3[2].x > 0.0f) {
            bV3[0] += panelLeftV3;
            bV3[1] += panelLeftV3;
        }
        if (bV3[2].x > bV3[0].x && bV3[0].x > 0.0f) {
            bV3[2] += panelLeftV3;
            bV3[3] += panelLeftV3;
        }

        for (int x=0;x<4;x++)
            bgPanelArr[x].transform.position = bV3[x];
    }

    public void MoveBot(int bgMoveFlag)
    {
        float xOffset = ScreenStuff.colSize * bgMoveFlag;

        Vector3[] bV3 = new Vector3[4];

        if (bgPanelArr[0] == null)
            return;

        // adjust background position

        for (int x = 0; x < 4; x++)
        {
            bV3[x] = bgPanelArr[x].transform.position;
            bV3[x] += new Vector3(xOffset, 0, 0);
            bgPanelArr[x].transform.position = bV3[x];
        }

        // adjust all non-player object positions

        GameObject[] movingObjectArr;
        movingObjectArr = GameObject.FindGameObjectsWithTag("Moveable");
        foreach (GameObject mo in movingObjectArr)
        {
            Vector3 v = mo.transform.position;
            v.x += xOffset;
            mo.transform.position = v;
        }

        Vector3 cameraOff = Camera.main.transform.position;
        cameraOff.x += xOffset;
        Camera.main.transform.position = cameraOff;
    }

    void EnemySpawnCheck() {
        if (!isLevelCompleteQueued)
        {
            enemySpawnTimer -= Time.deltaTime * adjustedSpeed;
            if (enemySpawnTimer <= 0)
            {
                int spawnType = ProbabilityPicker(speciesProbArr);
                enemySpawnTimer = enemySpawnRate;
                SpawnEnemy(spawnType);
            }
        }
    }

    void BlockSpawnCheck() {
        if (!isLevelCompleteQueued)
        {
            blockSpawnTimer -= Time.deltaTime * adjustedSpeed;
            if (blockSpawnTimer <= 0)
            {
                int blockType = ProbabilityPicker(blockProbArr);

                if (SpawnBlock(Random.Range(-ScreenStuff.screenRadius, ScreenStuff.screenRadius), blockType) != null)
                {
                    blockSpawnTimer = levelData.levelSections[levelSection].blockSpawnRate;

                }
            }
        }
    }

    public static int ProbabilityPicker(int[] pArr)
    {
        // returns a random element of the probability array of integers pArr (0 to pArr.length-1)

        int sum = 0;
        int num = pArr.Length;

        for (int x = 0; x < num; x++)
            sum += pArr[x];

        int r = Random.Range(1, sum+1);
        for (int x=0; x < num; x++)
        { 
            r -= pArr[x];
            if (r<=0)
                return x;
        }
        return (num-1);
    }

    public GameObject SpawnEnemy(int type) {
        GameObject newEnemyObj;

        float xPos = Random.Range(ScreenStuff.leftEdgeOfWorld,ScreenStuff.rightEdgeOfWorld);
        Vector3 vpos = new Vector3(xPos, ScreenStuff.RowToYPosition(spawnRow), 0);
        newEnemyObj = Instantiate(speciesSpawnData[type].species, vpos, Quaternion.identity);
        enemyList.Add(newEnemyObj);
        newEnemyObj.GetComponent<EnemyGeneral>().gameController = this;

        return newEnemyObj;
    }


    public GameObject SpawnBlock(int col, int type)
    {
        GameObject newBlockObj = null;

        Vector3 vpos = new Vector3(ScreenStuff.ColToXPosition(col), ScreenStuff.RowToYPosition(spawnRow), 0);
        int rotation = Random.Range(0, 4);
        float rotationAngle = rotation * 90.0f;

        newBlockObj = (GameObject)Instantiate(blockSpawns[type].block, vpos, Quaternion.Euler(0f, 0f, rotationAngle));
        Block newBlock = newBlockObj.GetComponent<Block>();
        newBlock.bot = bot;
        newBlock.blockRotation = rotation;

        bool canSpawnHere = false;
        //do
        //{
        ContactFilter2D newFilter = new ContactFilter2D();
        newFilter.NoFilter();
        newFilter.SetLayerMask(1 << LayerMask.NameToLayer("Bit"));
        List<Collider2D> bitHits = new List<Collider2D>();
        newBlockObj.GetComponent<Rigidbody2D>().OverlapCollider(newFilter, bitHits);
        canSpawnHere = true;
        foreach (Collider2D hit in bitHits)
        {
            if (hit.transform.parent != newBlockObj.transform)
            {
                canSpawnHere = false;
                break;
            }
        }
        Rigidbody2D[] childBodies = newBlockObj.GetComponentsInChildren<Rigidbody2D>();
        foreach (Rigidbody2D childBody in childBodies)
        {
            List<Collider2D> childHits = new List<Collider2D>();
            childBody.GetComponent<Rigidbody2D>().OverlapCollider(newFilter, childHits);
            foreach (Collider2D hit in childHits)
            {
                if (hit.transform.parent != newBlockObj.transform)
                {
                    canSpawnHere = false;
                    break;
                }
            }
        }
        if (!canSpawnHere)
        {
            //vpos.x = ScreenStuff.ColToXPosition(Random.Range(-ScreenStuff.screenRadius, ScreenStuff.screenRadius));
            // newBlockObj.transform.position = vpos;
            Destroy(newBlockObj);
            return null;
        }
        //}
        //while (!canSpawnHere);

        blockList.Add(newBlockObj);

        //Added by Carl
        if (enemyPathfinding != null)
            enemyPathfinding.SetDynamicObstacle(newBlockObj);

        return newBlockObj;
    }


    void SpawnBGPanels() {
        for (int x = 0;x<4;x++) {
            bgPanelArr[x] = new GameObject();
            bgPanelArr[x] = Instantiate(bgPanel,new Vector3(0,0,0),Quaternion.identity);
            bgPanelArr[x].transform.localScale = settings.bgScale;
            DontDestroyOnLoad(bgPanelArr[x]);
        }
        bgPanelArr[0].transform.position = new Vector3(0,settings.bgHeight,settings.bgZDepth);
        bgPanelArr[1].transform.position = new Vector3(0,0,settings.bgZDepth);
        bgPanelArr[2].transform.position = new Vector3(settings.bgWidth,settings.bgHeight,settings.bgZDepth);
        bgPanelArr[3].transform.position = new Vector3(settings.bgWidth,0,settings.bgZDepth);
    }
}
