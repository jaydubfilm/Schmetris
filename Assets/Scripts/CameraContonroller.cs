using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraContonroller : MonoBehaviour
{
    Vector3 startPos;
    public float smoothing = 4.0f;

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this);
        startPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.Lerp(transform.position, startPos, smoothing * Time.deltaTime);
    }
}
