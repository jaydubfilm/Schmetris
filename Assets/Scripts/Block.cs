using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
   // public GameObject[,] bitArr;
    public List<GameObject> bitList;
    public int column;
    //public int row;
    public Bot bot;

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
      // blockWidth = 2 * radius +1;
      //  blockHeight = 2 * radius +1;
      // bitArr = new GameObject[blockWidth,blockHeight];
    }



    // Update is called once per frame
    void Update()
    {

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

}
