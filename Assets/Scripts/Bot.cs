using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public class Bot : MonoBehaviour
{ 
    public float longPause;
    public float shortPause;

    public int coreCol = 0;
    public int maxBotRadius = 4;

    public static bool orphanCheckFlag = false;
    public static float ghostMoveSpeed = 30f;

    Rigidbody2D botBody;

    public Quaternion rotation1;
    public Quaternion rotation2;

    public Vector2Int[] directionV2Arr = new [] {
        new Vector2Int (0,1),
        new Vector2Int (1,0),
        new Vector2Int (0,-1),
        new Vector2Int (-1,0)};

    float coreX = 0.0f;
    float coreY = 0.0f;
    int maxBotWidth;
    int maxBotHeight;

    public GameObject[,] brickArr;
    public GameObject[] masterBrickList;

    public Vector2Int coreV2;
   
   
    public int botRotation = 1;
    
    GameObject coreBrick; 
    Tilemap startTileMap;
    Grid myGrid;

    private List<GameObject> pathList = new List<GameObject>();
    public List<GameObject> fuelBrickList = new List<GameObject>();

    private AudioSource source;
    public AudioClip tripleSound;

    float startTime;
    public float tripleDelay = 0.5f;
    float delay;

    void Awake() 
    {
        DontDestroyOnLoad(this.gameObject);
        coreBrick = masterBrickList[0];
        maxBotWidth = maxBotRadius * 2 +1;
        maxBotHeight = maxBotRadius * 2 + 1;
        coreV2 = new Vector2Int (maxBotRadius,maxBotRadius);
        brickArr  = new GameObject[maxBotWidth, maxBotHeight];
    }

    void Start()
    {
        gameObject.transform.position = new Vector3(coreX, coreY, 0);
        gameObject.transform.rotation = Quaternion.identity;
        botBody = gameObject.GetComponent<Rigidbody2D>();
        // brickArr[maxBotRadius,maxBotRadius] = coreBrick;
        // coreBrick.GetComponent<Brick>().arrPos = new Vector2Int(maxBotRadius,maxBotRadius);
        
        botRotation=1;
        // add fuel brick
        // AddBrick(new Vector2Int(maxBotRadius,maxBotRadius-1),1,0);
        source = GetComponent<AudioSource>();
        startTileMap = GetComponentInChildren<Tilemap>();
        myGrid = GetComponentInChildren<Grid>();
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
                    Vector3 world = myGrid.LocalToWorld(posV3);
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
        int bType = 2;
        foreach (GameObject brick in masterBrickList) {
            for (int level = 0; level < brick.GetComponent<Brick>().spriteArr.Length; level++)
                if (brick.GetComponent<Brick>().spriteArr[level]==sprite)
                    return brick.GetComponent<Brick>().brickType;

        }
        return bType;
    }


    // Update is called once per frame
    void Update()
    {
        if (GameController.lives == 1)
            MoveBot();

        if (GameController.tripleCheckFlag ==1) {
            GameController.tripleCheckFlag = 0;
            TripleTestBot();
        }
        if (orphanCheckFlag) {
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
        GameController.tripleCheckFlag = 1;
        orphanCheckFlag = true;
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

        brickArr[matchBrick1.GetComponent<Brick>().arrPos.x,matchBrick1.GetComponent<Brick>().arrPos.y] = null;
        brickArr[matchBrick2.GetComponent<Brick>().arrPos.x,matchBrick2.GetComponent<Brick>().arrPos.y] = null;
        RefreshNeighborLists();

        if (IsConnectedToCore(sideBrick1)||IsConnectedToCore(sideBrick2))
            centreIsStable = true;
        
        brickArr[matchBrick1.GetComponent<Brick>().arrPos.x,matchBrick1.GetComponent<Brick>().arrPos.y] = matchBrick1;
        brickArr[matchBrick2.GetComponent<Brick>().arrPos.x,matchBrick2.GetComponent<Brick>().arrPos.y] = matchBrick2;
        RefreshNeighborLists();

        if (centreIsStable) // collapse toward centre
        { 
            SlideDestroyBrick(matchBrick1,testBrick,false);
            SlideDestroyBrick(matchBrick2,testBrick,true);
        }
        else // collapse towards shortest path
        {
            int p1 = ShortestPath(matchBrick1, coreBrick);
            int p2 = ShortestPath(matchBrick2, coreBrick);
            if (p1 < p2) 
            {
                SlideDestroyBrick(testBrick,matchBrick1,false);
                SlideDestroyBrick(matchBrick2,matchBrick1,true);
            }  else  {
                SlideDestroyBrick(matchBrick1,matchBrick2,false);
                SlideDestroyBrick(testBrick,matchBrick2,true);                     
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

    public void SlideDestroyBrick(GameObject brick1, GameObject brick2,bool upgradeFlag) 
    {
        GameObject ghostBrick = new GameObject();
        Rigidbody2D ghostRb = ghostBrick.AddComponent<Rigidbody2D>();
        SpriteRenderer ghostSpriteRenderer = ghostBrick.AddComponent<SpriteRenderer>();
        
        ghostBrick.transform.position = brick1.transform.position;
        ghostRb.isKinematic = true;
        ghostSpriteRenderer.sprite = brick1.GetComponent<SpriteRenderer>().sprite;
       
        brick1.GetComponent<Brick>().DestroyBrick();
        StartCoroutine(SlideGhost(ghostRb,brick2,upgradeFlag));
        //StartCoroutine(WaitAndDestroyGhost(ghostBrick,0.1f));
    }

    IEnumerator WaitAndDestroyGhost(GameObject ghost, float pause) 
    {
        yield return new WaitForSeconds(pause);
        Destroy(ghost);
    }

    IEnumerator WaitAndTripleCheck(float pause)
    {
        yield return new WaitForSeconds(pause);
        GameController.tripleCheckFlag =1;
    }


    IEnumerator SlideGhost(Rigidbody2D ghostRb, GameObject brick2, bool upgradeFlag) {
        float t = 0f;
    
        Vector3 originalPos = ghostRb.transform.position;
        Vector3 newPos = brick2.GetComponent<Rigidbody2D>().transform.position;
        float duration = (newPos-originalPos).magnitude/ghostMoveSpeed;

        while (t< duration)
        {
            ghostRb.transform.position = Vector3.Lerp(originalPos,newPos,t/duration);
            yield return null;
            t+=Time.deltaTime;
        }
        ghostRb.transform.position = newPos;
        Destroy(ghostRb.gameObject);
        if (upgradeFlag == true)
           brick2.GetComponent<Brick>().UpgradeBrick();
    }
    
    public bool IsValidBrickPos(Vector2Int arrPos)
    {
        if ((0<=arrPos.x)&&(arrPos.x<maxBotWidth)&&(0<=arrPos.y)&&(arrPos.y<maxBotHeight))
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
                GameController.tripleCheckFlag = 1;
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
        if (closedGap)
            return gapDist;
        else    
            return 99;
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
        List<GameObject> orphanList = new List<GameObject>();

        for (int x = 0; x < maxBotWidth ; x++ )
            for (int y = 0; y < maxBotHeight ; y++)
                if (brickArr[x,y] != null) 
                    if (IsConnectedToCore(brickArr[x,y])==false)
                        orphanList.Add(brickArr[x,y]);

        return orphanList;
    }

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

    public bool AreConnected(GameObject brick1, GameObject brick2){
        if (ShortestPath(brick1, brick2) >= 99)
            return false;
        else    
            return true;
    }

    public bool IsConnectedToCore(GameObject brick)
    {
        if (brick == null)
            return false;
        if (ShortestPath(brick, brickArr[maxBotRadius,maxBotRadius]) >= 99)
            return false;
        else    
            return true;
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

    

    public GameObject AddBrick(Vector2Int arrPos, int type, int level = 0)
    {  
        Vector2Int offsetV2 = ArrToOffset(arrPos);

        Vector3 offsetV3 = new Vector3(offsetV2.x * ScreenStuff.colSize, offsetV2.y * ScreenStuff.colSize, 0);
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

            newBrickScript.arrPos = arrPos;
            newBrickScript.brickType = type;
            newBrickScript.parentBot = gameObject;
            newBrickScript.SetLevel(level);

            // update neighborLists

            RefreshNeighborLists();

            GameController.tripleCheckFlag = 1;

            return newBrick;
        } else {
            return null;
        }
    }

    public void CollapseDouble(Vector2Int ArrPos1, Vector2Int ArrPos2){
       
        // if a double is hit by similar energy type from out of bounds

        GameObject brick1 = brickArr[ArrPos1.x,ArrPos1.y];
        GameObject brick2 = brickArr[ArrPos2.x,ArrPos2.y];

        int p1 = ShortestPath(brick1, coreBrick);
        int p2 = ShortestPath(brick2, coreBrick);

        if (p1 < p2)
        {
            SlideDestroyBrick(brick2,brick1,true);
        } else {
            SlideDestroyBrick(brick1,brick2,true);
        }
        StartCoroutine(WaitAndTripleCheck(0.2f));
    }

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
        if (GameController.lives == 0)
            return;

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
        if (coreCol > ScreenStuff.leftEdgeCol)
            coreCol--;
        else {
            coreCol = ScreenStuff.rightEdgeCol;
        }
    }

    void MoveBotRight() {
        GameController.bgAdjustFlag = -1;
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
}
