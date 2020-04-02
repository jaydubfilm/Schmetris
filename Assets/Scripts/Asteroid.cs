using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Asteroid : MonoBehaviour
{

    public int hP;
    Block block;
    // Start is called before the first frame update
    void Start()
    {
        block = GetComponent<Block>();
    }

    public void AdjustHP(int damage, Transform bullet)
    {
        print("hit asteroid") ;

        hP -= damage;        
        if(hP <= 0)
        {
            block.DestroyBlock();
        }
            Destroy(bullet.gameObject);
    }
}
