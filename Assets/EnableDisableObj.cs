﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableDisableObj : MonoBehaviour
{

    //enables or disables obj on start
    public bool enable;
    public GameObject obj;
    public bool setFuel;


    void Start()
    {
        if (enable) obj.SetActive(true);
        else obj.SetActive(false);

        if(setFuel)
            TutorialManager.Instance.SetFuel(16);

    }

}
