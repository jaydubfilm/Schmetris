using UnityEngine;

//Space Invader type enemy
public class InvaderMovement : Enemy
{
    //Enemy stats from Asset
    float speed;
    float rateOfFire;
    int bulletDamage;

    [Tooltip("The distance frome the edge of the screen at which the Invader will reverse direction (screen width ranges 0 - 1)")]
    public float margin = 0.05f;

    [Tooltip("The distance frome the player along the x axis at which the enemy will fire")]
    public float fireOnProximity = 3;

    [Tooltip("The number of rows the invader drops down apon reaching the edge of the screen")]
    public float dropDownDistance = 4;

    //Bullets
    public GameObject bullet;
    public float bulletSpeed = 3f;
    public float bulletDuration;
    public Sprite bulletSprite;

    //Targeting
    Transform player;

    //Movement
    bool movingRight;
    Vector3 pos;
    bool getTime;
    float storedTime;

    //Init
    protected override void Init()
    {
        base.Init();
        speed = data.speed;
        rateOfFire = data.attackRate;
        bulletDamage = data.damage;
        player = FindObjectOfType<Bot>().transform;
    }

    //Update enemy behaviours
    protected override void UpdateLiveBehaviour()
    {
        base.UpdateLiveBehaviour();

        #region MOVEMENT
        if (movingRight == true)
            transform.Translate(Vector3.right * speed * Time.deltaTime);
        else
            transform.Translate(Vector3.right * -speed * Time.deltaTime);


        pos = Camera.main.WorldToViewportPoint(transform.position);

        if (pos.x < margin && movingRight == false)
        {

            movingRight = true;
            ShiftDownOneRow();

        }
        if (1 - margin < pos.x && movingRight == true)
        {

            movingRight = false;
            ShiftDownOneRow();
        }
        #endregion

        #region ATTACKING
        if (transform.position.x > player.position.x - fireOnProximity && transform.position.x < player.position.x + fireOnProximity)
        {

            if (getTime == false)
            {
                storedTime = Time.time;
                getTime = true;
            }

            FireCheck();
        }
        else
            getTime = false;

        #endregion
    }

    //Destroy after awarding points
    protected override void OnEnemyDeath()
    {
        base.OnEnemyDeath();
        Destroy(gameObject);
    }

    //Drop toward player
    void ShiftDownOneRow()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y - (ScreenStuff.rowSize * dropDownDistance));
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

    //Shoot at target
    void Fire()
    {
        EnemyBulletV2 thisBullet = Instantiate(bullet, transform.position, transform.rotation).GetComponent<EnemyBulletV2>();
        thisBullet.InvaderBulletBehaviour(bulletSprite, bulletSpeed, bulletDuration, bulletDamage); 
    }

    //Player can destroy this enemy type by running into it
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<Brick>() && !isDestroyed)
        {
            OnEnemyDeath();
        }
    }
}