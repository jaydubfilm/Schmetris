using UnityEngine;

//Basic gun brick
public class Gun : MonoBehaviour
{ 
    //Gun data
    public float[] rateOfFire;
    public int[] attackPower;
    public float[] range;
    public float speed;

    //Components
    Brick parentBrick;

    //Bullets
    float fireTimer;
    public GameObject[] bullet;
    Vector2Int direction;

    //Resources
    public int[] maxResource;

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
            FireGun(target.transform.position);
        }
    }

    //Look for closest enemy in range
    public GameObject FindTarget()
    {
        float closestDistance = 99;
        GameObject target = null;
        foreach (GameObject enemyObj in GameController.Instance.enemyList)
        {
            if (enemyObj)
            {
                float dist = Vector3.Distance(enemyObj.transform.position, transform.position);
                if ((dist < closestDistance) && (dist <= range[parentBrick.GetPoweredLevel()]))
                {
                    closestDistance = dist;
                    target = enemyObj;
                }
            }
        }
        return target;
    }

    //Shoot at target, burn resources, and begin reload
    public void FireGun(Vector3 targetPos)
    {
        GameObject newBulletObj = Instantiate(bullet[parentBrick.GetPoweredLevel()], transform.position, Quaternion.identity);
        Vector3 dirV3 = Vector3.Normalize(targetPos - transform.position);
        Bullet newBullet = newBulletObj.GetComponent<Bullet>();
        newBullet.direction = new Vector2(dirV3.x, dirV3.y);
        newBullet.speed = speed;
        newBullet.damage = attackPower[parentBrick.GetPoweredLevel()];
        newBullet.range = range[parentBrick.GetPoweredLevel()];

        fireTimer = rateOfFire[parentBrick.GetPoweredLevel()];
    }


}
