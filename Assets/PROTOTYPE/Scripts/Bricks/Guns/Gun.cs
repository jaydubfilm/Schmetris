using UnityEngine;

//Basic gun brick that unique guns can inherit from
public class Gun : MonoBehaviour
{ 
    //Gun data
    public float[] rateOfFire;
    public int[] attackPower;
    public float[] range;
    public float speed;
    public bool isHoming = true;
    public AudioClip fireSound;

    //Components
    protected Brick parentBrick;

    //Bullets
    protected float fireTimer;
    public GameObject[] bullet;
    Vector2Int direction;
    Transform enemyTarget;

    //Return the burn rate of resources of set type converted to per-second units
    public float GetConvertedBurnRate(ResourceType resourceType, int level)
    {
        float secondRate = rateOfFire[level] > 0 ? 1.0f / rateOfFire[level] : 0;
        switch (resourceType)
        {
            case ResourceType.Red:
                return GetComponent<Brick>().redBurn[level] * secondRate;
            case ResourceType.Blue:
                return GetComponent<Brick>().blueBurn[level] * secondRate;
            case ResourceType.Green:
                return GetComponent<Brick>().greenBurn[level] * secondRate;
            case ResourceType.Yellow:
                return GetComponent<Brick>().yellowBurn[level] * secondRate;
            case ResourceType.Grey:
                return GetComponent<Brick>().greyBurn[level] * secondRate;
        }
        return 0;
    }

    //Init
    void Start()
    {
        parentBrick = gameObject.GetComponent<Brick>();
        fireTimer = rateOfFire[parentBrick.GetPoweredLevel()];
    }

    // Update is called once per frame
    void Update()
    {
        if (fireTimer <= 0)
        {
            TryFire();
        }
        else
        {
            fireTimer -= Time.deltaTime;
        }
    }

    //Check for targets and ammo and try to shoot
    void TryFire()
    {
        GameObject target = FindTarget();
        if (target != null && parentBrick.TryBurnResources(1.0f))
        {
            enemyTarget = target.transform;
            FireGun();
        }
    }

    //Find something to fire at - implemented by gun classes
    protected virtual GameObject FindTarget()
    {
        return null;
    }

    //Shoot at target, burn resources, and begin reload
    protected virtual void FireGun()
    {
        GameObject newBulletObj = Instantiate(bullet[parentBrick.GetPoweredLevel()], transform.position, Quaternion.identity);
        Bullet newBullet = newBulletObj.GetComponent<Bullet>();
        newBullet.SetAsHoming(enemyTarget, isHoming);
        newBullet.direction = Vector3.Normalize(enemyTarget.position - transform.position);
        newBullet.speed = speed;
        newBullet.damage = attackPower[parentBrick.GetPoweredLevel()];
        newBullet.range = range[parentBrick.GetPoweredLevel()];

        fireTimer = rateOfFire[parentBrick.GetPoweredLevel()];

        GameController.Instance.bot.GetComponent<AudioSource>().PlayOneShot(fireSound, 0.5f);
    }
}
