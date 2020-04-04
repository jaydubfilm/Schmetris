using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;

//Basic gun brick
public class GunTripleShot : MonoBehaviour
{ 
    //Gun data
    public float[] rateOfFire;
    public int[] attackPower;
    public float[] range;
    public AudioClip fireSound;

    //Components
    Brick parentBrick;

    //Bullets
    float fireTimer;
    public GameObject[] bullet;
    Vector2Int direction;

    //Resources
    public int numberOfEnemies = 3;
    public List <GameObject> targets = new List<GameObject>();


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
            fireTimer = rateOfFire[parentBrick.GetPoweredLevel()];

        }
        else
        {
            fireTimer -= Time.deltaTime;
        }
    }

    //Check for targets and ammo and try to shoot
    void TryFire()
    {
        print("trying to fire sniper");
        if (GameController.Instance.enemyList.Count > 0)
        {
            print("enemies detected");
            targets = FindTargets();
            if (targets.Count > 0)
            {
                print("Found Targets");
                if (targets[0] != null)
                {
                    //print("try fire target found");
                    if (Vector3.Distance(targets[0].transform.position, transform.position) < range[parentBrick.GetPoweredLevel()] && parentBrick.TryBurnResources(1.0f))
                        FireGun();
                }
            }
        }
    }

    //Look for closest enemy in range
    public List<GameObject> FindTargets()
    {
    
        float closestDistance = 99;
        targets.Clear();

        for (int i = 0; i < numberOfEnemies; i++)
        {        
            foreach (GameObject enemyObj in GameController.Instance.enemyList)
            {
                if (enemyObj)
                {
                    if (!targets.Contains(enemyObj))
                    {
                        print(enemyObj.name);
                        float dist = Vector3.Distance(enemyObj.transform.position, transform.position);
                        if ((dist < closestDistance) && (dist <= range[parentBrick.GetPoweredLevel()]))
                        {
                            closestDistance = dist;
                            print(i);
                            targets.Add(enemyObj);
                            closestDistance = 99;
                        }
                    }
                }
            }
        }
        
        return targets;
              
    }

    //Shoot at target, burn resources, and begin reload
    public void FireGun()
    {
        print("Fire");
        foreach (GameObject target in targets)
        {
            print(target.name + " 1 of " + targets.Count);
        }
        for (int i = 0; i < targets.Count; i++)
        {

            GameObject newBulletObj = Instantiate(bullet[parentBrick.GetPoweredLevel()], transform.position, Quaternion.identity);
            //Vector3 dirV3 = Vector3.Normalize(targetPos - transform.position);
            Bullet newBullet = newBulletObj.GetComponent<Bullet>();
            newBullet.direction = Vector3.Normalize(targets[i].transform.position - transform.position); //new Vector2(dirV3.x, dirV3.y);
            newBullet.speed = speed;
            newBullet.damage = attackPower[parentBrick.GetPoweredLevel()];
            newBullet.range = range[parentBrick.GetPoweredLevel()];
            //newBullet.SetAsHoming(target.transform);

            fireTimer = rateOfFire[parentBrick.GetPoweredLevel()];
            if (targets[i].GetComponent<InvaderMovement>())
                newBullet.SetAsHoming (targets[i].transform, true);
        }
        GameController.Instance.bot.GetComponent<AudioSource>().PlayOneShot(fireSound, 0.5f);
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
