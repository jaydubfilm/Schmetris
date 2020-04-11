using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum ResourceType
{
    None,
    Red,
    Blue,
    Green,
    Yellow,
    Grey
};

[System.Serializable]
public class Bot : MonoBehaviour
{ 
    public float longPause;
    public float shortPause;

    public int coreCol = 0;
    public int maxBotRadius;

    public bool orphanCheckFlag = false;
    
    public bool tripleCheckFlag = false;

    public bool powerGridRefreshFlag = false;

    public Rigidbody2D botBody;

    [HideInInspector]
    public Quaternion rotation1;
    
    [HideInInspector]
    public Quaternion rotation2;

    bool isRotating = false;

    [HideInInspector]
    public Vector2Int[] directionV2Arr = new [] {
        new Vector2Int (0,1),
        new Vector2Int (1,0),
        new Vector2Int (0,-1),
        new Vector2Int (-1,0)};

    float coreX = 0.0f;
    float coreY = 0.0f;
    public int maxBotWidth;
    public int maxBotHeight;
 
    public GameObject[,] brickArr;
    public int[,] brickTypeArr;
    public GameObject[] masterBrickList;
    public List<GameObject> brickList;
    public List<GameObject> fuelBrickList = new List<GameObject>();

    //These bricks require resources to run
    public List<Brick> resourceBurnBricks = new List<Brick>();

    int[,] pathArr;
    public PowerGrid powerGrid;
    public PowerGrid powerGridPrefab;

    public Vector2Int coreV2;
    public int botRotation = 0;
    
    GameObject coreBrick; 
    Tilemap startTileMap;
    Sprite[,] savedTileMap;
    Sprite[,] startSprites;
    public Grid startingBrickGrid;
    public GameObject blockPrefab;
    public GameObject bitPrefab;
    public GameObject powerWarning;
    public bool hasDamagedCells = false;

    private List<GameObject> pathList = new List<GameObject>();
    private List<Vector2Int> pathArrList = new List<Vector2Int>();
    List<Container> containerList = new List<Container>();
    public List<ContainerData> savedContainerData = new List<ContainerData>();

    private AudioSource source;
    public AudioClip tripleSound;
    public AudioClip resourceSound;
    public AudioClip brickAttachSound;
    public AudioClip brickDestroySound;
    public AudioClip brickBumpSound;
    public bool queueDestroyedBrick = false;
    GameSettings settings;

    float startTime;
    public float tripleDelay = 0.5f;
    float delay;
    bool isReset = false;

    //Resources
    const float startRed = 30.0f;
    const float startRedTutorial = 500f;
    const float startBlue = 0;
    const float startGreen = 0;
    const float startYellow = 0;
    const float startGrey = 0;
    float totalCapacity = 0;
    float hangarRed = 0;
    float hangarBlue = 0;
    float hangarGreen = 0;
    float hangarYellow = 0;
    float hangarGrey = 0;
    public float totalReddite = 0;
    public float totalBlueSalt = 0;
    public float totalGreenAlgae = 0;
    public float totalYellectrons = 0;
    public float totalGreyscale = 0;

    float _storedRed = 0;
    public float storedRed
    {
        set
        {
            if (value > 0 && value > totalCapacity)
            {
                _storedRed = totalCapacity;
                hangarRed += value - totalCapacity;
            }
            else
            {
                _storedRed = Mathf.Max(0, value);
            }
        }
        get
        {
            return _storedRed;
        }
    }

    float _storedBlue = 0;
    public float storedBlue
    {
        set
        {
            if (value > 0 && value > totalCapacity)
            {
                _storedBlue = totalCapacity;
                hangarBlue += value - totalCapacity;
            }
            else
            {
                _storedBlue = Mathf.Max(0, value);
            }
        }
        get
        {
            return _storedBlue;
        }
    }

    float _storedGreen = 0;
    public float storedGreen
    {
        set
        {
            if (value > 0 && value > totalCapacity)
            {
                _storedGreen = totalCapacity;
                hangarGreen += value - totalCapacity;
            }
            else
            {
                _storedGreen = Mathf.Max(0, value);
            }
        }
        get
        {
            return _storedGreen;
        }
    }

    float _storedYellow = 0;
    public float storedYellow
    {
        set
        {
            if (value > 0 && value > totalCapacity)
            {
                _storedYellow = totalCapacity;
                hangarYellow += value - totalCapacity;
            }
            else
            {
                _storedYellow = Mathf.Max(0, value);
            }
        }
        get
        {
            return _storedYellow;
        }
    }

    float _storedGrey = 0;
    public float storedGrey
    {
        set
        {
            if (value > 0 && value > totalCapacity)
            {
                _storedGrey = totalCapacity;
                hangarGrey += value - totalCapacity;
            }
            else
            {
                _storedGrey = Mathf.Max(0, value);
            }
        }
        get
        {
            return _storedGrey;
        }
    }

    float totalResources
    {
        get
        {
            return storedRed + storedBlue + storedGreen + storedYellow + storedGrey;
        }
    }

    public void UpdateContainers()
    {
        float newCapacity = 0;
        foreach(Container ContainerCheck in containerList)
        {
            newCapacity += ContainerCheck.capacity[ContainerCheck.GetComponent<Brick>().brickLevel];
        }

        totalCapacity = newCapacity;
        storedRed = storedRed;
        storedBlue = storedBlue;
        storedYellow = storedYellow;
        storedGreen = storedGreen;
        storedGrey = storedGrey;
    }

    public void AddContainer(Container container)
    {
        if(!containerList.Contains(container))
        {
            containerList.Add(container);
            totalCapacity += container.capacity[container.GetComponent<Brick>().brickLevel];
        }
    }

    public void RemoveContainer(Container container)
    {
        if(containerList.Contains(container))
        {
            containerList.Remove(container);

            totalCapacity -= container.capacity[container.GetComponent<Brick>().brickLevel];
            storedRed = storedRed;
            storedBlue = storedBlue;
            storedYellow = storedYellow;
            storedGreen = storedGreen;
            storedGrey = storedGrey;
        }
    }

    public void SetTutorialStart()
    {
        totalReddite = startRedTutorial;
    }

    public void ResetTileMap()
    {
        SetTileMap(startSprites);
        totalReddite = startRed;
        totalGreenAlgae = startGreen;
        totalGreyscale = startGrey;
        totalYellectrons = startYellow;
        totalBlueSalt = startBlue;

        savedContainerData = new List<ContainerData>();
    }

    public Sprite[,] GetTileMap()
    {
        return savedTileMap;
    }

    bool tileMapSet = false;
    public void SetTileMap(Sprite[,] setTileMap)
    {
        savedTileMap = setTileMap;
        if (init)
        {
            while (brickList.Count > 0)
            {
                GameObject brick = brickList[0];
                if (brick)
                {
                    Brick newBrick = brick.GetComponent<Brick>();
                    SetBrickAtBotArr(newBrick.arrPos, null);
                    brickTypeArr[newBrick.arrPos.x, newBrick.arrPos.y] = -1;
                    if (newBrick.IsParasite())
                        GameController.Instance.enemyList.Remove(gameObject);
                }
                if (brickList.Contains(brick))
                {
                    brickList.Remove(brick);
                    Destroy(brick);
                }
            }
            
            while(containerList.Count > 0)
            {
                RemoveContainer(containerList[0]);
            }
            containerList = new List<Container>();
            resourceBurnBricks = new List<Brick>();
            fuelBrickList = new List<GameObject>();

            if (powerGrid)
                Destroy(powerGrid.gameObject);
            //OnLevelRestart();
        }
        else
        {
            tileMapSet = true;
        }
    }

    void OnGameOver()
    {
        while(brickList.Count > 0)
        {   
            GameObject brick = brickList[0];
            if(brick)
                brick.GetComponent<Brick>().ExplodeBrick();
            if(brickList.Contains(brick))
            {
                brickList.Remove(brick);
            }
        }
        Destroy(powerGrid.gameObject);
    }

    void OnLoseLife()
    {
        while (brickList.Count > 0)
        {
            GameObject brick = brickList[0];
            if(brick)
                brick.GetComponent<Brick>().ExplodeBrick();
            if (brickList.Contains(brick))
            {
                brickList.Remove(brick);
            }
        }
        Destroy(powerGrid.gameObject);
    }

    public void OnNewLevel()
    {
        savedTileMap = new Sprite[maxBotWidth, maxBotHeight];
        foreach (GameObject Brick in brickList)
        {
            if(Brick && Brick.GetComponent<Brick>())
            {
                //~What to do about parasites?
                Vector2Int brickPos = Brick.GetComponent<Brick>().arrPos;
                if(!Brick.GetComponent<Brick>().IsParasite())
                    savedTileMap[brickPos.x, brickPos.y] = Brick.GetComponent<SpriteRenderer>().sprite;
            }
        }

        savedContainerData = new List<ContainerData>();
        foreach(Container container in containerList)
        {
            if (container.canCollect)
            {
                ContainerData newData = new ContainerData();
                newData.coords = container.GetComponent<Brick>().arrPos;
                newData.openDirection = container.GetOpenDirection();
                savedContainerData.Add(newData);
            }
        }
    }

    public void SaveStartSprites()
    {
        startSprites = new Sprite[maxBotWidth, maxBotHeight];
        foreach (GameObject Brick in brickList)
        {
            if (Brick && Brick.GetComponent<Brick>())
            {
                //~What to do about parasites?
                Vector2Int brickPos = Brick.GetComponent<Brick>().arrPos;
                if (!Brick.GetComponent<Brick>().IsParasite())
                    startSprites[brickPos.x, brickPos.y] = Brick.GetComponent<SpriteRenderer>().sprite;
            }
        }
    }

    void OnGameRestart()
    {
        coreBrick = masterBrickList[0];
        coreV2 = new Vector2Int(maxBotRadius, maxBotRadius);
        brickArr = new GameObject[maxBotWidth, maxBotHeight];
        brickTypeArr = new int[maxBotWidth, maxBotHeight];
        savedTileMap = new Sprite[maxBotWidth, maxBotHeight];
        gameObject.transform.position = new Vector3(coreX, coreY, 0);
        gameObject.transform.rotation = Quaternion.identity;

        for (int x = 0; x < maxBotWidth; x++)
            for (int y = 0; y < maxBotHeight; y++)
            {
                brickTypeArr[x, y] = -1;
            }
        botRotation = 0;
        isRotating = false;
        powerGrid = Instantiate(powerGridPrefab, gameObject.transform);

        startTileMap = Instantiate(startingBrickGrid.GetComponent<Tilemap>(), new Vector3(0, 0, 0), Quaternion.identity);
        AddStartingBricks();
        powerGridRefreshFlag = true;

        if(tileMapSet)
        {
            OnLevelRestart();
        }

        OnNewLevel();
    }

    //Rebuild player's available resources from total supply
    public void LoadBotResources()
    {
        hangarRed = 0;
        hangarBlue = 0;
        hangarGreen = 0;
        hangarGrey = 0;
        hangarYellow = 0;
        storedRed = totalReddite;
        storedBlue = totalBlueSalt;
        storedGreen = totalGreenAlgae;
        storedYellow = totalYellectrons;
        storedGrey = totalGreyscale;
    }

