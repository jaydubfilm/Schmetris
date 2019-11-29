using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    public List<GameObject> bitList;
    public GameObject[,] bitArr;
    
    
    public int column;
    public Bot bot;
    public float blockSpeed;
    public int blockRadius; 
    public int blockWidth;
    public int blockRotation;
    public Vector2Int coreV2;
    public Rigidbody2D rb;
    Vector3 moveToPos;
    Vector3 stepDownV3;
    
  
    // Start is called before the first frame update
    void Start()
    {
      blockRadius = GameController.Instance.settings.blockRadius;
      int absoluteCol =  ScreenStuff.GetCol(gameObject);
      float step = blockSpeed*Time.deltaTime;
      column = ScreenStuff.WrapCol(absoluteCol,bot.coreCol);
      blockWidth = blockRadius*2+1;
      bitArr = new GameObject[blockWidth,blockWidth];
      coreV2 = new Vector2Int(blockRadius,blockRadius);
      rb =  gameObject.GetComponent<Rigidbody2D>();
      rb.velocity = new Vector3(0,-blockSpeed,0);
      //stepDownV3 = new Vector3 (0,-ScreenStuff.colSize,0);
      //moveToPos = transform.position + stepDownV3;
      //transform.position = Vector3.MoveTowards(transform.position,moveToPos,step);
    }


    // Update is called once per frame
    void Update()
    {
      /*
      if (transform.position.y<=moveToPos.y) {
          float step = blockSpeed*Time.deltaTime;
          // CollisionCheck();
          moveToPos += stepDownV3;
          transform.position = Vector3.MoveTowards(transform.position,moveToPos,step);
      }
      */
    }

    void StepDown() {
        Vector3 stepVector = new Vector3(0,-ScreenStuff.rowSize,0);
        transform.position += stepVector;
    }

    public int GetXOffset(int coreColumn) {
      int offset = column - coreColumn;
      if (offset > 20)
          offset -=40;
      return offset;
    }
    
    public void RotateBitsUpright() {
      foreach (GameObject bit in bitList) {
         bit.GetComponent<Bit>().RotateUpright();  
      }
    }


    public bool IsValidBitPos(Vector2Int arrPos)
    {
        if ((0<=arrPos.x)&&(arrPos.x<blockWidth)&&(0<=arrPos.y)&&(arrPos.y<blockWidth))
            return true;
        else   
            return false;
    }
/*
    public void CollisionCheck(int directionFlag){   
        // check for left-right bit-brick collisions 

        Bounds collisionBubble = botBounds;
        collisionBubble.Expand(2*settings.colSize);
        float xOffset = ScreenStuff.colSize*directionFlag;
        
        // get list of Bricks
        Collider2D[] possibleColliders = Physics2D.OverlapBoxAll(collisionBubble.center,collisionBubble.size,0);
            
        for (int x = 0; x < possibleColliders.Length; x++) {
            if (possibleColliders[x].GetComponent<Brick>()==null) {
                Vector2 v = possibleColliders[x].transform.position;
                v.x +=xOffset;
                Vector2Int offset = ScreenStuff.BotToScreenOffset(ScreenPosToOffset(v),botRotation);
                Vector2Int arrPos = OffsetToArray(offset);
                if (IsValidBrickPos(arrPos))  {
                    if (brickArr[arrPos.x,arrPos.y]!=null) {
                        Brick colliderBrick = brickArr[arrPos.x,arrPos.y].GetComponent<Brick>();
                        colliderBrick.BitBrickCollide(possibleColliders[x]);
                    }
                }
            }
        }
    }
    */

    public bool IsBotBelow(){
        bool botBelow = false;
        int row = ScreenStuff.GetRow(gameObject);

        Vector2Int blockOffset = new Vector2Int (column-bot.coreCol,row);
        
        foreach(GameObject bit in bitList) {
            Vector2Int bitOffset = bit.GetComponent<Bit>().blockOffset;
            Vector2Int testPos = bot.coreV2 + blockOffset + bitOffset + Vector2Int.down;
            Vector2Int rotatedTestPos = bot.MapToScreenCoords(testPos,9);//WRONG

            if (bot.IsValidBrickPos(rotatedTestPos)==true)
              if (bot.brickTypeArr[rotatedTestPos.x,rotatedTestPos.y]>=0)
                botBelow = true;
        }
        return botBelow;
    }

    public void DestroyBlock(){
      GameController.Instance.blockList.Remove(gameObject);
      Destroy(gameObject);
    }

    public void BounceBlock() {
        Vector2 force = new Vector2 (Random.Range(-10,10),5);

        GameController.Instance.blockList.Remove(gameObject);
        foreach(GameObject bitObj in bitList) 
            bitObj.GetComponent<BoxCollider2D>().enabled = false;
        
        rb.isKinematic = false;
        rb.AddForce(force,ForceMode2D.Impulse);
        rb.AddTorque(Random.Range(-1,1),ForceMode2D.Impulse);
        rb.gravityScale=4;
        
        gameObject.tag = "Moveable";
    }
}
