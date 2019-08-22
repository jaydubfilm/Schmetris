using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Trying to push this GitHub

public class Shape : MonoBehaviour
{
    public int cellCount = 0;
    public static int cellTypesNum = 3;
    public static int maxCellRadius = 4;
    public static int maxCellWidth = maxCellRadius *2 +1;
    public static int maxCellHeight = maxCellRadius *2 +1;
    public int[] shapeColorProbabilityArr = new int[3];
    bool dropFlag = false;
    public float moveDelay = 0.1f;
    public float moveStep = 0.1f;
    Vector3 camOffset;

    Camera mainCamera;
    float startMoveTime = 0.0f;
    
    float coreX;
    float coreY;
    int Dummy; //branch test

    public GameObject[] cellObjectsArr = new GameObject[cellTypesNum];

    Rigidbody2D shapeBody;

    /* public List<List<int>> ShapeForms = new List<List<int>>{
        new List<int> {2,2,3,2},
        new List<int> {2,2,2,3},
        new List<int> {1,2,2,2,2,3}
    }*/

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
        if (transform.position.y < -50)
            Destroy(gameObject);
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
/* 
    public GameObject AddRandomCell()
    {
        int c = GameController.ProbabilityPicker(shapeColorProbabilityArr);
        return AddRandomOffsetCell(c);
    }
*/
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
*/
    public GameObject AddCell (int xArr, int yArr, int color)
    {
        GameObject newCell;

        int xOffset = ArrToOffset(xArr);
        int yOffset = ArrToOffset(yArr);

        float coreX = gameObject.transform.position.x;
        float coreY = gameObject.transform.position.y;

        Vector3 offsetV3 = new Vector3(xOffset * ScreenStuff.colSize, yOffset * ScreenStuff.colSize, 0);

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


   public static int OffsetToArray(int offset)
    {
        return (offset + maxCellRadius);
    }

    public static int ArrToOffset(int arr)
    {
        return (arr - maxCellRadius);
    }

}
