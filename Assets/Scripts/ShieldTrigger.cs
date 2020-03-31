using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This script goes on a child of the shield brick, and is used to detect what bricks should be shielded

public class ShieldTrigger : MonoBehaviour
{
    public bool check;
    Bot parentBot;
    public List<Brick> protectedList = new List<Brick>();
    Brick parentBrick;
    public GameObject outline;


    // Start is called before the first frame update
    void Start()
    {
        parentBrick = GetComponentInParent<Brick>();
        parentBot = GetComponentInParent<Bot>();
    }

    // Update is called once per frame
    public void CheckRadius()
    {

        check = true;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {

        if (check == true)
        {
            //add this brick to the list
            if (!protectedList.Contains(parentBrick))
            {

                protectedList.Add(parentBrick);
                Instantiate(outline, transform.position, Quaternion.identity, transform);
                parentBrick.activeShields.Add(parentBrick.GetComponent<ShieldBrick>());
            }

            //add affected blocks to the list
            if (parentBot.brickList.Contains(collision.gameObject))
            {

                if (!protectedList.Contains(collision.GetComponent<Brick>()))
                {
                    collision.GetComponent<Brick>().activeShields.Add(parentBrick.GetComponent<ShieldBrick>());
                    protectedList.Add(collision.GetComponent<Brick>());
                    Instantiate(outline, collision.transform.position, Quaternion.identity, collision.transform);
                }
            }
        }
            //check = false;        
    }
}
