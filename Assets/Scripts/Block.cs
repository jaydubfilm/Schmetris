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
    
  
    // Start is called before the first frame update
    void Start()
    {
      blockRadius = GameController.Instance.settings.blockRadius;
      blockSpeed = GameController.Instance.blockSpeed;
      
      int absoluteCol =  ScreenStuff.GetCol(gameObject);
      float step = blockSpeed*Time.deltaTime;
        column = ScreenStuff.WrapCol(absoluteCol,bot.coreCol);
      blockWidth = blockRadius*2+1;
      bitArr = new GameObject[blockWidth,blockWidth];
      coreV2 = new Vector2Int(blockRadius,blockRadius);
      rb =  gameObject.GetComponent<Rigidbody2D>();
      rb.velocity = new Vector3(0,-blockSpeed,0);
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

    public void DestroyBlock(){
      GameController.Instance.blockList.Remove(gameObject);
      Destroy(gameObject);
    }

    public void BounceBlock() {
        Vector2 force = new Vector2 (Random.Range(-10,10),5);

        GameController.Instance.blockList.Remove(gameObject);
        foreach (GameObject bitObj in bitList)
        {
            bitObj.GetComponent<Bit>().hasBounced = true;
            bitObj.GetComponent<BoxCollider2D>().enabled = false;
            bitObj.GetComponent<BoxCollider2D>().isTrigger = false;
        }
        
        rb.isKinematic = false;
        rb.AddForce(force,ForceMode2D.Impulse);
        rb.AddTorque(Random.Range(-1,1),ForceMode2D.Impulse);
        rb.gravityScale=4;
        
        gameObject.tag = "Moveable";
    }
}
