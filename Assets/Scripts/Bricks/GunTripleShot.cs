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

    //Components
    Brick parentBrick;

    //Bullets
    float fireTimer;
    public GameObject[] bullet;
    Vector2Int direction;

    //Resources
    public int[] maxResource;
    public float[] burnPerShot;
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
        }
        else
        {
            fireTimer -= Time.deltaTime;
        }
    }

    //Check for targets and ammo and try to shoot
    void TryFire()
    {
        
        if (GameController.Instance.bot.storedBlue >= burnPerShot[parentBrick.GetPoweredLevel()])
        {
            print("try fire ");
            targets = FindTargets();
            if (targets.Count > 0)
            {
                print("Found Targets");
                if (targets[0] != null)
                {
                    //print("try fire target found");
                    if (Vector3.Distance(targets[0].transform.position, transform.position) < range[parentBrick.GetPoweredLevel()])
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

            GameController.Instance.bot.storedBlue -= burnPerShot[parentBrick.GetPoweredLevel()];
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
    }

   


}
