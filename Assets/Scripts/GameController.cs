using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
  
    public static int lives = 1;
    public static int bgAdjustFlag = 0;
    public static int tripleCheckFlag = 0;
    
    public GameObject gameOverPanel;
    public LevelData[] allLevelData;
    

    Text levelTimer;
    Text levelNumberString;

    public static float timeRemaining = 10.0f;
    public int currentScene = 1;
    public static int spawnRow = 40;
    public static int bitCount = 10;

    public GameObject BG1;
    public GameObject BG2;
    public GameObject BG3;
    public GameObject BG4;

    float bgHeight = 278;
    float bgWidth = 388;
    float bgZDepth = 400;
    float bgScrollSpeed = 0.1f;

    int columnNum = ScreenStuff.cols;
    public static int maxShapeCellCount = 3;
  
    int[] eProbArr;

    LevelData levelData;
    SpawnData[] spawns;

    public int[] shapeCellCountProbArr = new int[maxShapeCellCount];
   
    float spawnTimer;


    void Awake()
    {
        gameOverPanel.SetActive(false);
        DontDestroyOnLoad(this.gameObject);
        levelNumberString = GameObject.Find("Level").GetComponent<Text>();
        levelTimer = GameObject.Find("Timer").GetComponent<Text>();
        LoadLevelData(1);
    }


    public void Update()
    {
        CheckForSpawn();
 
        timeRemaining -= Time.deltaTime;
        levelTimer.text = "Time remaining: " + Mathf.Round(timeRemaining);
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
            if (transform.position.y < ScreenStuff.bottomEdgeOfWorld)
                Destroy(gameObject);
        }
    }

    public int[] GetSpawnProbabilities() {
        int[] pArr = new int[spawns.Length];
        for (int d = 0; d < spawns.Length; d++)
            pArr[d] = spawns[d].probability;
        return pArr;
    }

    public void LoadLevelData(int levelNumber) {
        SceneManager.LoadScene(levelNumber);
        levelNumberString.text = "Level: " + levelNumber;  
        levelData = allLevelData[levelNumber-1];
        spawns = levelData.spawns;
        eProbArr = GetSpawnProbabilities();
        spawnTimer = levelData.spawnRate;
        timeRemaining = levelData.levelDuration;
    }


    void ScrollBackground() {

        Vector3 BV1 = BG1.transform.position;
        Vector3 BV2 = BG2.transform.position;
        Vector3 BV3 = BG3.transform.position;
        Vector3 BV4 = BG4.transform.position;

        // scroll Background Down

        BV1 = new Vector3 (BV1.x,BV1.y-bgScrollSpeed,bgZDepth);
        BV2 = new Vector3 (BV2.x,BV2.y-bgScrollSpeed,bgZDepth);
        BV3 = new Vector3 (BV3.x,BV3.y-bgScrollSpeed,bgZDepth);
        BV4 = new Vector3 (BV4.x,BV4.y-bgScrollSpeed,bgZDepth);

        // flip bottom BG to top

        if (BV2.y < BV1.y && BV1.y < 0.0f) {
            BV2 = new Vector3(BV2.x,BV2.y+2*bgHeight,bgZDepth);
            BV4 = new Vector3(BV4.x,BV4.y+2*bgHeight,bgZDepth);
        }
        if (BV1.y < BV2.y && BV2.y < 0.0f) {
            BV1 = new Vector3(BV1.x,BV1.y+2*bgHeight,bgZDepth);
            BV3 = new Vector3(BV3.x,BV3.y+2*bgHeight,bgZDepth);
        }

        // on player move

        if (bgAdjustFlag!=0) {

            float xOffset = ScreenStuff.colSize*bgAdjustFlag;

            // adjust background position

            BV1 = new Vector3(BV1.x+xOffset,BV1.y,bgZDepth);
            BV2 = new Vector3(BV2.x+xOffset,BV2.y,bgZDepth);
            BV3 = new Vector3(BV3.x+xOffset,BV3.y,bgZDepth);
            BV4 = new Vector3(BV4.x+xOffset,BV4.y,bgZDepth);

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
            BV1 = new Vector3(BV1.x+2*bgWidth,BV1.y,bgZDepth);
            BV2 = new Vector3(BV2.x+2*bgWidth,BV2.y,bgZDepth);
        }
        if (BV3.x < BV1.x && BV1.x < 0.0f) {
            BV3 = new Vector3(BV3.x+2*bgWidth,BV3.y,bgZDepth);
            BV4 = new Vector3(BV4.x+2*bgWidth,BV4.y,bgZDepth);
        }
    
        // flip right BG to left

        if (BV1.x > BV3.x && BV3.x > 0.0f) {
            BV1 = new Vector3(BV1.x-2*bgWidth,BV1.y,bgZDepth);
            BV2 = new Vector3(BV2.x-2*bgWidth,BV2.y,bgZDepth);
        }
        if (BV3.x > BV1.x && BV1.x > 0.0f) {
            BV3 = new Vector3(BV3.x-2*bgWidth,BV3.y,bgZDepth);
            BV4 = new Vector3(BV4.x-2*bgWidth,BV4.y,bgZDepth);
        }

        BG1.transform.position = BV1;
        BG2.transform.position = BV2;
        BG3.transform.position = BV3;
        BG4.transform.position = BV4;
    }
/* 
 
    void ShapeSpawnCheck() {
        shapeSpawnTimer -= Time.deltaTime;
        if (shapeSpawnTimer <= 0)
        {
            shapeSpawnTimer = shapeSpawnRate;
            SpawnRandomSizeShape();
        }
    }
*/

    void CheckForSpawn() {
        spawnTimer -= Time.deltaTime;
        if (spawnTimer<= 0)
        {
            int blockType = ProbabilityPicker(eProbArr);
            spawnTimer = levelData.spawnRate;
            SpawnBlock(Random.Range(1, columnNum), blockType);
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

        newBlock = Instantiate(spawns[type].block, vpos, Quaternion.identity);

        newBlock.GetComponent<Block>().column = col;

        return newBlock;
    }
    
/* 
    public GameObject SpawnShape(int cellCount)
    {
        GameObject newShape; 

        int sCol = Random.Range(1,columnNum);
      
        Vector3 vpos = new Vector3(ScreenStuff.ColToXPosition(sCol), ScreenStuff.RowToYPosition(spawnRow), 0);

        newShape = Instantiate(shape,vpos,Quaternion.identity);
        newShape.GetComponent<Shape>().AddSeedCell();
        
        for (int x = 1; x < cellCount; x++){ 
            newShape.GetComponent<Shape>().AddRandomCell();
        } 
        return newShape;
    }

    void SpawnRandomSizeShape()
    {
        int r = ProbabilityPicker(shapeCellCountProbArr);
        SpawnShape(r+1);
    }
*/
    void GameOver () 
    {
        Debug.Log("Game Over");
        gameOverPanel.SetActive(true);
    }
    
}
