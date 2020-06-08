using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    Transform target = null;
    float duration = 0.1f;
    Vector3 startPos;
    float startDuration;

    public void Init(Transform newTarget)
    {
        startPos = transform.position;
        startDuration = duration;
        target = newTarget;
    }

    // Update is called once per frame
    void Update()
    {
        duration = Mathf.Max(0, duration - Time.deltaTime);
        if (!target || duration <= 0)
        {
            Destroy(gameObject);
        }
        else
        {
            transform.position = startPos + (target.position - startPos) * (startDuration - duration) / startDuration;
        }
    }
}
