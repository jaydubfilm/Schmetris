using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Container object holding a group of Bits
public class Block : MonoBehaviour
{
    //Child bits and their arrangement by coordinates
    public List<GameObject> bitList;
    public GameObject[,] bitArr;

    //Block movement
    public Rigidbody2D rb;
    public int column;
    Vector3 moveToPos;

    //Block stats
    public float blockSpeed;
    public int blockRadius;
    public int blockWidth;
    public int blockRotation;

    //Player components
    public Bot bot;
    public Vector2Int coreV2;

    // Start is called before the first frame update
    void Start()
    {
        blockRadius = GameController.Instance.settings.blockRadius;
        int absoluteCol = ScreenStuff.GetCol(gameObject);
        column = ScreenStuff.WrapCol(absoluteCol, bot.coreCol);
        blockWidth = blockRadius * 2 + 1;
        bitArr = new GameObject[blockWidth, blockWidth];
        coreV2 = new Vector2Int(blockRadius, blockRadius);
        rb = gameObject.GetComponent<Rigidbody2D>();
        GameController.OnSpeedChange += UpdateBlockSpeed;
        UpdateBlockSpeed();
        StartCoroutine(WaitAndRotateBits());
    }

    //Allow all bits to initialize properly before rotating them
    IEnumerator WaitAndRotateBits()
    {
        yield return new WaitForEndOfFrame();
        RotateBitsUpright();
    }

    //Remove events from this block when destroyed
    private void OnDestroy()
    {
        GameController.OnSpeedChange -= UpdateBlockSpeed;
    }

    //If player changes game speed, update block velocity to match
    void UpdateBlockSpeed()
    {
        blockSpeed = GameController.Instance.blockSpeed * GameController.Instance.adjustedSpeed;
        rb.velocity = new Vector3(0, -blockSpeed, 0);
    }

    //What column is this block in?
    public int GetXOffset(int coreColumn)
    {
        int offset = column - coreColumn;
        if (offset > 20)
            offset -= 40;
        return offset;
    }

    //Rotate bit sprites to the correct orientation
    public void RotateBitsUpright()
    {
        foreach (GameObject bit in bitList)
        {
            bit.GetComponent<Bit>().RotateUpright();
        }
    }

    //Is this position within the block bounds?
    public bool IsValidBitPos(Vector2Int arrPos)
    {
        if ((0<=arrPos.x)&&(arrPos.x<blockWidth)&&(0<=arrPos.y)&&(arrPos.y<blockWidth))
            return true;
        else   
            return false;
    }

    //Destroy this block and associated Bits
    public void DestroyBlock()
    {
        GameController.Instance.blockList.Remove(gameObject);
        Destroy(gameObject);
    }

    //Block has bounced off of something and is no longer falling in a straight line
    public void BounceBlock()
    {
        Vector2 force = new Vector2(Random.Range(-10, 10), 5);

        GameController.Instance.blockList.Remove(gameObject);
        foreach (GameObject bitObj in bitList)
        {
            bitObj.GetComponent<Bit>().hasBounced = true;
            bitObj.GetComponent<BoxCollider2D>().enabled = false;
            bitObj.GetComponent<BoxCollider2D>().isTrigger = false;
        }

        rb.isKinematic = false;
        rb.AddForce(force, ForceMode2D.Impulse);
        rb.AddTorque(Random.Range(-1, 1), ForceMode2D.Impulse);
        rb.gravityScale = 4;

        gameObject.tag = "Moveable";
    }
}
