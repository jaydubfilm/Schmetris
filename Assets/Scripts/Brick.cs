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
    public int[] brickMaxHP;
    public static int brickMaxLevel = 5;
    public static float brickMoveSpeed = 20f;

    public AudioClip addBrickSound;
    private AudioSource source;
  
    public Sprite[] spriteArr;
    
    public GameObject parentBot;
    Bot bot;
    Rigidbody2D rb2D;

    public List<GameObject> neighborList = new List<GameObject>();
    public HealthBar healthBar;
    
    void Awake () {
        brickLevel = 0;
        brickHP = brickMaxHP[brickLevel];
        source = GetComponent<AudioSource>();
        rb2D = gameObject.GetComponent<Rigidbody2D>();
        healthBar = GetComponentInChildren<HealthBar>();
        healthBar.gameObject.SetActive(false);
    }

    void Start () {
        bot = parentBot.GetComponent<Bot>();
        //bot.brickList.Add(gameObject);
        bot.RefreshBotBounds();
        FixedJoint2D fj = gameObject.GetComponent<FixedJoint2D>();
        fj.connectedBody = parentBot.GetComponent<Rigidbody2D>();
    }

    void Update () {
      
    }

    public bool IsParasite(){
        if (GetComponent<Parasite>()==null)
            return false;
        else
            return true;
    }

    public void AdjustHP(int damage){
        brickHP+=damage;
        if (brickHP<=0)
            DestroyBrick();
        else {
            if (brickHP>=brickMaxHP[brickLevel]) {
                brickHP = brickMaxHP[brickLevel];
                healthBar.gameObject.SetActive(false);
            } else if (healthBar.gameObject.activeInHierarchy==false) {
                healthBar.gameObject.SetActive(true);
                float normalizedHealth = (float)brickHP/(float)brickMaxHP[brickLevel];
                healthBar.SetSize(normalizedHealth);
            } else {
                float normalizedHealth = (float)brickHP/(float)brickMaxHP[brickLevel];
                healthBar.SetSize(normalizedHealth);
            }
        }
    }

