using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using Sirenix.OdinInspector;

public class Mama : MonoBehaviour
{

    [FoldoutGroup("Movement")]
    public float followSpeed = 3;

    [Tooltip("How frequently we check the distance")]
    [FoldoutGroup("Movement")]
    public float distanceCheckTimer = 1f;

    [FoldoutGroup("Weapon")]
    public float rateOfFire;

    [FoldoutGroup("Weapon")]
    public float bulletSpeed;

    [FoldoutGroup("Weapon")]
    public float bulletLifetime = 2;

    [FoldoutGroup("Weapon")]
    public GameObject bullet;

    [FoldoutGroup("Weapon")]
    public float bulletScale = 3;

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
    public int strength = 3;

    public GameObject debugExplosion;


    bool dying;

    AIPath aiPath;
    Transform player;
    AIDestinationSetter aiDestinationSetter;
    Rigidbody2D rb2d;
    SpriteRenderer spriteRenderer;

    public float force;
    float timer;
    float timeSinceFire;
    bool backAway;
    float storedTime;
    private float timeOfDeath;

    // Start is called before the first frame update
    void Start()
    {
        aiPath = GetComponent<AIPath>();
        aiDestinationSetter = GetComponent<AIDestinationSetter>();
        aiDestinationSetter.target = FindObjectOfType<Bot>().transform;
        player = aiDestinationSetter.target;
        rb2d = GetComponent<Rigidbody2D>();
        timer = Time.time;
        aiPath.maxSpeed = followSpeed;
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();


    }

    // Update is called once per frame
    void Update()
    {

        //Every x seconds, check the distance between the enemy and the player. Back away if we're too close. 
        if (Time.time - timer > distanceCheckTimer)
        {
            if (Vector3.Distance(transform.position, player.position) < aiPath.endReachedDistance)           
                backAway = true;
            else
            {
                backAway = false;

                //When the enemy is not too close, but near enough to fire
                if (Vector3.Distance(transform.position, player.position) < aiPath.endReachedDistance + 2)
                    FireCheck();
            }
                
        }
        
        if(backAway == true)
            BackAwayFromPlayer();

        //Death Procedure
        if (dying == true)
        {
            //change enemy color as it dies
            float timeUntilFlashing = timeUntilBlast - 1.5f;

            if (Time.timeSinceLevelLoad - timeOfDeath < timeUntilBlast)
            {
                Color enemyColor = new Color(spriteRenderer.color.r, 1 - ((Time.timeSinceLevelLoad - timeOfDeath) / timeUntilFlashing), 1 - ((Time.timeSinceLevelLoad - timeOfDeath) / timeUntilFlashing));
                spriteRenderer.color = enemyColor;
            }

            if (Time.timeSinceLevelLoad - timeOfDeath > timeUntilBlast)
            {
                DeathBlast();
            }
        }

        //testing
        if (Input.GetKeyDown(KeyCode.P))
        {
            Death();
            print(ScreenStuff.colSize);
        }
    }

    void BackAwayFromPlayer()
    {

        print("pushing");
        Vector3 angleFromPlayer = (transform.position - player.position).normalized;
        rb2d.AddForce(new Vector2(angleFromPlayer.x, angleFromPlayer.y) * force);
        Debug.DrawRay(transform.position, angleFromPlayer);
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
        thisBullet.transform.localScale = new Vector3(thisBullet.transform.localScale.x * bulletScale, thisBullet.transform.localScale.y * bulletScale, thisBullet.transform.localScale.z * bulletScale);
        Vector3 dir = (player.position - thisBullet.transform.position).normalized;
        thisBullet.MamaBulletBehaviour(player, bulletSprite, bulletSpeed, bulletLifetime, bulletDamage);
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
}