    //Save player's available resources to total supply
    public void SaveBotResources()
    {
        totalReddite = storedRed + hangarRed;
        totalBlueSalt = storedBlue + hangarBlue;
        totalGreenAlgae = storedGreen + hangarGreen;
        totalYellectrons = storedYellow + hangarYellow;
        totalGreyscale = storedGrey + hangarGrey;
    }

    //Rebuild player's bot from the start of this level
    public void OnLevelRestart()
    {
        coreBrick = masterBrickList[0];
        coreV2 = new Vector2Int(maxBotRadius, maxBotRadius);
        brickArr = new GameObject[maxBotWidth, maxBotHeight];
        brickTypeArr = new int[maxBotWidth, maxBotHeight];
        gameObject.transform.position = new Vector3(coreX, coreY, 0);
        gameObject.transform.rotation = Quaternion.identity;

        for (int x = 0; x < maxBotWidth; x++)
            for (int y = 0; y < maxBotHeight; y++)
            {
                brickTypeArr[x, y] = -1;
            }
        botRotation = 0;
        isRotating = false;
        powerGrid = Instantiate(powerGridPrefab, gameObject.transform);

        startTileMap = Instantiate(startingBrickGrid.GetComponent<Tilemap>(), new Vector3(0, 0, 0), Quaternion.identity);

        for (int x = 0; x < maxBotWidth; x++)
        {
            for (int y = 0; y < maxBotHeight; y++)
            {
                Tile newTile = ScriptableObject.CreateInstance<Tile>();
                newTile.sprite = savedTileMap[x, y];
                startTileMap.SetTile(new Vector3Int(x - maxBotRadius, y - maxBotRadius, 0), newTile);
            }
        }

        AddStartingBricks();
        powerGridRefreshFlag = true;

        foreach(ContainerData containerData in savedContainerData)
        {
            BrickAtBotArr(containerData.coords).GetComponent<Container>().SetOpenDirection(containerData.openDirection, true);
        }

        LoadBotResources();
    }

    private void OnEnable()
    {
        GameController.OnGameOver += OnGameOver;
        GameController.OnLoseLife += OnLoseLife;
        GameController.OnNewLevel += OnNewLevel;
        GameController.OnLevelComplete += OnLevelComplete;
    }

    private void OnDisable()
    {
        GameController.OnGameOver -= OnGameOver;
        GameController.OnLoseLife -= OnLoseLife;
        GameController.OnNewLevel -= OnNewLevel;
        GameController.OnLevelComplete -= OnLevelComplete;
    }

    void OnLevelComplete()
    {
        hasDamagedCells = false;
        foreach(GameObject brickObject in brickList)
        {
            Brick checkBrick = brickObject.GetComponent<Brick>();
            if (checkBrick.brickHP < checkBrick.brickMaxHP[checkBrick.GetPoweredLevel()])
            {
                hasDamagedCells = true;
                break;
            }
        }
    }

    bool init = false;
    public void Init()
    {
        DontDestroyOnLoad(this.gameObject);
        init = true;
        settings = GameController.Instance.settings;
        maxBotRadius = settings.maxBotRadius;
        coreBrick = masterBrickList[0];
        maxBotWidth = maxBotRadius * 2 +1;
        maxBotHeight = maxBotRadius * 2 + 1;
        coreV2 = new Vector2Int (maxBotRadius,maxBotRadius);
        brickArr  = new GameObject[maxBotWidth, maxBotHeight];
        brickTypeArr = new int[maxBotWidth, maxBotHeight];
        savedTileMap = new Sprite[maxBotWidth, maxBotHeight];
        gameObject.transform.position = new Vector3(coreX, coreY, 0);
        gameObject.transform.rotation = Quaternion.identity;
        botBody = gameObject.GetComponent<Rigidbody2D>();

        for (int x = 0; x < maxBotWidth; x++)
            for (int y = 0; y < maxBotHeight ; y++) {
                brickTypeArr[x,y] = -1;
            }
        botRotation=0;
        isRotating = false;
        powerGrid = Instantiate(powerGridPrefab, gameObject.transform);

        source = GetComponent<AudioSource>();
        startTileMap = Instantiate(startingBrickGrid.GetComponent<Tilemap>(),new Vector3 (0,0,0), Quaternion.identity);
        AddStartingBricks();
        powerGridRefreshFlag = true;

        if (tileMapSet)
        {
            OnLevelRestart();
            tileMapSet = false;
        }

        SaveStartSprites();
    }

     // Update is called once per frame
    void Update()
    {
        if(queueDestroyedBrick)
        {
            queueDestroyedBrick = false;
            source.PlayOneShot(brickDestroySound, 0.5f);
        }

        if (GameController.Instance.isPaused || !BrickAtBotArr(coreV2))
            return;

        MoveCheck();

        if (tripleCheckFlag == true) {
            powerGridRefreshFlag = false;
            tripleCheckFlag = false;
            tripleWaitFlag = false;
            TripleTestBot();
            StartCoroutine(WaitAndRefreshPower(0.5f));
        }  
    
        if ((orphanCheckFlag)&&(settings.Schmetris==false)) {
            StartCoroutine(WaitAndReleaseOrphans(0.2f));
            orphanCheckFlag = false;
        }

        //Backup fuel supply if core runs out
        Brick coreBurn = (!GameController.Instance.isLevelCompleteQueued && BrickAtBotArr(coreV2)) ? BrickAtBotArr(coreV2).GetComponent<Brick>() : null;
        if (coreBurn && !coreBurn.hasResources)
        {
            if (storedRed > 0)
            {
                storedRed = Mathf.Max(0, storedRed - coreBurn.redBurn[coreBurn.GetPoweredLevel()] * Time.deltaTime);
                if (fuelBrickList.Count > 0)
                {
                    fuelBrickList[0].GetComponent<Fuel>().CancelBurnFuel();
                }
            }
            else if (fuelBrickList.Count > 0)
            {
                fuelBrickList[0].GetComponent<Fuel>().BurnFuel(coreBurn.redBurn[coreBurn.GetPoweredLevel()] * Time.deltaTime);
            }
            else if (!GameController.Instance.isBotDead && Input.GetKeyDown(KeyCode.Space))
            {
                coreBurn.ExplodeBrick();
            }
        }
        else if (fuelBrickList.Count > 0)
        {
            fuelBrickList[0].GetComponent<Fuel>().CancelBurnFuel();
        }
    }


    public void AddStartingBricks(){
        startTileMap.CompressBounds();
        BoundsInt bounds = startTileMap.cellBounds;
        TileBase[] allTiles = startTileMap.GetTilesBlock(bounds);
        Vector3Int origin = startTileMap.origin;  

        for (int x = origin.x; x < origin.x+bounds.size.x; x++) {
            for (int y = origin.y; y < origin.y+bounds.size.y; y++) {
                Vector3Int posV3 = new Vector3Int(x,y,0);
                Sprite mySprite = startTileMap.GetSprite(posV3);
                
                if (mySprite != null) {
                    Vector3 world = startingBrickGrid.LocalToWorld(posV3);
                    int bType = 1;
                    int bLevel = 0;

                    startTileMap.SetTile(posV3,null);
                  
                    foreach (GameObject brick in masterBrickList) {
                        for (int level = 0; level < brick.GetComponent<Brick>().spriteArr.Length; level++)
                            if (brick.GetComponent<Brick>().spriteArr[level]==mySprite) {
                                bType = brick.GetComponent<Brick>().brickType;
                                bLevel = level;
                            }  
                    }
                    AddBrick(new Vector2Int(x+maxBotRadius,y+maxBotRadius),bType,bLevel);                
                }
            }
        }
        Destroy(startTileMap.gameObject);
        if(gameObject.activeSelf)
            StartCoroutine(WaitAndTripleCheck(0.2f));
    }

    public int GetBrickType(Sprite sprite){
        foreach (GameObject brick in masterBrickList) {
            for (int level = 0; level < brick.GetComponent<Brick>().spriteArr.Length; level++)
                if (brick.GetComponent<Brick>().spriteArr[level]==sprite)
                    return brick.GetComponent<Brick>().brickType;
        }
        return 2; // default
    }


    public void BumpColumn(Vector2Int startArrPos, Vector2Int bumpDirV2) {

        source.PlayOneShot(brickBumpSound, 0.5f);

        if ((IsValidBrickPos(startArrPos)==false)||(BrickAtBotArr(startArrPos)==null)||(startArrPos==coreV2))
            return;

        Vector2Int startCoords = BotToScreenCoords(startArrPos);

        // check to see if bump would bump core 

        /*Vector2Int testVector = coreV2-startCoords;
        if (((testVector.x==0)&&bumpDirV2.x==0)||((testVector.y==0)&&bumpDirV2.y==0))
            return; */

        // find end of line

        int length = 1;
       
        Vector2Int endCoords = startCoords;

        Brick resource = null;
        while (bumpDirV2 != Vector2.zero && resource == null && (IsValidScreenPos(endCoords + bumpDirV2)) && (BrickAtScreenArr((endCoords + bumpDirV2)) != null))
        {
            Brick resourceCheck = BrickAtScreenArr(endCoords).GetComponent<Brick>();
            if (AddResourceCheck(BrickAtScreenArr(endCoords + bumpDirV2), resourceCheck.brickType, bumpDirV2))
            {
                resource = resourceCheck;
            }
            else
            {
                endCoords += bumpDirV2;
                length++;
                if (endCoords == coreV2)
                    return;
            }
        }

        // if last brick is pushed out of bounds - orphan it
        if (IsValidScreenPos(endCoords+bumpDirV2)==false) {
            BrickAtScreenArr(endCoords).GetComponent<Brick>().MakeOrphan();
            length--;
        }

        // if last brick is added to a container, collect it
        if (resource)
        {
            AddResource(BrickAtScreenArr(endCoords + bumpDirV2).GetComponent<Brick>(), resource.brickType, resource.brickLevel);
            resource.DestroyBrick();
            length--;
        }

        // shift all bricks by one
        for (int l = length ; l > 0 ; l --)
        {
            Vector2Int brickArrPos = startCoords+bumpDirV2*(l-1);
            BrickAtScreenArr(brickArrPos).GetComponent<Brick>().MoveBrick(ScreenToBotCoords(brickArrPos+bumpDirV2));
        }
        RefreshNeighborLists();
        // powerGrid.Refresh();
        tripleCheckFlag = true;
        orphanCheckFlag = true;
    }
    

    public void slideGroup(List<GameObject> group,Vector2Int screenDirV2) {
        // slide group of bricks in a direction - until they hit something.
        int minDist = 99;

        // find distance to slide
    
        foreach(GameObject brick in group){
            int dist=99;
            Vector2Int brickCoords = brick.GetComponent<Brick>().ScreenArrPos();
            for (int x = 1; x < maxBotWidth; x++) {
                Vector2Int testCoords = brickCoords+screenDirV2*new Vector2Int(x,x);
                if (IsValidBrickPos(testCoords)==false) {
                    dist = 99;
                    break;
                }  else if (BrickAtScreenArr(testCoords)!=null) {
                    dist = x;
                    break;
                }
            }
            if (dist < minDist)
                minDist = dist;
        }   
        if (minDist == 99)
            ConvertBricksToBlock(group);
        else { 
            foreach(GameObject brickObj in group) {
                Brick brick = brickObj.GetComponent<Brick>();
                Vector2Int destBotCoords = ScreenToBotCoords(brick.ScreenArrPos()+screenDirV2*minDist);
                brick.MoveBrick(destBotCoords);
            }
        }
    }


