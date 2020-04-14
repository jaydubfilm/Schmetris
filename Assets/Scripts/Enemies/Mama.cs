using UnityEngine;
using Pathfinding;

//Large Parasite-spawning enemy
public class Mama : Enemy
{
    //Enemy stats from Asset
    float followSpeed;
    float rateOfFire;
    int bulletDamage = 1;

    [Tooltip("How frequently we check the distance")]
    public float distanceCheckTimer = 1f;

    //Bullets
    public float bulletSpeed;
    public float bulletLifetime = 2;
    public GameObject bullet;
    public float bulletScale = 3;
    public Sprite bulletSprite;
    float timer;
    float timeSinceFire;

    //Death effect
    public float timeUntilBlast = 5;
    public float blastRadius = 5;
    public int blastDamage = 5;
    public GameObject debugExplosion;
    public GameObject parasitePrefab;
    float timeOfDeath;
    SpriteRenderer spriteRenderer;

    //Movement
    AIPath aiPath;
    Transform player;
    AIDestinationSetter aiDestinationSetter;
    public float force;
    float storedTime;


    //Init
    protected override void Init()
    {
        base.Init();
        followSpeed = data.speed;
        rateOfFire = data.attackRate;
        bulletDamage = data.damage;
        aiPath = GetComponent<AIPath>();
        aiDestinationSetter = GetComponent<AIDestinationSetter>();
        aiDestinationSetter.target = FindObjectOfType<Bot>().transform;
        player = aiDestinationSetter.target;
        timer = Time.time;
        aiPath.maxSpeed = followSpeed;
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    //While alive, keep within range of player and shoot
    protected override void UpdateLiveBehaviour()
    {
        base.UpdateLiveBehaviour();

        //Every x seconds, check the distance between the enemy and the player. Back away if we're too close. 
        if (Time.time - timer > distanceCheckTimer)
        {
            if (Vector3.Distance(transform.position, player.position) < aiPath.endReachedDistance)
            {
                BackAwayFromPlayer();
            }
            else
            {
                //When the enemy is not too close, but near enough to fire
                if (Vector3.Distance(transform.position, player.position) < aiPath.endReachedDistance + 2)
                {
                    FireCheck();
                }
            }

        }
    }

    //Once dead, count down to explosion and release of Parasites
    protected override void UpdateDeathBehaviour()
    {
        base.UpdateDeathBehaviour();

        //change enemy color as it dies
        float timeUntilFlashing = timeUntilBlast - 1.5f;

        if (Time.timeSinceLevelLoad - timeOfDeath < timeUntilBlast)
        {
            Color enemyColor = new Color(spriteRenderer.color.r, 1 - ((Time.timeSinceLevelLoad - timeOfDeath) / timeUntilFlashing), 1 - ((Time.timeSinceLevelLoad - timeOfDeath) / timeUntilFlashing));
            spriteRenderer.color = enemyColor;
        }
        else if (Time.timeSinceLevelLoad - timeOfDeath > timeUntilBlast)
        {
            DeathBlast();
        }
    }

    //Begin explosion timer when hp hits 0
    protected override void OnEnemyDeath()
    {
        base.OnEnemyDeath();
        timeOfDeath = Time.timeSinceLevelLoad;
    }

    //Turn off AI pathfinding on level completion
    protected override void OnLevelComplete()
    {
        base.OnLevelComplete();
        aiPath.enabled = false;
        aiDestinationSetter.enabled = false;
    }

    //Keep a certain distance away from player
    void BackAwayFromPlayer()
    {
        Vector3 angleFromPlayer = (transform.position - player.position).normalized;
        rb2d.AddForce(new Vector2(angleFromPlayer.x, angleFromPlayer.y) * force);
    }

    //Count down time until enemy can shoot again
    void FireCheck()
    {
        if (Time.time - storedTime > rateOfFire)
        {
            Fire();
            storedTime = Time.time;
        }
    }

    //Shoot at player
    void Fire()
    {
        EnemyBulletV2 thisBullet = Instantiate(bullet, transform.position, Quaternion.identity).GetComponent<EnemyBulletV2>();
        thisBullet.transform.localScale = new Vector3(thisBullet.transform.localScale.x * bulletScale, thisBullet.transform.localScale.y * bulletScale, thisBullet.transform.localScale.z * bulletScale);
        Vector3 dir = (player.position - thisBullet.transform.position).normalized;
        thisBullet.MamaBulletBehaviour(player, bulletSprite, bulletSpeed, bulletLifetime, bulletDamage);
    }

    //Explode and release Parasites when destroyed
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

        //Spawn parasites in blast range
        for(int i = 0;i<4;i++)
        {
            Vector3 blastOffset = new Vector3(Random.Range(-blastRadius / 2.0f, blastRadius / 2.0f), Random.Range(-blastRadius / 2.0f, blastRadius / 2.0f), 0);
            GameObject newParasite = Instantiate(parasitePrefab, transform.position + blastOffset, Quaternion.identity);
        }

        //Instantiate Explosion here
        Destroy(gameObject);
    }
}