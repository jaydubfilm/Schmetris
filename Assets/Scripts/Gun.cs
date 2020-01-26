using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public float[] rateOfFire;
    public int[] attackPower;
    public float[] range;
    public float speed;
    int gunLevel;

    float startTime;
    Brick parentBrick;
    public GameObject[] bullet;
    Vector2Int direction;
    GameObject target;

    // Start is called before the first frame update
    void Start()
    {
        startTime = Time.time;
        parentBrick = gameObject.GetComponent<Brick>();
        gunLevel = parentBrick.brickLevel;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time - startTime >= rateOfFire[parentBrick.brickLevel])
        {
            target = FindTarget();
            if (target!=null)
                FireGun();
            startTime = Time.time;
        }
    }

    public void FireGun(){ 
        GameObject newBulletObj = Instantiate(bullet[gunLevel],transform.position, Quaternion.identity);
        Vector3 dirV3 = Vector3.Normalize(target.transform.position-transform.position);
        Bullet newBullet = newBulletObj.GetComponent<Bullet>();
        newBullet.direction = new Vector2(dirV3.x,dirV3.y);
        newBullet.speed = speed;
        newBullet.damage = attackPower[gunLevel];
        newBullet.range = range[gunLevel];
    }

    public GameObject FindTarget(){
        float closestDistance = 99;
        GameObject target = null;
        foreach (GameObject enemyObj in GameController.Instance.enemyList){
            if (enemyObj)
            {
                float dist = Vector3.Distance(enemyObj.transform.position, transform.position);
                if ((dist < closestDistance) && (dist <= range[gunLevel]))
                {
                    closestDistance = dist;
                    target = enemyObj;
                }
            }
        }
        return target;
    }
}
