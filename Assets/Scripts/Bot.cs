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
    
    public static bool squareCheckFlag = false;
    public bool tripleCheckFlag = false;

    Rigidbody2D botBody;

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
        
    [HideInInspector]
    public Vector2Int[] upOffsetV2Arr = new [] { // given bot rotation
        new Vector2Int (0,1),
        new Vector2Int (-1,0),
        new Vector2Int (0,-1),
        new Vector2Int (1,0)};

    [HideInInspector]
    public Vector2Int[] downOffsetV2Arr = new [] {
        new Vector2Int (0,-1),
        new Vector2Int (1,0),
        new Vector2Int (0,1),
        new Vector2Int (-1,0)};

    float coreX = 0.0f;
    float coreY = 0.0f;
    int maxBotWidth;
    int maxBotHeight;
 
    public GameObject[,] brickArr;
    public int[,] brickTypeArr;
    public GameObject[] masterBrickList;
    public List<GameObject> brickList;
    int[,] pathArr;

    public Vector2Int coreV2;
    public int botRotation = 1;
    
    GameObject coreBrick; 
    Tilemap startTileMap;
    public Grid startingBrickGrid;

    private List<GameObject> pathList = new List<GameObject>();
    private List<Vector2Int> pathArrList = new List<Vector2Int>();
    public List<GameObject> fuelBrickList = new List<GameObject>();

    private AudioSource source;
    public AudioClip tripleSound;
    GameSettings settings;

    float startTime;
    public float tripleDelay = 0.5f;
    float delay;

    void Awake() 
    {
        DontDestroyOnLoad(this.gameObject);
    }

    void Start()
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
            for (int y = 0; y < maxBotHeight ; y++)
                brickTypeArr[x,y] = -1;
        botRotation=1;
        // add fuel brick
        // AddBrick(new Vector2Int(maxBotRadius,maxBotRadius-1),1,0);
        source = GetComponent<AudioSource>();
        startTileMap = Instantiate(startingBrickGrid.GetComponent<Tilemap>(),new Vector3 (0,0,0), Quaternion.identity);
        AddStartingBricks();
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
    }

    public int GetBrickType(Sprite sprite){
        foreach (GameObject brick in masterBrickList) {
            for (int level = 0; level < brick.GetComponent<Brick>().spriteArr.Length; level++)
                if (brick.GetComponent<Brick>().spriteArr[level]==sprite)
                    return brick.GetComponent<Brick>().brickType;
        }
        return 2; // default
    }


    // Update is called once per frame
    void Update()
    {
        if (GameController.Instance.lives == 1)
            MoveBot();
        /*
        if (squareCheckFlag) {
            squareCheck();
            squareCheckFlag = false;
        }
        */
        if (tripleCheckFlag == true) {
            tripleCheckFlag = false;
            TripleTestBot();
        }  
    
        if ((orphanCheckFlag)&&(settings.Schmetris==false)) {
            ReleaseOrphans();
            orphanCheckFlag = false;
        }
    }

    public void BumpColumn(Vector2Int startArrPos) {
        int length = 1;
        Vector2Int bumpV2 = GetDownVector();
        Vector2Int endVector = startArrPos;
        bool bitIsAboveCore = false;

        if ((IsValidBrickPos(startArrPos)==false)||(brickArr[startArrPos.x,startArrPos.y]==null)||(startArrPos==coreV2))
            return;

        // check to see if bumper is above core
      
       // Vector2Int eOffsetV2 = bot.ArrToOffset(eArrPos);

        switch (botRotation) {
            case 1:
                if (startArrPos.x == 0)
                    bitIsAboveCore = true;
                break;
            case 2:
                if (startArrPos.y == 0)
                    bitIsAboveCore = true;
                break;
            case 3:
                if (startArrPos.x == 0)
                    bitIsAboveCore = true;
                break;
            case 4:
                if (startArrPos.y == 0)
                    bitIsAboveCore = true;
                break;
        }
        if (bitIsAboveCore == false)   

        while ((IsValidBrickPos(endVector+bumpV2))
            && (brickArr[(endVector+bumpV2).x,(endVector+bumpV2).y]!=null))    
        {
            endVector+=bumpV2;
            length++;
            if (endVector == coreV2)
                return;
        }

        // if last brick is pushed out of bounds - orphan it
        if (IsValidBrickPos(endVector+bumpV2)==false) {
            brickArr[endVector.x,endVector.y].GetComponent<Brick>().MakeOrphan();
            length--;
        }
        
        // shift all bricks by one
        for (int l = length ; l > 0 ; l --)
        {
            Vector2Int brickArrPos = startArrPos+bumpV2*(l-1);
            brickArr[brickArrPos.x,brickArrPos.y].GetComponent<Brick>().MoveBrick(brickArrPos+bumpV2);
        }
        RefreshNeighborLists();
        tripleCheckFlag = true;
        orphanCheckFlag = true;
    }

    public void squareCheck()
    {
        bool[] completesquareList = new bool[maxBotRadius+1];

        for (int r = 1; r <= maxBotRadius; r++) 
            completesquareList[r] = IsSquareComplete(r);
        
        for (int r = 1; r <= maxBotRadius; r++) 
            if (completesquareList[r])
                RemoveSquare(r);
    }

    public bool IsSquareComplete(int squareNumber) {
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

    public void RemoveSquare(int squareNumber){
        // top and bottom
        for (int x = -squareNumber; x <= squareNumber; x++) {
            brickArr[maxBotRadius+x,maxBotRadius+squareNumber].GetComponent<Brick>().DestroyBrick();
            brickArr[maxBotRadius+x,maxBotRadius-squareNumber].GetComponent<Brick>().DestroyBrick();
        }
        // sides
        for (int y = 1-squareNumber; y <= squareNumber-1; y++) {
            brickArr[maxBotRadius-squareNumber,maxBotRadius+y].GetComponent<Brick>().DestroyBrick();
            brickArr[maxBotRadius+squareNumber,maxBotRadius+y].GetComponent<Brick>().DestroyBrick(); 
        }
    }


    public void TripleTestBot()
    {
        for (int x = 0; x < maxBotWidth ; x++) {
            for (int y = 0; y < maxBotHeight ; y++) {
                if (brickArr[x,y]!=null)
                    if (TripleTestBrick(new Vector2Int (x,y)) == true)
                        return;
            }
        }
    }

    public bool TripleTestBrick(Vector2Int arrPos)
    {
        bool hMatch = false;
        bool vMatch = false;
        bool centreIsStable = false;
   
        GameObject testBrick = brickArr[arrPos.x, arrPos.y];
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
            matchBrick1 = brickArr[hTestArrPos1.x,hTestArrPos1.y];
            matchBrick2 = brickArr[hTestArrPos2.x,hTestArrPos2.y];
            if (IsValidBrickPos(vTestArrPos1))
                sideBrick1 = brickArr[vTestArrPos1.x,vTestArrPos1.y];
            if (IsValidBrickPos(vTestArrPos2))
                sideBrick2 = brickArr[vTestArrPos2.x,vTestArrPos2.y];
        }
        else if (vMatch) {
            matchBrick1 = brickArr[vTestArrPos1.x,vTestArrPos1.y];
            matchBrick2 = brickArr[vTestArrPos2.x,vTestArrPos2.y];
            if (IsValidBrickPos(hTestArrPos1))
                sideBrick1 = brickArr[hTestArrPos1.x,hTestArrPos1.y];
            if (IsValidBrickPos(hTestArrPos2))
                sideBrick2 = brickArr[hTestArrPos2.x,hTestArrPos2.y];
        } else 
            return false;
    
        // we found a triple!  Collapse it!

        // temporarily remove edge bricks from Array

        Vector2Int m1Pos = matchBrick1.GetComponent<Brick>().arrPos;
        Vector2Int m2Pos = matchBrick2.GetComponent<Brick>().arrPos;

        brickArr[m1Pos.x,m1Pos.y] = null;
        brickArr[m2Pos.x,m2Pos.y] = null;
        brickTypeArr[m1Pos.x,m1Pos.y] = -1;
        brickTypeArr[m2Pos.x,m2Pos.y] = -1;

        RefreshNeighborLists();

        if ((IsConnectedToCore(sideBrick1))||(IsConnectedToCore(sideBrick2)))
            centreIsStable = true;
        
        brickArr[matchBrick1.GetComponent<Brick>().arrPos.x,matchBrick1.GetComponent<Brick>().arrPos.y] = matchBrick1;
        brickArr[matchBrick2.GetComponent<Brick>().arrPos.x,matchBrick2.GetComponent<Brick>().arrPos.y] = matchBrick2;
        brickTypeArr[m1Pos.x,m1Pos.y] = matchBrick1.GetComponent<Brick>().brickType;
        brickTypeArr[m2Pos.x,m2Pos.y] = matchBrick2.GetComponent<Brick>().brickType;

        RefreshNeighborLists();

        if (centreIsStable) // collapse toward centre
        { 
            SlideDestroy(matchBrick1,matchBrick2,testBrick);
        }
        else // collapse towards shortest path
        {
            //int p1 = ShortestPath(matchBrick1, coreBrick);
            //int p2 = ShortestPath(matchBrick2, coreBrick);
            Vector2Int arrPos1 = matchBrick1.GetComponent<Brick>().arrPos;
            Vector2Int arrPos2 = matchBrick2.GetComponent<Brick>().arrPos;

            int p1 = ShortestPathArray(arrPos1, coreV2);
            int p2 = ShortestPathArray(arrPos2, coreV2);
            if (p1 < p2) 
            {
                SlideDestroy(testBrick,matchBrick2,matchBrick1);
            }  else  {
                SlideDestroy(matchBrick1,testBrick,matchBrick2);                    
            }   
        }
        source.PlayOneShot(tripleSound,1.0f);
        StartCoroutine(WaitAndTripleCheck(0.2f));
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

        StartCoroutine(SlideGhost(ghostRb1,obj3,false));
        StartCoroutine(SlideGhost(ghostRb2,obj3,true));
        StartCoroutine(WaitAndTripleCheck(0.2f));
     
        //StartCoroutine(WaitAndDestroyGhost(ghostBrick,0.1f));
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


    IEnumerator SlideGhost(Rigidbody2D ghostRb, GameObject brickObj, bool upgradeFlag) {
        float t = 0f;
    
        Vector3 originalPos = ghostRb.transform.position;
        Vector3 newPos = brickObj.GetComponent<Rigidbody2D>().transform.position;
        float duration = (newPos-originalPos).magnitude/settings.ghostMoveSpeed;

        while (t< duration)
        {
            ghostRb.transform.position = Vector3.Lerp(originalPos,newPos,t/duration);
            yield return null;
            t+=Time.deltaTime;
        }
        ghostRb.transform.position = newPos;
        Destroy(ghostRb.gameObject);
        if (upgradeFlag == true)
           brickObj.GetComponent<Brick>().UpgradeBrick();
    }
    
    public bool IsValidBrickPos(Vector2Int arrPos)
    {
        if ((0<=arrPos.x)&&(arrPos.x<maxBotWidth)&&(0<=arrPos.y)&&(arrPos.y<maxBotHeight))
            return true;
        else   
            return false;
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
        Vector2Int downV2 = GetDownVector();
       
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
            int minFallDist = 99;
            foreach (GameObject orphanBrick in group) {
                int dist = GapToNextBrick(orphanBrick, downV2);
                if ((dist < minFallDist)&&(dist!=0))
                    minFallDist = dist;
            }
            if (minFallDist == 99) { // release Bricks in group
                foreach (GameObject orphanBrick in group) {
                    Brick orphan = orphanBrick.GetComponent<Brick>();
                    if (orphan.brickType == 1) {
                        orphanBrick.GetComponent<Fuel>().Deactivate();
                    }
                    orphan.MakeOrphan();
                }
            }  else {
                foreach (GameObject orphanBrick in group) {
                    Brick orphan = orphanBrick.GetComponent<Brick>();
                    orphan.MoveBrick(orphan.arrPos+(downV2*minFallDist));
                }
                tripleCheckFlag = true;
            }
        }
        RefreshNeighborLists();
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

    public Vector2Int TwistCoordsUpright(Vector2Int arrXY) {
        Vector2Int newCoords = new Vector2Int();

        switch (botRotation) {
            case 1:
                newCoords = arrXY;
                break;
            case 2:
                newCoords.x = maxBotHeight - 1 - arrXY.y;
                newCoords.y = arrXY.x;
                break;
            case 3:
                newCoords.x = maxBotWidth - 1 - arrXY.x;
                newCoords.y = maxBotHeight - 1 - arrXY.y;
                break;
            default:
                newCoords.x = arrXY.y;
                newCoords.y = maxBotWidth - 1 - arrXY.x;
                break;
        }
        return newCoords;
    }

    public Vector2Int TwistCoordsRotated(Vector2Int arrV2) {
        Vector2Int newCoords = new Vector2Int();

        switch (botRotation) {
            case 1:
                newCoords = arrV2;
                break;
            case 2:
                newCoords.x = arrV2.y;
                newCoords.y = maxBotWidth - 1 - arrV2.x;
                break;
            case 3:
                newCoords.x = maxBotWidth - 1 - arrV2.x;
                newCoords.y = maxBotHeight - 1 - arrV2.y;
                break;
            default:
                newCoords.x = maxBotHeight - 1 - arrV2.y;
                newCoords.y = arrV2.x;
                break;
        }
        return newCoords;
    }

    public Vector2Int TwistOffsetRotated(Vector2Int offset) {
        Vector2Int newCoords = new Vector2Int();

        switch (botRotation) {
            case 1: 
                newCoords = offset;
                break;
            case 2: 
                newCoords.x = offset.y;
                newCoords.y = -offset.x; 
                break;
            case 3: 
                newCoords.x = -offset.x;
                newCoords.y = -offset.y;
                break;
            default:
                newCoords.x = -offset.y;
                newCoords.y = offset.x;
                break;
        }
        return newCoords;
    }

    public Vector2Int TwistOffsetUpright(Vector2Int offset){
        Vector2Int newCoords = new Vector2Int();

        switch (botRotation) {
            case 1: 
                newCoords = offset;
                break;
            case 2: 
                newCoords.x = -offset.y;
                newCoords.y = offset.x;
                break;
            case 3: 
                newCoords.x = -offset.x;
                newCoords.y = -offset.y;
                break;
            default:
                newCoords.x = offset.y;
                newCoords.y = -offset.x; 
                break;
        }
        return newCoords;
    }
    

    public List<GameObject> GetOrphans(){
        List<GameObject> connectedList = new List<GameObject>();
        List<GameObject> orphanList = new List<GameObject>();

        pathArr = brickTypeArr.Clone() as int[,];
        
        ExpandConnectedList(ref connectedList,coreV2);

        for (int x = 0; x < maxBotWidth ; x++ )
            for (int y = 0; y < maxBotHeight ; y++)
                if (brickTypeArr[x,y] != -1) 
                    if (connectedList.Contains(brickArr[x,y]) == false)
                        orphanList.Add(brickArr[x,y]);

        return orphanList;
    }

    public void ExpandConnectedList(ref List<GameObject> currentList, Vector2Int arrPos){
        GameObject thisBrick = brickArr[arrPos.x,arrPos.y];

        foreach (GameObject neighborBrick in thisBrick.GetComponent<Brick>().neighborList){
            Vector2Int neighborArrPos = neighborBrick.GetComponent<Brick>().arrPos;
            if (pathArr[neighborArrPos.x,neighborArrPos.y] >= 0) {
                currentList.Add(neighborBrick);
                pathArr[neighborArrPos.x,neighborArrPos.y] = -2;
                ExpandConnectedList(ref currentList, neighborArrPos);
            }
        }
    }

/* 
    public int ShortestPath(GameObject brick1, GameObject brick2)
    {
        int pathDistance = 1; // default for neighbours
        int minNeighborDist = 99;
        int nDist;

        if ((brick1==null)||(brick2==null))
            return 99;
        else if (brick1 == brick2)
            return 0;
        else if (IsNeighbor(brick1, brick2) == false)
        {
            // add this brick to the pathList
            // add the shortestPath of all non-path neighbors to pathDistance
            Brick brickScript1 = brick1.GetComponent<Brick>();
            int neighborCount = brickScript1.neighborList.Count;

            pathList.Add(brick1);
           
            for (int n = 0; n < neighborCount; n++)
            {
                GameObject nBrick = brickScript1.neighborList[n];

                if (pathList.Contains(nBrick))
                    nDist = 99;
                else
                    nDist = ShortestPath(nBrick, brick2);
                if (nDist < minNeighborDist)
                    minNeighborDist = nDist;
            }
            pathList.Remove(brick1); 
            pathDistance = minNeighborDist + 1;
        }
       
        return pathDistance;
    }
    */

    // Shortest Path Using Arrays
    public int ShortestPathArray(Vector2Int arrPos1, Vector2Int arrPos2)
    {
        if ((IsValidBrickPos(arrPos1)==false)||(IsValidBrickPos(arrPos2)==false))
            return 99;
        if ((brickArr[arrPos1.x,arrPos1.y]==null)||(brickArr[arrPos2.x,arrPos2.y]==null))
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
        return pathArr[arrPos2.x,arrPos2.y]-1;
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

    /*
     public bool AreConnected(GameObject brick1, GameObject brick2){
        if (ShortestPath(brick1, brick2) >= 99)
            return false;
        else    
            return true;
    }
    */

    public bool AreConnected(GameObject brick1, GameObject brick2){
        if ((brick1 == null) || (brick2 == null))
            return false;
        if (ShortestPathArray(brick1.GetComponent<Brick>().arrPos,brick2.GetComponent<Brick>().arrPos) >= 99)
            return false;
        else    
            return true;
    }

    
   /*  public bool IsConnectedToCore(GameObject brick)
    {
        if (brick == null)
            return false;
        if (ShortestPathArray(brick.GetComponent<Brick>().arrPos, coreV2) >= 99)
            return false;
        else    
            return true;
    }
    
*/
    

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
        // if ((brickTypeArr[arrPos1.x,arrPos1.y]==0)||(brickTypeArr[arrPos2.x,arrPos2.y]==0))
         //   return false;
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
        Component[] brickScripts = GetComponentsInChildren<Brick>();

        foreach (Brick brick in brickScripts) {
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
        foreach (Brick brick in brickScripts) {
            // if ((!brick.IsCore())&&(brick!=null))
            if (brick!=null)
                brick.RotateUpright();
        }
    }
/* 
    IEnumerator MoveBrickOverTime (Vector2Int originalPos, Vector2Int finalPos, float duration) {
        float t = 0f;
        GameObject brick = brickArr[originalPos.x,originalPos.y];

        if (brickArr[finalPos.x,finalPos.y]!=null)
            return;
        
        while (t<duration)
        {
            
        }
    }
    */


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

       // GameObject[] brickTypesArr = new GameObject[8] {coreBrick, redBrick, greenBrick, blueBrick, cyanBrick, pinkBrick, greyBrick, purpleBrick};

        // check to see if Brick will form vertical triple

        // check to see that position is valid and empty

        if ((IsValidBrickPos(arrPos))&&(brickArr[arrPos.x,arrPos.y]==null)){
            offsetV3 = gameObject.transform.rotation * offsetV3;

            newBrick = Object.Instantiate(masterBrickList[type], new Vector3(coreX,coreY,0), Quaternion.identity, gameObject.transform);
            Brick newBrickScript = newBrick.GetComponent<Brick>();

            newBrick.transform.Translate(offsetV3);

            brickArr[arrPos.x, arrPos.y] = newBrick;
            brickTypeArr[arrPos.x,arrPos.y] = type;

            newBrickScript.arrPos = arrPos;
            newBrickScript.brickType = type;
            newBrickScript.ID = type*100;
            newBrickScript.parentBot = gameObject;
            newBrickScript.SetLevel(level);

            // update neighborLists

            RefreshNeighborLists();

            tripleCheckFlag = true;

            return newBrick;
        } else {
            return null;
        }
    }


    public void ResolveCollision(Vector2Int arrPos,Vector2Int hitDir, GameObject colliderBitObj)
    {
        
    }

    public void AddBlock(Vector2Int arrPos,Vector2Int hitDir, GameObject colliderBitObj) {
        if(colliderBitObj == null)
            return;

        int[,] mergedTypeArr = new int[maxBotWidth+4,maxBotHeight+4];
        GameObject blockObj = colliderBitObj.transform.parent.gameObject;
        Block block = blockObj.GetComponent<Block>();
        Bit colliderBit = colliderBitObj.GetComponent<Bit>();
        
        int xOffset = block.GetXOffset(coreCol);
        int blockRow = ScreenStuff.GetRow(blockObj);
    
        Vector2Int blockOffset = new Vector2Int (xOffset, blockRow); 
        
        bool bounceBlockFlag = false;
    
        List<BorderTriple> deepTripleList = new List<BorderTriple>();
        List<BorderTriple> shallowTripleList = new List<BorderTriple>();

        // combine Brick types and Bit types into mergedTypeArr

        for (int x=0;x<maxBotWidth+4;x++){
            for (int y=0;y<maxBotWidth+4;y++){
                mergedTypeArr[x,y]=-1;
            }
        }

        foreach (GameObject brickObj in brickList) {
            Brick brick = brickObj.GetComponent<Brick>();
            mergedTypeArr[brick.arrPos.x+2,brick.arrPos.y+2]=brickTypeArr[brick.arrPos.x,brick.arrPos.y];
        }
        
        foreach(GameObject bitObj in block.bitList) {
            // Vector2Int mergedBitArrPos = GetMergedArrPos(bitObj.GetComponent<Bit>(),colliderBit,hitDir,arrPos);
            Vector2Int mergedBitArrPos = bitObj.GetComponent<Bit>().offset;
            if(IsValidMergedBrickPos(mergedBitArrPos))
                mergedTypeArr[mergedBitArrPos.x,mergedBitArrPos.y]=bitObj.GetComponent<Bit>().ConvertToBrickType();
        }

        // Collapse Cross-Border Triples //

        foreach(GameObject bitObj in block.bitList) {
            Vector2Int borderBitArrPos = GetMergedArrPos(bitObj.GetComponent<Bit>(),colliderBit,hitDir,arrPos);
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

        // Add remaining Bits within Bounds

        List<GameObject[]> newBrickList = new List<GameObject[]>();

        foreach (GameObject bitObj in block.bitList) {
            Bit bit = bitObj.GetComponent<Bit>();
            Vector2Int mergedArrPos = GetMergedArrPos(bitObj.GetComponent<Bit>(),colliderBit,hitDir,arrPos);
            Vector2Int bitArrPos = mergedArrPos - new Vector2Int(2,2); 
            if (IsValidBrickPos(bitArrPos)){
                int brickType = bit.ConvertToBrickType();
                GameObject newBrick = AddBrick(bitArrPos,brickType,bit.bitLevel);
                GameObject[] brickBitPair = new GameObject[] {newBrick,bitObj};
                newBrickList.Add(brickBitPair);
            }
        }

        // remove bits connected to Bot from Block.

        foreach (GameObject[] brickBitPair in newBrickList){
            if (IsConnectedToCore(brickBitPair[0])){
                brickBitPair[1].GetComponent<Bit>().RemoveFromBlock("Destroy");
            } else {
                brickBitPair[0].GetComponent<Brick>().DestroyBrick();
            }
        }

        // if the block still has bits directly above bricks - bounce the block

        if (blockObj!=null) {
            foreach (GameObject bitObj in block.bitList) {
                Bit bit = bitObj.GetComponent<Bit>();
                Vector2Int mergedArrPos = GetMergedArrPos(bitObj.GetComponent<Bit>(),colliderBit,hitDir,arrPos);
                Vector2Int testArrPos = mergedArrPos + TwistOffsetUpright(hitDir);
                if (mergedTypeArr[testArrPos.x,testArrPos.y]>=0)
                    bounceBlockFlag = true;
            }
            if (bounceBlockFlag) {
                blockObj.GetComponent<Block>().BounceBlock();
            } 
        }

        StartCoroutine(WaitAndTripleCheck(0.2f));
    }

    void CollapseBorderTripleShallow(BorderTriple borderTriple){
        // Two Bricks and one Bit 
        GameObject bitObj = borderTriple.BitObj;
        Vector2Int startArrPos = borderTriple.StartArrPos;
        Vector2Int endArrPos = borderTriple.EndArrPos;
        Bit bit = bitObj.GetComponent<Bit>();
        GameObject blockObj = bitObj.transform.parent.gameObject;
        Block block = blockObj.GetComponent<Block>();
        GameObject innerBrickObj = brickArr[endArrPos.x-2,endArrPos.y-2];
        GameObject borderObj = brickArr[startArrPos.x-2,startArrPos.y-2];
        if (borderObj == null) {
            Vector2Int borderBrickDir = TwistOffsetUpright(endArrPos-startArrPos);
            borderObj = block.bitArr[bit.blockArrPos.x+borderBrickDir.x,bit.blockArrPos.y+borderBrickDir.y];
        }
        SlideDestroy(bitObj, borderObj, innerBrickObj);
    }

    void CollapseBorderTripleDeep(BorderTriple borderTriple){
          // two bits and one Brick
        GameObject bitObj = borderTriple.BitObj;
        Bit bit = bitObj.GetComponent<Bit>();
        GameObject blockObj = bitObj.transform.parent.gameObject;
        Block block = blockObj.GetComponent<Block>();
        Vector2Int outerBitDir = TwistOffsetUpright(borderTriple.EndArrPos-borderTriple.StartArrPos);
        GameObject outerBitObj = block.bitArr[bit.blockArrPos.x+outerBitDir.x/2,bit.blockArrPos.y+outerBitDir.y/2];
        GameObject borderObj = brickArr[borderTriple.EndArrPos.x-2,borderTriple.EndArrPos.y-2];
        SlideDestroy(outerBitObj, bitObj, borderObj);
    }


    public Vector2Int GetMergedArrPos(Bit bit,Bit colliderBit,Vector2Int hitDir,Vector2Int colliderBrickArrPos){
        Vector2Int twistedBitOffset = TwistOffsetUpright(bit.offset-colliderBit.offset);
        Vector2Int twistedHitDir = TwistOffsetUpright(hitDir);
        Vector2Int mergedBitArrPos = colliderBrickArrPos - twistedHitDir - twistedBitOffset + new Vector2Int (2,2);
           
        return mergedBitArrPos;
    }

/*
    public bool DoesBlockFit(Block block){
        bool fit = true;
        if (block == null)
            return false;

        Vector2Int blockOffset = new Vector2Int (block.column-coreCol, block.row);
        
        foreach(GameObject bit in block.bitList) {
            Vector2Int bitPos = blockOffset + bit.GetComponent<Bit>().offset + coreV2;
            Vector2Int rotatedBitPos = TwistCoordsUpright(bitPos);

            if (IsValidBrickPos(rotatedBitPos)==false)
                fit = false;
        }
        return fit;
    }

    public void CollapseDouble(Vector2Int ArrPos1, Vector2Int ArrPos2){
       
        // if a double is hit by similar energy type from out of bounds

        GameObject brick1 = brickArr[ArrPos1.x,ArrPos1.y];
        GameObject brick2 = brickArr[ArrPos2.x,ArrPos2.y];

        //int p1 = ShortestPath(brick1, coreBrick);
        //int p2 = ShortestPath(brick2, coreBrick);
        int p1 = ShortestPathArray(ArrPos1,coreV2);
        int p2 = ShortestPathArray(ArrPos2,coreV2);

        if (p1 < p2)
        {
            SlideDestroy(brick2,brick1,true);
        } else {
            SlideDestroy(brick1,brick2,true);
        }
        StartCoroutine(WaitAndTripleCheck(0.2f));
    }
*/
    public Vector2Int OffsetToArray(Vector2Int offsetV2)
    {
        return offsetV2 + coreV2;
    }

    public Vector2Int ArrToOffset(Vector2Int arrPos)
    {
        return arrPos - coreV2;
    }

    void MoveBot()
    {
        if (GameController.Instance.lives == 0)
            return;

        if (settings.Schmetris==false)
            if (fuelBrickList.Count == 0)
                return;
            
        if (Input.GetKeyDown(KeyCode.UpArrow)||Input.GetKeyDown("e")) {
            botRotation++;
            rotation1 = botBody.transform.rotation;
            rotation2 = rotation1*Quaternion.Euler(0,0,-90);
            StartCoroutine(RotateOverTime(rotation1, rotation2, 0.05f));
           
        }       

        if (Input.GetKeyDown(KeyCode.DownArrow)||Input.GetKeyDown("q")){
            botRotation--;
            rotation1 = botBody.transform.rotation;
            rotation2 = rotation1*Quaternion.Euler(0,0,90);   
            StartCoroutine(RotateOverTime(rotation1, rotation2, 0.05f));
        }

        if (botRotation == 5)
            botRotation = 1;
        if (botRotation == 0)
            botRotation = 4;
      
        if (Input.GetKeyDown(KeyCode.LeftArrow)||Input.GetKeyDown("a"))
        {
           // m = leftstep;
            delay = longPause;
            startTime = Time.time;
            MoveBotLeft();
        } 
        else if (Input.GetKey(KeyCode.LeftArrow)||Input.GetKey("a"))
        {
            if (startTime + delay <= Time.time)
            {
                startTime = Time.time;
                delay = shortPause;
                MoveBotLeft();
            }
        }

        if (Input.GetKeyDown(KeyCode.RightArrow)||Input.GetKeyDown("d"))
        {
            delay = longPause;
            startTime = Time.time;
            MoveBotRight();
        }
        else if (Input.GetKey(KeyCode.RightArrow)||Input.GetKey("d"))
        {
            if (startTime + delay <= Time.time)
            {
                startTime = Time.time;
                delay = shortPause;
                MoveBotRight(); 
            }
        }
    }
    
    void MoveBotLeft() {
        GameController.bgAdjustFlag = 1;
        /*
        List<GameObject> leftBlockList = GetBlocksLeft();

        foreach (GameObject block in leftBlockList) {
            AddBlock(block);
        }*/

        if (coreCol > ScreenStuff.leftEdgeCol)
            coreCol--;
        else {
            coreCol = ScreenStuff.rightEdgeCol;
        }
    }

    void MoveBotRight() {
        GameController.bgAdjustFlag = -1;
       /*
        List<GameObject> rightBlockList = GetBlocksRight();

        foreach (GameObject block in rightBlockList) {
            AddBlock(block);
        } */

        if (coreCol < ScreenStuff.rightEdgeCol)
            coreCol++;
        else {
            coreCol = ScreenStuff.leftEdgeCol;
        }
    }

    public Vector2Int GetDownVector(){
        Vector2Int downV2;

        switch (botRotation) {
            case 1:
                downV2 = Vector2Int.down;
                break;
            case 2:
                downV2 = Vector2Int.right;
                break;
            case 3:
                downV2 = Vector2Int.up;
                break;
            default:
                downV2 = Vector2Int.left;
                break;
        }
        return downV2;
    }


    public class CollisionMap {
        public Block block;
        public Vector2Int botCoreCoords;
        public Vector2Int blockCoreCoords;
        public Vector2Int hitDir;
        public GameObject[,] objArr;
        public int[,] IDArr;
        public int width;
        public int height;

        public CollisionMap(Block b, Vector2Int h)
        {
            block = b;
            hitDir = h;
        }

        public void AddBot(){

        }
    }

    
    public CollisionMap CreateCollisionMap(GameObject blockObj, Vector2Int hitDir){
        Block block = blockObj.GetComponent<Block>();
        CollisionMap cMap = new CollisionMap(block,hitDir);
        int xOffset = block.GetXOffset(coreCol);
        int blockWidth = block.blockWidth;
        int blockRadius = block.blockRadius;
        int blockRow = ScreenStuff.GetRow(blockObj);
        int mapRadius = block.blockWidth+maxBotRadius;
    
        Vector2Int blockOffset = new Vector2Int (xOffset, blockRow);
        cMap.width = maxBotWidth+2*block.blockWidth;
        cMap.height = maxBotHeight+2*block.blockWidth;
        cMap.objArr = new GameObject[cMap.width,cMap.height];
        cMap.IDArr = new int[cMap.width,cMap.height];
        cMap.botCoreCoords = new Vector2Int (mapRadius,mapRadius);
        cMap.blockCoreCoords = cMap.botCoreCoords + blockOffset;

        for (int x = 0; x< cMap.width; x++)
            for (int y = 0; y<cMap.height; y++)
                cMap.IDArr[x,y] = -1;

        foreach (GameObject brickObj in brickList) {
            Brick brick = brickObj.GetComponent<Brick>();
            Vector2Int mapCoords = new Vector2Int (brick.arrPos.x+blockWidth,brick.arrPos.y+blockWidth);

            cMap.objArr[mapCoords.x,mapCoords.y] = brickObj;
            cMap.IDArr[mapCoords.x,mapCoords.y] = brick.ID;
        }
        
        foreach(GameObject bitObj in block.bitList) {
            Vector2Int bitOffset = bitObj.GetComponent<Bit>().offset;
            Vector2Int mapCoords = new Vector2Int (cMap.blockCoreCoords.x+bitOffset.x,cMap.blockCoreCoords.y+bitOffset.y);

            cMap.objArr[mapCoords.x,mapCoords.y] = bitObj;
            cMap.IDArr[mapCoords.x,mapCoords.y] = bitObj.GetComponent<Bit>().ID;
        }

        return cMap;
    }



            



        return cMap;
    }


}

