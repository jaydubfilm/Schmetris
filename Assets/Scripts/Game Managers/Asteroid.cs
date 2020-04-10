using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Asteroid : MonoBehaviour
{

    public int hP;
    Bit bit;

    // Start is called before the first frame update
    void Start()
    {
        bit = GetComponent<Bit>();
    }

    public void AdjustHP(int damage, Transform bullet)
    {
        hP -= damage;
        if (hP <= 0)
        {
            bit.RemoveFromBlock("explode");
        }
    }
}
