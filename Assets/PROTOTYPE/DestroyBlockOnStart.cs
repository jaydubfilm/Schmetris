﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.Prototype
{
    [System.Obsolete("Prototype Only Script")]
    public class DestroyBlockOnStart : MonoBehaviour
    {

        Block block;

        // Start is called before the first frame update
        void Start()
        {
            block = GetComponent<Block>();
            block.DestroyBlock();
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}