/*
    public List<GameObject> GetBlocksLeft() {
        List<GameObject> leftBlocks = new List<GameObject>();
        foreach (GameObject blockObj in GameController.Instance.blockList) {
            if (blockObj==null)
                break;
            Block block = blockObj.GetComponent<Block>();
            Vector2Int blockOffset = new Vector2Int (block.column-coreCol,block.row-maxBotRadius);
            
            foreach(GameObject bit in block.bitList) {
                Vector2Int bitOffset = bit.GetComponent<Bit>().offset;
                Vector2Int testPos = blockOffset + bitOffset + coreV2 + Vector2Int.right;
                Vector2Int rotatedTestPos = TwistCoordsUpright(testPos);

                if (IsValidBrickPos(rotatedTestPos)==true)
                if (brickTypeArr[rotatedTestPos.x,rotatedTestPos.y]>=0) {
                    leftBlocks.Add(blockObj);
                    break;
                }
            }
        }
        return leftBlocks;
    }

     public List<GameObject> GetBlocksRight() {
        List<GameObject> rightBlocks = new List<GameObject>();
        foreach (GameObject blockObj in GameController.Instance.blockList) {
            if (blockObj==null)
                break;
            Block block = blockObj.GetComponent<Block>();
            Vector2Int blockOffset = new Vector2Int (block.column-coreCol,block.row-maxBotRadius);
            
            foreach(GameObject bit in block.bitList) {
                Vector2Int bitOffset = bit.GetComponent<Bit>().offset;
                Vector2Int testPos = blockOffset + bitOffset + coreV2 + Vector2Int.left;
                Vector2Int rotatedTestPos = TwistCoordsUpright(testPos);

                if (IsValidBrickPos(rotatedTestPos)==true)
                if (brickTypeArr[rotatedTestPos.x,rotatedTestPos.y]>=0) {
                    rightBlocks.Add(blockObj);
                    break;
                }
            }
        }
        return rightBlocks;
    }*/

