using UnityEngine;

public class HealthBar : MonoBehaviour
{
    public Transform bar;
   
    void Awake()
    {
        if (!bar)
        {
            bar = gameObject.transform.GetChild(2);
        }
    }

    public void SetSize(float sizeNormalized)
    {
        bar.localScale = new Vector3(sizeNormalized, 1f);
    }
}
