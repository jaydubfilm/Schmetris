using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

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
    int[,] pathArr;
    public PowerGrid powerGrid;

    public Vector2Int coreV2;
    public int botRotation = 0;
    
    GameObject coreBrick; 
    Tilemap startTileMap;
    public Grid startingBrickGrid;
    public GameObject blockPrefab;
    public GameObject bitPrefab;
    public GameObject powerWarning;


    private List<GameObject> pathList = new List<GameObject>();
    private List<Vector2Int> pathArrList = new List<Vector2Int>();
    public List<GameObject> fuelBrickList = new List<GameObject>();

    private AudioSource source;
    public AudioClip tripleSound;
    GameSettings settings;

    float startTime;
    public float tripleDelay = 0.5f;
    float delay;

    void OnGameOver()
    {
        for (int x = 0; x < brickList.Count; x++ )
            
        {   
            GameObject brick = brickList[x];
            brick.GetComponent<Brick>().ExplodeBrick();
        }
    }

    void OnGameRestart()
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
        powerGrid = Instantiate(powerGrid, gameObject.transform);

        startTileMap = Instantiate(startingBrickGrid.GetComponent<Tilemap>(), new Vector3(0, 0, 0), Quaternion.identity);
        AddStartingBricks();
        powerGridRefreshFlag = true;
    }

    private void OnEnable()
    {
        GameController.OnGameOver += OnGameOver;
        GameController.OnGameRestart += OnGameRestart;
    }

    private void OnDisable()
    {
        GameController.OnGameOver -= OnGameOver;
        GameController.OnGameRestart -= OnGameRestart;
    }

    void Awake() 
    {
        DontDestroyOnLoad(this.gameObject);
    }

    public void Start()
    {
        settings = GameController.Instance.settings;
        maxBotRadius = settings.maxBotRadius;
        coreBrick = masterBrickList[0];
        maxBotWidth = maxBotRadius * 2 +1;
        maxBotHeight = maxBotRadius * 2 + 1;
        coreV2 = new Vector2Int (maxBotRadius,maxBotRadius);
        brickArr  = new GameObject[maxBotWidth, maxBotHeight];
        brickTypeArr = new int[maxBotWidth, maxBotHeight];
        gameObject.transform.position = new Vector3(coreX, coreY, 0);
        gameObject.transform.rotation = Quaternion.identity;
        botBody = gameObject.GetComponent<Rigidbody2D>();

        for (int x = 0; x < maxBotWidth; x++)
            for (int y = 0; y < maxBotHeight ; y++) {
                brickTypeArr[x,y] = -1;
            }
        botRotation=0;
        powerGrid = Instantiate(powerGrid,gameObject.transform);
  
        source = GetComponent<AudioSource>();
        startTileMap = Instantiate(startingBrickGrid.GetComponent<Tilemap>(),new Vector3 (0,0,0), Quaternion.identity);
        AddStartingBricks();
        powerGridRefreshFlag = true;
    }

     // Update is called once per frame
    void Update()
    {
        MoveCheck();

        if (tripleCheckFlag == true) {
            powerGridRefreshFlag = false;
            tripleCheckFlag = false;
            TripleTestBot();
            StartCoroutine(WaitAndRefreshPower(0.5f));
        }  
    
        if ((orphanCheckFlag)&&(settings.Schmetris==false)) {
            StartCoroutine(WaitAndReleaseOrphans(0.2f));
            orphanCheckFlag = false;
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

        if ((IsValidBrickPos(startArrPos)==false)||(BrickAtBotArr(startArrPos)==null)||(startArrPos==coreV2))
            return;

        Vector2Int startCoords = BotToScreenCoords(startArrPos);

        // check to see if bump would bump core 

        Vector2Int testVector = coreV2-startCoords;

        if (((testVector.x==0)&&bumpDirV2.x==0)||((testVector.y==0)&&bumpDirV2.y==0))
            return; 

        // find end of line

        int length = 1;
       
        Vector2Int endCoords = startCoords;

        while (bumpDirV2 != Vector2.zero && (IsValidScreenPos(endCoords + bumpDirV2)) && (BrickAtScreenArr((endCoords + bumpDirV2)) != null))
        {
            endCoords += bumpDirV2;
            length++;
            if (endCoords == coreV2)
                return;
        }

        // if last brick is pushed out of bounds - orphan it
        if (IsValidScreenPos(endCoords+bumpDirV2)==false) {
            BrickAtScreenArr(endCoords).GetComponent<Brick>().MakeOrphan();
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
            if (!brickObj.GetComponent<Parasite>() && TripleTestBrick(brickObj.GetComponent<Brick>().arrPos) == true) {
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
        }
        else // collapse towards shortest path
        {
            int p1 = ShortestPathArray(m1Pos, coreV2);
            int p2 = ShortestPathArray(m2Pos, coreV2);
            if (p1 < p2) 
            {
                SlideDestroy(testBrick,matchBrick2,matchBrick1);
            }  else  {
                SlideDestroy(matchBrick1,testBrick,matchBrick2);                    
            }   
        }
        source.PlayOneShot(tripleSound,1.0f);
        orphanCheckFlag = true;
        StartCoroutine(WaitAndTripleCheck(0.2f));
        RefreshNeighborLists();
        return true;
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
        if (brick == null) {
            bit = obj1.GetComponent<Bit>();
            bit.RemoveFromBlock("destroy");
        } else
            brick.DestroyBrick();

        brick = obj2.GetComponent<Brick>();
        if (brick == null) {
            bit = obj2.GetComponent<Bit>();
            bit.RemoveFromBlock("destroy");
        } else
            brick.DestroyBrick();

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

    IEnumerator WaitAndTripleCheck(float pause)
    {
        yield return new WaitForSeconds(pause);
        tripleCheckFlag = true;
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
                newEnemyObj = Instantiate(GameController.Instance.speciesSpawnData[type].species, orphanBrickObj.transform.position, Quaternion.identity);
                newEnemyObj.GetComponent<Enemy>().hP = orphanBrick.brickHP;
            } else {
                GameObject newBitObj;
                Vector3 bPos = orphanBrick.transform.position;
                int type = orphanBrick.ConvertToBitType();
                int level = orphanBrick.brickLevel;

                newBitObj = Instantiate(bitPrefab,bPos,Quaternion.identity);
                newBitObj.transform.parent = newBlockObj.transform;  
                Bit newBit = newBitObj.GetComponent<Bit>();
                newBit.bitType = type;
                newBit.bitLevel = level;
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
            if (pathArr[nPos.x,nPos.y] >= 0)
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
            brickObj.GetComponent<Brick>().RotateUpright();
        }
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

        if ((type>8)||(type<0))
            return null;

        // check to see that array position is valid and empty

        if ((IsValidBrickPos(arrPos)==false)||(BrickAtBotArr(arrPos)!=null))
            return null;

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
    
        if (!IsValidBrickPos(bCoords))
            return;

        if (BrickAtBotArr(bCoords)!=null)
            return;

        // check to see if the enemy can attach to a brick;

        bool hasNeighbor = false;   
        for (int x = 0; x< 4; x++) {
            
            Vector2Int testCoords = bCoords + directionV2Arr[x];
            if (IsValidBrickPos(testCoords))
                if (BrickAtBotArr(testCoords)!=null)
                    hasNeighbor = true;
        }
        if (!hasNeighbor)
            return;
        
        int brickType = enemy.data.type;

        // enemies turn into bricks once they collide with Bot

        GameObject newBrick = AddBrick(bCoords,brickType,0);
        newBrick.GetComponent<Parasite>().data = enemy.data;
        newBrick.GetComponent<Parasite>().targetBrick = enemy.targetBrick;
        newBrick.GetComponent<Brick>().brickHP = enemy.hP;
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

        foreach (GameObject bitObj in block.bitList) {
            Bit bit = bitObj.GetComponent<Bit>();
            Vector2Int bitMapCoords = cMap.GetMapCoordsBit(bitObj);
            Vector2Int botCoords = cMap.MapCoordsToBotCoords(bitMapCoords);
           if (IsValidBrickPos(botCoords)){
                int brickType = bit.ConvertToBrickType();
                GameObject newBrick = AddBrick(botCoords,brickType,bit.bitLevel);
                if (newBrick!=null) {
                    BrickBitPair brickBitPair = new BrickBitPair(newBrick,bitObj);
                    brickBitPairList.Add(brickBitPair);
                    GameController.Instance.money++;
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

    void MoveCheck()
    {
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
    }

    void Rotate(int direction) {
        if (!HasFuel())
            return;
        
        botRotation+=direction;
        rotation1 = botBody.transform.rotation;
        rotation2 = rotation1*Quaternion.Euler(0,0,-direction*90);
        StartCoroutine(RotateOverTime(rotation1, rotation2, 0.05f));
        CorrectBotRotation();   
    }

    void CorrectBotRotation(){
        if (botRotation == 4)
            botRotation = 0;
        if (botRotation == -1)
            botRotation = 3;
    }
    
    public bool HasFuel() {
        if (fuelBrickList.Count == 0)
            return false;
        else { // activate new fuel cell
            if (fuelBrickList[0]==null)
                Debug.Log("wtf");
            Fuel fuel = fuelBrickList[0].GetComponent<Fuel>();
            if (fuel.active==false)
                fuel.Activate();
            return true;
        }
    }

    void MoveBot(int direction) {
        if (GameController.Instance.lives == 0)
            return;

        if (!HasFuel())
            return;

        bool cFlag = true;

        
        cFlag = CollisionCheck(direction);
        

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

        GameController.bgAdjustFlag = -direction;
    }

    public bool CollisionCheck(int directionFlag){  
        bool collisionFlag = false;
        // check for left-right bit-brick collisions 
        LayerMask bitMask = LayerMask.GetMask("Bit");

        foreach (GameObject brickObj in brickList) {
            RaycastHit2D rH = Physics2D.Raycast(brickObj.transform.position, new Vector2(directionFlag,0), ScreenStuff.colSize,bitMask); 
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
}


