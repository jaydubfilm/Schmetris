using UnityEngine;

//Handles conversions between screen and game math
public class ScreenStuff : MonoBehaviour
{
    //Screen and game size values determined by app settings
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

    //Components
    public Sprite bgSprite;

    //Get values from current app settings
    void Start()
    {
        rows = GameController.Instance.settings.rows;
        colSize = GameController.Instance.settings.colSize;
        rowSize = GameController.Instance.settings.rowSize;
        screenRadius = GameController.Instance.settings.screenRadius;
        topEdgeOfWorld = GameController.Instance.settings.topEdgeOfWorld;
        cols = GameController.Instance.settings.screenRadius * 2 + 1;
        leftEdgeCol = -GameController.Instance.settings.screenRadius;
        rightEdgeCol = GameController.Instance.settings.screenRadius;
        leftEdgeOfWorld = leftEdgeCol * GameController.Instance.settings.colSize;
        rightEdgeOfWorld = rightEdgeCol * GameController.Instance.settings.colSize;
        bottomEdgeOfWorld = GameController.Instance.settings.bottomEdgeOfWorld;
    }

    //Determine world x position in units using position on game grid
    public static float ColToXPosition(int column)
    {
        float cfloat = colSize * (column);

        return (cfloat);
    }

    //Determine world y position in units using position on game grid
    public static float RowToYPosition(int row)
    {
        float rfloat = rowSize * (row);
        return (rfloat);
    }

    //Determine a position on the game grid from its x position in the world
    public static int XPositionToCol(float xpos)
    {
        return (Mathf.RoundToInt(xpos / colSize));
    }

    //Determine a position on the game grid from its y position in the world
    public static int YPositionToRow(float ypos)
    {
        return (Mathf.RoundToInt(ypos / rowSize));
    }

    //Find a Transform's column in the game grid from its x position in the world
    public static int GetCol(GameObject obj)
    {
        return ScreenStuff.XPositionToCol(obj.transform.position.x);
    }

    //Find a Transform's row in the game grid from its y position in the world
    public static int GetRow(GameObject obj)
    {
        return ScreenStuff.YPositionToRow(obj.transform.position.y);
    }

    //Return a Transform's coordinates on the game grid using its position in the world
    public static Vector2Int GetOffset(GameObject obj)
    {
        return new Vector2Int(GetCol(obj), GetRow(obj));
    }

    //Objects that move too far left or right on the game grid will wrap around to the other side
    public static int WrapCol(int column, int coreCol)
    {
        int newCol = column + coreCol;

        if (newCol > rightEdgeCol)
            newCol -= cols;
        if (newCol < leftEdgeCol)
            newCol += cols;
        return newCol;
    }

    //Determine a position on the bot using its position on the game grid
    public static Vector2Int ScreenToBotOffset(Vector2Int offset, int rotation)
    {
        Vector2Int newCoords = new Vector2Int();

        switch (rotation)
        {
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

    //Determine a position on the game grid using its position on the Bot
    public static Vector2Int BotToScreenOffset(Vector2Int offset, int rotation)
    {
        Vector2Int newCoords = new Vector2Int();

        switch (rotation)
        {
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
}