    public bool IsSquareComplete(int squareNumber) {
        // square checking is not currently implemented
        bool squareIsComplete = true;
        // check top and bottom
        for (int x = -squareNumber; x <= squareNumber; x++)
            if ((brickTypeArr[maxBotRadius+x,maxBotRadius+squareNumber] == -1) || (brickTypeArr[maxBotRadius+x,maxBotRadius-squareNumber] == -1))
                squareIsComplete = false;
        // check sides
        for (int y = 1-squareNumber; y <= squareNumber-1; y++)
            if ((brickTypeArr[maxBotRadius-squareNumber,maxBotRadius+y] == -1) || (brickTypeArr[maxBotRadius+squareNumber,maxBotRadius+y] == -1))
                squareIsComplete = false;
        return squareIsComplete;
    }

    public void TripleTestBot()
    {
        foreach (GameObject brickObj in brickList){
            if (!brickObj.GetComponent<Parasite>() && !brickObj.GetComponent<CraftedPart>() && TripleTestBrick(brickObj.GetComponent<Brick>().arrPos) == true) {
                return;
            }   
        }
    }

    public bool TripleTestBrick(Vector2Int arrPos)
    {

        bool hMatch = false;
        bool vMatch = false;
        bool centreIsStable = false;
   
        GameObject testBrick = BrickAtBotArr(arrPos);
        GameObject matchBrick1;
        GameObject matchBrick2;
        GameObject sideBrick1 = null;
        GameObject sideBrick2 = null;
        Brick testScript = testBrick.GetComponent<Brick>();

        Vector2Int hTestArrPos1 = new Vector2Int (arrPos.x-1,arrPos.y);
        Vector2Int hTestArrPos2 = new Vector2Int (arrPos.x+1,arrPos.y);
        Vector2Int vTestArrPos1 = new Vector2Int (arrPos.x,arrPos.y-1);
        Vector2Int vTestArrPos2 = new Vector2Int (arrPos.x,arrPos.y+1);
        
        if (testBrick==null)
            return false;

        // test for horizontal match

        hMatch = DoTypeAndLevelMatch(hTestArrPos1,arrPos,hTestArrPos2);
        vMatch = DoTypeAndLevelMatch(vTestArrPos1,arrPos,vTestArrPos2);
 
        if (hMatch) {  
            matchBrick1 = BrickAtBotArr(hTestArrPos1);
            matchBrick2 = BrickAtBotArr(hTestArrPos2);
            if (IsValidBrickPos(vTestArrPos1))
                sideBrick1 = BrickAtBotArr(vTestArrPos1);
            if (IsValidBrickPos(vTestArrPos2))
                sideBrick2 = BrickAtBotArr(vTestArrPos2);
        }
        else if (vMatch) {
            matchBrick1 = BrickAtBotArr(vTestArrPos1);
            matchBrick2 = BrickAtBotArr(vTestArrPos2);
            if (IsValidBrickPos(hTestArrPos1))
                sideBrick1 = BrickAtBotArr(hTestArrPos1);
            if (IsValidBrickPos(hTestArrPos2))
                sideBrick2 = BrickAtBotArr(hTestArrPos2);
        } else 
            return false;
    
        // we found a triple!  Collapse it!

        // temporarily remove edge bricks from Array

        Brick mBrick1 = matchBrick1.GetComponent<Brick>();
        Brick mBrick2 = matchBrick2.GetComponent<Brick>();

        Vector2Int m1Pos = mBrick1.arrPos;
        Vector2Int m2Pos = mBrick2.arrPos;

        SetBrickAtBotArr(m1Pos,null);
        SetBrickAtBotArr(m2Pos,null);
        brickTypeArr[m1Pos.x,m1Pos.y] = -1;
        brickTypeArr[m2Pos.x,m2Pos.y] = -1;

        RefreshNeighborLists();
        // powerGrid.Refresh();

        //Look for orphaned bricks that need to be reattached around upgraded brick
        List<GameObject> m1Orphans = FindUpgradeOrphans(m1Pos, arrPos);
        List<GameObject> m2Orphans = FindUpgradeOrphans(m2Pos, arrPos);
        List<GameObject> arrOrphans = FindUpgradeOrphans(arrPos, arrPos);

        if ((IsConnectedToCore(sideBrick1))||(IsConnectedToCore(sideBrick2)))
            centreIsStable = true;
        
        SetBrickAtBotArr(m1Pos,matchBrick1);
        SetBrickAtBotArr(m2Pos,matchBrick2);
        brickTypeArr[m1Pos.x,m1Pos.y] = mBrick1.brickType;
        brickTypeArr[m2Pos.x,m2Pos.y] = mBrick2.brickType;

        RefreshNeighborLists();
       //  powerGrid.Refresh();

        if (centreIsStable) // collapse toward centre
        {
            SlideDestroy(matchBrick1,matchBrick2,testBrick);
            RepositionUpgradeOrphans(hMatch, arrPos, m1Orphans, m2Orphans);
        }
        else // collapse towards shortest path
        {
            int p1 = ShortestPathArray(m1Pos, coreV2);
            int p2 = ShortestPathArray(m2Pos, coreV2);
            if (p1 < p2) 
            {
                SlideDestroy(testBrick,matchBrick2,matchBrick1);
                RepositionUpgradeOrphans(hMatch, m1Pos, arrOrphans, m2Orphans);
            }
            else  {
                SlideDestroy(matchBrick1,testBrick,matchBrick2);
                RepositionUpgradeOrphans(hMatch, m2Pos, arrOrphans, m1Orphans);
            }
        }
        source.PlayOneShot(tripleSound,1.0f);
        orphanCheckFlag = true;
        StartCoroutine(WaitAndTripleCheck(0.2f));
        RefreshNeighborLists();
        return true;
    }

    //Determine where bricks orphaned by upgrades must move to in order to remain attached
    void RepositionUpgradeOrphans(bool isHorizontal, Vector2Int upgradeFinal, List<GameObject> setA, List<GameObject> setB)
    {
        if (isHorizontal)
        {
            foreach (GameObject OrphanA in setA)
            {
                Brick orphanBrick = OrphanA.GetComponent<Brick>();
                if (orphanBrick)
                {
                    orphanBrick.MoveBrick(new Vector2Int(orphanBrick.arrPos.x + (upgradeFinal.x > orphanBrick.arrPos.x ? 1 : -1), orphanBrick.arrPos.y));
                }
            }

            foreach (GameObject OrphanB in setB)
            {
                Brick orphanBrick = OrphanB.GetComponent<Brick>();
                if (orphanBrick)
                {
                    orphanBrick.MoveBrick(new Vector2Int(orphanBrick.arrPos.x + (upgradeFinal.x > orphanBrick.arrPos.x ? 1 : -1), orphanBrick.arrPos.y));
                }
            }
        }
        else
        {
            foreach (GameObject OrphanA in setA)
            {
                Brick orphanBrick = OrphanA.GetComponent<Brick>();
                if (orphanBrick)
                {
                    orphanBrick.MoveBrick(new Vector2Int(orphanBrick.arrPos.x, orphanBrick.arrPos.y + (upgradeFinal.y > orphanBrick.arrPos.y ? 1 : -1)));
                }
            }

            foreach (GameObject OrphanB in setB)
            {
                Brick orphanBrick = OrphanB.GetComponent<Brick>();
                if (orphanBrick)
                {
                    orphanBrick.MoveBrick(new Vector2Int(orphanBrick.arrPos.x, orphanBrick.arrPos.y + (upgradeFinal.y > orphanBrick.arrPos.y ? 1 : -1)));
                }
            }
        }
    }

    //Return a list of bricks that will be orphaned if the originPos brick disappears
    List<GameObject> FindUpgradeOrphans(Vector2Int originPos, Vector2Int upgradePos)
    {
        List<GameObject> foundOrphans = new List<GameObject>();
        if(originPos != upgradePos)
        {
            bool isVertical = originPos.x == upgradePos.x;

            //Check all blocks that might only be attached to the core by the originPos brick (any that are not touching upgradePos)
            if(isVertical)
            {
                //Right brick
                Vector2Int checkBrick = new Vector2Int(originPos.x + 1, originPos.y);
                GameObject targetBrick = BrickAtBotArr(checkBrick);
                if (targetBrick && !IsConnectedToCore(targetBrick))
                {
                    foundOrphans.Add(targetBrick);
                }

                //Left brick
                checkBrick = new Vector2Int(originPos.x - 1, originPos.y);
                targetBrick = BrickAtBotArr(checkBrick);
                if (targetBrick && !IsConnectedToCore(targetBrick))
                {
                    foundOrphans.Add(targetBrick);
                }

                //Outer edge bricks
                checkBrick = new Vector2Int(originPos.x, originPos.y + (upgradePos.y < originPos.y ? 1 : -1));
                targetBrick = BrickAtBotArr(checkBrick);
                if (targetBrick && !IsConnectedToCore(targetBrick))
                {
                    foundOrphans.Add(targetBrick);
                }

                checkBrick = new Vector2Int(originPos.x + 1, originPos.y + (upgradePos.y < originPos.y ? 1 : -1));
                targetBrick = BrickAtBotArr(checkBrick);
                if (targetBrick && !IsConnectedToCore(targetBrick))
                {
                    foundOrphans.Add(targetBrick);
                }

                checkBrick = new Vector2Int(originPos.x - 1, originPos.y + (upgradePos.y < originPos.y ? 1 : -1));
                targetBrick = BrickAtBotArr(checkBrick);
                if (targetBrick && !IsConnectedToCore(targetBrick))
                {
                    foundOrphans.Add(targetBrick);
                }
            }
            else
            {
                //Top brick
                Vector2Int checkBrick = new Vector2Int(originPos.x, originPos.y + 1);
                GameObject targetBrick = BrickAtBotArr(checkBrick);
                if (targetBrick && !IsConnectedToCore(targetBrick))
                {
                    foundOrphans.Add(targetBrick);
                }

                //Bottom brick
                checkBrick = new Vector2Int(originPos.x, originPos.y - 1);
                targetBrick = BrickAtBotArr(checkBrick);
                if (targetBrick && !IsConnectedToCore(targetBrick))
                {
                    foundOrphans.Add(targetBrick);
                }

                //Outer edge bricks
                checkBrick = new Vector2Int(originPos.x + (upgradePos.x < originPos.x ? 1 : -1), originPos.y);
                targetBrick = BrickAtBotArr(checkBrick);
                if (targetBrick && !IsConnectedToCore(targetBrick))
                {
                    foundOrphans.Add(targetBrick);
                }

                checkBrick = new Vector2Int(originPos.x + (upgradePos.x < originPos.x ? 1 : -1), originPos.y + 1);
                targetBrick = BrickAtBotArr(checkBrick);
                if (targetBrick && !IsConnectedToCore(targetBrick))
                {
                    foundOrphans.Add(targetBrick);
                }

                checkBrick = new Vector2Int(originPos.x + (upgradePos.x < originPos.x ? 1 : -1), originPos.y - 1);
                targetBrick = BrickAtBotArr(checkBrick);
                if (targetBrick && !IsConnectedToCore(targetBrick))
                {
                    foundOrphans.Add(targetBrick);
                }
            }
        }
        else
        {
            //Temporarily remove center brick in upgrade to check for orphans on edges
            GameObject upgradeBrick = BrickAtBotArr(upgradePos);
            SetBrickAtBotArr(upgradePos, null);
            brickTypeArr[upgradePos.x, upgradePos.y] = -1;
            RefreshNeighborLists();

            //Top brick
            Vector2Int checkBrick = new Vector2Int(originPos.x, originPos.y + 1);
            GameObject targetBrick = BrickAtBotArr(checkBrick);
            if (targetBrick && !IsConnectedToCore(targetBrick))
            {
                foundOrphans.Add(targetBrick);
            }

            //Bottom brick
            checkBrick = new Vector2Int(originPos.x, originPos.y - 1);
            targetBrick = BrickAtBotArr(checkBrick);
            if (targetBrick && !IsConnectedToCore(targetBrick))
            {
                foundOrphans.Add(targetBrick);
            }

            //Right brick
            checkBrick = new Vector2Int(originPos.x + 1, originPos.y);
            targetBrick = BrickAtBotArr(checkBrick);
            if (targetBrick && !IsConnectedToCore(targetBrick))
            {
                foundOrphans.Add(targetBrick);
            }

            //Left brick
            checkBrick = new Vector2Int(originPos.x - 1, originPos.y);
            targetBrick = BrickAtBotArr(checkBrick);
            if (targetBrick && !IsConnectedToCore(targetBrick))
            {
                foundOrphans.Add(targetBrick);
            }

            SetBrickAtBotArr(upgradePos, upgradeBrick);
            brickTypeArr[upgradePos.x, upgradePos.y] = upgradeBrick.GetComponent<Brick>().brickType;
            RefreshNeighborLists();
        }

        return foundOrphans;
    }

