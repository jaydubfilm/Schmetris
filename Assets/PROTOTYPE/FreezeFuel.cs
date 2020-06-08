using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.Prototype
{
    [System.Obsolete("Prototype Only Script")]
    public class FreezeFuel : MonoBehaviour
    {
        float fuelAmt;

        int fuelAsInt;
        //int fuelAsInt = int(fuelAmt);

        // Start is called before the first frame update
        void Start()
        {
            fuelAmt = TutorialManager.Instance.playerPos.GetComponent<Bot>().storedRed;
            fuelAsInt = Mathf.RoundToInt(fuelAmt);
        }

        // Update is called once per frame
        void Update()
        {
            TutorialManager.Instance.SetFuel(fuelAsInt);
        }
    }
}