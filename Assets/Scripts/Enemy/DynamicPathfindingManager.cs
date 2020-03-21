using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class DynamicPathfindingManager : MonoBehaviour
{


    //TODO adjust offsetDistance dynamically to account for object speed
    public float offsetDistance = 1;
    Bit[] childBlock;
    bool recheckNextFrame;

    //Called from GameController, equips bricks with pathfinding obstacles
    public void SetDynamicObstacle(GameObject block)
    {
        childBlock = block.GetComponentsInChildren<Bit>();


            for (int i = 0; i < childBlock.Length; i++)
            {

                //Create a dynamic obstacle slightly below the block, to fake predictive AI behaviour
                GameObject obstacle = new GameObject();
                obstacle.layer = 11;
                obstacle.name = "Enemy Collider";
                obstacle.transform.parent = childBlock[i].transform;
                obstacle.transform.rotation = new Quaternion(0, 0, 0, 0);

                //Set up Collider
                BoxCollider2D col = obstacle.AddComponent<BoxCollider2D>();
                col.isTrigger = true;
                col.offset = new Vector2(0, -1 * (offsetDistance * 0.5f));
                col.size = new Vector2(1, offsetDistance);
                col.edgeRadius = 0.65f;

                //Add scripts to our new object
                obstacle.gameObject.AddComponent<DynamicGridObstacle>().enabled = true;
                obstacle.gameObject.AddComponent<AiObstaclePlacement>().AssignObj(childBlock[i].gameObject, offsetDistance);

        }


    }

}
