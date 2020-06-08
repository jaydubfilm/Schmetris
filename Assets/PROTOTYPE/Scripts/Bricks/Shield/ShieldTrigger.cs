using System.Collections.Generic;
using UnityEngine;

//This script goes on a child of the shield brick, and is used to detect what bricks should be shielded
public class ShieldTrigger : MonoBehaviour
{
    //Components
    Bot parentBot;
    ShieldBrick parentBrick;
    public GameObject outline;

    //Shield stats
    bool hasShield = false;

    //Bricks within range
    public List<Brick> protectedList = new List<Brick>();

    //Init
    void Start()
    {
        parentBrick = GetComponentInParent<ShieldBrick>();
        parentBot = GetComponentInParent<Bot>();
    }

    //Toggle shield outline when shield is activated or deactivated
    public void SetShieldOutline(bool isActive)
    {
        if (hasShield != isActive)
        {
            hasShield = isActive;
            if (hasShield)
            {
                for(int i = 0;i<protectedList.Count;i++)
                {
                    if (protectedList[i])
                    {
                        AddShield(protectedList[i]);
                    }
                    else
                    {
                        protectedList.RemoveAt(i--);
                    }
                }
            }
            else
            {
                for (int i = 0; i < protectedList.Count; i++)
                {
                    if (protectedList[i])
                    {
                        RemoveShield(protectedList[i]);
                    }
                    else
                    {
                        protectedList.RemoveAt(i--);
                    }
                }
            }
        }
    }

    //Add a shield to this object
    public void AddShield(Brick targetBrick)
    {
        if (!targetBrick.IsParasite() && !targetBrick.activeShields.Contains(parentBrick))
        {
            targetBrick.activeShields.Add(parentBrick);
        }
        if (targetBrick.activeShields.Contains(parentBrick) && !targetBrick.GetComponentInChildren<OutlineCheck>(false))
        {
            Instantiate(outline, targetBrick.transform.position, targetBrick.transform.rotation, targetBrick.transform);
            parentBrick.UpdateShieldColour();
        }
    }

    //Remove a shield from this object
    public void RemoveShield(Brick targetBrick)
    {
        if (targetBrick.activeShields.Contains(parentBrick))
        {
            targetBrick.activeShields.Remove(parentBrick);
        }
        if (targetBrick.activeShields.Count == 0 && targetBrick.GetComponentInChildren<OutlineCheck>(false))
        {
            targetBrick.GetComponentInChildren<OutlineCheck>(false).RemoveShieldOutline();
        }
    }

    //An object has entered shield range
    void OnEnterShield(Brick targetBrick)
    {
        if(targetBrick && !protectedList.Contains(targetBrick) && parentBot.brickList.Contains(targetBrick.gameObject))
        {
            protectedList.Add(targetBrick);
            if(hasShield)
            {
                AddShield(targetBrick);
            }
        }
    }

    //An object has left shield range
    public void OnExitShield(Brick targetBrick)
    {
        if(targetBrick && protectedList.Contains(targetBrick))
        {
            protectedList.Remove(targetBrick);
            RemoveShield(targetBrick);
        }
    }

    //Update bricks in range
    private void Update()
    {
        //Get bricks in range this frame
        List<Brick> bricksInRange = new List<Brick>();
        Collider2D[] boxCheck = Physics2D.OverlapBoxAll(transform.position, GetComponent<BoxCollider2D>().size, 0);
        foreach(Collider2D collision in boxCheck)
        {
            if(collision.GetComponent<Brick>())
            {
                bricksInRange.Add(collision.GetComponent<Brick>());
            }
        }

        //Parent brick is always in range
        bricksInRange.Add(parentBrick.GetComponent<Brick>());

        //Remove bricks that have moved out of range or no longer exist
        for(int i = 0;i<protectedList.Count;i++)
        {
            if(!protectedList[i])
            {
                protectedList.RemoveAt(i--);
            }
            else if (!GameController.Instance.bot.brickList.Contains(protectedList[i].gameObject) || !bricksInRange.Contains(protectedList[i]))
            {
                OnExitShield(protectedList[i--]);
            }
        }

        //Add shield to new bricks
        foreach(Brick brickInRange in bricksInRange)
        {
            OnEnterShield(brickInRange);
        }
    }
}
