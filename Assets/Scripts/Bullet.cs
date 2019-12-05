using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed;
    public int damage;
    public Vector2 direction;
    LayerMask enemyMask;
    LayerMask brickMask;
    public float range;

    // Start is called before the first frame update
    void Start()
    {
        enemyMask = LayerMask.GetMask("Enemy");
        brickMask = LayerMask.GetMask("Brick");
    }

    // Update is called once per frame
    void Update()
    {
        // check for collisions

        RaycastHit2D r1 = Physics2D.Raycast(transform.position, direction, ScreenStuff.colSize/4,enemyMask); 
        if (r1.collider!=null) {
            GameObject enemyObj = r1.collider.gameObject;
            enemyObj.GetComponent<Enemy>().hP-=damage;
            Destroy(gameObject);
        } 
        RaycastHit2D r2 = Physics2D.Raycast(transform.position, direction, ScreenStuff.colSize/4,brickMask);
        if (r2.collider!=null) {
            GameObject brickObj = r2.collider.gameObject;
            if (brickObj.GetComponent<Brick>().IsParasite()) {
                brickObj.GetComponent<Brick>().AdjustHP(-damage);
                Destroy(gameObject);
             }
        }
        // move bullet
        Vector2 step = direction*speed*Time.deltaTime;
        range -= step.magnitude;
        if (range<0)
            Destroy(gameObject);
        transform.position += new Vector3(step.x,step.y,0);
        if (transform.position.y>ScreenStuff.topEdgeOfWorld)
            Destroy(gameObject);
    }
    /*
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
    */

}
