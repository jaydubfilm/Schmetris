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
    public bool homing;
    Transform homingTarget;
    Rigidbody2D rb2d;

    // Start is called before the first frame update
    void Start()
    {
        enemyMask = LayerMask.GetMask("Enemy");
        brickMask = LayerMask.GetMask("Brick");
        rb2d = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        // check for collisions
        if (homing == false)
        {
            RaycastHit2D r1 = Physics2D.Raycast(transform.position, direction, ScreenStuff.colSize / 4, enemyMask);
            if (r1.collider != null)
            {
                GameObject enemyObj = r1.collider.gameObject;
                enemyObj.GetComponent<Enemy>().hP -= damage;
                Destroy(gameObject);
            }

            RaycastHit2D r2 = Physics2D.Raycast(transform.position, direction, ScreenStuff.colSize / 4, brickMask);
            if (r2.collider != null)
            {
                GameObject brickObj = r2.collider.gameObject;
                if (brickObj.GetComponent<Brick>().IsParasite())
                {
                    brickObj.GetComponent<Brick>().AdjustHP(-damage);
                    Destroy(gameObject);
                }
            }

            // move bullet
            Vector2 step = direction * speed * Time.deltaTime;
            range -= step.magnitude;
            if (range < 0)
                Destroy(gameObject);
            transform.position += new Vector3(step.x, step.y, 0);
            if (transform.position.y > ScreenStuff.topEdgeOfWorld)
                Destroy(gameObject);
        }

        //Homing
        else 
        {
            rb2d.bodyType = RigidbodyType2D.Dynamic;

            Vector3 angleToPlayer = (homingTarget.position - transform.position).normalized;
            rb2d.AddForce(new Vector2(angleToPlayer.x, angleToPlayer.y) * speed * 200* Time.deltaTime);

            if(GetComponentInChildren<SpriteRenderer>().isVisible == false)
            {
                Destroy(gameObject);
            }
        }

       
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        print(" Hit");

        if (collision.transform.GetComponent<EnemyGeneral>())
        {
            print(collision.gameObject.name + " Hit");
            EnemyGeneral enemyGeneral = collision.transform.GetComponent<EnemyGeneral>();
            enemyGeneral.hp -= damage;
            if (enemyGeneral.hp < 0)
                enemyGeneral.EnemyDeath();
            Destroy(gameObject);
        }
    }

    public void SetAsHoming(Transform target)
    {

        homingTarget = target;
        homing = true;
    }
}
