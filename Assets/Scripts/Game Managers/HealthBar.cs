using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    private Transform bar;
   
    void Awake()
    {
        bar = gameObject.transform.GetChild(2);
    }

    public void SetSize(float sizeNormalized){
         bar.localScale = new Vector3(sizeNormalized,1f);
    }
}
