using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }
    public List<GameObject> blockList;

    public static int lives = 1;
    public static int bgAdjustFlag = 0;
    public static int tripleCheckFlag = 0;
    public static int shapeScore = 0;
    
    public GameObject gameOverPanel;
    //public LevelData[] allLevelData;
    public Game game;
    // public LevelData currentGame;
    public Bot bot;
    public GameSettings settings;

    Text levelTimer;
    Text levelNumberString;
    Text shapeScoreString;

    public static float timeRemaining = 10.0f;
    public int currentScene = 1;
    public static int spawnRow = 40;
    //public static int bitCount = 10;

    GameObject BG1;
    GameObject BG2;
    GameObject BG3;
    GameObject BG4;

    int columnNum = ScreenStuff.cols;
  
    int[] eProbArr;

    LevelData levelData;
    BlockSpawnData[] blockSpawns;
   
    float blockSpawnTimer;
    float shapeSpawnRate = 0;
    float shapeSpawnTimer;
    float lastShapeSpawnTime;
    float firstShapeSpawnTime;
    int shapeCount;
    int numberOfShapes;

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
        SpawnBGPanels();
        gameOverPanel.SetActive(false);
        levelNumberString = GameObject.Find("Level").GetComponent<Text>();
        levelTimer = GameObject.Find("Timer").GetComponent<Text>();
        shapeScoreString = GameObject.Find("Shapes").GetComponent<Text>();
        LoadLevelData(1);
    }

    public void Update()
    {
        timeRemaining -= Time.deltaTime;
        levelTimer.text = "Time remaining: " + Mathf.Round(timeRemaining);
        shapeScoreString.text = "Shapes matched: " + shapeScore;
        if (timeRemaining < 0) {
            if (currentScene == 3) {
                levelTimer.enabled = false;
                levelNumberString.enabled = false;
                lives = 0;
            } else {
                currentScene++;
                LoadLevelData(currentScene);
            }
        }
        BlockSpawnCheck();
        ShapeSpawnCheck();
        if (lives == 0)
        {
            GameOver();
        }   

        if(Input.GetKeyDown(KeyCode.Escape)) 
            Application.Quit();

        ScrollBackground();

        foreach (GameObject moveableObject in GameObject.FindGameObjectsWithTag("Moveable")) {
            Vector3 pos = moveableObject.transform.position;

            if (pos.x < ScreenStuff.leftEdgeOfWorld)
                pos.x = ScreenStuff.rightEdgeOfWorld;
            if (pos.x > ScreenStuff.rightEdgeOfWorld)
                pos.x = ScreenStuff.leftEdgeOfWorld;
            moveableObject.transform.position = pos;
            if (pos.y < ScreenStuff.bottomEdgeOfWorld)
                Destroy(moveableObject);
        }
    }

    public int[] GetSpawnProbabilities() {
        int[] pArr = new int[blockSpawns.Length];
        for (int d = 0; d < blockSpawns.Length; d++)
            pArr[d] = blockSpawns[d].probability;
        return pArr;
    }

    public void LoadLevelData(int levelNumber) {
        SceneManager.LoadScene(levelNumber);
        levelNumberString.text = "Level: " + levelNumber;  
        levelData = game.levelDataArr[levelNumber-1];
        blockSpawns = levelData.blocks;
        eProbArr = GetSpawnProbabilities();
        
        blockSpawnTimer = levelData.blockSpawnRate;
        timeRemaining = levelData.levelDuration;
        numberOfShapes = levelData.shapes.Length;
        if (numberOfShapes!=0) {
            lastShapeSpawnTime = levelData.levelDuration * 0.75f;
            firstShapeSpawnTime = lastShapeSpawnTime; 
            if (numberOfShapes > 1) {
                shapeSpawnRate = (levelData.levelDuration * 0.5f) / numberOfShapes;
                firstShapeSpawnTime = levelData.levelDuration *0.25f;
            }
            shapeSpawnTimer = firstShapeSpawnTime;
            shapeCount = 0;
        }
    }

    void ScrollBackground() {
        if (BG1 == null)
            return;

        Vector3 BV1 = BG1.transform.position;
        Vector3 BV2 = BG2.transform.position;
        Vector3 BV3 = BG3.transform.position;
        Vector3 BV4 = BG4.transform.position;

        // scroll Background Down

        BV1 = new Vector3 (BV1.x,BV1.y-settings.bgScrollSpeed,settings.bgZDepth);
        BV2 = new Vector3 (BV2.x,BV2.y-settings.bgScrollSpeed,settings.bgZDepth);
        BV3 = new Vector3 (BV3.x,BV3.y-settings.bgScrollSpeed,settings.bgZDepth);
        BV4 = new Vector3 (BV4.x,BV4.y-settings.bgScrollSpeed,settings.bgZDepth);

        // flip bottom BG to top

        if (BV2.y < BV1.y && BV1.y < 0.0f) {
            BV2 = new Vector3(BV2.x,BV2.y+2*settings.bgHeight,settings.bgZDepth);
            BV4 = new Vector3(BV4.x,BV4.y+2*settings.bgHeight,settings.bgZDepth);
        }
        if (BV1.y < BV2.y && BV2.y < 0.0f) {
            BV1 = new Vector3(BV1.x,BV1.y+2*settings.bgHeight,settings.bgZDepth);
            BV3 = new Vector3(BV3.x,BV3.y+2*settings.bgHeight,settings.bgZDepth);
        }

        // on player move

        if (bgAdjustFlag!=0) {

            float xOffset = ScreenStuff.colSize*bgAdjustFlag;

            // adjust background position

            BV1 = new Vector3(BV1.x+xOffset,BV1.y,settings.bgZDepth);
            BV2 = new Vector3(BV2.x+xOffset,BV2.y,settings.bgZDepth);
            BV3 = new Vector3(BV3.x+xOffset,BV3.y,settings.bgZDepth);
            BV4 = new Vector3(BV4.x+xOffset,BV4.y,settings.bgZDepth);

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

        if (BV1.x < BV3.x && BV3.x < 0.0f) {
            BV1 = new Vector3(BV1.x+2*settings.bgWidth,BV1.y,settings.bgZDepth);
            BV2 = new Vector3(BV2.x+2*settings.bgWidth,BV2.y,settings.bgZDepth);
        }
        if (BV3.x < BV1.x && BV1.x < 0.0f) {
            BV3 = new Vector3(BV3.x+2*settings.bgWidth,BV3.y,settings.bgZDepth);
            BV4 = new Vector3(BV4.x+2*settings.bgWidth,BV4.y,settings.bgZDepth);
        }
    
        // flip right BG to left

        if (BV1.x > BV3.x && BV3.x > 0.0f) {
            BV1 = new Vector3(BV1.x-2*settings.bgWidth,BV1.y,settings.bgZDepth);
            BV2 = new Vector3(BV2.x-2*settings.bgWidth,BV2.y,settings.bgZDepth);
        }
        if (BV3.x > BV1.x && BV1.x > 0.0f) {
            BV3 = new Vector3(BV3.x-2*settings.bgWidth,BV3.y,settings.bgZDepth);
            BV4 = new Vector3(BV4.x-2*settings.bgWidth,BV4.y,settings.bgZDepth);
        }

        BG1.transform.position = BV1;
        BG2.transform.position = BV2;
        BG3.transform.position = BV3;
        BG4.transform.position = BV4;
    }
 
    void ShapeSpawnCheck() {
        if (shapeCount < numberOfShapes) {
            shapeSpawnTimer -= Time.deltaTime;
            if (shapeSpawnTimer <= 0)
            {
                SpawnShape();
                shapeCount++;
                shapeSpawnTimer = shapeSpawnRate;
            }
        }
    }


    void BlockSpawnCheck() {
        blockSpawnTimer -= Time.deltaTime;
        if (blockSpawnTimer<= 0)
        {
            int blockType = ProbabilityPicker(eProbArr);

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

    public GameObject SpawnBlock(int col, int type)
    {
        GameObject newBlock;
      
        Vector3 vpos = new Vector3(ScreenStuff.ColToXPosition(col), ScreenStuff.RowToYPosition(spawnRow), 0);
        float rotationAngle = Random.Range(0,4) * 90.0f;
        newBlock = Instantiate(blockSpawns[type].block, vpos, Quaternion.Euler(0f,0f,rotationAngle));
        newBlock.GetComponent<Block>().bot = bot;
       
        blockList.Add(newBlock);
        return newBlock;
    }
    
    public Shape SpawnShape()
    {
        Shape newShape; 

        int sCol = Random.Range(ScreenStuff.leftEdgeCol,ScreenStuff.rightEdgeCol);
        Vector3 vpos = new Vector3(ScreenStuff.ColToXPosition(sCol), ScreenStuff.RowToYPosition(spawnRow)-10, 0);

        newShape = Instantiate(levelData.shapes[shapeCount],vpos,Quaternion.identity);
        newShape.column = ScreenStuff.WrapCol(sCol,bot.coreCol);
  
        return newShape;
    }

    void GameOver () 
    {
        gameOverPanel.SetActive(true);
    }

    void SpawnBGPanels() {
        BG1 = Instantiate(new GameObject(),new Vector3(0,settings.bgHeight,settings.bgZDepth),Quaternion.identity);
        BG2 = Instantiate(new GameObject(),new Vector3(0,0,settings.bgZDepth),Quaternion.identity);
        BG3 = Instantiate(new GameObject(),new Vector3(settings.bgWidth,settings.bgHeight,settings.bgZDepth),Quaternion.identity);
        BG4 = Instantiate(new GameObject(),new Vector3(settings.bgWidth,0,settings.bgZDepth),Quaternion.identity);
        BG1.AddComponent<SpriteRenderer>();
        BG2.AddComponent<SpriteRenderer>();
        BG3.AddComponent<SpriteRenderer>();
        BG4.AddComponent<SpriteRenderer>();
        BG1.GetComponent<SpriteRenderer>().sprite = settings.bgSprite;
        BG2.GetComponent<SpriteRenderer>().sprite = settings.bgSprite;
        BG3.GetComponent<SpriteRenderer>().sprite = settings.bgSprite;
        BG4.GetComponent<SpriteRenderer>().sprite = settings.bgSprite;
        BG1.transform.localScale = settings.bgScale;
        BG2.transform.localScale = settings.bgScale;
        BG3.transform.localScale = settings.bgScale;
        BG4.transform.localScale = settings.bgScale;
        DontDestroyOnLoad(BG1);
        DontDestroyOnLoad(BG2);
        DontDestroyOnLoad(BG3);
        DontDestroyOnLoad(BG4);
    }
    
}


public class bgPanel : MonoBehaviour {


}
