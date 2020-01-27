using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Used to clean up FX or other temporary assets when finished
public class TempAsset : MonoBehaviour
{
    public float destroyAfterSeconds = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, destroyAfterSeconds);
    }
}
