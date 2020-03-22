using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvaderBullet : MonoBehaviour
{

    public float speed = 1;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        transform.Translate(-Vector3.up * speed * Time.deltaTime);
    }

    void Damage()
    {

        //Link up to Megan's Damage System Here
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.transform.GetComponent<Bot>() != null)
        {
            //hit player core
            //Apply Damage Here

            Destroy(gameObject); 
            return;
        }

        if(other.transform.GetComponentInParent<Bot>() != null)
        {
            //hit a block on player ship
            //Apply Damage Here

            Destroy(gameObject);
            return;
        }

        if (other.transform.GetComponentInParent<Bit>() != null)
        {
            print("hit floating brick");
            Destroy(gameObject);
            //hit a floating brick
            return;
        }
    }


}
