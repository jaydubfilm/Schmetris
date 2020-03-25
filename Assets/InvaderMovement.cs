﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;


public class InvaderMovement : MonoBehaviour
{

    [FoldoutGroup("Movement")]
    public float speed = 1;

    [FoldoutGroup("Movement")]
    [Tooltip("The distance frome the edge of the screen at which the Invader will reverse direction (screen width ranges 0 - 1)")]
    public float margin = 0.05f;

    [FoldoutGroup("Movement")]
    [Tooltip("The distance frome the player along the x axis at which the enemy will fire")]
    public float fireOnProximity = 3;

    [FoldoutGroup("Weapon")]
    public GameObject bullet;
    [FoldoutGroup("Weapon")]
    public int bulletDamage = 1;

    [FoldoutGroup("Weapon")]
    public float rateOfFire;

    [FoldoutGroup("Weapon")]
    public float bulletSpeed = 3f;

    [FoldoutGroup("Weapon")]
    public float bulletDuration;

    [FoldoutGroup("Weapon")]
    public Sprite bulletSprite;

    Transform player;

    bool movingRight;
    Vector3 pos;
    bool getTime;
    float storedTime;

    // Start is called before the first frame update
    void Start()
    {

        player = FindObjectOfType<Bot>().transform;
    }

    // Update is called once per frame
    void Update()
    {

        #region MOVEMENT
        if(movingRight == true)        
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

    void ShiftDownOneRow()
    {

        transform.position = new Vector3(transform.position.x, transform.position.y - ScreenStuff.rowSize);
    }

    void FireCheck()
    {

        if (Time.time - storedTime > rateOfFire)
        {

            print("firing");
            Fire();
            storedTime = Time.time;
        }
    }
    void Fire()
    {

        EnemyBulletV2 thisBullet = Instantiate(bullet, transform.position, transform.rotation).GetComponent<EnemyBulletV2>();
        thisBullet.InvaderBulletBehaviour(bulletSprite, bulletSpeed, bulletDuration, bulletDamage); 
    }
}
