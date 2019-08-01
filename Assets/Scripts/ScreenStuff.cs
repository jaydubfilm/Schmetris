using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenStuff : MonoBehaviour
{
    public static float colSize = 1.4f;
    public static float rowSize = 1.4f;
    public static int cols = 40;
    public static int rows = 20;
    public static float topEdgeOfWorld = 40;
    public static float leftEdgeOfWorld = -40f;
    public static float rightEdgeOfWorld = 40f;
    public static float bottomEdgeOfWorld = -50f;

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
        float cfloat = colSize*(column - cols/2.0f);
        return (cfloat);
    }

    public static float RowToYPosition(int row)
    {
        float rfloat = 1.4f*(row - rows / 2.0f);
        return (rfloat);
    }

    public static int XPositionToCol (float xpos)
    {
        return (Mathf.RoundToInt((xpos / colSize)+(cols/2)));
    }

    public static int YPositionToRow (float ypos)
    {
        return (Mathf.RoundToInt(ypos / rowSize));
    }
}
