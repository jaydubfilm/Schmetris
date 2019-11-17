using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brick : MonoBehaviour
{
    public Vector2Int arrPos;
  
    public int brickType;
    public int ID;
    public int brickLevel;
    public int brickHP;
    public int brickMaxHP;
    public static int brickMaxLevel = 5;
    public static float brickMoveSpeed = 20f;

    public AudioClip addBrickSound;
    private AudioSource source;

    public Sprite[] spriteArr;
    
    public GameObject parentBot;
    Bot bot;
    Rigidbody2D rb2D;

    public List<GameObject> neighborList = new List<GameObject>();
    
    void Awake () {
       brickLevel = 0;
       source = GetComponent<AudioSource>();
       rb2D = gameObject.GetComponent<Rigidbody2D>();
    }

    void Start () {
        bot = parentBot.GetComponent<Bot>();
        bot.brickList.Add(gameObject);
    }

    void Update () {
        //if (transform.position.y < ScreenStuff.bottomEdgeOfWorld)
           // Destroy(gameObject);
        if (brickHP <= 0)
            ExplodeBrick();
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        GameObject bitObj = collider.gameObject;
        Transform t = bitObj.transform.parent;
        if (t==null)
            return;
        
        GameObject blockObj = t.gameObject;
        Block block = blockObj.GetComponent<Block>();
        Bit bit = bitObj.GetComponent<Bit>();
        int bitType = bit.bitType;
        float rA = parentBot.transform.rotation.eulerAngles.z;

        if (blockObj == null)
            return;

        if (bit==null)
            return;

        if (bitType == 0) // black bit - hurt the brick
        {
            brickHP-=10;
            bot.GetComponent<Overheat>().AddHeat();
            bit.RemoveFromBlock("Destroy");
        } 
      
        else
        {
            if (!((rA == 0) || (rA == 90) || (rA == 180) || (rA == 270))) 
                block.BounceBlock();
            else {
                Vector2Int bitCoords = ScreenStuff.GetCoords(bitObj);
                Vector2Int brickCoords = ScreenStuff.GetCoords(gameObject);
                Vector2Int hitDirV2 = brickCoords-bitCoords;

                if (hitDirV2 == new Vector2Int(0,0))
                    block.BounceBlock();

                if (bitType == 1) // white bit - bump the brick
                {     
                    bot.BumpColumn(arrPos);
                    block.BounceBlock();
                } else {   
                    // add the block
                    bot.ResolveCollision(blockObj,hitDirV2);
                    //bot.AddBlock(arrPos,hitDirV2,bitObj);
                }
            } 
        }
    }

/*
    public bool CheckForCollapsableDouble(Bit bit, Vector2Int hitDirV2){ 
        Vector2Int lowerPos = arrPos+=bot.downOffsetV2Arr[bot.botRotation];

        int newBrickType = bit.ConvertToBrickType();

        GameObject lowerBrick;
        bool isCollapsible = false;

        if (bot.IsValidBrickPos(lowerPos)) {
            lowerBrick = bot.brickArr[lowerPos.x,lowerPos.y];
            if (lowerBrick!=null){
                if ((((bot.IsValidBrickPos(eArrPos)==false)&&(brickType == bitType-2))&&
                (lowerBrick.GetComponent<Brick>().brickType == bitType - 2)) &&
                    ((brickLevel == 0) &&
                        (lowerBrick.GetComponent<Brick>().brickLevel == 0))) {
                    isCollapsible = true;
                }
            }
        }
        return isCollapsible;
    }   
*/

    public void RotateUpright(){
            transform.rotation = Quaternion.identity;
    }

    public bool BitIsAboveBrick(Collision2D col)
    {
        if (col.transform.position.y >= (gameObject.transform.position.y + ScreenStuff.rowSize - 0.8))
            return true;
        else    
            return false;
    }

    public void ExplodeBrick() {
        Animator anim;
        float animDuration;

        if (brickType == 0)
            GameController.Instance.lives = 0;

        if (brickType == 1)
        {
            gameObject.GetComponent<Fuel>().Deactivate();
            foreach(GameObject neighbor in this.neighborList)
                if (neighbor.GetComponent<Brick>().brickType == 1)
                    neighbor.GetComponent<Brick>().brickHP-=10;
        } 

        anim = gameObject.GetComponent<Animator>();
        anim.enabled = true;
        animDuration = 0.3f;

        RemoveBrickFromBotArray();
        StartCoroutine(DestroyAfterAnimation(animDuration));
    }

    IEnumerator DestroyAfterAnimation(float duration){  
        yield return new WaitForSeconds(duration);
        Destroy(gameObject); 
    }

    public void DestroyBrick() {
        RemoveBrickFromBotArray();  

        if (brickType == 0)
            GameController.Instance.lives = 0;

        /*if (brickType ==1) {
            gameObject.GetComponent<Fuel>().Deactivate();
        }*/
        
        Destroy(gameObject);
    }

    public void MakeOrphan() {
        RemoveBrickFromBotArray();  
        GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        transform.parent = null;
        tag = "Moveable";
    }

    public void RemoveBrickFromBotArray() {
        bot = parentBot.GetComponent<Bot>();
        bot.brickArr[arrPos.x,arrPos.y] = null;
        bot.brickTypeArr[arrPos.x,arrPos.y]=-1;
        bot.brickList.Remove(gameObject);
        bot.RefreshNeighborLists();
        bot.orphanCheckFlag = true;
    }

    public void MoveBrick(Vector2Int newArrPos) {

        // move this brick to newArrPos.x, newArrPos.y

        if ((bot.IsValidBrickPos(newArrPos) &&
            (bot.brickArr[newArrPos.x,newArrPos.y] == null))) 
        {
            // update the array

            bot.brickArr[newArrPos.x,newArrPos.y] = gameObject;
            bot.brickTypeArr[newArrPos.x,newArrPos.y] = brickType;
            bot.brickArr[arrPos.x,arrPos.y] = null;
            bot.brickTypeArr[arrPos.x,arrPos.y] = -1;

            // move the gameObject

            SmoothMoveBrickObj(newArrPos);
  
            arrPos.x = newArrPos.x;
            arrPos.y = newArrPos.y;

            // update neighbor lists

            bot.RefreshNeighborLists();
        }  
    }

    public void SmoothMoveBrickObj(Vector2Int newArrPos){
        Vector2Int newOffset = ScreenStuff.TwistOffsetRotated(bot.ArrToOffset(newArrPos),bot.botRotation);
        Vector3 newOffsetV3 = new Vector3(newOffset.x*ScreenStuff.colSize,newOffset.y*ScreenStuff.colSize,0);

        StartCoroutine(SlideBrickOverTime(rb2D.transform.position,newOffsetV3));
    }


    IEnumerator SlideBrickOverTime (Vector3 originalPos, Vector3 newPos) {
        float t = 0f;
        float duration = (newPos-originalPos).magnitude/brickMoveSpeed;

        while (t< duration)
        {
            rb2D.transform.position = Vector3.Lerp(originalPos,newPos,t/duration);
            yield return null;
            t+=Time.deltaTime;
        }
        rb2D.transform.position = newPos;
    }


    public bool IsCore() {
        if (arrPos == bot.coreV2)
            return true;
        else    
            return false;
    }

    public void SetLevel(int level) {
        brickLevel = level;
        if (level<spriteArr.Length) 
            this.GetComponent<SpriteRenderer>().sprite = spriteArr[brickLevel];
    }

    public void UpgradeBrick() 
    {
        if (brickLevel<spriteArr.Length-1) {
            brickLevel++;
            ID++;
            GetComponent<SpriteRenderer>().sprite = spriteArr[brickLevel];
            if (brickType == 1)
                gameObject.GetComponent<Fuel>().UpgradeFuelLevel();
        }
    }

    public int ConvertToBitType(){
        return brickType + 2;
    }

    public bool CompareToBit(Bit bit) {
        int compType = Mathf.RoundToInt((ID-bit.bitLevel)/1000) - 2;

        return((compType == brickType)&&(bit.bitLevel==brickLevel));
    }

}
