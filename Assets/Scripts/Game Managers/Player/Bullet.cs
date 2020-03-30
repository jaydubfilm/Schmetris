using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
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
    public bool isGrenade;
    [ShowIf("isGrenade", true)]
    public float blastRadius;
    [ShowIf("isGrenade", true)]
    public float blastForce;
    public GameObject debugSphere;
    Vector3 startPos;
    public bool isBlaster;

    // Start is called before the first frame update
    void Start()
    {
        enemyMask = LayerMask.GetMask("Enemy");
        brickMask = LayerMask.GetMask("Brick");
        rb2d = GetComponent<Rigidbody2D>();
        startPos = transform.position;
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

            Vector3 angleToPlayer = (homingTarget.position - transform.position).normalized;

            Vector2 step = new Vector2(angleToPlayer.x, angleToPlayer.y) * speed * Time.deltaTime;
            range -= step.magnitude;
            if (range < 0)
                Destroy(gameObject);
            transform.position += new Vector3(step.x, step.y, 0);

            //saftey check
            if (GetComponentInChildren<SpriteRenderer>().isVisible == false)
            {
                Destroy(gameObject);
            }
        }


    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
       

        if (collision.transform.GetComponent<Asteroid>())
        {
            Asteroid asteroid = collision.transform.GetComponent<Asteroid>();
            asteroid.AdjustHP(damage, transform);
        }

        else if (collision.transform.GetComponent<EnemyGeneral>())
        {
            EnemyGeneral enemyGeneral = collision.transform.GetComponent<EnemyGeneral>();

            if (isGrenade == false)
            {

                enemyGeneral.hp -= damage;
                CheckKillEnemy(enemyGeneral);
                print("hit");

            }

            

            else
            {

                Collider2D[] colliders = Physics2D.OverlapCircleAll(new Vector2(transform.position.x, transform.position.y), blastRadius);

                for (int i = 0; i < colliders.Length; i++)
                {

                    if (colliders[i].GetComponent<EnemyGeneral>())
                    {

                        colliders[i].GetComponent<EnemyGeneral>().hp -= damage;
                        CheckKillEnemy(enemyGeneral);
                       
                        GameObject debugSphereInstance = Instantiate(debugSphere, transform.position, Quaternion.identity);
                        debugSphereInstance.transform.localScale = new Vector3(blastRadius * 2, blastRadius * 2, blastRadius * 2);
                    }

                    //if (colliders[i].GetComponent<Enemy>())
                    //{
                    //    colliders[i].GetComponent<Enemy>().hP -= damage;
                    //    GameObject debugSphereInstance = Instantiate(debugSphere, transform.position, Quaternion.identity);
                    //    debugSphereInstance.transform.localScale = new Vector3(blastRadius * 2, blastRadius * 2, blastRadius * 2);
                    //}
                    print("test");
                }
            }
            Destroy(gameObject);
        }
    }

    public void SetAsHoming(Transform target, bool trueOrFalse)
    {

        homingTarget = target;
        homing = true;
        //rb2d.bodyType = RigidbodyType2D.Dynamic;
        //rb2d.gravityScale = 0;
    }

    void CheckKillEnemy(EnemyGeneral enemyGeneral)
    {

        if (enemyGeneral.hp < 0)
            enemyGeneral.EnemyDeath();
        Destroy(gameObject);
    }
}
