using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brick : MonoBehaviour
{
    public Vector2Int arrPos;
  
    public int brickType;
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
    }

    void Update () {
        //if (transform.position.y < ScreenStuff.bottomEdgeOfWorld)
           // Destroy(gameObject);
        if (brickHP <= 0)
            ExplodeBrick();
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        GameObject bitObj= collider.gameObject;
        Bit bit = bitObj.GetComponent<Bit>();
     
        if (transform.parent == null)
            return;


        if (bit!=null) {
            int bitType = bit.bitType;
            int brickType = gameObject.GetComponent<Brick>().brickType;
            float rA = parentBot.transform.rotation.eulerAngles.z;
            bool bounceBitFlag = true;

            if (bitType == 0) // black bit - hurt the brick
            {
                brickHP-=10;
                bounceBitFlag = false;
                bot.GetComponent<Overheat>().AddHeat();
            } 
            else if (transform.parent != null)
            {
                int rotation = bot.botRotation;
                Vector2Int eArrPos = arrPos;
                
                if (rotation == 1) { // up facing
                    eArrPos.y +=1;
                } else if (rotation == 2) { // right facing
                    eArrPos.x -=1;
                }  else if (rotation == 3) { // down facing
                    eArrPos.y -=1;
                } else { // left facing
                    eArrPos.x +=1;
                }
                if (((rA == 0) || (rA == 90) || (rA == 180) || (rA == 270)) /* && (BitIsAboveBrick(collider))*/) {
                    if (bitType == 1) // white bit - bump the brick
                    {     
                        bot.BumpColumn(arrPos);
                    } else {   // test for collapsable double below collision
                        
                        Vector2Int lowerPos = arrPos;

                        int newBrickType = bitType - 2;
                        GameObject lowerBrick;

                        switch (rotation) {
                            case 1:  // up facing
                                lowerPos.y -=1;
                                break;
                            case 2:  // right facing
                                lowerPos.x +=1;
                                break;
                            case 3: // down facing
                                lowerPos.y +=1;
                                break;
                            default: // left facing
                                lowerPos.x -=1;
                                break;
                        }

                        if (bot.IsValidBrickPos(lowerPos)) {
                            lowerBrick = bot.brickArr[lowerPos.x,lowerPos.y];
                            if (lowerBrick!=null){
                                if ((((bot.IsValidBrickPos(eArrPos)==false)&&(brickType == bitType-2))&&
                                (lowerBrick.GetComponent<Brick>().brickType == bitType - 2)) &&
                                    ((brickLevel == 0) &&
                                        (lowerBrick.GetComponent<Brick>().brickLevel == 0))) {
                                    bot.CollapseDouble(arrPos,lowerPos);
                                    bounceBitFlag = false;
                                }
                            }
                        }

                        // add a new brick

                        if ((bot.IsValidBrickPos(eArrPos)) &&
                                (bot.brickArr[eArrPos.x,eArrPos.y]==null)) {
                            bot.AddBrick(eArrPos, newBrickType);
                            bounceBitFlag = false;
                            source.PlayOneShot(addBrickSound,1.0f);
                        }
                      
                    }
                }
            }
            if (bounceBitFlag == false) {
                 Destroy(collider.gameObject);
            } else { // bounce the bit away
                bit.RemoveFromBlock("bounce");
            }
        }
    }

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
            GameController.lives = 0;

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
            GameController.lives = 0;

        if (brickType ==1) {
            gameObject.GetComponent<Fuel>().Deactivate();
        }
        Destroy(gameObject);
    }

    public void MakeOrphan() {
        RemoveBrickFromBotArray();  
        GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        transform.parent = null;
        tag = "Moveable";
    }


    public void RemoveBrickFromBotArray() {
        bot.brickArr[arrPos.x,arrPos.y] = null;
        bot.brickTypeArr[arrPos.x,arrPos.y]=-1;
        bot.RefreshNeighborLists();
        Bot.orphanCheckFlag = true;
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
        Vector2Int newOffset = bot.TwistOffsetRotated(bot.ArrToOffset(newArrPos));
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
            GetComponent<SpriteRenderer>().sprite = spriteArr[brickLevel];
            if (brickType == 1)
                gameObject.GetComponent<Fuel>().UpgradeFuelLevel();
        }
    }

}
