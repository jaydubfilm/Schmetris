using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class PlayerShield : MonoBehaviour
{

    [Header("Pieces")]
    [FoldoutGroup("Pieces")]
    public GameObject verticalPiece;
    [FoldoutGroup("Pieces")]
    public GameObject horizontalPiece;
    [FoldoutGroup("Pieces")]
    public GameObject curvedPiece;

    float shieldOffset = 1;

    Bot bot;
    List<GameObject> shieldParts = new List<GameObject>(); 

    bool topOccupied;
    bool bottomOccupied;
    bool leftOccupied;
    bool rightOccupied;

    // Start is called before the first frame update
    void Start()
    {
        bot = GetComponent<Bot>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    [Button(ButtonSizes.Large)]
    void UpdateShieldFormation()
    {
        for (int i = 0; i < shieldParts.Count; i++)
        {
            Destroy(shieldParts[i]);
            print("Destroyed");
        }

        for (int x = 0; x < bot.brickList.Count; x++)
        {

            topOccupied = false;
            bottomOccupied = false;
            leftOccupied = false;
            rightOccupied = false;

            int neighbourListSize = bot.brickList[x].GetComponent<Brick>().neighborList.Count;


            Vector3 brickPos = bot.brickList[x].GetComponent<Brick>().transform.localPosition;

            for (int y = 0; y < neighbourListSize; y++)
            {
               
                Vector3 neighborPos = bot.brickList[x].GetComponent<Brick>().neighborList[y].transform.localPosition;

                //Check for a neighbor at each position
                if (neighborPos.y > brickPos.y)
                    topOccupied = true;

                if (neighborPos.y < brickPos.y)
                    bottomOccupied = true;

                if (neighborPos.x < brickPos.x)
                    leftOccupied = true;

                if (neighborPos.x > brickPos.x)
                    rightOccupied = true;
            }

            if (topOccupied == false)
                AddShieldSegment("top", bot.brickList[x].transform);

            if (bottomOccupied == false)
                AddShieldSegment("bottom", bot.brickList[x].transform);

            if (leftOccupied == false)
                AddShieldSegment("left", bot.brickList[x].transform);

            if (rightOccupied == false)
                AddShieldSegment("right", bot.brickList[x].transform);

        }

        SetShieldShape();
    }

    void AddShieldSegment(string placement, Transform brick)
    {
        if (placement == "top")
        {

            float yPos;
            yPos = brick.position.y + shieldOffset;
            Vector3 piecePos = new Vector3(brick.position.x, yPos, brick.position.z);
            GameObject topPiece = Instantiate(horizontalPiece,  piecePos, Quaternion.identity, brick);
            shieldParts.Add(topPiece);
            topPiece.name = "Top";
            print("add top");
            
        }

        if (placement == "bottom")
        {

            float yPos;
            yPos = brick.position.y - shieldOffset;
            Vector3 piecePos = new Vector3(brick.position.x, yPos, brick.position.z);
            GameObject bottomPiece = Instantiate(horizontalPiece, piecePos, Quaternion.identity, brick);
            bottomPiece.name = "Bottom";
            shieldParts.Add(bottomPiece);

            print("add bottom");
        }

        if (placement == "left")
        {

            float xPos;
            xPos = brick.position.x - shieldOffset;
            Vector3 piecePos = new Vector3(xPos, brick.position.y, brick.position.z);
            GameObject leftPiece = Instantiate(verticalPiece, piecePos, Quaternion.identity, brick);
            leftPiece.name = "Left";
            shieldParts.Add(leftPiece);

            print("add left");
        }

        if (placement == "right")
        {

            float xPos;
            xPos = brick.position.x + shieldOffset;
            Vector3 piecePos = new Vector3(xPos, brick.position.y, brick.position.z);
            GameObject rightPiece = Instantiate(verticalPiece, piecePos, Quaternion.identity, brick);
            rightPiece.name = "Right";
            shieldParts.Add(rightPiece);
            print("add right");
        }
    }


    void SetShieldShape()
    {
        //for every vertical piece
            //intersect at top? - inside curve
                //is there a top neighbour - outside curve
                    //Neither - straight
                //Curve Right
            //intersect at bottom? - inside curve
                //Bottom neighbor?  - inside curve
                    //Neither? straight



    }   
}
