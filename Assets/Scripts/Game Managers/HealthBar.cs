using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    private Transform bar;
   
    // Start is called before the first frame update

    void Awake()
    {
        bar = gameObject.transform.GetChild(2);
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SetSize(float sizeNormalized){
         bar.localScale = new Vector3(sizeNormalized,1f);
    }
}
