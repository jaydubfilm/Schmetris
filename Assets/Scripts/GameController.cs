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
    public static event GameEvent OnGameOver, OnGameRestart, OnLoseLife, OnLevelRestart, OnNewLevel, OnSpeedChange;

    public static GameController Instance { get; private set; }
    public List<GameObject> blockList;
    public List<GameObject> enemyList;
    public List<GameObject> bitReference;

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
            moneyString.text = "$" + _money;
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
            UpdateLivesUI();
        }
    }

    public bool isBotDead = false;
    public bool isPaused = false;

    //public static int bgAdjustFlag = 0;
    
    public GameObject gameOverPanel;
    public Text progressText;
    public GameObject restartText;

    public GameObject levelMenu;
    public GameObject pauseMenu;
    public GameObject mainPanel;
    public GameObject helpPanel;
    public GameObject loadPanel;
    public GameObject savePanel;

    public GameObject loseLifePanel;
    public GameObject retryText;

    public GameObject scrapyard;
    public GameObject hud;

    //public LevelData[] allLevelData;
    public Game easyGame;
    public Game mediumGame;
    public Game hardGame;
    public Game game;
    // public LevelData currentGame;
    public Bot bot;
    public GameSettings settings;

    Text levelTimer;
    Text levelNumberString;

    Text quitString;

    Text moneyString;
    public GameObject scoreIncreasePrefab;

    Text noFuelString;
    float noFuelAlpha = 0;

    public Transform livesGroup;
    public GameObject livesIcon;
    List<GameObject> livesUI = new List<GameObject>();

    public static float timeRemaining = 10.0f;
    public int currentScene = 1;
    public static int spawnRow = 25;
    //public static int bitCount = 10;

    public GameObject[] bgPanelArr;
    public GameObject bgPanel;

    int columnNum = ScreenStuff.cols;
  
    int[] blockProbArr;
    int[] speciesProbArr;

    LevelData levelData;
    BlockSpawnData[] blockSpawns;
    public SpeciesSpawnData[] speciesSpawnData;
   
    float blockSpawnTimer = 3f;
    float enemySpawnTimer;
    float enemySpawnRate;

    public float blockSpeed;
    Bounds collisionBubble;

    bool isRestarting = false;
    public SaveManager saveManager = null;
    public Transform[] saveIcons;
    public Transform[] loadIcons;
    public GameObject iconGrid;
    public GameObject iconColumn;
    public GameObject iconTile;
    const string atlasResource = "MasterDiceSprites";
    Sprite[] tilesAtlas;

    Text speedText;
    int speedMultiplier = 2;
    float[] speedOptions = new float[] { 0.5f, 0.75f, 1.0f, 1.5f, 2.0f };
    public float adjustedSpeed
    {
        get
        {
            return speedOptions[speedMultiplier];
        }
    }
   
    void UpdateLivesUI()
    {
        while(_lives < livesUI.Count && livesUI.Count > 0)
        {
            Destroy(livesUI[0]);
            livesUI.RemoveAt(0);
        }
        while(_lives > livesUI.Count)
        {
            livesUI.Add(Instantiate(livesIcon, livesGroup));
        }
    }

    public void EndGame(string endgameMessage)
    {
        if (!isBotDead)
        {
            isBotDead = true;
            gameOverPanel.GetComponent<Text>().text = endgameMessage + " - Game Over";
            loseLifePanel.GetComponent<Text>().text = endgameMessage + " - Life Lost";
            lives--;
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
            RefreshBotIcons();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start () {
        scrapyard.SetActive(false);
        pauseMenu.SetActive(false);
        levelMenu.SetActive(true);
        isPaused = true;
        Time.timeScale = 0;
        lives = 3;
        bgPanelArr = new GameObject[4];
        SpawnBGPanels();
        loseLifePanel.SetActive(false);
        retryText.SetActive(false);
        gameOverPanel.SetActive(false);
        restartText.SetActive(false);
        levelNumberString = GameObject.Find("Level").GetComponent<Text>();
        levelTimer = GameObject.Find("Timer").GetComponent<Text>();
        quitString = GameObject.Find("Quit").GetComponent<Text>();
        moneyString = GameObject.Find("Money").GetComponent<Text>();
        noFuelString = GameObject.Find("NoFuel").GetComponent<Text>();
        speedText = GameObject.Find("Speed").GetComponent<Text>();
#if UNITY_IOS || UNITY_ANDROID
        quitString.enabled = false;
#else
        quitString.GetComponentInChildren<Image>().enabled = false;
        GameObject.Find("SpeedUp").SetActive(false);
        GameObject.Find("SpeedDown").SetActive(false);
#endif
        hud.SetActive(false);
    }

    void RefreshBotIcons()
    {
        for (int i = 0; i < saveIcons.Length; i++)
        {
            BuildBotIcon(i, saveIcons[i]);
        }

        for (int i = 0; i < loadIcons.Length; i++)
        {
            BuildBotIcon(i, loadIcons[i]);
        }
    }

    void BuildBotIcon(int index, Transform target )
    {
        if(target.GetComponentInChildren<VerticalLayoutGroup>())
        {
            Destroy(target.GetComponentInChildren<VerticalLayoutGroup>().gameObject);
        }

        SaveData targetFile = saveManager.GetSave(index);
        if(targetFile != null && targetFile.game != "" && targetFile.bot.Length > 0)
        {
            GameObject newGrid = Instantiate(iconGrid, target);
            newGrid.GetComponent<RectTransform>().anchoredPosition = new Vector2(10, 0);

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
                for (int y = maxY; y >= minY; y--)
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

    void StartGame()
    {
        bot.gameObject.SetActive(true);
        LoadLevelData(1);
        InvokeRepeating("GameOverCheck", 1.0f, 0.2f);
    }

    public void SaveGame(int index)
    {
        saveManager.SetSave(index, lives, money, currentScene, game.name, bot.GetTileMap());
        RefreshBotIcons();
    }

    public void LoadGame(int index)
    {
        SaveData loadData = saveManager.GetSave(index);
        if (loadData != null && loadData.game != "")
        {
            lives = loadData.lives;
            money = loadData.money;
            currentScene = loadData.level;

            //~Add in resource loading?
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

            //~Best way to load a tilemap?
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

            LoadLevelData(currentScene);
            InvokeRepeating("GameOverCheck", 1.0f, 0.2f);
        }
        else
        {
            StartGame();
        }
    }

    //Used for external Canvas buttons for touchscreen controls
    public void SpeedUp()
    {
        speedMultiplier = Mathf.Min(speedMultiplier + 1, speedOptions.Length - 1);
        speedText.text = "x" + adjustedSpeed.ToString() + " Speed";
        if (OnSpeedChange != null)
        {
            OnSpeedChange();
        }
    }

    //Used for external Canvas buttons for touchscreen controls
    public void SpeedDown()
    {
        speedMultiplier = Mathf.Max(speedMultiplier - 1, 0);
        speedText.text = "x" + adjustedSpeed.ToString() + " Speed";
        if (OnSpeedChange != null)
        {
            OnSpeedChange();
        }
    }

    //Used for external Canvas buttons for touchscreen controls
    public void EasyGame()
    {
        game = easyGame;
        StartGame();
    }

    //Used for external Canvas buttons for touchscreen controls
    public void MediumGame()
    {
        game = mediumGame;
        StartGame();
    }

    //Used for external Canvas buttons for touchscreen controls
    public void HardGame()
    {
        game = hardGame;
        StartGame();
    }

    //Used for external Canvas buttons for touchscreen controls
    public void PauseGame()
    {
        hud.SetActive(false);
        mainPanel.SetActive(true);
        helpPanel.SetActive(false);
        pauseMenu.SetActive(true);
        savePanel.SetActive(false);
        loadPanel.SetActive(false);
        isPaused = true;
        Time.timeScale = 0;
    }

    //Used for external Canvas buttons for touchscreen controls
    public void ResumeGame()
    {
        hud.SetActive(true);
        pauseMenu.SetActive(false);
        isPaused = false;
        Time.timeScale = 1.0f;
    }

    //Used for external Canvas buttons for touchscreen controls
    public void RestartGame()
    {
        lives = 0;
        isBotDead = true;
        gameOverPanel.GetComponent<Text>().text = "Game Over";
        loseLifePanel.GetComponent<Text>().text = "Life Lost";
        StartCoroutine(RestartLevelOnDelay());
    }

    //Used for external Canvas buttons for touchscreen controls
    public void HelpMenu()
    {
        mainPanel.SetActive(false);
        helpPanel.SetActive(true);
    }

    //Used for external Canvas buttons for touchscreen controls
    public void PauseMenu()
    {
        helpPanel.SetActive(false);
        savePanel.SetActive(false);
        loadPanel.SetActive(false);
        mainPanel.SetActive(true);
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
        LoadLevelData(currentScene);
    }

    void LoadScrapyard()
    {
        hud.SetActive(false);
        isPaused = true;
        Time.timeScale = 0;
        bot.OnNewLevel();
        bot.gameObject.SetActive(false);
        SceneManager.LoadScene(1);
        scrapyard.SetActive(true);
        scrapyard.GetComponent<Scrapyard>().UpdateScrapyard();
    }

    public void Update()
    {
        if (!isBotDead && !isPaused)
        {
            //Adjust game speed
            if (Input.GetKeyDown(KeyCode.Equals))
            {
                SpeedUp();
            }
            else if (Input.GetKeyDown(KeyCode.Minus))
            {
                SpeedDown();
            }

            timeRemaining -= Time.deltaTime;
            levelTimer.text = "Time remaining: " + Mathf.Round(timeRemaining);
            if (timeRemaining < 0)
            {
                /*if (currentScene > game.levelDataArr.Length)
                {
                    levelTimer.enabled = false;
                    levelNumberString.enabled = false;
                    GameController.Instance.EndGame("OUT OF LEVELS");
                }
                else
                {*/
                LoadScrapyard();
                //}
            }

            BlockSpawnCheck();

            if (enemySpawnRate > 0)
                EnemySpawnCheck();
        }

        if (isPaused)
        {
            if(scrapyard.activeSelf)
            {

            }
            else if(levelMenu.activeSelf)
            {
                if(Input.GetKeyDown(KeyCode.Alpha1))
                {
                    EasyGame();
                }
                else if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    MediumGame();
                }
                else if (Input.GetKeyDown(KeyCode.Alpha3))
                {
                    HardGame();
                }
                else if (Input.GetKeyDown(KeyCode.Alpha4))
                {
                    QuitGame();
                }
            }
            else if(helpPanel.activeSelf)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    PauseMenu();
                }
            }
            else if (savePanel.activeSelf)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    SaveGame(0);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    SaveGame(1);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha3))
                {
                    SaveGame(2);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha4))
                {
                    PauseMenu();
                }
            }
            else if(loadPanel.activeSelf)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    LoadGame(0);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    LoadGame(1);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha3))
                {
                    LoadGame(2);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha4))
                {
                    PauseMenu();
                }
            }
            else if(Input.GetKeyDown(KeyCode.Alpha1))
            {
                ResumeGame();
            }
            /*else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                EndGame("Life Lost");
                StartCoroutine(ReplayOnLevelDelay());
            }*/
            else if (!isRestarting && Input.GetKeyDown(KeyCode.Alpha2))
            {
                RestartGame();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                HelpMenu();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                SaveMenu();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                LoadMenu();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                QuitGame();
            }
        }
        else
        {
            if(Input.GetKeyDown(KeyCode.Escape))
            {
                PauseGame();
            }
        }

        if(!isRestarting && restartText.activeSelf && Input.anyKeyDown)
        {
            isRestarting = true;
            Restart();
        }
        else if(retryText.activeSelf && Input.anyKeyDown)
        {
            ReplayLevel();
        }

        noFuelAlpha = Mathf.Max(0, noFuelAlpha - Time.deltaTime);
        noFuelString.color = new Color(1, 1, 1, noFuelAlpha);

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

    public void LoadMenu()
    {
        loadPanel.SetActive(true);
        mainPanel.SetActive(false);
    }

    public void SaveMenu()
    {
        savePanel.SetActive(true);
        mainPanel.SetActive(false);
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

    IEnumerator ReplayOnLevelDelay()
    {
        pauseMenu.SetActive(false);
        isPaused = false;
        Time.timeScale = 1.0f;
        yield return new WaitForSecondsRealtime(1.0f);
        ReplayLevel();
    }

    IEnumerator RestartLevelOnDelay()
    {
        isRestarting = true;
        pauseMenu.SetActive(false);
        isPaused = false;
        Time.timeScale = 1.0f;
        if (OnGameOver != null)
        {
            OnGameOver();
        }
        yield return new WaitForSecondsRealtime(1.0f);
        Restart();
    }

    public void NoFuelMessage()
    {
        noFuelAlpha = 1;
        noFuelString.color = Color.white;
    }

    public void CreateFloatingText(string message, Vector3 worldPos)
    {
        GameObject scoreFX = Instantiate(scoreIncreasePrefab);
        scoreFX.transform.SetParent(GetComponentInChildren<Canvas>().transform);
        scoreFX.transform.rotation = Quaternion.identity;
        scoreFX.transform.position = Camera.main.WorldToScreenPoint(worldPos);
        scoreFX.GetComponent<FloatingText>().Init(message, moneyString.transform.position);
    }

    //Like restart but resets only the current level - for when player has lost a life but not gotten a game over
    void ReplayLevel()
    {
        isBotDead = false;
        loseLifePanel.SetActive(false);
        retryText.SetActive(false);
        gameOverPanel.SetActive(false);
        restartText.SetActive(false);
        LoadLevelData(currentScene);
        if(OnLevelRestart != null)
        {
            OnLevelRestart();
        }
    }

    void Restart()
    {
        isBotDead = false;
        lives = 3;
        money = 0;
        loseLifePanel.SetActive(false);
        retryText.SetActive(false);
        gameOverPanel.SetActive(false);
        restartText.SetActive(false);
        isRestarting = false;
        currentScene = 1;
        LoadLevelData(currentScene);
        if(OnGameRestart != null)
        {
            OnGameRestart();
        }
    }

    IEnumerator DelayedRestart()
    {
        yield return new WaitForSeconds(2.0f);
        restartText.SetActive(true);
    }

    IEnumerator DelayedReload()
    {
        yield return new WaitForSeconds(2.0f);
        retryText.SetActive(true);
    }

    void GameOverCheck(){
        if (lives == 0)
        {
            if(!gameOverPanel.activeSelf && !isRestarting)
            {
                progressText.text = "Level " + currentScene + " attained. $" + money + " Salvaged.";
                StartCoroutine(DelayedRestart());
                if (OnGameOver != null)
                {
                    OnGameOver();
                }
                gameOverPanel.SetActive(true);
            }
        }
        else if (isBotDead && !isRestarting)
        {
            if (!loseLifePanel.activeSelf)
            {
                StartCoroutine(DelayedReload());
                if (OnLoseLife != null)
                {
                    OnLoseLife();
                }
                loseLifePanel.SetActive(true);
            }
        }
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

    public void LoadLevelData(int levelNumber) {
        hud.SetActive(true);
        SceneManager.LoadScene(Mathf.Min(SceneManager.sceneCountInBuildSettings - 1,levelNumber));
        levelNumberString.text = "Level: " + levelNumber;  
        levelData = game.levelDataArr[levelNumber-1];
        blockSpawns = levelData.blocks;
        speciesSpawnData = levelData.speciesSpawnData;
        blockSpeed = levelData.blockSpeed;
        blockProbArr = GetSpawnProbabilities();
        speciesProbArr = GetSpeciesProbabilities();
        
        blockSpawnTimer = levelData.blockSpawnRate;
        enemySpawnRate = levelData.enemySpawnRate;
        enemySpawnTimer = enemySpawnRate;
        timeRemaining = levelData.levelDuration;

        levelMenu.SetActive(false);
        pauseMenu.SetActive(false);
        isPaused = false;
        Time.timeScale = 1.0f;
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
    }

    void EnemySpawnCheck() {
        enemySpawnTimer -= Time.deltaTime * adjustedSpeed;
        if (enemySpawnTimer <= 0)
        {
            int spawnType = ProbabilityPicker(speciesProbArr);
            enemySpawnTimer = enemySpawnRate;
            SpawnEnemy(spawnType);
        }
    }

    void BlockSpawnCheck() {
        blockSpawnTimer -= Time.deltaTime * adjustedSpeed;
        if (blockSpawnTimer<= 0)
        {
            int blockType = ProbabilityPicker(blockProbArr);

            blockSpawnTimer = levelData.blockSpawnRate;
            SpawnBlock(Random.Range(-ScreenStuff.screenRadius,ScreenStuff.screenRadius), blockType);
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

        return newEnemyObj;
    }


    public GameObject SpawnBlock(int col, int type)
    {
        GameObject newBlockObj;
        
        Vector3 vpos = new Vector3(ScreenStuff.ColToXPosition(col), ScreenStuff.RowToYPosition(spawnRow), 0);
        int rotation = Random.Range(0,4);
        float rotationAngle = rotation * 90.0f;
        
        newBlockObj = (GameObject) Instantiate(blockSpawns[type].block, vpos, Quaternion.Euler(0f,0f,rotationAngle));
        Block newBlock = newBlockObj.GetComponent<Block>();
        newBlock.bot = bot;
        newBlock.blockRotation = rotation;
     
        blockList.Add(newBlockObj);

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
