using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerGrid 
{
    public int[,] grid;
    Bot bot;
    public int width;

    // Start is called before the first frame update
    void Start()
    {
    
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public PowerGrid(Bot parentBot)
    {
            bot = parentBot;
            width = bot.maxBotWidth;
            grid = new int[width,width];
            Refresh();
    }

    public void Refresh() {
        for (int x = 0;x<width;x++)
            for (int y = 0;y<width;y++)
                grid[x,y] = 0;
        
        // update Power Grid levels

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

        for (int x = 0; x<count ;x++) {
            GameObject brickObj = bot.brickList[x];
            Parasite parasite = brickObj.GetComponent<Parasite>();
            if (parasite==null) {
                Brick brick = brickObj.GetComponent<Brick>();
                if (brick.brickType!=0) {
                    if (PowerAtBotCoords(brick.arrPos)==0) {
                        brick.MakeOrphan();
                        count--;
                    }
                }
            }
        }
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
