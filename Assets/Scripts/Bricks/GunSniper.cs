using UnityEngine;
using Sirenix.OdinInspector;

//Basic gun brick
public class GunSniper : MonoBehaviour
{ 
    //Gun data
    public float[] rateOfFire;
    public int[] attackPower;
    public float[] range;
    bool isInvader;

    //Components
    Brick parentBrick;

    //Bullets
    float fireTimer;
    public GameObject[] bullet;
    Vector2Int direction;

    //Resources
    GameObject target;

    public float speed;

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
        if (GameController.Instance.enemyList.Count > 0)
        {
            print("try fire 0");
            target = FindTarget();
            if (target != null)
            {
                print("try fire target found");
                if (Vector3.Distance(target.transform.position, transform.position) < range[parentBrick.GetPoweredLevel()] && parentBrick.TryBurnResources(1.0f))
                    FireGun(target.transform.position);
            }
        }
    }

    //Look for closest enemy in range
    public GameObject FindTarget()
    {
        int enemyStregth = 0;
        float closestDistance = 99;
        GameObject target = null;
        foreach (GameObject enemyObj in GameController.Instance.enemyList)
        {
            if (enemyObj)
            {
                if (enemyObj.GetComponentInChildren<SpriteRenderer>().isVisible)
                {
                    //Strongest enemy type yet selected
                    if (enemyObj.GetComponent<EnemyGeneral>().strength > enemyStregth)
                    {

                        float dist = Vector3.Distance(enemyObj.transform.position, transform.position);
                        enemyStregth = enemyObj.GetComponent<EnemyGeneral>().strength;
                        closestDistance = dist;
                        target = enemyObj;
                    }

                    //same enemy type as the current strongest enemy
                    else if (enemyObj.GetComponent<EnemyGeneral>().strength == enemyStregth)
                    {

                        float dist = Vector3.Distance(enemyObj.transform.position, transform.position);

                        //first pass - set this as current target
                        if (target == null)
                        {

                            closestDistance = dist;
                            target = enemyObj;
                        }

                        else
                        {
                            //This is the nearest enemy of this type
                            if ((dist < closestDistance))
                            {

                                closestDistance = dist;
                                target = enemyObj;
                                if (enemyObj.GetComponent<InvaderMovement>())
                                    isInvader = true;
                                else
                                    isInvader = false;
                            }
                        }
                    }
                }            
            }
        }

        if (target != null)
        {
            print("target is " + target.name);
        }

        return target;
    }

    //Shoot at target, burn resources, and begin reload
    public void FireGun(Vector3 targetPos)
    {

        GameObject newBulletObj = Instantiate(bullet[parentBrick.GetPoweredLevel()], transform.position, Quaternion.identity);
        //Vector3 dirV3 = Vector3.Normalize(targetPos - transform.position);
        Bullet newBullet = newBulletObj.GetComponent<Bullet>();
        newBullet.direction = Vector3.Normalize(targetPos - transform.position); //new Vector2(dirV3.x, dirV3.y);
        newBullet.speed = speed;
        newBullet.damage = attackPower[parentBrick.GetPoweredLevel()];
        newBullet.range = range[parentBrick.GetPoweredLevel()];
        //newBullet.SetAsHoming(target.transform);
        if(isInvader)
            newBullet.SetAsHoming(target.transform, true);
        fireTimer = rateOfFire[parentBrick.GetPoweredLevel()];
    }

    //Return the resources of set type converted to per-second units
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
}
