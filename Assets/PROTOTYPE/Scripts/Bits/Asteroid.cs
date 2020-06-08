using UnityEngine;

namespace StarSalvager.Prototype
{
    [System.Obsolete("Prototype Only Script")]
//Asteroid bits
    public class Asteroid : MonoBehaviour
    {
        //Components
        Bit bit;

        //Health
        public int hP;

        //Init
        void Start()
        {
            bit = GetComponent<Bit>();
        }

        //Adjust health and check if bit is destroyed
        public void AdjustHP(int amount)
        {
            hP += amount;
            if (hP <= 0)
            {
                bit.RemoveFromBlock("explode");
            }
        }
    }
}