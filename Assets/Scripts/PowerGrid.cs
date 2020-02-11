using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerGrid : MonoBehaviour
{
    public int[,] grid;
    Bot bot;
    public int width;
    public GameObject gridSymbol;

    //float timer;
    
    // Start is called before the first frame update

    void Awake()
    {
        bot = transform.parent.gameObject.GetComponent<Bot>();
        width = bot.maxBotWidth;
        grid = new int[width,width];
        //timer = Time.time;
        InvokeRepeating("Refresh",1.0f,0.2f);
    }

    // Update is called once per frame
    void Update()
    {
      
    }

    public void Refresh() {

        if (!bot.powerGridRefreshFlag || bot.tripleCheckFlag || bot.tripleWaitFlag)
            return;

        for (int x = 0;x<width;x++)
            for (int y = 0;y<width;y++)
                grid[x,y] = 0;
        
        // update Power Grid levels
        if (bot.brickList.Count==0)
            return;

        foreach (GameObject brickObj in bot.brickList){
            Brick brick = brickObj.GetComponent<Brick>();
            if (brick.brickType == 0) {
                int r = brick.brickLevel+1;
                Vector2Int sourcePos = brick.arrPos;
                for (int x = -r;x<=r;x++) {
                    for (int y = -r; y<=r; y++) {
                        if (IsValidGridPos(new Vector2Int(sourcePos.x+x,sourcePos.y+y))){
                            int increase = r-Mathf.Max(Mathf.Abs(x),Mathf.Abs(y))+1;
                            grid[sourcePos.x+x,sourcePos.y+y]+=increase;
                        }
                    }
                }
            }
        }   

        // make bricks with no power orphans

        int count = bot.brickList.Count;
        bool zoneReminder = false;

        for (int x = 0; x<count ;x++) {
            GameObject brickObj = bot.brickList[x];
            Parasite parasite = brickObj.GetComponent<Parasite>();
            if (parasite==null) {
                Brick brick = brickObj.GetComponent<Brick>();
                if (brick.brickType!=0) {
                    if (PowerAtBotCoords(brick.arrPos)==0) {
                        brick.MakeOrphan();
                        zoneReminder = true;
                        count--;
                    }
                    else
                    {
                        brick.isPowered = PowerAtBotCoords(brick.arrPos) > brick.brickLevel;
                    }
                }
            }
        }

        if (zoneReminder) {
            FlashGridCells();
        }
    }

    public void FlashGridCells(){
        for (int x = 0;x<width;x++) {
            for (int y = 0;y<width;y++) {
                if (grid[x,y]>0) {
                    Vector2Int botPos = new Vector2Int(x,y);
                    Vector3 symbolPos = bot.BotCoordsToScreenPos(botPos);
                    GameObject newSymbol = Instantiate (gridSymbol,symbolPos,Quaternion.identity);
                    StartCoroutine(FlashGridCell(botPos,newSymbol));
                }
            }
        }
    }



    IEnumerator FlashGridCell(Vector2Int botPos, GameObject symbol){
        
        SpriteRenderer gridCellSprite = symbol.GetComponent<SpriteRenderer>();

        Color tmpColor = gridCellSprite.color;
        float fadeTime = 1.0f;
        tmpColor.a = 0.5f;
        while (tmpColor.a > 0f) {
            tmpColor.a -= Time.deltaTime / fadeTime;
            gridCellSprite.color = tmpColor;
            if (tmpColor.a <=0)
                tmpColor.a = 0;
            yield return null;
            gridCellSprite.color = tmpColor;
        }
        Destroy(symbol);
    }

    IEnumerator WaitFlashNoPower(GameObject brickObj) {
        yield return new WaitForSeconds(0.1f);
        GameObject pWarning = Instantiate(bot.powerWarning,brickObj.transform);
    }


    public bool IsValidGridPos(Vector2Int gridArrPos) {
        if (((gridArrPos.x>=0)&&(gridArrPos.x<width))&&((gridArrPos.y>=0)&&(gridArrPos.y<width)))
            return true;
        else
            return false;
    }

    public int PowerAtBotCoords(Vector2Int arrPos){
        return grid[arrPos.x,arrPos.y];
    }

    public void WarningFlash(){


    }

}
