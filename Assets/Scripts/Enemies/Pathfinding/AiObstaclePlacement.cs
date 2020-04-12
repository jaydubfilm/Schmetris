using UnityEngine;

public class AiObstaclePlacement : MonoBehaviour
{
    GameObject linkedBlock;
    float offsetDistance;

    // Start is called before the first frame update
    public void AssignObj(GameObject associatedBlock, float offset)
    {

        linkedBlock = associatedBlock;
        offsetDistance = offset;
        transform.position = linkedBlock.transform.position;
    }

    // Update is called once per frame
    void Update()
    {

        if(linkedBlock != null)
        {

            //transform.position = new Vector3(linkedBlock.transform.position.x, linkedBlock.transform.position.y - offsetDistance, linkedBlock.transform.position.z);
        }
    }
}
 