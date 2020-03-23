using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;


public class InvaderMovement : MonoBehaviour
{
    public float speed = 1;
    
    [Tooltip("The distance frome the edge of the screen at which the Invader will reverse direction (screen width ranges 0 - 1)")]
    public float margin = 0.05f;

    [Tooltip("The distance frome the player along the x axis at which the enemy will fire")]
    public float fireOnProximity = 3;

    public float rateOfFire;
    public GameObject bullet;
    public float bulletSpeed = 3f;

    Transform player;

    public bool movingRight;
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

        print("firing");
        if (Time.time - storedTime > rateOfFire)
        {

            Fire();
            storedTime = Time.time;
        }
    }
    void Fire()
    {

        InvaderBullet thisBullet = Instantiate(bullet, transform.position, transform.rotation).GetComponent<InvaderBullet>();
        thisBullet.speed = bulletSpeed;
    }
}
