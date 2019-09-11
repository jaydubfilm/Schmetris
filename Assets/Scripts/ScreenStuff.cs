using System.Collections;
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

    public static int WrapCol(int column)
    {
        Bot bot = (Bot)FindObjectOfType(typeof(Bot));

        if (bot==null)
            return column;
       
        int newCol = column + bot.coreCol;

        if (newCol > rightEdgeCol)
                newCol -= cols;
        if (newCol < leftEdgeCol)
                newCol += cols;
        return newCol;
    }

    public static float RowToYPosition(int row)
    {
        float rfloat = 1.4f*(row - rows / 2.0f);
        return (rfloat);
    }

    public static int XPositionToCol (float xpos)
    {
        return (Mathf.RoundToInt(xpos / colSize));
    }

    public static int YPositionToRow (float ypos)
    {
        return (Mathf.RoundToInt(ypos / rowSize));
    }

   
}