    public bool DoTypeAndLevelMatch(Vector2Int arrPos1, Vector2Int arrPos2, Vector2Int arrPos3) {
        if (IsValidBrickPos(arrPos1) && IsValidBrickPos(arrPos2) && IsValidBrickPos(arrPos3))
        {
            GameObject brick1 =brickArr[arrPos1.x,arrPos1.y];
            GameObject brick2 =brickArr[arrPos2.x,arrPos2.y];
            GameObject brick3 =brickArr[arrPos3.x,arrPos3.y];
            if ((brick1!=null)&&(brick2!=null)&&(brick3!=null))
            {
                Brick brick1Script = brick1.GetComponent<Brick>();
                Brick brick2Script = brick2.GetComponent<Brick>();
                Brick brick3Script = brick3.GetComponent<Brick>();
                int testType1 = brick1Script.brickType;
                int testType2 = brick2Script.brickType;
                int testType3 = brick3Script.brickType;
                int testBrickLevel1 = brick1Script.brickLevel;
                int testBrickLevel2 = brick2Script.brickLevel;
                int testBrickLevel3 = brick3Script.brickLevel;

                if ((testType1 == testType2) && (testType2 == testType3) 
                    && (testBrickLevel1 == testBrickLevel2) && (testBrickLevel2 == testBrickLevel3))
                    return true;
                else    
                    return false;
            }
        }
        return false;
    }

    public void SlideDestroy(GameObject obj1, GameObject obj2, GameObject obj3) 
    {
        // Destroys obj1 and obj2 (bits or bricks)... Slides ghosts of obj1 and obj2 to obj3's position.  
        Brick brick;
        Bit bit;
        Rigidbody2D ghostRb1 = CreateGhost(obj1);
        Rigidbody2D ghostRb2 = CreateGhost(obj2);
       
        brick = obj1.GetComponent<Brick>();
        if (brick == null)
        {
            bit = obj1.GetComponent<Bit>();
            bit.RemoveFromBlock("destroy");
        }
        else
        {
            brick.DestroyBrick();
        }

        brick = obj2.GetComponent<Brick>();
        if (brick == null)
        {
            bit = obj2.GetComponent<Bit>();
            bit.RemoveFromBlock("destroy");
        }
        else
        {
            brick.DestroyBrick();
        }

        Vector3 newPos = obj3.GetComponent<Rigidbody2D>().transform.position;
        StartCoroutine(SlideGhost(ghostRb1,newPos));
        StartCoroutine(SlideGhost(ghostRb2,newPos));
        obj3.GetComponent<Brick>().UpgradeBrick();

        StartCoroutine(WaitAndTripleCheck(0.2f));
    }

    public Rigidbody2D CreateGhost (GameObject obj){
        GameObject ghostObj = new GameObject();
        Rigidbody2D ghostRb = ghostObj.AddComponent<Rigidbody2D>();
        SpriteRenderer ghostSpriteRenderer = ghostObj.AddComponent<SpriteRenderer>();
      
        ghostObj.transform.position = obj.transform.position;
        ghostRb.isKinematic = true;
        ghostSpriteRenderer.sprite = obj.GetComponent<SpriteRenderer>().sprite;
        return ghostRb;
    }

    IEnumerator WaitAndReleaseOrphans(float pause)
    {
        yield return new WaitForSeconds(pause);
        ReleaseOrphans();
    }

    IEnumerator WaitAndDestroyGhost(GameObject ghost, float pause) 
    {
        yield return new WaitForSeconds(pause);
        Destroy(ghost);
    }

    public bool tripleWaitFlag = false;
    IEnumerator WaitAndTripleCheck(float pause)
    {
        tripleWaitFlag = true;
        yield return new WaitForSeconds(pause);
        tripleCheckFlag = true;
        tripleWaitFlag = false;
    }

    IEnumerator WaitAndRefreshPower(float pause)
    {
        yield return new WaitForSeconds(pause);
        powerGridRefreshFlag = true;
    }


    IEnumerator SlideGhost(Rigidbody2D ghostRb, Vector3 newPos) {
        float t = 0f;
    
        Vector3 originalPos = ghostRb.transform.position;
        float duration = (newPos-originalPos).magnitude/settings.ghostMoveSpeed;

        while (t< duration)
        {
            ghostRb.transform.position = Vector3.Lerp(originalPos,newPos,t/duration);
            yield return null;
            t+=Time.deltaTime;
        }
        ghostRb.transform.position = newPos;
        Destroy(ghostRb.gameObject);
    }
    
    public bool IsValidBrickPos(Vector2Int arrPos)
    {
        if ((0<=arrPos.x)&&(arrPos.x<maxBotWidth)&&(0<=arrPos.y)&&(arrPos.y<maxBotHeight))
            return true;
        else   
            return false;
    }

    public Vector2Int GetNearestValidBrickPos(Vector2Int arrPos, bool isRecursing)
    {
        //If this isn't a valid position, we'll need to look for another
        bool isValidBrickPos = true;
        if (!IsValidBrickPos(arrPos))
            isValidBrickPos = false;
        if (BrickAtBotArr(arrPos) != null)
            isValidBrickPos = false;

        //If we're too far from the bot to find neighbours, stop looking in this direction
        bool hasNeighbor = false;
        for (int x = 0; x < 4; x++)
        {
            Vector2Int testCoords = arrPos + directionV2Arr[x];
            if (IsValidBrickPos(testCoords))
            {
                if (BrickAtBotArr(testCoords) != null)
                {
                    hasNeighbor = true;
                }
            }
        }

        //Return a 'default' value so we know to stop recursing this function
        if (!hasNeighbor)
        {
            if (isRecursing)
                return coreV2;
            else
                isValidBrickPos = false;
        }

        //Nearest valid brick pos has been found
        if (isValidBrickPos)
        {
            return arrPos;
        }
        else
        {
            //Check right
            Vector2Int testPos = GetNearestValidBrickPos(new Vector2Int(arrPos.x + 1, arrPos.y), true);
            if (testPos != coreV2)
                return testPos;

            //Check left
            testPos = GetNearestValidBrickPos(new Vector2Int(arrPos.x - 1, arrPos.y), true);
            if (testPos != coreV2)
                return testPos;

            //Check up
            testPos = GetNearestValidBrickPos(new Vector2Int(arrPos.x, arrPos.y + 1), true);
            if (testPos != coreV2)
                return testPos;

            //Check down
            testPos = GetNearestValidBrickPos(new Vector2Int(arrPos.x, arrPos.y - 1), true);
            if (testPos != coreV2)
                return testPos;

            //Check top right
            testPos = GetNearestValidBrickPos(new Vector2Int(arrPos.x + 1, arrPos.y + 1), true);
            if (testPos != coreV2)
                return testPos;

            //Check top left
            testPos = GetNearestValidBrickPos(new Vector2Int(arrPos.x - 1, arrPos.y + 1), true);
            if (testPos != coreV2)
                return testPos;

            //Check bottom right
            testPos = GetNearestValidBrickPos(new Vector2Int(arrPos.x + 1, arrPos.y - 1), true);
            if (testPos != coreV2)
                return testPos;

            //Check bottom left
            testPos = GetNearestValidBrickPos(new Vector2Int(arrPos.x - 1, arrPos.y - 1), true);
            if (testPos != coreV2)
                return testPos;
        }

        //Return a 'default' value so we know to stop recursing this function
        return coreV2;
    }

    public bool IsValidScreenPos(Vector2Int arrCoords)
    {
        return IsValidBrickPos(ScreenToBotCoords(arrCoords));
    }


    public bool IsValidMergedBrickPos(Vector2Int mergedArrPos)
    {
        if ((0<=mergedArrPos.x)&&(mergedArrPos.x<maxBotWidth+4)&&(0<=mergedArrPos.y)&&(mergedArrPos.y<maxBotHeight+4))
            return true;
        else   
            return false;
    }

    public void ReleaseOrphans(){
        List<List<GameObject>> orphanGroupList = new List<List<GameObject>>();
        List<GameObject> orphanList = GetOrphans();
        List <GameObject> firstGroup = new List<GameObject>();
        int groupIndex;

        int orphanCount = orphanList.Count;
        // Vector2Int downV2 = GetDownVector();
       
        if (orphanCount == 0)
            return;
        
        firstGroup.Add(orphanList[0]);
        orphanGroupList.Add(firstGroup);
        orphanList.Remove(orphanList[0]);

        // sort orphans into connected groups

        foreach (GameObject orphanBrick in orphanList) {
            groupIndex = 99;
            foreach (List<GameObject> group in orphanGroupList) {
                int len = group.Count;
                if (AreConnected(orphanBrick,group[0])) {
                    groupIndex = orphanGroupList.IndexOf(group);
                    break;
                }
            }
            if (groupIndex == 99) {
                orphanGroupList.Add(new List<GameObject>());
                orphanGroupList[orphanGroupList.Count-1].Add(orphanBrick);
            } else {
                orphanGroupList[groupIndex].Add(orphanBrick);
            }
        }
        foreach (List<GameObject> group in orphanGroupList) {  
             ConvertBricksToBlock(group);
        }
        RefreshNeighborLists();
        // powerGrid.Refresh();
    }

