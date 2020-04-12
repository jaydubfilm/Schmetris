using System.Collections;
using UnityEngine;

//Tracks the power level of each brick on the bot
public class PowerGrid : MonoBehaviour
{
    //Components
    Bot bot;
    public GameObject gridSymbol;

    //Bot grid
    public int[,] grid;
    public int width;

    //Init
    void Awake()
    {
        bot = transform.parent.gameObject.GetComponent<Bot>();
        width = bot.maxBotWidth;
        grid = new int[width, width];
        InvokeRepeating("Refresh", 1.0f, 0.2f);
    }

    //Check for changes in power levels and update connected bricks if needed
    public void Refresh()
    {

        if (!bot.powerGridRefreshFlag || bot.tripleCheckFlag || bot.tripleWaitFlag)
            return;

        for (int x = 0; x < width; x++)
            for (int y = 0; y < width; y++)
                grid[x, y] = 0;

        // update Power Grid levels
        if (bot.brickList.Count == 0)
            return;

        foreach (GameObject brickObj in bot.brickList)
        {
            Brick brick = brickObj.GetComponent<Brick>();
            PowerSource brickPower = brick.GetComponent<PowerSource>();
            if (brickPower)
            {
                int r = brick.hasResources ? brickPower.powerAtLevel[brick.brickLevel] : brickPower.powerAtLevel[0];
                Vector2Int sourcePos = brick.arrPos;
                for (int x = -r; x <= r; x++)
                {
                    for (int y = -r; y <= r; y++)
                    {
                        if (IsValidGridPos(new Vector2Int(sourcePos.x + x, sourcePos.y + y)))
                        {
                            int increase = r - Mathf.Max(Mathf.Abs(x), Mathf.Abs(y)) + 1;
                            grid[sourcePos.x + x, sourcePos.y + y] += increase;
                        }
                    }
                }
            }
        }

        // make bricks with no power orphans

        int count = bot.brickList.Count;
        bool zoneReminder = false;

        for (int x = 0; x < count; x++)
        {
            GameObject brickObj = bot.brickList[x];
            if (!brickObj.GetComponent<Brick>().IsParasite())
            {
                Brick brick = brickObj.GetComponent<Brick>();
                if (!brick.GetComponent<PowerSource>())
                {
                    if (PowerAtBotCoords(brick.arrPos) == 0)
                    {
                        brick.MakeOrphan();
                        zoneReminder = true;
                        count--;
                    }
                    else
                    {
                        brick.isPowered = PowerAtBotCoords(brick.arrPos) >= brick.requiredPower[brick.brickLevel];
                        if (!brick.isPowered)
                        {
                            GameController.Instance.hud.SetUnpoweredPopup(true);
                        }
                    }
                }
                else
                {
                    brick.isPowered = true;
                }
            }
        }

        if (zoneReminder)
        {
            GameController.Instance.hud.SetNoPowerPopup(true);
            FlashGridCells();
        }
    }

    //Reminder effect showing player where the power grid ends
    public void FlashGridCells()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < width; y++)
            {
                if (grid[x, y] > 0)
                {
                    Vector2Int botPos = new Vector2Int(x, y);
                    Vector3 symbolPos = bot.BotCoordsToScreenPos(botPos);
                    GameObject newSymbol = Instantiate(gridSymbol, symbolPos, Quaternion.identity);
                    StartCoroutine(FlashGridCell(botPos, newSymbol));
                }
            }
        }
    }

    //Animate power icons flashing
    IEnumerator FlashGridCell(Vector2Int botPos, GameObject symbol)
    {
        SpriteRenderer gridCellSprite = symbol.GetComponent<SpriteRenderer>();

        Color tmpColor = gridCellSprite.color;
        float fadeTime = 1.0f;
        tmpColor.a = 0.5f;
        while (tmpColor.a > 0f)
        {
            tmpColor.a -= Time.deltaTime / fadeTime;
            gridCellSprite.color = tmpColor;
            if (tmpColor.a <= 0)
                tmpColor.a = 0;
            yield return null;
            gridCellSprite.color = tmpColor;
        }
        Destroy(symbol);
    }

    //Is this coordinate within the grid bounds?
    public bool IsValidGridPos(Vector2Int gridArrPos)
    {
        if (((gridArrPos.x >= 0) && (gridArrPos.x < width)) && ((gridArrPos.y >= 0) && (gridArrPos.y < width)))
            return true;
        else
            return false;
    }

    //How much power is available at these coordinates?
    public int PowerAtBotCoords(Vector2Int arrPos)
    {
        return grid[arrPos.x, arrPos.y];
    }
}
