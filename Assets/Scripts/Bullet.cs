using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    float speed;
    float power;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
         if (transform.position.y > ScreenStuff.topEdgeOfWorld)
                Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D collider){
        Bit bit = collider.gameObject.GetComponent<Bit>();
        Brick brick = collider.gameObject.GetComponent<Brick>();
        if(bit!= null)
        {
            bit.RemoveFromBlock("explode");
            Destroy(gameObject);
        }
        if (brick != null)
            if (transform.parent != brick.transform)
                Destroy(gameObject);
    } 

}
