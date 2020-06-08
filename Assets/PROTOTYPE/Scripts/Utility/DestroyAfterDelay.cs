using UnityEngine;

namespace StarSalvager.Prototype
{
    [System.Obsolete("Prototype Only Script")]
//Used to clean up FX or other temporary assets when finished
    public class DestroyAfterDelay : MonoBehaviour
    {
        //Destroy asset after a set time has passed
        public float secondsUntilDestroy = 1.5f;

        //Init
        void Start()
        {
            Destroy(gameObject, secondsUntilDestroy);
        }
    }
}