/*
    void OnTriggerEnter2D(Collider2D collider)
    {
        BitBrickCollide(collider);
    }
    */

    public void BitBrickCollide(GameObject bitObj) {
        Transform t = bitObj.transform.parent;
        if (t==null)
            return;
        
        GameObject blockObj = t.gameObject;
        Block block = blockObj.GetComponent<Block>();
        Bit bit = bitObj.GetComponent<Bit>();
        if (bit== null)
            return;

        int bitType = bit.bitType;
        float rA = parentBot.transform.rotation.eulerAngles.z;

        if (blockObj == null)
            return;

        if (bit==null)
            return;

        if (bitType == 0) // black bit - hurt the brick
        {
            AdjustHP(-1000);
            bot.GetComponent<Overheat>().AddHeat();
            bit.RemoveFromBlock("Destroy");
        } 
        else
        {
            if (!((rA == 0) || (rA == 90) || (rA == 180) || (rA == 270))) 
                block.BounceBlock();
            else {
                Vector2Int bitCoords = ScreenStuff.GetOffset(bitObj);
                Vector2Int brickCoords = ScreenStuff.GetOffset(gameObject);
                Vector2Int hitDirV2 = brickCoords-bitCoords;

                if (hitDirV2 == new Vector2Int(0,0))
                    block.BounceBlock();

                if (bitType == 1) // white bit - bump the brick
                {     
                    bot.BumpColumn(arrPos,hitDirV2);
                    block.BounceBlock();
                } else {   
                    bot.ResolveCollision(blockObj,hitDirV2);
                }
            } 
        }
    }

    public Vector2Int ScreenArrPos(){
        return bot.BotToScreenCoords(arrPos);
    }


    public void RotateUpright(){
       // Destroy(gameObject.GetComponent<FixedJoint2D>());
        transform.rotation = Quaternion.identity;
      //  gameObject.AddComponent<FixedJoint2D>();
      //  gameObject.GetComponent<FixedJoint2D>().connectedBody = bot.GetComponent<Rigidbody2D>();
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
                    neighbor.GetComponent<Brick>().AdjustHP(-10);
        } 

        if (brickType == 6) {
            Bomb bomb = GetComponent<Bomb>();
            int damage = bomb.damage[brickLevel];
            // StartCoroutine(WaitAndBombEnemies(damage));
            BombEnemies(damage);
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

    IEnumerator WaitAndBombEnemies(int damage){
        yield return new WaitForSeconds(0.5f);
        BombEnemies(damage);
    }

    public void BombEnemies(int damage){
        int c = GameController.Instance.enemyList.Count;
        GameObject[] enemyArr = new GameObject[c];

        for (int x = 0;x < c; x++)
            enemyArr[x] = GameController.Instance.enemyList[x];
        
        for (int x = 0;x < c; x++) {
            Brick brick = enemyArr[x].GetComponent<Brick>();
            if (brick!=null)
                brick.AdjustHP(-damage);    
            else {
                Enemy enemy = enemyArr[x].GetComponent<Enemy>();
                enemy.hP -=damage;
            }
        }
    }

    public void DestroyBrick() {

        if (brickType ==1) {
            gameObject.GetComponent<Fuel>().Deactivate();
        }

        if (brickType == 6) {
            Bomb bomb = GetComponent<Bomb>();
            int damage = bomb.damage[brickLevel];
            // StartCoroutine(WaitAndBombEnemies(damage));
            BombEnemies(damage);
        }

        RemoveBrickFromBotArray();  
        if (IsParasite())
            GameController.Instance.enemyList.Remove(gameObject);
        Destroy(gameObject);
        bot.RefreshBotBounds();
    }

    public void MakeOrphan() {
        RemoveBrickFromBotArray();
        rb2D.isKinematic = false;
    
        transform.parent = null;
        tag = "Moveable";
       // GameController.Instance.blockList.Remove(gameObject);
        GetComponent<BoxCollider2D>().enabled = false;
        rb2D.gravityScale=4;
    }

    public void RemoveBrickFromBotArray() {
        bot = parentBot.GetComponent<Bot>();
        bot.SetBrickAtBotArr(arrPos,null);
        bot.brickTypeArr[arrPos.x,arrPos.y]=-1;
        bot.brickList.Remove(gameObject);
        if (IsParasite())
            GameController.Instance.enemyList.Remove(gameObject);
        if (bot.BrickAtBotArr(bot.coreV2)==null)
            GameController.Instance.lives = 0;
        bot.RefreshNeighborLists();
        // bot.powerGrid.Refresh();
        bot.orphanCheckFlag = true;
    }

    public void MoveBrick(Vector2Int newArrPos) {

        // move this brick to newArrPos.x, newArrPos.y

        if ((bot.IsValidBrickPos(newArrPos) &&
            (bot.BrickAtBotArr(newArrPos) == null))) 
        {
            // update the array

            bot.SetBrickAtBotArr(newArrPos,gameObject);
            bot.brickTypeArr[newArrPos.x,newArrPos.y] = brickType;
            
            bot.SetBrickAtBotArr(arrPos,null);
            bot.brickTypeArr[arrPos.x,arrPos.y] = -1;

            // move the gameObject

            SmoothMoveBrickObj(newArrPos);
  
            arrPos = newArrPos;

            // update neighbor lists

            bot.RefreshNeighborLists();
            // bot.powerGrid.Refresh();
        }  
    }

    public void SmoothMoveBrickObj(Vector2Int newArrPos){
        Vector2Int newOffset = ScreenStuff.ScreenToBotOffset(bot.ArrToOffset(newArrPos),bot.botRotation);
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
            brickHP = brickMaxHP[brickLevel];
            healthBar.gameObject.SetActive(false);
            GetComponent<SpriteRenderer>().sprite = spriteArr[brickLevel];
            if (brickType == 1)
                gameObject.GetComponent<Fuel>().UpgradeFuelLevel();
        }
    }


    public void HealMaxHP(){
        brickHP = brickMaxHP[brickLevel];
        healthBar.gameObject.SetActive(false);
    }

    public int ConvertToBitType(){
        return brickType + 2;
    }

    public int ConvertToEnemyType(){
        return brickType-7;
    }

    public bool CompareToBit(Bit bit) {
        int compType = Mathf.RoundToInt((ID-bit.bitLevel)/1000) - 2;

        return((compType == brickType)&&(bit.bitLevel==brickLevel));
    }

}