    public void ConvertBricksToBlock(List<GameObject> group) {    
        // creates new orphan block from group of bricks
        
        Vector3 vpos = group[0].transform.position;
    
        GameObject newBlockObj = (GameObject) Instantiate(blockPrefab, vpos, Quaternion.identity);
        Block newBlock = newBlockObj.GetComponent<Block>();
        newBlock.bot = this;
        newBlock.blockRotation = 0;
        GameController.Instance.blockList.Add(newBlockObj);

        // add bits to new block
        foreach (GameObject orphanBrickObj in group) {
            Brick orphanBrick = orphanBrickObj.GetComponent<Brick>();
            if (orphanBrick.IsParasite()){
                GameObject newEnemyObj;
                int type = orphanBrick.ConvertToEnemyType();
                newEnemyObj = Instantiate(GameController.Instance.enemyReference[type], orphanBrick.transform.position, Quaternion.identity);
                newEnemyObj.GetComponent<EnemyGeneral>().hp = orphanBrick.brickHP;
                newEnemyObj.GetComponent<EnemyGeneral>().AdjustHP(0);
            } else {
                GameObject newBitObj;
                Vector3 bPos = orphanBrick.transform.position;
                int type = orphanBrick.ConvertToBitType();
                int level = orphanBrick.brickLevel;

                newBitObj = Instantiate(GameController.Instance.bitReference[type],bPos,Quaternion.identity);
                newBitObj.transform.parent = newBlockObj.transform;  
                Bit newBit = newBitObj.GetComponent<Bit>();
                newBit.bitType = type;
                newBit.SetLevel(level);
            }

            orphanBrick.DestroyBrick(); 
        }
    }

    public int GapToNextBrick(GameObject brick, Vector2Int directionV2) {
        int gapDist = 0;
        bool closedGap = false;
        Vector2Int brickCoords = new Vector2Int();
        Vector2Int testCoords = new Vector2Int();
        brickCoords = brick.GetComponent<Brick>().arrPos;
        testCoords = brickCoords+directionV2;
       
        while (IsValidBrickPos(testCoords)&&(closedGap == false)) {
            if (brickArr[testCoords.x,testCoords.y]==null){
                gapDist++;
            } else {
                closedGap = true;
            }
            testCoords+=directionV2;
        }
       
        return closedGap? gapDist : 99;
    }
    
    public Vector2Int MapToScreenCoords(Vector2Int arrXY,int mapWidth) {
        return UnTwistCoords(arrXY,mapWidth);
    }

    public Vector2Int ScreenToMapCoords(Vector2Int arrXY, int mapWidth) {
        return TwistCoords(arrXY,mapWidth);
    }

    public Vector2Int BotToScreenCoords(Vector2Int arrXY) {
        return TwistCoords(arrXY,maxBotWidth);
    }

    public Vector2Int ScreenToBotCoords(Vector2Int arrXY) {
        return UnTwistCoords(arrXY,maxBotWidth);
    }

    public Vector3 BotCoordsToScreenPos(Vector2Int botCoords){
        Vector2Int screenOffset = ArrToOffset((BotToScreenCoords(botCoords)));

        Vector3 screenPos = transform.position + new Vector3(screenOffset.x*ScreenStuff.colSize,screenOffset.y*ScreenStuff.colSize);
        return screenPos;
    }
     
    public Vector2Int UnTwistCoords(Vector2Int arrXY, int arrWidth) {
        Vector2Int newCoords = new Vector2Int();

        switch (botRotation) {
            case 0:
                newCoords = arrXY;
                break;
            case 1:
                newCoords.x = arrWidth - 1 - arrXY.y;
                newCoords.y = arrXY.x;
                break;
            case 2:
                newCoords.x = arrWidth - 1 - arrXY.x;
                newCoords.y = arrWidth - 1 - arrXY.y;
                break;
            default:
                newCoords.x = arrXY.y;
                newCoords.y = arrWidth - 1 - arrXY.x;
                break;
        }
        return newCoords;
    }

    public Vector2Int TwistCoords(Vector2Int arrXY, int arrWidth){
        Vector2Int newCoords = new Vector2Int();

        switch (botRotation) {
            case 0:
                newCoords = arrXY;
                break;
            case 1:
                newCoords.x = arrXY.y;
                newCoords.y = arrWidth - 1 - arrXY.x;
                break;
            case 2:
                newCoords.x = arrWidth - 1 - arrXY.x;
                newCoords.y = arrWidth - 1 - arrXY.y;
                break;
            default:
                newCoords.x = maxBotWidth - 1 - arrXY.y;
                newCoords.y = arrXY.x;
                break;
        }
        return newCoords;
    }

    public Vector2Int ScreenPosToOffset(Vector2 point) {
        return new Vector2Int (ScreenStuff.XPositionToCol(point.x),ScreenStuff.YPositionToRow(point.y));
    }
    

    public List<GameObject> GetOrphans(){
        List<GameObject> connectedList = new List<GameObject>();
        List<GameObject> orphanList = new List<GameObject>();

        pathArr = brickTypeArr.Clone() as int[,];
        
        connectedList.Add(BrickAtBotArr(coreV2));
        pathArr[coreV2.x,coreV2.y] = -2; // mark to not check again
      
        ExpandConnectedList(ref connectedList,coreV2);

        foreach(GameObject brickObj in brickList)
            if (connectedList.Contains(brickObj) == false)
                orphanList.Add(brickObj);

        return orphanList;
    }

    public void ExpandConnectedList(ref List<GameObject> connectedList, Vector2Int arrPos){
        GameObject thisBrickObj = BrickAtBotArr(arrPos);
         if (thisBrickObj==null)
            return;
        Brick thisBrick = thisBrickObj.GetComponent<Brick>();

        foreach (GameObject neighborBrick in thisBrick.neighborList){
            Vector2Int neighborArrPos = neighborBrick.GetComponent<Brick>().arrPos;
            if (pathArr[neighborArrPos.x,neighborArrPos.y] >= 0) {
                connectedList.Add(neighborBrick);
                pathArr[neighborArrPos.x,neighborArrPos.y] = -2;
                ExpandConnectedList(ref connectedList, neighborArrPos);
            }
        }
    }


    // Shortest Path Using Arrays
    public int ShortestPathArray(Vector2Int arrPos1, Vector2Int arrPos2)
    {
        int distance;
        
        if ((IsValidBrickPos(arrPos1)==false)||(IsValidBrickPos(arrPos2)==false))
            return 99;
        if ((BrickAtBotArr(arrPos1)==null)||(BrickAtBotArr(arrPos2)==null))
            return 99;
        else if (arrPos1 == arrPos2)
            return 0;

        pathArr = brickTypeArr.Clone() as int[,];

        for (int x = 0; x < maxBotWidth; x++)
            for (int y = 0; y < maxBotHeight; y++)
                if (pathArr[x,y] >= 0)
                    pathArr[x,y] = 0;

        pathArr[arrPos1.x,arrPos1.y]=1;
        ExpandPath(arrPos1);
        
        distance = pathArr[arrPos2.x,arrPos2.y]-1;
        if (distance == -2)
            return 99;
        else    
            return distance;

    }

    void ExpandPath(Vector2Int arrPos){
        for (int n = 0; n < 4; n++) {
            Vector2Int nPos = arrPos+directionV2Arr[n];
            if (IsValidBrickPos(nPos))
                if (pathArr[nPos.x,nPos.y]==0)
                    pathArr[nPos.x,nPos.y] = pathArr[arrPos.x,arrPos.y]+1;
        }
        for (int n = 0; n < 4; n++) {
            Vector2Int nPos = arrPos+directionV2Arr[n];
            if (IsValidBrickPos(nPos))
                if (pathArr[nPos.x,nPos.y]>pathArr[arrPos.x,arrPos.y])
                    ExpandPath(nPos);
        }
    }

    public bool AreConnected(GameObject brick1, GameObject brick2){
        if ((brick1 == null) || (brick2 == null))
            return false;
        if (ShortestPathArray(brick1.GetComponent<Brick>().arrPos,brick2.GetComponent<Brick>().arrPos) >= 99)
            return false;
        else    
            return true;
    }
    

    public bool IsConnectedToCore(GameObject brick)
    {
        if (brick == null)
            return false;

        pathArr = brickTypeArr.Clone() as int[,];
        Vector2Int bPos = brick.GetComponent<Brick>().arrPos;
     
        return (IsBranchConnectedToCore(bPos));
    }

    public bool IsBranchConnectedToCore(Vector2Int bPos) {
        
        if (brickTypeArr[bPos.x,bPos.y] == -1)
            return false;
        
        if (bPos == coreV2)
            return true;

        pathArr[bPos.x,bPos.y] = -2;
        GameObject brick = brickArr[bPos.x,bPos.y];
           
        foreach (GameObject nBrick in brick.GetComponent<Brick>().neighborList) {
            Vector2Int nPos = nBrick.GetComponent<Brick>().arrPos;
            if (pathArr[nPos.x,nPos.y] >= 0 && !nBrick.GetComponent<Brick>().IsParasite())
                if (IsBranchConnectedToCore(nPos))
                    return true;
        }
        return false;
    }


    public bool IsNeighbor(GameObject brick1, GameObject brick2)
    {
        if ((brick1==null)||(brick2==null))
            return false;
        else if (brick1.GetComponent<Brick>().neighborList.Contains(brick2)) 
            return true;
        else
            return false;
    }

    public bool IsNeighborArr(Vector2Int arrPos1, Vector2Int arrPos2) {
        if (arrPos1.x==arrPos2.x) {
            if (Mathf.Abs(arrPos1.y-arrPos2.y)==1) {
                return true;
            } else
                return false;
        } else if (arrPos1.y==arrPos2.y) {
            if (Mathf.Abs(arrPos1.x-arrPos2.x)==1) {
                return true;
            } else 
                return false;
        } else
            return false;
    }

    public void RefreshNeighborLists() {
        foreach (GameObject brickObj in brickList) {
            Brick brick = brickObj.GetComponent<Brick>();
            List<GameObject> newNeighborList = new List<GameObject>();

            for (int d = 0; d < 4; d++) {
                Vector2Int neighborArrPos = brick.arrPos+directionV2Arr[d];

                if ((IsValidBrickPos(neighborArrPos))&&(brickArr[neighborArrPos.x,neighborArrPos.y]!=null))
                    newNeighborList.Add(brickArr[neighborArrPos.x,neighborArrPos.y]);
            }
            brick.neighborList = newNeighborList;
        }
    }

    IEnumerator RotateOverTime (Quaternion originalRotation, Quaternion finalRotation, float duration) {
        isRotating = true;
        float t = 0f;
        Component[] brickScripts = GetComponentsInChildren<Brick>();

        while (t<duration)
        {
            botBody.transform.rotation = Quaternion.Slerp(originalRotation, finalRotation, t/duration);
            yield return null;
            t += Time.deltaTime;
        }
        botBody.transform.rotation = finalRotation;
        foreach (GameObject brickObj in brickList) {
            Container brickContainer = brickObj.GetComponent<Container>();
            if (brickContainer)
            {
                brickContainer.SetOpenDirection(brickContainer.startDirection + botBody.transform.eulerAngles.z, false);
            }
            brickObj.GetComponent<Brick>().RotateUpright();
        }
        isRotating = false;
    }

    public GameObject BrickAtBotArr(Vector2Int arrPos) {
        return brickArr[arrPos.x,arrPos.y];
    }

    public GameObject BrickAtScreenArr(Vector2Int arrPos) {
        Vector2Int screenCoords = ScreenToBotCoords(arrPos);
        return brickArr[screenCoords.x,screenCoords.y];
    }
    
