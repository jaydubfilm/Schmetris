using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public float[] rateOfFire;
    public int[] attackPower;
    int gunLevel;
    float startTime;
    Brick parentBrick;
    public GameObject[] bullet;
    Vector2Int direction;

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
            FireGun();
            startTime = Time.time;
        }
    }

    public void FireGun(){
        GameObject newBullet = Instantiate(bullet[gunLevel],parentBrick.transform.position, Quaternion.identity);
        newBullet.GetComponent<Rigidbody2D>().velocity = new Vector2(0.0f,30f);
        newBullet.transform.parent = gameObject.transform;
    }
}
