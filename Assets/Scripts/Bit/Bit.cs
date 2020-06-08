using System.Collections;
using System.Collections.Generic;
using StarSalvager;
using UnityEngine;

namespace StarSalvager
{
    public class Bit : AttachableBase, IBit
    {
        public BIT_TYPE Type { get; set; }
        public int level { get; set; }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        protected override void OnCollide()
        {
            throw new System.NotImplementedException();
        }
    }
}