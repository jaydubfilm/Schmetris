using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    //GameObject player;

    //private Vector3 offset;

    // Start is called before the first frame update
    void Awake()
    {
       // offset = transform.position-player.transform.position;
        DontDestroyOnLoad(this.gameObject);
        //player = GameController.game.bot;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        //transform.position = player.transform.position + offset;
    }
    
}
