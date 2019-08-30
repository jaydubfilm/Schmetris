using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    public GameObject[,] bitArr;
    public int column;
    public int bitCount = 1;
    public int radius = 2;
    public int blockWidth;
    public int blockHeight;
    public GameObject coreBit;
    public GameObject newBit;

    // Start is called before the first frame update
    void Start()
    {
        blockWidth = 2 * radius +1;
        blockHeight = 2 * radius +1;
        bitArr = new GameObject[blockWidth,blockHeight];
    }

    // Update is called once per frame
    void Update()
    {
    }
}
