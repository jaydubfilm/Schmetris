using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FuelBar : MonoBehaviour
{
    private Transform bar;


    // Start is called before the first frame update
    void Awake()
    {
        DontDestroyOnLoad(this);
        bar = transform.Find("Bar");

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetLevel (float levelNormalized){
        bar.localScale = new Vector3(levelNormalized,1f);
    }
}
