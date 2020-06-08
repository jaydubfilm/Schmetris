using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.Prototype
{
    [System.Obsolete("Prototype Only Script")]
    public class EnableDisableObj : MonoBehaviour
    {

        //enables or disables obj on start
        public bool enable;
        public GameObject obj;
        public bool setFuelAmt;


        void OnEnable()
        {
            if (enable) obj.SetActive(true);
            else obj.SetActive(false);

            if (setFuelAmt)
                TutorialManager.Instance.SetFuel(27);

        }

    }
}