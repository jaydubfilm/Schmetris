using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bit : MonoBehaviour
{
    public int bitType;
    public int ID;
    public Vector2Int blockOffset;
    public Vector2Int screenOffset;
    public Vector2Int blockArrPos;
    public int bitLevel=0;
    GameObject parentObj;
    Block parentBlock;
    LayerMask brickMask; 

    bool CanCollideFlag;
    public bool hasBounced = false;

    public Sprite[] spriteArr;

    private void OnEnable()
    {
        GameController.OnGameOver += ExplodeBit;
        GameController.OnLoseLife += ExplodeBit;
    }

    private void OnDisable()
    {
        GameController.OnGameOver -= ExplodeBit;
        GameController.OnLoseLife -= ExplodeBit;
    }

    public void SetLevel(int level)
    {
        bitLevel = level;
        if (level < spriteArr.Length)
            this.GetComponent<SpriteRenderer>().sprite = spriteArr[bitLevel];
    }

    // Start is called before the first frame update

    void Start()
    {
        parentObj = transform.parent.gameObject;
        parentBlock = parentObj.GetComponent<Block>();
        Vector3 parentOffsetV3 = parentObj.transform.position;
        screenOffset = new Vector2Int (Mathf.RoundToInt((transform.position.x-parentOffsetV3.x) / ScreenStuff.colSize), Mathf.RoundToInt((transform.position.y-parentOffsetV3.y) / ScreenStuff.rowSize));
        blockOffset = ScreenStuff.BotToScreenOffset(screenOffset,parentBlock.blockRotation);
        blockArrPos = parentBlock.coreV2 + blockOffset;
        brickMask = LayerMask.GetMask("Brick");
        CanCollideFlag = true;

        if (parentBlock.IsValidBitPos(blockArrPos)) {
            parentBlock.bitList.Add(gameObject);
            parentBlock.bitArr[blockArrPos.x,blockArrPos.y] = gameObject;
        } else  
            gameObject.transform.parent = null;

       
        RotateUpright();
        ID = bitType*1000;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!hasBounced && CanCollideFlag && !GetComponent<Enemy>()) {
            //RaycastHit2D rH = Physics2D.Raycast(transform.position, Vector2.down, ScreenStuff.colSize/2,brickMask); 
            RaycastHit2D rH = Physics2D.BoxCast(transform.position, Vector2.one * ScreenStuff.colSize, 0, Vector2.down, Mathf.Max(ScreenStuff.colSize / 2.0f, parentBlock ? parentBlock.rb.velocity.magnitude * Time.fixedDeltaTime : ScreenStuff.colSize / 2.0f), brickMask);
            if (rH.collider!=null) {
                if (rH.collider.gameObject.GetComponent<Brick>() != null)
                {
                    if (rH.collider.gameObject.GetComponent<Brick>().BitBrickCollide(gameObject) > 0)
                    {
                        CanCollideFlag = false;
                        StartCoroutine(WaitToCollideAgain(0.2f));
                    }
                }
            }
        }
    }

    public IEnumerator WaitToCollideAgain(float pause){
        yield return new WaitForSeconds(pause);
        CanCollideFlag = true;
    }

    public void RemoveFromBlock(string actionType){
        if (transform.parent == null)
            return;

        gameObject.transform.parent = null;
        parentBlock.bitArr[blockArrPos.x,blockArrPos.y] = null;
        parentBlock.bitList.Remove(gameObject);

        if (parentBlock.bitList.Count==0)
            parentBlock.DestroyBlock();

        switch (actionType) {
            case ("bounce"):
                BounceBit();
                break;
            case ("explode"):
                ExplodeBit();
                break;
            default :
                Destroy(gameObject);
                break;
        }
    }  

    public void ExplodeBit() {
        Animator anim;
        float animDuration;

        anim = gameObject.GetComponent<Animator>();
        anim.enabled = true;

        animDuration = 0.3f;
        StartCoroutine(DestroyAfterAnimation(animDuration));
    }

    public void RotateUpright() {
        transform.rotation = Quaternion.identity;
    }

    IEnumerator DestroyAfterAnimation(float duration){  
        yield return new WaitForSeconds(duration);
        Destroy(gameObject); 
    }

    public int ConvertToBrickType(){
        return bitType - 2;
    }

    public bool CompareToBrick(Brick brick) {
        // part of a system to compare the IDs of Bricks and Bits.  Not currently implemented
        int compType = Mathf.RoundToInt((ID-bitLevel)/1000) - 2;

        return((compType == brick.brickType)&&(bitLevel==brick.brickLevel));
    }

    public void BounceBit() {
        Vector2 force = new Vector2 (Random.Range(-10,10),5);
        Rigidbody2D rb2D = GetComponent<Rigidbody2D>();
        BoxCollider2D box = GetComponent<BoxCollider2D>();

        hasBounced = true;
        box.enabled = false;
        CanCollideFlag = false;
        box.isTrigger = false;
        rb2D.isKinematic = false;
        rb2D.velocity = new Vector2(0,0); 
        rb2D.AddForce(force,ForceMode2D.Impulse);
        rb2D.AddTorque(Random.Range(-1,1),ForceMode2D.Impulse);
        rb2D.gravityScale=4;

        tag = "Moveable";
    }

}
