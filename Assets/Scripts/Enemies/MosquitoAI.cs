using UnityEngine;
using Pathfinding;

//Mosquito type enemy behaviour
public class MosquitoAI : Enemy
{
    //Enemy stats from Asset
    float followSpeed;
    float rateOfFire;
    int bulletDamage;

    //Movement
    AIDestinationSetter aiDestinationSetter;
    AIPath aiPath;
    Transform player;
    GameObject pivot;
    SpringJoint2D spring;
    public float impulseBurstFrequency = 1;
    public AnimationCurve enemyDistance;
    private float storedTime;
    public GameObject mosquitoShield;
    bool attackMode;
    bool attached;

    //Bullets
    public float bulletSpeed;
    public float bulletLifetime = 2;
    public GameObject bullet;
    public Sprite bulletSprite;

    //Death effect
    public float timeUntilBlast = 5;
    public float blastRadius = 5;
    public int blastDamage = 5;
    public GameObject debugExplosion;
    float timeOfDeath;
    SpriteRenderer spriteRenderer;

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
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        //create a shield at the player position if one does not exist
        if (player.GetComponentInChildren<MosquitoShield>() == null)
        {
            Instantiate(mosquitoShield, player.transform, false);
        }

        enemyDistance.preWrapMode = WrapMode.PingPong;
        enemyDistance.postWrapMode = WrapMode.PingPong;
        spring = GetComponent<SpringJoint2D>();
        blastRadius = blastRadius * ScreenStuff.colSize;
    }

    //Movement and shooting behaviour when alive
    protected override void UpdateLiveBehaviour()
    {
        base.UpdateLiveBehaviour();
        aiPath.maxSpeed = followSpeed;

        if (attackMode == false)
        {
            //constantly adjust spring distance
            spring.distance = enemyDistance.Evaluate(Time.time);
        }
        else
        {
            if (Time.time > impulseBurstFrequency)
            {
                SetRandomTimer();
            }
            FireCheck();
        }

        Vector3 dir = (player.position - transform.position).normalized;
    }

    //Once dead, count down to explosion
    protected override void UpdateDeathBehaviour()
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
        if (spring)
        {
            Destroy(spring);
        }
        GetComponent<Rigidbody2D>().velocity = new Vector3(0, -GameController.Instance.blockSpeed * GameController.Instance.adjustedSpeed, 0);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (GameController.Instance.isLevelCompleteQueued)
            return;

        if (collision.gameObject.layer == 13)
        {
            aiPath.canMove = false;

            //Create a Spring at player Pos
            spring.connectedBody = player.GetComponent<Rigidbody2D>();
            spring.autoConfigureDistance = false;
            spring.enableCollision = true;
            attackMode = true;

            //storedTime = Time.time;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (GameController.Instance.isLevelCompleteQueued)
            return;

        if (collision.gameObject.layer == 13)
        {
            attackMode = false;
            spring.connectedBody = null;
            aiPath.canMove = true;
        }
    }

    //Make movement a little unpredictable
    void SetRandomTimer()
    {
        AddRandomForce();
        impulseBurstFrequency = Random.Range(0.3f, 1f);
        impulseBurstFrequency += Time.time;
    }

    //Random movement effect
    void AddRandomForce()
    {
        rb2d.AddForce(new Vector2(Random.Range(20f, 20f), Random.Range(55f, 55f)), ForceMode2D.Impulse);
        rb2d.AddTorque(Random.Range(8f, 12f));
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
        Vector3 dir = (player.position - thisBullet.transform.position).normalized;
        thisBullet.MosquitoBulletBehaviour(player.position, bulletSprite, dir, bulletLifetime, bulletSpeed, bulletDamage);
    }

    //Explode on death and damage bricks in range
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
