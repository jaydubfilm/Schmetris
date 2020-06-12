using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AITestingCameraMovement : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey("w"))
        {
            gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y + 20 * Time.deltaTime, gameObject.transform.position.z);
        }

        if (Input.GetKey("s"))
        {
            gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y - 20 * Time.deltaTime, gameObject.transform.position.z);
        }

        if (Input.GetKey("a"))
        {
            gameObject.transform.position = new Vector3(gameObject.transform.position.x - 20 * Time.deltaTime, gameObject.transform.position.y, gameObject.transform.position.z);
        }

        if (Input.GetKey("d"))
        {
            gameObject.transform.position = new Vector3(gameObject.transform.position.x + 20 * Time.deltaTime, gameObject.transform.position.y, gameObject.transform.position.z);
        }
    }
}
