using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shape : MonoBehaviour
{
    public int column;
    public int cellCount;
    public int deadFlag;
    GameObject[] cellArr; 
    Bot bot;
    int botWidth;

    // Start is called before the first frame update
    void Start()
    {
        cellCount = 0;
        deadFlag = 0;
        cellArr = new GameObject[transform.childCount];
        foreach (Transform child in gameObject.transform) {
            cellArr[cellCount] = child.gameObject;
            cellCount++;
        }
        //AssignCellOffsets();
        bot = (Bot)FindObjectOfType(typeof(Bot));
        botWidth = bot.maxBotRadius * 2 +1;
    }
    
    void Update(){
        if (CheckForPatternMatch()&&(deadFlag==0)) {
            deadFlag=1;
            ExplodeShape();
        }   
    }
    

    bool CheckForPatternMatch() {
        int bX = column - bot.coreCol + bot.maxBotRadius;
        if ((bX < 0)||(bX > botWidth-1))
            return false;
        for (int bY = 0; bY < botWidth; bY++) {
            Vector2Int bCoords = new Vector2Int(bX,bY);  
            Vector2Int bCoordsUpright = bot.TwistCoordsUpright(bCoords);

            GameObject brick = bot.brickArr[bCoordsUpright.x,bCoordsUpright.y];
            if (brick!=null) {
                Cell cellZero = cellArr[0].GetComponent<Cell>();
                if (brick.GetComponent<Brick>().brickType == cellZero.matchType) {
                    int matchCount = 1;
                    for (int c = 1; c < cellCount; c++) {
                        Cell testCell = cellArr[c].GetComponent<Cell>();
                        Vector2Int testCellOffset = new Vector2Int(testCell.xOffset,testCell.yOffset);
                        Vector2Int testCellOffsetUpright = bot.TwistOffsetUpright(testCellOffset);
                        Vector2Int tCoords = bCoordsUpright+testCellOffsetUpright;

                        if (bot.IsValidBrickPos(tCoords)) {
                            GameObject testBrick = bot.brickArr[tCoords.x,tCoords.y];
                            if (testBrick!=null) {
                                if (testBrick.GetComponent<Brick>().brickType == testCell.matchType)
                                    matchCount++;
                            }
                        }
                    }
                    if (matchCount == cellCount)
                        return true;
                }
            }
        }
        return false;
    }

    void ExplodeShape() {
        GameController.shapeScore++;
        for(int x = 0; x < cellCount; x++)
            cellArr[x].GetComponent<Cell>().ExplodeCell();
        StartCoroutine(DestroyAfterSeconds(0.3f));
    }

    IEnumerator DestroyAfterSeconds(float duration){  
        yield return new WaitForSeconds(duration);
        Destroy(gameObject); 
    }

    /* 
    public static int cellTypesNum = 3;
    public static int maxCellRadius = 4;
    public static int maxCellWidth = maxCellRadius *2 +1;
    public static int maxCellHeight = maxCellRadius *2 +1;
    // public int[] shapeColorProbabilityArr = new int[3];
    bool dropFlag = false;
    public float moveDelay = 0.1f;
    public float moveStep = 0.1f;
    Vector3 camOffset;

    Camera mainCamera;
    float startMoveTime = 0.0f;
    
    float coreX;
    float coreY;

    public GameObject[] cellObjectsArr = new GameObject[cellTypesNum];

    Rigidbody2D shapeBody;

    public GameObject[,] cellArr = new GameObject[maxCellWidth,maxCellHeight];


    // Start is called before the first frame update
    void Start()
    {
        cellCount = 0;
        mainCamera = Camera.main;
        //mainCamera = GameObject.Find("NameOfCameraGameObject").GetComponent<Camera>();
        shapeBody = gameObject.GetComponent<Rigidbody2D>();
        shapeBody.bodyType = RigidbodyType2D.Kinematic;
        camOffset = transform.position - mainCamera.transform.position;
        startMoveTime = Time.time;
    }

  // Update is called once per frame
    void Update()
    {
    }

    void LateUpdate()
    {
        if(!dropFlag) {
          transform.position = mainCamera.transform.position + camOffset;
          if (startMoveTime + moveDelay <= Time.time)
            MoveShape();
        }
    }
    
    void MoveShape ()
    {
        if  (Mathf.Abs(camOffset.x)<moveStep) {
            dropFlag = true;
            shapeBody.bodyType = RigidbodyType2D.Dynamic;
        } else {
            if (camOffset.x < 0)
                camOffset = new Vector3(camOffset.x + moveStep,camOffset.y,camOffset.z);
            else    
                camOffset = new Vector3(camOffset.x - moveStep,camOffset.y,camOffset.z);
        }
        startMoveTime = Time.time;
    }

    public GameObject AddRandomCell()
    {
        int c = GameController.ProbabilityPicker(shapeColorProbabilityArr);
        return AddRandomOffsetCell(c);
    }

    public GameObject AddRandomOffsetCell (int color)
    {
        int newXpos;
        int newYpos;
        int cXpos = 0;
        int cYpos = 0;
       
        int[,] directionArr = new int[4,2] {
            {0,1},
            {1,0},
            {0,-1},
            {-1,0}
        };

        for (int x = 0; x < maxCellWidth ; x++) {
            for (int y = 0; y < maxCellHeight ; y++){
                if (cellArr[x,y]!=null) {
                    cXpos = x;
                    cYpos = y;
                }
            }
        }

        while (true) {
            int r = Random.Range(0,4);
            newXpos = cXpos+directionArr[r,0];
            newYpos = cYpos+directionArr[r,1];
            if (IsValidCellPos(newXpos,newYpos)&&(IsEmptyCellPos(newXpos,newYpos)))
                break;
        }
        return AddCell(newXpos,newYpos,color);
    }

    public bool IsValidCellPos(int xArr, int yArr)
    {
        if ((0<=xArr)&&(xArr<maxCellWidth)&&(0<=yArr)&&(yArr<maxCellHeight))
            return true;
        else   
            return false;
    }

    public bool IsEmptyCellPos(int xArr, int yArr)
    {
        if (cellArr[xArr,yArr]==null)
            return true;
        else   
            return false;
    }
/* 
    public GameObject AddSeedCell ()
    {
        int c = GameController.ProbabilityPicker(shapeColorProbabilityArr);
        return AddCell(OffsetToArray(0),OffsetToArray(0),c);
    }

    public GameObject AddCell (int xArr, int yArr, int color)
    {
        GameObject newCell;

        int xOffset = ArrToOffset(xArr);
        int yOffset = ArrToOffset(yArr);

        float coreX = gameObject.transform.position.x;
        float coreY = gameObject.transform.position.y;

        Vector3 offsetV3 = new Vector3(xOffset * ScreenStuff.Instance.colSize, yOffset * ScreenStuff.Instance.colSize, 0);

        newCell = Object.Instantiate(cellObjectsArr[color], new Vector3(coreX,coreY,0),
         Quaternion.identity, gameObject.transform);
        
        newCell.transform.Translate(offsetV3);
        newCell.GetComponent<Cell>().type = color;
        newCell.GetComponent<Cell>().xPos = xArr;
        newCell.GetComponent<Cell>().yPos = yArr;
        cellArr[xArr,yArr] = newCell;
        cellCount++;

        return newCell;
    }
*/


}
