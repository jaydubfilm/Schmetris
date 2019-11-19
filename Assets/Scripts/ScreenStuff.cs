﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenStuff : MonoBehaviour
{
    //public static ScreenStuff Instance { get; private set; }

    public static int rows;
    public static float colSize;
    public static float rowSize;
    public static int screenRadius;
    public static int cols;
    public static int leftEdgeCol;
    public static int rightEdgeCol;
    public static float topEdgeOfWorld;
    public static float leftEdgeOfWorld;
    public static float rightEdgeOfWorld;
    public static float bottomEdgeOfWorld;

    public Sprite bgSprite;

/* 
    public int rows;
    public float colSize;
    public float rowSize = gameSettings.rowSize;
    public int screenRadius = gameSettings.screenRadius;
    public float topEdgeOfWorld = gameSettings.topEdgeOfWorld;
    public int cols = gameSettings.screenRadius*2+1;
    public int leftEdgeCol = -gameSettings.screenRadius;
    public int rightEdgeCol = gameSettings.screenRadius;
    public float leftEdgeOfWorld = leftEdgeCol*gameSettings.colSize;
    public float rightEdgeOfWorld = rightEdgeCol*gameSettings.colSize;
    public float bottomEdgeOfWorld = gameSettings.bottomEdgeOfWorld; 
    
 
*/

    // Start is called before the first frame update

    void Awake()
    {
        /* if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        */
     
    }

    void Start()
    {
        rows = GameController.Instance.settings.rows;
        colSize = GameController.Instance.settings.colSize;
        rowSize = GameController.Instance.settings.rowSize;
        screenRadius = GameController.Instance.settings.screenRadius;
        topEdgeOfWorld = GameController.Instance.settings.topEdgeOfWorld;
        cols = GameController.Instance.settings.screenRadius*2+1;
        leftEdgeCol = -GameController.Instance.settings.screenRadius;
        rightEdgeCol = GameController.Instance.settings.screenRadius;
        leftEdgeOfWorld = leftEdgeCol*GameController.Instance.settings.colSize;
        rightEdgeOfWorld = rightEdgeCol*GameController.Instance.settings.colSize;
        bottomEdgeOfWorld = GameController.Instance.settings.bottomEdgeOfWorld;
    }


    public static float ColToXPosition(int column)
    {
        float cfloat = colSize*(column);
     
        return (cfloat);
    }

    public static float RowToYPosition(int row)
    {
        float rfloat = rowSize*(row);
        return (rfloat);
    }

    public static int MinXoffSet(int column, int coreColumn){
        int offset;
        offset = column - coreColumn;
        if (offset > 20)
            offset -=40;
        return offset;
    }


    public static int WrapCol(int column, int coreCol)
    {
        int newCol = column + coreCol;

        if (newCol > rightEdgeCol)
                newCol -= cols;
        if (newCol < leftEdgeCol)
                newCol += cols;
        return newCol;
    }

    public static int GetRow(GameObject obj) {
      return ScreenStuff.YPositionToRow(obj.transform.position.y);
    }

    public static int GetCol(GameObject obj){
      return ScreenStuff.XPositionToCol(obj.transform.position.x);
    }

    public static Vector2Int GetCoords(GameObject obj)
    {
        return new Vector2Int(GetCol(obj),GetRow(obj));
    }

    public static int XPositionToCol (float xpos)
    {
        return (Mathf.RoundToInt(xpos / colSize));
    }

    public static int YPositionToRow (float ypos)
    {
        return (Mathf.RoundToInt(ypos / rowSize));
    }

    public static Vector2Int TwistOffsetRotated(Vector2Int offset, int rotation) {
        Vector2Int newCoords = new Vector2Int();

        switch (rotation) {
            case 0: 
                newCoords = offset;
                break;
            case 1: 
                newCoords.x = offset.y;
                newCoords.y = -offset.x; 
                break;
            case 2: 
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

    public static Vector2Int TwistOffsetUpright(Vector2Int offset, int rotation){
        Vector2Int newCoords = new Vector2Int();

        switch (rotation) {
            case 0: 
                newCoords = offset;
                break;
            case 1: 
                newCoords.x = -offset.y;
                newCoords.y = offset.x;
                break;
            case 2: 
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

    public static void BounceObject(GameObject obj) {
        if (obj == null)
            return;

        Vector2 force = new Vector2 (Random.Range(-10,10),45);
        Rigidbody2D rb2D = obj.GetComponent<Rigidbody2D>();
        BoxCollider2D box = obj.GetComponent<BoxCollider2D>();
        rb2D.isKinematic = false;
        rb2D.velocity = new Vector2(0,0);

        if (rb2D == null)
            rb2D = obj.AddComponent<Rigidbody2D>();
        if (box != null) {
            box.enabled = false;
            box.isTrigger = false;
        }

        rb2D.AddForce(force,ForceMode2D.Impulse);
        rb2D.AddTorque(Random.Range(-10,10),ForceMode2D.Impulse);
        rb2D.gravityScale=4;
       
        
        obj.tag = "Moveable";
    }
}
