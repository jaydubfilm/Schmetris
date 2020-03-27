using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using Sirenix.OdinInspector;

public class MosquitoAI : MonoBehaviour
{
    AIDestinationSetter aiDestinationSetter;
    AIPath aiPath;
    Transform player;
    GameObject pivot;
    SpringJoint2D spring;
    Rigidbody2D rb;

    [FoldoutGroup("Movement")]
    public float followSpeed;

    [FoldoutGroup("Movement")]
    public float impulseBurstFrequency = 1;

    [FoldoutGroup("Movement")]
    public AnimationCurve enemyDistance;

    private float storedTime;
    [FoldoutGroup("Weapon")]
    public  float rateOfFire;

    [FoldoutGroup("Weapon")]
    public float bulletSpeed;

    [FoldoutGroup("Weapon")]
    public float bulletLifetime = 2;

    [FoldoutGroup ("Weapon")]
    public GameObject bullet;

    [FoldoutGroup("Weapon")]
    public Sprite bulletSprite;

    [FoldoutGroup("Weapon")]
    public int bulletDamage = 1;

    [FoldoutGroup("Death")]
    public float timeUntilBlast = 5;

    [Tooltip("Range of the blast (how many columns and rows are affected)")]
    [FoldoutGroup("Death")]
    public float blastRadius = 5;

    [FoldoutGroup("Death")]
    public int blastDamage = 5;

    [Tooltip("Used to sort enemies from least to most powerful. Used to determine targets when firing")]
    public int strength = 2;

    public GameObject mosquitoShield;
    public GameObject debugExplosion;

    bool attackMode;
    bool attached;
    bool dying;
    float timeOfDeath;
    SpriteRenderer spriteRenderer;

    private void Start()
    {

        aiPath = GetComponent<AIPath>();
        aiDestinationSetter = GetComponent<AIDestinationSetter>();
        aiDestinationSetter.target = FindObjectOfType<Bot>().transform;
        player = aiDestinationSetter.target;
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        //create a shield at the player position if one does not exist
        if (player.GetComponentInChildren<MosquitoShield>() == null)
        {

            Instantiate(mosquitoShield, player.transform, false);
            transform.localPosition = Vector3.zero;
        }

        enemyDistance.preWrapMode = WrapMode.PingPong;
        enemyDistance.postWrapMode = WrapMode.PingPong;

        spring = GetComponent<SpringJoint2D>();
        rb = GetComponent<Rigidbody2D>();

        blastRadius = blastRadius * ScreenStuff.colSize;
    }

    private void Update()
    {

        aiPath.maxSpeed = followSpeed;

        if (attackMode == false)
            //constantly adjust spring distance
            spring.distance = enemyDistance.Evaluate(Time.time);
        
        else
        {
            if(Time.time > impulseBurstFrequency)           
                SetRandomTimer();
            
            FireCheck();
        }
 

        Vector3 dir = (player.position - transform.position).normalized;


        //Death Procedure
        if(dying == true)
        {

            //change enemy color as it dies
            float timeUntilFlashing = timeUntilBlast - 1.5f;

            if (Time.timeSinceLevelLoad - timeOfDeath < timeUntilBlast)
            {
                Color enemyColor = new Color( spriteRenderer.color.r, 1 - ((Time.timeSinceLevelLoad - timeOfDeath) / timeUntilFlashing), 1 - ((Time.timeSinceLevelLoad - timeOfDeath) / timeUntilFlashing));
                spriteRenderer.color = enemyColor;
            }

            if (Time.timeSinceLevelLoad - timeOfDeath > timeUntilBlast)
            {
                print(Time.timeSinceLevelLoad - timeOfDeath);
                DeathBlast();
            }
        }

        //testing
        if (Input.GetKeyDown(KeyCode.P))
        {
            Death();
        }

    }

    
    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.layer == 13)
        {

            aiPath.canMove = false;

            //Create a Spring at player Pos
            spring.connectedBody = player.GetComponent<Rigidbody2D>();
            spring.autoConfigureDistance = false;
            spring.enableCollision = true;
            attackMode = true;

            storedTime = Time.time;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {

        if (collision.gameObject.layer == 13)
        {
           
            attackMode = false;
            spring.connectedBody = null;
            aiPath.canMove = true;
        }
    }
    
    void SetRandomTimer()
    {

        AddRandomForce();
        impulseBurstFrequency = Random.Range(0.3f, 1f);
        impulseBurstFrequency += Time.time;
    }

    void AddRandomForce()
    {

        rb.AddForce(new Vector2(Random.Range(20f, 20f), Random.Range(55f, 55f)), ForceMode2D.Impulse);
        rb.AddTorque(Random.Range(8f, 12f));
    }


    void FireCheck()
    {

        if (Time.time - storedTime > rateOfFire)
        {

            Fire();
            storedTime = Time.time;
        }
    }
    void Fire()
    {
        
        EnemyBulletV2 thisBullet = Instantiate(bullet, transform.position, Quaternion.identity).GetComponent<EnemyBulletV2>();
        Vector3 dir = (player.position - thisBullet.transform.position).normalized;
        thisBullet.MosquitoBulletBehaviour(player.position, bulletSprite, dir, bulletLifetime, bulletSpeed, bulletDamage);
    }

    public void Death()
    {

        timeOfDeath = Time.timeSinceLevelLoad;
        dying = true;
    }

    void DeathBlast()
    {

        Collider2D[] colliders = Physics2D.OverlapCircleAll(new Vector2(transform.position.x, transform.position.y), blastRadius);

        for (int i = 0; i < colliders.Length; i++)
        {

            if (colliders[i].GetComponent<Brick>())
            {

                colliders[i].GetComponent<Brick>().AdjustHP(-blastDamage);
            }
        }
        
        GameObject blastSphere = Instantiate(debugExplosion, transform.position, Quaternion.identity, null);
        blastSphere.transform.localScale = new Vector3(blastRadius * 2, blastRadius * 2, blastRadius * 2);

        //Instantiate Explosion here
        Destroy(gameObject);
    }

    //void OnDrawGizmosSelected()
    //{
    //    Gizmos.DrawSphere(transform.position, blastRadius);
    //}
}
