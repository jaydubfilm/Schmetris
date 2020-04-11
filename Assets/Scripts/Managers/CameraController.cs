using UnityEngine;

//Camera smoothing controls
public class CameraController : MonoBehaviour
{
    Vector3 startPos;
    public float smoothing = 4.0f;

    //Init
    void Start()
    {
        DontDestroyOnLoad(this);
        startPos = transform.position;
    }

    //Smooth camera to center over bot
    void Update()
    {
        transform.position = Vector3.Lerp(transform.position, startPos, smoothing * Time.deltaTime);
    }
}
