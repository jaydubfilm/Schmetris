using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brick : MonoBehaviour
{

    const int brickMoneyMultiplier = 10;

    public Vector2Int arrPos;
  
    public int brickType;
    public int ID;
    public int brickLevel;
    public int brickHP;
    public int[] brickMaxHP;
    public static int brickMaxLevel = 5;
    public static float brickMoveSpeed = 20f;

    //Resource burn rates
    public bool passiveBurn = false;
    public bool hasResources = true;
    public float[] redBurn;
    public float[] blueBurn;
    public float[] greenBurn;
    public float[] yellowBurn;
    public float[] greyBurn;

    public AudioClip addBrickSound;
    private AudioSource source;
  
    public Sprite[] spriteArr;
    
    public GameObject parentBot;
    Bot bot;
    Rigidbody2D rb2D;

    public List<GameObject> neighborList = new List<GameObject>();
    public HealthBar healthBar;

    //Is this brick fully powered by the grid?
    bool _isPowered = false;
    public bool isPowered
    {
        get
        {
            return _isPowered;
        }
        set
        {
            if(_isPowered != value)
            {
                if(value)
                {
                    brickHP = Mathf.Min(brickMaxHP[brickLevel], brickHP + healthDiff);
                    healthDiff = 0;

                    Fuel fuelBrick = GetComponent<Fuel>();
                    if(fuelBrick)
                    {
                        fuelBrick.fuelLevel = Mathf.Min(fuelBrick.maxFuelArr[brickLevel], fuelBrick.fuelLevel + fuelBrick.fuelDiff);
                        fuelBrick.fuelDiff = 0;
                    }
                }
                else
                {
                    healthDiff = Mathf.Max(0, brickHP - brickMaxHP[0]);
                    brickHP = Mathf.Min(brickHP, brickMaxHP[0]);

                    Fuel fuelBrick = GetComponent<Fuel>();
                    if (fuelBrick)
                    {
                        fuelBrick.fuelDiff = Mathf.Max(0, fuelBrick.fuelLevel - fuelBrick.maxFuelArr[0]);
                        fuelBrick.fuelLevel = Mathf.Min(fuelBrick.fuelLevel, fuelBrick.maxFuelArr[0]);
                    }
                }
            }
            _isPowered = value;
            GetComponent<SpriteRenderer>().color = _isPowered ? Color.white : Color.gray;
        }
    }

    //Store change in brick's health if power level changes
    int healthDiff = 0;

    //Return adjusted brick level based on available power
    public int GetPoweredLevel()
    {
        return isPowered ? brickLevel : 0;
    }

    void Awake () {
        brickLevel = 0;
        brickHP = brickMaxHP[brickLevel];
        source = GetComponent<AudioSource>();
        rb2D = gameObject.GetComponent<Rigidbody2D>();
        healthBar = GetComponentInChildren<HealthBar>();
        healthBar.gameObject.SetActive(false);
        _isPowered = false;
    }

    void Start () {
        bot = parentBot.GetComponent<Bot>();
        FixedJoint2D fj = gameObject.GetComponent<FixedJoint2D>();
        fj.connectedBody = parentBot.GetComponent<Rigidbody2D>();
        InvokeRepeating("CheckHP",0.5f,0.1f);

        bool burnsResource = false;
        foreach(float Resource in redBurn)
        {
            if(Resource > 0)
            {
                burnsResource = true;
                break;
            }
        }
        foreach (float Resource in blueBurn)
        {
            if (Resource > 0)
            {
                burnsResource = true;
                break;
            }
        }
        foreach (float Resource in greenBurn)
        {
            if (Resource > 0)
            {
                burnsResource = true;
                break;
            }
        }
        foreach (float Resource in yellowBurn)
        {
            if (Resource > 0)
            {
                burnsResource = true;
                break;
            }
        }
        foreach (float Resource in greyBurn)
        {
            if (Resource > 0)
            {
                burnsResource = true;
                break;
            }
        }
        if(burnsResource)
        {
            bot.resourceBurnBricks.Add(this);
        }
    }

    //If resources burn constantly, apply here
    void Update()
    {
        if (passiveBurn)
        {
            hasResources = TryBurnResources(Time.deltaTime);
        }
    }

    //Burn corresponding resource amounts, if able
    public bool TryBurnResources(float interval)
    {
        int burnLevel = GetPoweredLevel();
        if (bot.storedRed >= interval * redBurn[burnLevel] && bot.storedBlue >= interval * blueBurn[burnLevel] && bot.storedGreen >= interval * greenBurn[burnLevel] && bot.storedYellow >= interval * yellowBurn[burnLevel] && bot.storedGrey >= interval * greyBurn[burnLevel])
        {
            bot.storedRed -= interval * redBurn[burnLevel];
            bot.storedBlue -= interval * blueBurn[burnLevel];
            bot.storedGreen -= interval * greenBurn[burnLevel];
            bot.storedYellow -= interval * yellowBurn[burnLevel];
            bot.storedGrey -= interval * greyBurn[burnLevel];
            return true;
        }
        return false;
    }

    void CheckHP(){
        if (brickHP<=0)
            ExplodeBrick();
    }
    public bool IsParasite(){
        if (GetComponent<Parasite>()==null)
            return false;
        else
            return true;
    }

    public bool IsCrafted()
    {
        return GetComponent<CraftedPart>() != null;
    }

    public void AdjustHP(int damage) {
        brickHP+=damage;
     
        if (brickHP>0){
            if (brickHP>=brickMaxHP[GetPoweredLevel()]) {
                brickHP = brickMaxHP[GetPoweredLevel()];  
                healthBar.gameObject.SetActive(false);
            } else if (healthBar.gameObject.activeInHierarchy==false) {
                healthBar.gameObject.SetActive(true);
                float normalizedHealth = (float)brickHP/(float)brickMaxHP[GetPoweredLevel()];
                healthBar.SetSize(normalizedHealth);
            } else {
                float normalizedHealth = (float)brickHP/(float)brickMaxHP[GetPoweredLevel()];
                healthBar.SetSize(normalizedHealth);
            }
        }
        else if (IsParasite())
        {
            GetComponent<Parasite>().ScoreEnemy();
        }
    }

    public int BitBrickCollide(GameObject bitObj) {
        Transform t = bitObj.transform.parent;
        if (t==null)
            return 0;
        
        GameObject blockObj = t.gameObject;
        Block block = blockObj.GetComponent<Block>();
        Bit bit = bitObj.GetComponent<Bit>();
        if (bit == null)
            return 0;

        int bitType = bit.bitType;
        float rA = parentBot.transform.rotation.eulerAngles.z;

        if (blockObj == null)
            return 0;

        if (bit==null)
            return 0;

        if (bitType == 0) // black bit - hurt the brick
        {
            AdjustHP(-1000);
            if (!IsParasite())
            {
                bot.GetComponent<Overheat>().AddHeat();
            }
            bit.RemoveFromBlock("explode");
        } 
        else
        {
            if (!((rA == 0) || (rA == 90) || (rA == 180) || (rA == 270))) {
                block.BounceBlock();
            } else {
                Vector2Int bitCoords = ScreenStuff.GetOffset(bitObj);
                Vector2Int brickCoords = ScreenStuff.GetOffset(gameObject);
                Vector2Int hitDirV2 = brickCoords-bitCoords;

                if (hitDirV2 == new Vector2Int(0, 0))
                {
                    block.BounceBlock();
                }

                if (bitType == 1) // white bit - bump the brick
                {
                    bot.BumpColumn(arrPos,hitDirV2);
                    block.BounceBlock();
                } else {   
                    bot.ResolveCollision(blockObj,hitDirV2);
                }
            } 
        }
        return 1;
    }

    public Vector2Int ScreenArrPos(){
        return bot.BotToScreenCoords(arrPos);
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

        if (brickType == 9 && (bot.BrickAtBotArr(bot.coreV2) == null))
            GameController.Instance.EndGame("CORE DESTROYED");

        if (brickType == 6) {
            Bomb bomb = GetComponent<Bomb>();
            int damage = bomb.damage[GetPoweredLevel()];
            BombEnemies(damage, bomb.bombEffect);
        }

        if ((brickType == 1)&&(GetComponent<Fuel>().fuelLevel>0))
        {
            for (int x = 0; x < neighborList.Count; x++) {
                if (!neighborList[x] || !neighborList[x].GetComponent<Brick>())
                {
                    neighborList.RemoveAt(x--);
                }
                else if (neighborList[x].GetComponent<Brick>().brickType == 1)
                {
                    neighborList[x].GetComponent<Brick>().AdjustHP(-10);
                }
            }
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

    IEnumerator WaitAndBombEnemies(int damage, GameObject effect){
        yield return new WaitForSeconds(0.5f);
        BombEnemies(damage, effect);
    }

    public void BombEnemies(int damage, GameObject effect)
    {
        List<GameObject> enemyArr = new List<GameObject>();

        for (int x = 0; x < GameController.Instance.enemyList.Count; x++)
        {
            if (GameController.Instance.enemyList[x])
            {
                enemyArr.Add(GameController.Instance.enemyList[x]);
            }
            else
            {
                GameController.Instance.enemyList.RemoveAt(x--);
            }
        }

        for (int x = 0; x < enemyArr.Count; x++)
        {
            Brick brick = enemyArr[x].GetComponent<Brick>();
            if (brick != null)
            {
                brick.AdjustHP(-damage);
                GameObject explosion = Instantiate(effect, brick.transform.position, Quaternion.identity);
            }
            else
            {
                Enemy enemy = enemyArr[x].GetComponent<Enemy>();
                enemy.hP -= damage;
                GameObject explosion = Instantiate(effect, enemy.transform.position, Quaternion.identity);
            }
        }
    }

    public void DestroyBrick() {
        RemoveBrickFromBotArray();  
        Destroy(gameObject);
    }

    public void MakeOrphan() {

        RemoveBrickFromBotArray();
        rb2D.isKinematic = false;
    
        transform.parent = null;
        tag = "Moveable";

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
            GameController.Instance.EndGame("CORE DESTROYED");

        if (GetComponent<Container>())
            bot.RemoveContainer(GetComponent<Container>());

        if (bot.fuelBrickList.Contains(gameObject))
        {
            GetComponent<Fuel>().CancelBurnFuel();
            bot.fuelBrickList.Remove(gameObject);
        }

        if (bot.resourceBurnBricks.Contains(this))
            bot.resourceBurnBricks.Remove(this);

        bot.RefreshNeighborLists();
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
        if (bot && arrPos == bot.coreV2)
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
            isPowered = true;
            brickLevel++;
            ID++;
            brickHP = brickMaxHP[brickLevel];
            healthBar.gameObject.SetActive(false);
            GetComponent<SpriteRenderer>().sprite = spriteArr[brickLevel];
            if (brickType == 1)
                gameObject.GetComponent<Fuel>().UpgradeFuelLevel();

            if(GetComponent<Container>())
            {
                bot.UpdateContainers();
            }

            int scoreIncrease = (int)Mathf.Pow(brickMoneyMultiplier, brickLevel);
            GameController.Instance.money += scoreIncrease;
            GameController.Instance.CreateFloatingText("$" + scoreIncrease, transform.position, 40, Color.white);
        }
    }


    public void HealMaxHP(){
        brickHP = brickMaxHP[GetPoweredLevel()];
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
