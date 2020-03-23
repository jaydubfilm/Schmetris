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

    public GameObject mosquitoShield;

    bool attackMode;
    bool attached;

    private void Start()
    {

        aiDestinationSetter = GetComponent<AIDestinationSetter>();
        aiPath = GetComponent<AIPath>();
        player = aiDestinationSetter.target;

        //create a shieldat the player position if one does not exist
        if (!player.GetComponentInChildren<MosquitoShield>())
        {
            Instantiate(mosquitoShield, player.transform, false);
            transform.localPosition = Vector3.zero;

        }

        enemyDistance.preWrapMode = WrapMode.PingPong;
        enemyDistance.postWrapMode = WrapMode.PingPong;

        spring = GetComponent<SpringJoint2D>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {

        aiPath.maxSpeed = followSpeed;

        if (attackMode == false)
        {            
       
            //constantly adjust spring distance
            spring.distance = enemyDistance.Evaluate(Time.time);
        }
        else
        {
            if(Time.time > impulseBurstFrequency)
            {

                SetRandomTimer();
            }

            FireCheck();
        }
 

        Vector3 dir = (player.position - transform.position).normalized;
        Debug.DrawRay(transform.position, dir * 1000, Color.red);


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
            print("attached");
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

        print("firing");
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
}
