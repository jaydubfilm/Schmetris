using System.Collections;
using System.Collections.Generic;
using StarSalvager;
using UnityEngine;

namespace StarSalvager
{


    public class Bit_Collidable : CollidableBase, IBit
    {
        public BIT_TYPE Type { get; set; }
        
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