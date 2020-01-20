using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    //Game-wide events - Individual assets can connect to these to perform actions on specific game/level events (end, restart, etc)
    public delegate void GameEvent();
    public static event GameEvent OnGameOver, OnGameRestart;

    public static GameController Instance { get; private set; }
    public List<GameObject> blockList;
    public List<GameObject> enemyList;
    

    public int lives;
    public static int bgAdjustFlag = 0;
  

    
    public GameObject gameOverPanel;
    //public LevelData[] allLevelData;
    public Game game;
    // public LevelData currentGame;
    public Bot bot;
    public GameSettings settings;

    Text levelTimer;
    Text levelNumberString;

    Text quitString;

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
   

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start () {
        lives = 1;
        bgPanelArr = new GameObject[4];
        SpawnBGPanels();
        gameOverPanel.SetActive(false);
        levelNumberString = GameObject.Find("Level").GetComponent<Text>();
        levelTimer = GameObject.Find("Timer").GetComponent<Text>();
        quitString = GameObject.Find("Quit").GetComponent<Text>();
        LoadLevelData(1);
        InvokeRepeating("GameOverCheck",1.0f,0.2f);
    }

    public void Update()
    {
        if (lives > 0)
        {
            timeRemaining -= Time.deltaTime;
            levelTimer.text = "Time remaining: " + Mathf.Round(timeRemaining);
            if (timeRemaining < 0)
            {
                if (currentScene == 3)
                {
                    levelTimer.enabled = false;
                    levelNumberString.enabled = false;
                    lives = 0;
                }
                else
                {
                    currentScene++;
                    LoadLevelData(currentScene);
                }
            }

            BlockSpawnCheck();

            if (enemySpawnRate > 0)
                EnemySpawnCheck();
        }

        if(Input.GetKeyDown(KeyCode.Escape)) 
            Application.Quit();

        if(Input.GetKeyDown(KeyCode.Space) && gameOverPanel.activeSelf)
        {
            Restart();
        }

        ScrollBackground();

        foreach (GameObject moveableObject in GameObject.FindGameObjectsWithTag("Moveable")) {
            Vector3 pos = moveableObject.transform.position;

            if (pos.x < ScreenStuff.leftEdgeOfWorld-ScreenStuff.colSize)
                pos.x = ScreenStuff.rightEdgeOfWorld;
            if (pos.x > ScreenStuff.rightEdgeOfWorld+ScreenStuff.colSize)
                pos.x = ScreenStuff.leftEdgeOfWorld;
            moveableObject.transform.position = pos;
            if (pos.y < ScreenStuff.bottomEdgeOfWorld) {
                Block block = moveableObject.GetComponent<Block>();
                if (block!=null) {
                    block.DestroyBlock();
                } else 
                    Destroy(moveableObject);
            }
        }
    }

    void Restart()
    {
        lives = 1;
        gameOverPanel.SetActive(false);
        LoadLevelData(1);
        if(OnGameRestart != null)
        {
            OnGameRestart();
        }
    }

    void GameOverCheck(){
        if (lives == 0)
        {
            if(!gameOverPanel.activeSelf && OnGameOver != null)
            {
                OnGameOver();
            }
            gameOverPanel.SetActive(true);
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
        SceneManager.LoadScene(levelNumber);
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
            bV3[x] += new Vector3 (0,-settings.bgScrollSpeed,0);
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

        if (bgAdjustFlag!=0) {
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
        }

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


    void EnemySpawnCheck() {
        enemySpawnTimer -= Time.deltaTime;
        if (enemySpawnTimer <= 0)
        {
            int spawnType = ProbabilityPicker(speciesProbArr);
            enemySpawnTimer = enemySpawnRate;
            SpawnEnemy(spawnType);
        }
    }

    void BlockSpawnCheck() {
        blockSpawnTimer -= Time.deltaTime;
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
