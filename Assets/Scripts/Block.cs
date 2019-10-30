using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    public List<GameObject> bitList;
    public GameObject[,] bitArr;
    
    public int column;
    //public int row;
    public Bot bot;
    public float blockSpeed = 0.2f;
  
    // Start is called before the first frame update
    void Start()
    {
      int absoluteCol =  ScreenStuff.GetCol(gameObject);
      column = ScreenStuff.WrapCol(absoluteCol,bot.coreCol);
      //row = GameController.spawnRow;
      // blockWidth = 2 * radius +1;
      //  blockHeight = 2 * radius +1;
      // bitArr = new GameObject[blockWidth,blockHeight];
      //StartCoroutine("MoveController");
    }


    // Update is called once per frame
    void Update()
    {

    }
  /*
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
    */


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

    public void BounceBlock() {
      GameController.Instance.blockList.Remove(gameObject);
      ScreenStuff.BounceObject(gameObject);
    }


    public bool IsBotBelow(){
        bool botBelow = false;
        int row = ScreenStuff.GetRow(gameObject);

        Vector2Int blockOffset = new Vector2Int (column-bot.coreCol,row);
        
        foreach(GameObject bit in bitList) {
            Vector2Int bitOffset = bit.GetComponent<Bit>().offset;
            Vector2Int testPos = bot.coreV2 + blockOffset + bitOffset + Vector2Int.down;
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