    public void SetBrickAtBotArr(Vector2Int arrPos, GameObject brickObj){
        brickArr[arrPos.x,arrPos.y] = brickObj;
    }      

    class BorderTriple {
        public GameObject BitObj;
        public Vector2Int StartArrPos;
        public Vector2Int EndArrPos;
        public BorderTriple (GameObject bitObj,Vector2Int startArrPos,Vector2Int endArrPos)
        {
            BitObj = bitObj;
            StartArrPos = startArrPos;
            EndArrPos = endArrPos;
        }
    }

    public GameObject AddBrick(Vector2Int arrPos, int type, int level = 0)
    {

        Vector2Int offsetV2 = ArrToOffset(arrPos);

        Vector3 offsetV3 = new Vector3(offsetV2.x * settings.colSize, offsetV2.y * settings.colSize, 0);
        GameObject newBrick;
        Vector2Int screenPos = BotToScreenCoords(arrPos);

        // check to see if brickType is valid - MAKE FUNCTION!!

        if (type >= masterBrickList.Length || (type < 0))
        {
            return null;
        }

        // check to see that array position is valid and empty

        if ((IsValidBrickPos(arrPos) == false) || (BrickAtBotArr(arrPos) != null))
        {
            return null;
        }

        offsetV3 = gameObject.transform.rotation * offsetV3;

        newBrick = Object.Instantiate(masterBrickList[type], new Vector3(coreX,coreY,0), Quaternion.identity, gameObject.transform);
        Brick newBrickScript = newBrick.GetComponent<Brick>();

        newBrick.transform.Translate(offsetV3);

        SetBrickAtBotArr(arrPos,newBrick);
        brickList.Add(newBrick);
        brickTypeArr[arrPos.x,arrPos.y] = type;
        newBrickScript.brickHP = newBrickScript.brickMaxHP[level];

        newBrickScript.arrPos = arrPos;
        newBrickScript.brickType = type;
        newBrickScript.ID = type*100;
        newBrickScript.parentBot = gameObject;
        newBrickScript.SetLevel(level);
        
        RefreshNeighborLists();

        tripleCheckFlag = true;

        return newBrick;
     
    }

    public void ResolveEnemyCollision (GameObject enemyObj)
    {
        Enemy enemy = enemyObj.GetComponent<Enemy>();
        if (enemy == null)
            return;
        float rA = transform.rotation.eulerAngles.z;

        if (!((rA == 0) || (rA == 90) || (rA == 180) || (rA == 270))) 
            return;

        Vector2Int sOffset = ScreenStuff.GetOffset(enemyObj);
        Vector2Int sCoords = OffsetToArray(sOffset);
        Vector2Int bCoords = ScreenToBotCoords(sCoords);
        bool hasValidBrickPos = true;

        if (!IsValidBrickPos(bCoords))
            hasValidBrickPos = false;

        if (BrickAtBotArr(bCoords)!=null)
            hasValidBrickPos = false;

        // check to see if the enemy can attach to a brick;

        bool hasNeighbor = false;   
        for (int x = 0; x< 4; x++) {
            
            Vector2Int testCoords = bCoords + directionV2Arr[x];
            if (IsValidBrickPos(testCoords))
                if (BrickAtBotArr(testCoords)!=null)
                    hasNeighbor = true;
        }
        if (!hasNeighbor)
            hasValidBrickPos = false;
        
        if(!hasValidBrickPos)
        {
            bCoords = GetNearestValidBrickPos(bCoords, false);
            if (bCoords == coreV2)
            {
                return;
            }
        }

        int brickType = enemy.data.type;

        // enemies turn into bricks once they collide with Bot

        GameObject newBrick = AddBrick(bCoords, brickType, 0);
        Parasite parasite = newBrick.GetComponent<Parasite>();
        parasite.data = enemy.data;
        parasite.targetBrick = enemy.targetBrick;
        newBrick.GetComponent<Brick>().brickMaxHP[0] = enemy.data.maxHP;
        newBrick.GetComponent<Brick>().brickHP = enemy.GetComponent<EnemyGeneral>().hp;
        newBrick.GetComponent<Brick>().AdjustHP(0);
        GameController.Instance.enemyList.Add(newBrick);

        enemy.DestroyEnemy();

        StartCoroutine(WaitAndTripleCheck(0.2f));
    }

    public void ResolveCollision(GameObject blockObj,Vector2Int hitDir)
    {
        if (blockObj == null)
            return;
        Block block = blockObj.GetComponent<Block>();
        CollisionMap cMap = CreateCollisionMap(blockObj,hitDir);
        bool bounceBlockFlag = false;

        /*
        foreach (CollisionPair cp in cMap.collisionPairs) {

            int xOffset = block.GetXOffset(coreCol);
            int blockRow = ScreenStuff.GetRow(blockObj);
        
            Vector2Int blockOffset = new Vector2Int (xOffset, blockRow); 
            
            bool bounceBlockFlag = false;
        
            List<BorderTriple> deepTripleList = new List<BorderTriple>();
            List<BorderTriple> shallowTripleList = new List<BorderTriple>();
        }


            // Collapse Cross-Border Triples //

            foreach(GameObject bitObj in block.bitList) {
                Vector2Int borderBitArrPos = cMap.GetMapCoordsBit(bitObj);
                Vector2Int borderObjArrPos = new Vector2Int(-1,-1);
                Vector2Int innerBrickArrPos = new Vector2Int(-1,-1);
                Vector2Int outerBitArrPos = new Vector2Int(-1,-1);
                int bType = mergedTypeArr[borderBitArrPos.x,borderBitArrPos.y];
                Bit bit = bitObj.GetComponent<Bit>();

                if((borderBitArrPos.x)==1) {
                    if ((borderBitArrPos.y)>1&&(borderBitArrPos.y<maxBotHeight+2)) {
                        borderObjArrPos = borderBitArrPos+Vector2Int.right;
                        if (mergedTypeArr[borderObjArrPos.x,borderObjArrPos.y]==bType){
                            outerBitArrPos = borderBitArrPos+Vector2Int.left; 
                            innerBrickArrPos = borderObjArrPos+Vector2Int.right;
                            if(brickArr[borderObjArrPos.x-2,borderObjArrPos.y-2].GetComponent<Brick>().brickLevel==0){
                                if (mergedTypeArr[outerBitArrPos.x,outerBitArrPos.y]==bType) {
                                    deepTripleList.Add(new BorderTriple(bitObj,outerBitArrPos,borderObjArrPos));
                                } else {  
                                    if ((mergedTypeArr[innerBrickArrPos.x,innerBrickArrPos.y]==bType)
                                        && (brickArr[innerBrickArrPos.x-2,innerBrickArrPos.y-2].GetComponent<Brick>().brickLevel==0)) {
                                        shallowTripleList.Add(new BorderTriple(bitObj,borderObjArrPos,innerBrickArrPos));
                                        mergedTypeArr[borderObjArrPos.x,borderObjArrPos.y]=-1;
                                    }
                                }
                            }
                        }
                    } 
                } else if ((borderBitArrPos.x)==maxBotWidth+2){
                    if ((borderBitArrPos.y)>1&&(borderBitArrPos.y<maxBotHeight+2)) {
                        borderObjArrPos = borderBitArrPos+Vector2Int.left;
                        if (mergedTypeArr[borderObjArrPos.x,borderObjArrPos.y]==bType){
                            outerBitArrPos = borderBitArrPos+Vector2Int.right; 
                            innerBrickArrPos = borderObjArrPos+Vector2Int.left;
                            if(brickArr[borderObjArrPos.x-2,borderObjArrPos.y-2].GetComponent<Brick>().brickLevel==0){
                                if (mergedTypeArr[outerBitArrPos.x,outerBitArrPos.y]==bType) {
                                    deepTripleList.Add(new BorderTriple(bitObj,outerBitArrPos,borderObjArrPos));
                                } else {  
                                    if ((mergedTypeArr[innerBrickArrPos.x,innerBrickArrPos.y]==bType) 
                                        && (brickArr[innerBrickArrPos.x-2,innerBrickArrPos.y-2].GetComponent<Brick>().brickLevel==0)) {
                                        shallowTripleList.Add(new BorderTriple(bitObj,borderObjArrPos,innerBrickArrPos));
                                        mergedTypeArr[borderObjArrPos.x,borderObjArrPos.y]=-1;
                                    }
                                }
                            }
                        }
                    } 
                } else if ((borderBitArrPos.y)==1) {
                    if ((borderBitArrPos.x)>1&&(borderBitArrPos.x<maxBotWidth+2)) {
                        borderObjArrPos = borderBitArrPos+Vector2Int.up;
                        if (mergedTypeArr[borderObjArrPos.x,borderObjArrPos.y]==bType){
                            outerBitArrPos = borderBitArrPos+Vector2Int.down; 
                            innerBrickArrPos = borderObjArrPos+Vector2Int.up;
                            if(brickArr[borderObjArrPos.x-2,borderObjArrPos.y-2].GetComponent<Brick>().brickLevel==0){
                                if (mergedTypeArr[outerBitArrPos.x,outerBitArrPos.y]==bType) {
                                    deepTripleList.Add(new BorderTriple(bitObj,outerBitArrPos,borderObjArrPos));
                                } else {  
                                    if ((mergedTypeArr[innerBrickArrPos.x,innerBrickArrPos.y]==bType) 
                                        && (brickArr[innerBrickArrPos.x-2,innerBrickArrPos.y-2].GetComponent<Brick>().brickLevel==0)) {
                                        shallowTripleList.Add(new BorderTriple(bitObj,borderObjArrPos,innerBrickArrPos));
                                        mergedTypeArr[borderObjArrPos.x,borderObjArrPos.y]=-1;
                                    }
                                }
                            }
                        }
                    } 
                } else if ((borderBitArrPos.y)==maxBotHeight+2){
                    if ((borderBitArrPos.x)>1&&(borderBitArrPos.x<maxBotWidth+2)) {
                        borderObjArrPos = borderBitArrPos+Vector2Int.down;
                        if (mergedTypeArr[borderObjArrPos.x,borderObjArrPos.y]==bType){
                            outerBitArrPos = borderBitArrPos+Vector2Int.up; 
                            innerBrickArrPos = borderObjArrPos+Vector2Int.down;
                            if(brickArr[borderObjArrPos.x-2,borderObjArrPos.y-2].GetComponent<Brick>().brickLevel==0){ 
                                if (mergedTypeArr[outerBitArrPos.x,outerBitArrPos.y]==bType) {
                                    deepTripleList.Add(new BorderTriple(bitObj,outerBitArrPos,borderObjArrPos));
                                } else {  
                                    if ((mergedTypeArr[innerBrickArrPos.x,innerBrickArrPos.y]==bType) 
                                        && (brickArr[innerBrickArrPos.x-2,innerBrickArrPos.y-2].GetComponent<Brick>().brickLevel==0)) {
                                        shallowTripleList.Add(new BorderTriple(bitObj,borderObjArrPos,innerBrickArrPos));
                                        mergedTypeArr[borderObjArrPos.x,borderObjArrPos.y]=-1;
                                    }
                                }
                            }
                        }
                    } 
                }
            }

            foreach (BorderTriple deepTriple in deepTripleList) 
                CollapseBorderTripleDeep(deepTriple);
            foreach (BorderTriple shallowTriple in shallowTripleList) 
                CollapseBorderTripleShallow(shallowTriple);
                */

        // Convert Bits into Bricks within Bounds

        List<BrickBitPair> brickBitPairList = new List<BrickBitPair>();

        //foreach (GameObject bitObj in block.bitList)
        for(int i = 0;i<block.bitList.Count;i++)
        {
            GameObject bitObj = block.bitList[i];
            Bit bit = bitObj.GetComponent<Bit>();
            Vector2Int bitMapCoords = cMap.GetMapCoordsBit(bitObj);
            Vector2Int botCoords = cMap.MapCoordsToBotCoords(bitMapCoords);
            if (IsValidBrickPos(botCoords))
            {
                int brickType = bit.ConvertToBrickType();
                GameObject containerTest = BrickAtBotArr(cMap.MapCoordsToBotCoords(bitMapCoords + hitDir));
                if (!AddResourceCheck(containerTest, brickType, hitDir))
                {
                    GameObject newBrick = AddBrick(botCoords, brickType, bit.bitLevel);
                    if (newBrick != null)
                    {
                        source.PlayOneShot(brickAttachSound, 1.0f);
                        BrickBitPair brickBitPair = new BrickBitPair(newBrick, bitObj);
                        brickBitPairList.Add(brickBitPair);
                        //GameController.Instance.money++;
                    }
                }
                else
                {
                    AddResource(containerTest.GetComponent<Brick>(), brickType, bit.bitLevel);
                    bitObj.GetComponent<Bit>().RemoveFromBlock("");
                    i--;
                }
            }
        }

        // remove bits connected to Bot from Block.

        foreach (BrickBitPair brickBitPair in brickBitPairList){
            if (IsConnectedToCore(brickBitPair.brickObj)){

                brickBitPair.bitObj.GetComponent<Bit>().RemoveFromBlock("Destroy");
            } else {

                brickBitPair.brickObj.GetComponent<Brick>().DestroyBrick();
            }
        }

        // if the block still has bits directly above bricks - bounce the block

        if (blockObj!=null) {
            foreach (GameObject bitObj in block.bitList) {
                Bit bit = bitObj.GetComponent<Bit>();
                Vector2Int mapCoords = cMap.GetMapCoordsBit(bitObj);
                Vector2Int testCoords = mapCoords + hitDir;
                if (cMap.IsValidMapPos(testCoords))
                    if (cMap.IDArr[testCoords.x,testCoords.y]>=0)
                        bounceBlockFlag = true;
            }
            if (bounceBlockFlag) {
                block.BounceBlock();
            } 
        }

        StartCoroutine(WaitAndTripleCheck(0.2f));
    }

