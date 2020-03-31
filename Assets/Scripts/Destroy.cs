using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroy : MonoBehaviour
{

    public float secondsUntilDestroy = 1.5f;
    float timeAtStart;
    // Start is called before the first frame update
    void Start()
    {
        timeAtStart = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.time - timeAtStart > secondsUntilDestroy)
        {
            Destroy(gameObject);
        }
    }
}
