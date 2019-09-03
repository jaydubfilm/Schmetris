using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenStuff : MonoBehaviour
{
    public static float colSize = 1.4f;
    public static float rowSize = 1.4f;
    public static int screenRadius = 20;
    public static int cols = 2*screenRadius+1;
    public static int leftEdgeCol = -screenRadius;
    public static int rightEdgeCol = screenRadius;
    public static int rows = 20;
    public static float topEdgeOfWorld = 40;
    public static float leftEdgeOfWorld = leftEdgeCol*colSize;
    public static float rightEdgeOfWorld = rightEdgeCol*colSize;
    public static float bottomEdgeOfWorld = -20f;

    // Start is called before the first frame update

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public static float ColToXPosition(int column)
    {
        float cfloat = colSize*(column);
     
        return (cfloat);
    }

    public static int WrapCol(int column)
    {
        Bot bot = (Bot)FindObjectOfType(typeof(Bot));
       
        int newCol = column + bot.coreCol;

        if (newCol > ScreenStuff.rightEdgeCol)
                newCol -= cols;
        if (newCol < ScreenStuff.leftEdgeCol)
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