    bool AddResourceCheck(GameObject container, int bitType, Vector2Int hitDir)
    {
        if (container)
        {
            Container containerBrick = container.GetComponent<Container>();
            if (containerBrick && containerBrick.IsOpenDirection(hitDir))
            {
                return bitType == 0 || bitType == 1 || bitType == 2 || bitType == 3 || bitType == 5;
            }
        }
        return false;
    }

    void AddResource(Brick containerBrick, int type, int level)
    {
        source.PlayOneShot(resourceSound, 1.0f);
        //GameController.Instance.money++;

        switch(type)
        {
            case 0:
                storedYellow += masterBrickList[type].GetComponent<Yellectrons>().maxResource[level];
                break;
            case 1:
                storedRed += masterBrickList[type].GetComponent<Fuel>().maxFuelArr[level];
                break;
            case 2:
                storedGreen += masterBrickList[type].GetComponent<Repair>().maxResource[level];
                break;
            case 3:
                storedBlue += masterBrickList[type].GetComponent<BlueSaltGun>().maxResource[level];
                break;
            case 5:
                storedGrey += masterBrickList[type].GetComponent<Greyscale>().maxResource[level];
                break;
        }
    }

    public class BrickBitPair{
        public GameObject brickObj;
        public GameObject bitObj;

        public BrickBitPair(GameObject aBrickObj, GameObject aBitObj){
            brickObj = aBrickObj;
            bitObj = aBitObj;
        }
    }

    public Vector2Int OffsetToArray(Vector2Int offsetV2)
    {
        return offsetV2 + coreV2;
    }

    public Vector2Int ArrToOffset(Vector2Int arrPos)
    {
        return arrPos - coreV2;
    }

    bool isHoldingScreen = false;
    float holdingScreenTimer = 0;
    const float maxTapTimer = 0.15f;
    const float slideBuffer = 50.0f;
    float moveBuffer = 25.0f;
    Vector3 prevMousePos = Vector3.zero;
    Vector3 bufferedMovePos = Vector3.zero;
    float rotateBuffer = 200.0f;
    float bufferTimer = 0;
    float bufferMaxTime = 3.0f;
    bool hasRotated = false;
    void TouchInputCheck()
    {
        if(Input.GetMouseButtonDown(0))
        {
            isHoldingScreen = true;
            prevMousePos = Input.mousePosition;
            bufferedMovePos = Input.mousePosition;
            holdingScreenTimer = 0;
        }
        else if (Input.GetMouseButton(0))
        {
            isHoldingScreen = true;
            if(holdingScreenTimer < maxTapTimer)
            {
                holdingScreenTimer += Time.deltaTime;
            }
            else if (Vector3.Distance(Input.mousePosition,prevMousePos) <= slideBuffer)
            {
                if (hasRotated || bufferTimer > 0)
                {
                    bufferTimer += Time.deltaTime;
                    if (bufferTimer >= bufferMaxTime)
                    {
                        bufferedMovePos = Input.mousePosition;
                        bufferTimer = 0;
                        hasRotated = false;
                    }
                }
                else
                {
                    if (Input.mousePosition.x < Screen.width / 2.0f)
                    {
                        if (startTime + delay <= Time.time)
                        {
                            startTime = Time.time;
                            delay = shortPause;
                            MoveBot(-1);
                        }
                    }
                    else if (Input.mousePosition.x > Screen.width / 2.0f)
                    {
                        if (startTime + delay <= Time.time)
                        {
                            startTime = Time.time;
                            delay = shortPause;
                            MoveBot(1);
                        }
                    }
                }
            }
            else
            {
                bufferTimer = 0;
                bool isLeftBot = Input.mousePosition.x < Screen.width / 2.0f;
                bool isBelowBot = Input.mousePosition.y < Camera.main.WorldToScreenPoint(coreBrick.transform.position).y;
                bool isHorizontal = Mathf.Abs(bufferedMovePos.x - Input.mousePosition.x) >= Mathf.Abs(bufferedMovePos.y - Input.mousePosition.y);
                if (isHorizontal && bufferedMovePos.x - Input.mousePosition.x >= rotateBuffer)
                {
                    hasRotated = true;
                    bufferedMovePos = Input.mousePosition;
                    Rotate(!isBelowBot ? -1 : 1);
                }
                else if (!isHorizontal && bufferedMovePos.y - Input.mousePosition.y >= rotateBuffer)
                {
                    hasRotated = true;
                    bufferedMovePos = Input.mousePosition;
                    Rotate(isLeftBot ? -1 : 1);
                }
                else if (isHorizontal && bufferedMovePos.x - Input.mousePosition.x <= -rotateBuffer)
                {
                    hasRotated = true;
                    bufferedMovePos = Input.mousePosition;
                    Rotate(!isBelowBot ? 1 : -1);
                }
                else if (!isHorizontal && bufferedMovePos.y - Input.mousePosition.y <= -rotateBuffer)
                {
                    hasRotated = true;
                    bufferedMovePos = Input.mousePosition;
                    Rotate(isLeftBot ? 1 : -1);
                }
            }
            prevMousePos = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            bool isLeftBot = Input.mousePosition.x < Screen.width / 2.0f;
            bool isBelowBot = Input.mousePosition.y < Camera.main.WorldToScreenPoint(coreBrick.transform.position).y;
            bool isHorizontal = Mathf.Abs(bufferedMovePos.x - Input.mousePosition.x) >= Mathf.Abs(bufferedMovePos.y - Input.mousePosition.y);
            if (isHorizontal && bufferedMovePos.x - Input.mousePosition.x >= rotateBuffer)
            {
                hasRotated = true;
                bufferedMovePos = Input.mousePosition;
                Rotate(!isBelowBot ? -1 : 1);
            }
            else if (!isHorizontal && bufferedMovePos.y - Input.mousePosition.y >= rotateBuffer)
            {
                hasRotated = true;
                bufferedMovePos = Input.mousePosition;
                Rotate(isLeftBot ? -1 : 1);
            }
            else if (isHorizontal && bufferedMovePos.x - Input.mousePosition.x <= -rotateBuffer)
            {
                hasRotated = true;
                bufferedMovePos = Input.mousePosition;
                Rotate(!isBelowBot ? 1 : -1);
            }
            else if (!isHorizontal && bufferedMovePos.y - Input.mousePosition.y <= -rotateBuffer)
            {
                hasRotated = true;
                bufferedMovePos = Input.mousePosition;
                Rotate(isLeftBot ? 1 : -1);
            }
            else if (holdingScreenTimer < maxTapTimer)
            {
                if (prevMousePos.x < Screen.width / 2.0f)
                {
                    MoveBot(-1);
                }
                else if (prevMousePos.x > Screen.width / 2.0f)
                {
                    MoveBot(1);
                }
            }
            bufferTimer = 0;
            isHoldingScreen = false;
            hasRotated = false;
            holdingScreenTimer = 0;
        }
        else
        {
            bufferTimer = 0;
            isHoldingScreen = false;
            hasRotated = false;
            holdingScreenTimer = 0;
        }

        /*if (!isHoldingScreen && Input.GetMouseButton(0))
        {
            isHoldingScreen = true;
            holdingScreenTimer = 0;
            prevMouse = Input.mousePosition.x;
        }
        else if (isHoldingScreen && !Input.GetMouseButton(0))
        {
            if (holdingScreenTimer <= maxTapTimer)
            {
                if (Input.mousePosition.x < Screen.width / 2.0f)
                {
                    Rotate(-1);
                }
                else
                {
                    Rotate(1);
                }
            }
            isHoldingScreen = false;
            prevMouse = 0;
        }
        else if (isHoldingScreen)
        {
            holdingScreenTimer += Time.unscaledDeltaTime;
            if (holdingScreenTimer > maxTapTimer && Mathf.Abs(prevMouse - Input.mousePosition.x) > moveBuffer)
            {
                if (startTime + delay <= Time.time)
                {
                    startTime = Time.time;
                    delay = shortPause;
                    MoveBot(Input.mousePosition.x < prevMouse ? -1 : 1);
                    prevMouse = Input.mousePosition.x;
                }
            }
        }*/
    }

