using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
   // public GameObject[,] bitArr;
    public List<GameObject> bitList;
    public int column;
    public int row;
    //public int row;
    public Bot bot;
    public float blockSpeed = 0.2f;
   

   // public int bitCount = 1;
    // public int radius = 2;
    // public int blockWidth;
   // public int blockHeight;

    // public GameObject coreBit;
    // public GameObject newBit;

    // Start is called before the first frame update
    void Start()
    {
      int absoluteCol =  ScreenStuff.XPositionToCol(transform.position.x);
      column = ScreenStuff.WrapCol(absoluteCol,bot.coreCol);
      row = GameController.spawnRow;
      // blockWidth = 2 * radius +1;
      //  blockHeight = 2 * radius +1;
      // bitArr = new GameObject[blockWidth,blockHeight];
      StartCoroutine("MoveController");
    }



    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator MoveController()
    {
      for (int r = GameController.spawnRow; r > -10; r--) {
          if (IsBotBelow()) {
              bot.AddBlock(gameObject);
              yield break;
          } else {
              StepDown();
              row--;
          }
          yield return new WaitForSeconds(blockSpeed);
      }
      DestroyBlock();
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
    public bool IsBotBelow(){
        bool botBelow = false;
        Vector2Int blockOffset = new Vector2Int (column-bot.coreCol,row-bot.maxBotRadius);
        
        foreach(GameObject bit in bitList) {
            Vector2Int bitOffset = bit.GetComponent<Bit>().offset;
            Vector2Int testPos = blockOffset + bitOffset + bot.coreV2 + Vector2Int.down;
            Vector2Int rotatedTestPos = bot.TwistCoordsUpright(testPos);

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
}
