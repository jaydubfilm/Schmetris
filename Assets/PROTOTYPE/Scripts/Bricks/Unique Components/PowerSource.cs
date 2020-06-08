using UnityEngine;

namespace StarSalvager.Prototype
{
    [System.Obsolete("Prototype Only Script")]//This brick provides power to other nearby bricks
    public class PowerSource : MonoBehaviour
    {
        //Available power at this brick's location by its associated brick level
        public int[] powerAtLevel;
    }
}