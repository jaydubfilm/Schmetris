using UnityEngine;
using Sirenix.OdinInspector;

//Bullet base class
public class Bullet : MonoBehaviour
{
    //Basic stats
    public float speed;
    public int damage;
    public float range;

    //Extra stats
    public bool isBlaster;
    public bool isGrenade;
    [ShowIf("isGrenade", true)]
    public float blastRadius;
    [ShowIf("isGrenade", true)]
    public float blastForce;

    //Targeting
    public Vector2 direction;
    public bool homing;
    Transform homingTarget;

    //Collision
    LayerMask enemyMask;
    LayerMask brickMask;
    LayerMask mosquitoMask;
    LayerMask bitMask;
    Rigidbody2D rb2d;

    //Debug
    public GameObject debugSphere;

    //Init
    void Start()
    {
        enemyMask = LayerMask.GetMask("Enemy");
        brickMask = LayerMask.GetMask("Brick");
        mosquitoMask = LayerMask.GetMask("Mosquito");
        bitMask = LayerMask.GetMask("Bit");
        rb2d = GetComponent<Rigidbody2D>();
    }

    //Determine bullet movement this frame
    Vector2 CalculateBulletStep()
    {
        if (homing && homingTarget)
        {
            Vector3 angleToPlayer = (homingTarget.position - transform.position).normalized;
            direction = new Vector2(angleToPlayer.x, angleToPlayer.y);
        }
        return direction * speed * Time.deltaTime;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 step = CalculateBulletStep();

        //Look for non-brick enemies in path
        RaycastHit2D r1 = Physics2D.Raycast(transform.position, direction, step.magnitude, enemyMask);
        if (r1.collider != null)
        {
            TryDamageTarget(r1.collider.gameObject);
        }

        //Look for brick enemies in path
        RaycastHit2D r2 = Physics2D.Raycast(transform.position, direction, step.magnitude, brickMask);
        if (r2.collider != null && r2.collider.GetComponent<Brick>())
        {
            TryDamageTarget(r2.collider.gameObject);
        }

        //Look for mosquito enemies in path
        RaycastHit2D r3 = Physics2D.Raycast(transform.position, direction, step.magnitude, brickMask);
        if (r3.collider != null)
        {
            TryDamageTarget(r3.collider.gameObject);
        }

        //Look for asteroids in path
        RaycastHit2D r4 = Physics2D.Raycast(transform.position, direction, step.magnitude, bitMask);
        if (r4.collider != null && r4.collider.GetComponent<Asteroid>())
        {
            TryDamageTarget(r4.collider.gameObject);
        }

        //Move bullet
        transform.position += new Vector3(step.x, step.y, 0);

        //If bullet reaches edge of range, destroy it
        range -= step.magnitude;
        if (range < 0)
        {
            Destroy(gameObject);
        }

        //If bullet reaches edge of world, destroy it
        if (transform.position.y > ScreenStuff.topEdgeOfWorld || transform.position.y < ScreenStuff.bottomEdgeOfWorld || transform.position.x > ScreenStuff.rightEdgeOfWorld || transform.position.x < ScreenStuff.leftEdgeOfWorld)
            Destroy(gameObject);

        //Destroy empty bullets
        if (GetComponentInChildren<SpriteRenderer>().isVisible == false)
        {
            Destroy(gameObject);
        }
    }

    //Deal damage to targeted enemy
    void TryDamageTarget(GameObject targetEnemy)
    {
        //Determine target type and deal initial damage
        bool hasHitTarget = false;
        if(targetEnemy.GetComponent<Asteroid>())
        {
            targetEnemy.GetComponent<Asteroid>().AdjustHP(damage, transform);
            hasHitTarget = true;
        }
        else if (targetEnemy.GetComponent<EnemyGeneral>())
        {
            targetEnemy.GetComponent<EnemyGeneral>().AdjustHP(-damage);
            hasHitTarget = true;
        }
        else if (targetEnemy.GetComponent<Brick>() && targetEnemy.GetComponent<Brick>().IsParasite())
        {
            targetEnemy.GetComponent<Brick>().AdjustHP(-damage);
            hasHitTarget = true;
        }

        if (hasHitTarget)
        {
            //Deal grenade damage in blast radius
            if (isGrenade)
            {
                Collider2D[] colliders = Physics2D.OverlapCircleAll(new Vector2(transform.position.x, transform.position.y), blastRadius);
                for (int i = 0; i < colliders.Length; i++)
                {
                    if (colliders[i].GetComponent<EnemyGeneral>())
                    {
                        //Damage already deal to targetEnemy, so skip this one
                        if(colliders[i] != targetEnemy)
                            colliders[i].GetComponent<EnemyGeneral>().AdjustHP(-damage);

                        GameObject debugSphereInstance = Instantiate(debugSphere, transform.position, Quaternion.identity);
                        debugSphereInstance.transform.localScale = new Vector3(blastRadius * 2, blastRadius * 2, blastRadius * 2);
                    }
                }
            }

            //A target has been hit - destroy bullet
            Destroy(gameObject);
        }
    }

    //Check hit colliders for damageable targets
    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryDamageTarget(collision.gameObject);
    }

    //Toggle homing ability
    public void SetAsHoming(Transform target, bool trueOrFalse)
    {
        homingTarget = target;
        homing = trueOrFalse;
    }
}
