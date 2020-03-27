﻿using UnityEngine;
using Sirenix.OdinInspector;

//Basic gun brick
public class GunSniper : MonoBehaviour
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
        print("try fire 0");
        if (GameController.Instance.bot.storedBlue >= burnPerShot[parentBrick.GetPoweredLevel()])
        {
            print("try fire 1");
            target = FindTarget();
            if (target != null)
            {
                print("try fire target found");
                if(Vector3.Distance(target.transform.position, transform.position) < range[parentBrick.GetPoweredLevel()])
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

        GameController.Instance.bot.storedBlue -= burnPerShot[parentBrick.GetPoweredLevel()];
        GameObject newBulletObj = Instantiate(bullet[parentBrick.GetPoweredLevel()], transform.position, Quaternion.identity);
        //Vector3 dirV3 = Vector3.Normalize(targetPos - transform.position);
        Bullet newBullet = newBulletObj.GetComponent<Bullet>();
        newBullet.direction = Vector3.Normalize(targetPos - transform.position); //new Vector2(dirV3.x, dirV3.y);
        newBullet.speed = speed;
        newBullet.damage = attackPower[parentBrick.GetPoweredLevel()];
        newBullet.range = range[parentBrick.GetPoweredLevel()];
        //newBullet.SetAsHoming(target.transform);

        fireTimer = rateOfFire[parentBrick.GetPoweredLevel()];
    }

   


}