    void MoveCheck()
    {

#if UNITY_IOS || UNITY_ANDROID
        TouchInputCheck();
#else
        if (Input.GetKeyDown(KeyCode.UpArrow)||Input.GetKeyDown("e")) 
            Rotate(1);     

        if (Input.GetKeyDown(KeyCode.DownArrow)||Input.GetKeyDown("q"))
            Rotate(-1);
      
        if (Input.GetKeyDown(KeyCode.LeftArrow)||Input.GetKeyDown("a"))
        {
            delay = longPause;
            startTime = Time.time;
            MoveBot(-1);
        } 
        else if (Input.GetKey(KeyCode.LeftArrow)||Input.GetKey("a")){
            if (startTime + delay <= Time.time) {
                startTime = Time.time;
                delay = shortPause;
                MoveBot(-1);
            }
        }

        if (Input.GetKeyDown(KeyCode.RightArrow)||Input.GetKeyDown("d"))
        {
            delay = longPause;
            startTime = Time.time;
            MoveBot(1);
        }

        else if (Input.GetKey(KeyCode.RightArrow)||Input.GetKey("d"))
        {
            if (startTime + delay <= Time.time)
            {
                startTime = Time.time;
                delay = shortPause;
                MoveBot(1); 
            }
        }
#endif
    }

    void Rotate(int direction) {
        //if (!HasFuel())
        //   return;

        if (!isRotating)
        {
            botRotation += direction;
            rotation1 = botBody.transform.rotation;
            rotation2 = rotation1 * Quaternion.Euler(0, 0, -direction * 90);
            StartCoroutine(RotateOverTime(rotation1, rotation2, 0.05f));
            CorrectBotRotation();
        }
    }

    void CorrectBotRotation(){
        if (botRotation == 4)
            botRotation = 0;
        if (botRotation == -1)
            botRotation = 3;
    }

    public bool HasFuel()
    {
        return GetResourcePercent(ResourceType.Red) > 0 || fuelBrickList.Count > 0;
    }

    public float GetResourcePercent(ResourceType resourceType)
    {
        if (totalCapacity == 0)
            return 0;

        switch (resourceType)
        {
            case ResourceType.Blue:
                return storedBlue / totalCapacity;
            case ResourceType.Red:
                return storedRed / totalCapacity;
            case ResourceType.Yellow:
                return storedYellow / totalCapacity;
            case ResourceType.Green:
                return storedGreen / totalCapacity;
            case ResourceType.Grey:
                return storedGrey / totalCapacity;
        }

        return 0;
    }

    public float GetBurnRate(ResourceType resourceType)
    {
        float burnRate = 0;

        foreach (Brick brickRef in resourceBurnBricks)
        {
            if (brickRef.passiveBurn)
            {
                switch (resourceType)
                {
                    case ResourceType.Blue:
                        burnRate += brickRef.blueBurn[brickRef.GetPoweredLevel()];
                        break;
                    case ResourceType.Red:
                        burnRate += brickRef.redBurn[brickRef.GetPoweredLevel()];
                        break;
                    case ResourceType.Yellow:
                        burnRate += brickRef.yellowBurn[brickRef.GetPoweredLevel()];
                        break;
                    case ResourceType.Green:
                        burnRate += brickRef.greenBurn[brickRef.GetPoweredLevel()];
                        break;
                    case ResourceType.Grey:
                        burnRate += brickRef.greyBurn[brickRef.GetPoweredLevel()];
                        break;
                }
            }
            else if (brickRef.GetComponent<Repair>())
            {
                burnRate += brickRef.GetComponent<Repair>().GetConvertedBurnRate(resourceType, brickRef.GetPoweredLevel());
            }
            else if (brickRef.GetComponent<Gun>())
            {
                burnRate += brickRef.GetComponent<Gun>().GetConvertedBurnRate(resourceType, brickRef.GetPoweredLevel());
            }
        }

        return burnRate;
    }

    public float GetResourceCapacity()
    {
        return totalCapacity;
    }

    void MoveBot(int direction) {
        if (GameController.Instance.isBotDead || GameController.Instance.isPaused)
            return;

        if (!HasFuel())
        {
            GameController.Instance.hud.SetNoFuelPopup(true);
            return;
        }

        bool cFlag = true;

        
        cFlag = CollisionCheck(direction, 0);
        

        if (direction==-1) {  // move left
            if (coreCol > ScreenStuff.leftEdgeCol)
                coreCol--;
            else 
                coreCol = ScreenStuff.rightEdgeCol;
        } else { // move right
            if (coreCol < ScreenStuff.rightEdgeCol)
                coreCol++;
            else 
                coreCol = ScreenStuff.leftEdgeCol;
        }

        GameController.Instance.MoveBot(-direction);      
        CollisionCheck(0, 1);

        //GameController.bgAdjustFlag = -direction;
    }

    public bool CollisionCheck(int directionFlag, int vertFlag){  
        bool collisionFlag = false;
        // check for left-right bit-brick collisions 
        LayerMask bitMask = LayerMask.GetMask("Bit");

        foreach (GameObject brickObj in brickList) {
            //RaycastHit2D rH = Physics2D.Raycast(brickObj.transform.position, new Vector2(directionFlag,0), ScreenStuff.colSize,bitMask);
            RaycastHit2D rH = Physics2D.BoxCast(brickObj.transform.position, Vector2.one * ScreenStuff.colSize, 0, new Vector2(directionFlag, vertFlag), ScreenStuff.colSize, bitMask);
            if (rH.collider!=null) {
                Brick brick = brickObj.GetComponent<Brick>();
                if (rH.collider.gameObject.GetComponent<Bit>() != null)
                {
                    if(!rH.collider.gameObject.GetComponent<Bit>().hasBounced)
                        brick.BitBrickCollide(rH.collider.gameObject);
                }
                else
                {
                    ResolveEnemyCollision(rH.collider.gameObject);
                }
                collisionFlag = true;
                break;
            }
        }
        return collisionFlag;
    }

    public Vector2 GetTopLeftPoint(){
        Vector2 bPos = transform.position;

        return (bPos + new Vector2 (-(maxBotWidth*settings.colSize/2),(maxBotHeight*settings.colSize/2)));

    }

    public Vector2 GetBottomRightPoint(){
        Vector2 bPos = transform.position;

        return (bPos + new Vector2 ((maxBotWidth*settings.colSize/2),-(maxBotHeight*settings.colSize/2)));
    }

    public Vector2Int GetDownVector(){

        // returns a screen vector that represents where 'down' is relative to the bot
        
        Vector2Int downV2;

        switch (botRotation) {
            case 0:
                downV2 = Vector2Int.down;
                break;
            case 1:
                downV2 = Vector2Int.right;
                break;
            case 2:
                downV2 = Vector2Int.up;
                break;
            default:
                downV2 = Vector2Int.left;
                break;
        }
        return downV2;
    }

    public class CollisionPair{
        public GameObject bitObj;
        public GameObject brickObj;
        public Vector2Int bitCoords;
        public Vector2Int brickCoords;
    }

    public class CollisionMap {

        // map of intersection between the bot and a block

        public Block block;
        public Bot bot;
        public Vector2Int botCoreCoords;
        public Vector2Int blockCoreCoords;
        public Vector2Int hitDir;
        public GameObject[,] objArr;
        public int[,] IDArr;
        public int width;
        public int height;
        public List<CollisionPair> collisionPairs;

        public CollisionMap(Block b, Vector2Int h)
        {
            block = b;
            hitDir = h;
        }

        public void AddBot(){

        }

        public Vector2Int GetMapCoordsBit(GameObject bitObj){
            Bit bit = bitObj.GetComponent<Bit>();
            Vector2Int mapPos = new Vector2Int(0,0);

            if (bit!=null)
                mapPos = blockCoreCoords+bit.screenOffset;

            return mapPos;
        }
        
        public Vector2Int GetMapCoordsBrick(GameObject brickObj){
            Brick brick = brickObj.GetComponent<Brick>();
            Vector2Int mapPos = new Vector2Int(0,0);
            
            if (brick!=null)
                mapPos = botCoreCoords+bot.ArrToOffset(brick.arrPos);

            return mapPos;
        }

        public bool IsValidMapPos(Vector2Int arrPos)
        {
            if ((0<=arrPos.x)&&(arrPos.x<width&&(0<=arrPos.y)&&(arrPos.y<height)))
                return true;
            else   
                return false;
        }

        public Vector2Int MapCoordsToBotCoords(Vector2Int mapCoords) {
            Vector2Int botCoords = bot.MapToScreenCoords(mapCoords,width);
            Vector2Int blockV2 = new Vector2Int (block.blockWidth,block.blockWidth);

            return (botCoords-blockV2);
        }
    }

    public CollisionMap CreateCollisionMap(GameObject blockObj, Vector2Int hitDir){
        Block block = blockObj.GetComponent<Block>();
        CollisionMap cMap = new CollisionMap(block,hitDir);
        int blockColOffset = block.GetXOffset(coreCol);
        int blockWidth = block.blockWidth;
        int blockRadius = block.blockRadius;
        int blockRowOffset = ScreenStuff.GetRow(blockObj);
        int mapRadius = block.blockWidth+maxBotRadius;
    
        Vector2Int blockOffset = new Vector2Int (blockColOffset, blockRowOffset);
        Vector2Int borderOffset = new Vector2Int (blockWidth,blockWidth);

        cMap.bot = gameObject.GetComponent<Bot>();
        cMap.width = maxBotWidth+2*block.blockWidth;
        cMap.height = maxBotHeight+2*block.blockWidth;
        cMap.objArr = new GameObject[cMap.width,cMap.height];
        cMap.IDArr = new int[cMap.width,cMap.height];
        cMap.botCoreCoords = new Vector2Int (mapRadius,mapRadius);
        cMap.blockCoreCoords = cMap.botCoreCoords + blockOffset;
        cMap.collisionPairs = new List<CollisionPair>();

        for (int x = 0; x< cMap.width; x++)
            for (int y = 0; y<cMap.height; y++)
                cMap.IDArr[x,y] = -1;

        // add Bot to Map

        foreach (GameObject brickObj in brickList) {
            Brick brick = brickObj.GetComponent<Brick>();
            Vector2Int rotatedCoords = ScreenToBotCoords(brick.arrPos);
            Vector2Int mapCoords = rotatedCoords+borderOffset;

            cMap.objArr[mapCoords.x,mapCoords.y] = brickObj;
            cMap.IDArr[mapCoords.x,mapCoords.y] = brick.ID;
        }
        
        // add Block to Map
        foreach(GameObject bitObj in block.bitList) {
            Vector2Int bitOffset = bitObj.GetComponent<Bit>().screenOffset;
            Vector2Int mapCoords = cMap.blockCoreCoords+bitOffset;
            if (cMap.IsValidMapPos(mapCoords)) {
                cMap.objArr[mapCoords.x,mapCoords.y] = bitObj;
                cMap.IDArr[mapCoords.x,mapCoords.y] = bitObj.GetComponent<Bit>().ID;

                GameObject brickObj = cMap.objArr[mapCoords.x+hitDir.x,mapCoords.y+hitDir.y];
            
                // create list of bits and bricks that are touching

                if (brickObj!=null) {
                    Brick brick = brickObj.GetComponent<Brick>();
                    if (brick!=null) {
                        CollisionPair cPair = new CollisionPair();
                        cPair.bitObj = bitObj;
                        cPair.brickObj = brick.gameObject;
                        cPair.bitCoords = mapCoords;
                        cPair.brickCoords = mapCoords+hitDir;
                        cMap.collisionPairs.Add(cPair);
                    }
                }
            }
        }
        return cMap;
    }

    public void SetFuelAmt(int value)
    {
        storedRed = value;

    }
    
}


